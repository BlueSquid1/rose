// See https://aka.ms/new-console-template for more information
using Microsoft.Win32;
using Shell32;
using System.Net.Http;

namespace rose
{
    class Program
    {
        static bool StartupChecks()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", false);
            if (key?.GetValue("fAllowUnlistedRemotePrograms")?.ToString() != "1")
            {
                Console.WriteLine(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\fAllowUnlistedRemotePrograms needs to be set to 1");
                key?.Close();
                return false;
            }
            key?.Close();
            return true;
        }
        static string GetTargetPath(string lnkPath)
        {
            string filenameOnly = System.IO.Path.GetFileName(lnkPath);
            // Some shortcuts don't have a target path. Handle these shortcuts manually
            switch(filenameOnly)
            {
                case "File Explorer.lnk":
                {
                    return @"C:\Windows\explorer.exe";
                }
            }

            // Due to limitations with shell32 it normally needs to run as administrator if pointed
            // to a shortcut in C:\ProgramData. Therefore copy to temp folder so don't need to run as admin.
            string tempFolder = System.IO.Path.GetTempPath();
            string tempFilePath = $"{tempFolder}{filenameOnly}";
            File.Copy(lnkPath, tempFilePath, true);

            string pathOnly = System.IO.Path.GetDirectoryName(tempFilePath);
            

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }
            return string.Empty;
        }

        static string FindShortcut(string targetName, string[] allPrograms)
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
            return null;
        }

        static void DisplayHelp()
        {
            Console.WriteLine("Description:");
            Console.WriteLine("ROSE - RDP Over SSH Executor (ROSE) starts a remoteapp.");
            Console.WriteLine("It's intend to be like x11 forwarding over ssh but using RDP instead.");
            Console.WriteLine("");
            Console.WriteLine("rose [list|<program name>]");
            Console.WriteLine("");
            Console.WriteLine("list - dislay all the programs that can be launched");
            Console.WriteLine("<program name> - the program to launch");
            Console.WriteLine("");
        }

        [STAThread]
        static int Main(string[] args)
        {
            if(StartupChecks() == false)
            {
                return -1;
            }

            // Get all the programs that can be launched
            string userDir = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            string[] systemPrograms = Directory.GetFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\", "*.lnk", SearchOption.AllDirectories);
            string[] userPrograms = Directory.GetFiles(userDir + @"\AppData\Roaming\Microsoft\Windows\Start Menu", "*.lnk", SearchOption.AllDirectories);
            string[] allPrograms = systemPrograms.Concat(userPrograms).ToArray();

            if(args.Length < 1)
            {
                DisplayHelp();
                return -1;
            }
            if( args[0] == "list" )
            {
                foreach(string program in allPrograms)
                {
                    string filenameOnly = System.IO.Path.GetFileNameWithoutExtension(program);
                    Console.WriteLine(filenameOnly);
                }
                Console.WriteLine($"Num of programs detected: {allPrograms.Length}");
            }
            else if( args[0] == "-h" )
            {
                DisplayHelp();
            }
            else
            {
                string targetName = string.Join(" ", args);
                string shortcutPath = FindShortcut(targetName, allPrograms);
                if(shortcutPath == null)
                {
                    Console.WriteLine($"failed to find the program: {targetName}");
                    return 1;
                }
                Console.WriteLine(GetTargetPath(shortcutPath));
            }
            return 0;
        }
    }
}