using Microsoft.Win32;
using Shell32;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;


namespace rose
{
    public struct ProgramPair
    {
        public string DisplayName;
        public string Path;
    }
    public struct Config
    {
        public string serverIpAddress;
        public List<ProgramPair> customPrograms;
        
    }

    class RoseClient
    {
        private Config config {get; init;}

        public RoseClient(FileInfo configFile)
        {
            this.config = this.ParseConfigFile(configFile);
        }
        public List<string> ListAvailableShortcuts()
        {
            string userDir = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            string[] systemPrograms = Directory.GetFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\", "*.lnk", SearchOption.AllDirectories);
            string[] userPrograms = Directory.GetFiles(userDir + @"\AppData\Roaming\Microsoft\Windows\Start Menu", "*.lnk", SearchOption.AllDirectories);
            string[] allPrograms = systemPrograms.Concat(userPrograms).ToArray();

            List<string> availablePrograms = new List<string>();
            foreach(string possibleProgram in allPrograms)
            {
                string? result = this.GetTargetPath(possibleProgram);
                if(result != null)
                {
                    if(System.IO.Path.GetExtension(result).ToLower() == ".exe")
                    {
                        availablePrograms.Add(possibleProgram);
                    }
                }
            }

            // Add on the custom programs as well
            foreach(ProgramPair custProgram in this.config.customPrograms)
            {
                availablePrograms.Add(custProgram.DisplayName);
            }
            return availablePrograms;
        }

        public void StartRdpForProgram(string requestedProgram)
        {
            try
            {
                if(this.AllowRdpCheck() == false)
                {
                    this.EnableRdpRegEdit();
                }

                List<string> availablePrograms = this.ListAvailableShortcuts();

                string shortcut = this.FindShortcut(requestedProgram, availablePrograms);

                string? programPath = this.GetTargetPath(shortcut!);

                if(programPath == null)
                {
                    throw new Exception($"failed to get target path from shortcut: {shortcut!}");
                }

                this.SendOpenRequest(requestedProgram, programPath, "", this.config.serverIpAddress);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private Config ParseConfigFile(FileInfo fileName)
        {
            string jsonString = File.ReadAllText(fileName.FullName);
            Config? config = JsonConvert.DeserializeObject<Config>(jsonString);
            if( config == null )
            {
                throw new Exception("failed to parse config file");
            }
            return (Config)config;
        }
        private bool AllowRdpCheck()
        {
            RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", false);
            if(key == null)
            {
                return false;
            }

            object? keyValue = key.GetValue("fAllowUnlistedRemotePrograms");
            if(keyValue == null)
            {
                return false;
            }

            if ( key.GetValueKind("fAllowUnlistedRemotePrograms") != RegistryValueKind.DWord )
            {
                key.Close();
                return false;
            }

            if (keyValue.ToString() != "1")
            {
                key.Close();
                return false;
            }
            key.Close();
            return true;
        }

        private void EnableRdpRegEdit()
        {
            RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", true);
            if(key == null)
            {
                throw new Exception(@"can't find key: SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services");
            }
            key.SetValue("fAllowUnlistedRemotePrograms", 1, RegistryValueKind.DWord);
            key.Close();
        }

        private string? GetTargetPath(string lnkPath)
        {
            // Handle custom programs
            foreach(ProgramPair programPair in this.config.customPrograms)
            {
                if(lnkPath == programPair.DisplayName)
                {
                    return programPair.Path;
                }
            }

            // Due to limitations with shell32 it normally needs to run as administrator if pointed
            // to a shortcut in C:\ProgramData. Therefore copy to temp folder so don't need to run as admin.
            string filenameOnly = System.IO.Path.GetFileName(lnkPath);
            string tempFolder = System.IO.Path.GetTempPath();
            string tempFilePath = $"{tempFolder}{filenameOnly}";
            File.Copy(lnkPath, tempFilePath, true);
            File.SetAttributes(tempFilePath, FileAttributes.Normal);

            try
            {
                string? pathOnly = System.IO.Path.GetDirectoryName(tempFilePath);
                if(pathOnly == null)
                {
                    throw new Exception($"failed to get parent folder from path: {tempFilePath}");
                }

                string? result = null;
                var thread = new Thread( () => 
                    {
                        Shell shell = new Shell();
                        Folder folder = shell.NameSpace(pathOnly!);
                        FolderItem folderItem = folder.ParseName(filenameOnly);
                        if (folderItem == null)
                        {
                            throw new Exception($"can't find shortcut file with shell32 at: {filenameOnly}");
                        }
                        Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                        if (link.Path != string.Empty)
                        {
                            result = link.Path;
                        }
                    }
                );

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                return result;
            }
            catch(System.UnauthorizedAccessException)
            {
                return null;
            }
            finally
            {
                File.Delete(tempFilePath);
            }
            
        }

        private string FindShortcut(string targetName, List<string> allPrograms)
        {
            string targetNameStripped = targetName.ToLower().Replace(" ", string.Empty);
            foreach(string program in allPrograms)
            {
                string programName = System.IO.Path.GetFileNameWithoutExtension(program);
                string programNameStripped = programName.ToLower().Replace(" ", string.Empty);
                if(targetNameStripped == programNameStripped)
                {
                    return program;
                }
            }
            throw new Exception($"can't find program: {targetName}");
        }

        private void SendOpenRequest(string displayName, string targetBinPath, string arguements, string ipAddress)
        {
            RdpRequest request = new RdpRequest
            {
                DisplayName = displayName,
                Command = targetBinPath,
                Arguements = arguements
            };
            string requestBody = JsonConvert.SerializeObject(request);

            StringContent httpContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            Console.WriteLine("sending message");
            HttpClient client = new HttpClient();
            var response = client.PostAsync($"http://{ipAddress}/request", httpContent).Result;
            Console.WriteLine("sent message");
            if(response.IsSuccessStatusCode == false)
            {
                Console.WriteLine($"failed to open: {displayName} for reason: {response.ReasonPhrase}");
            }
        }
    }
}