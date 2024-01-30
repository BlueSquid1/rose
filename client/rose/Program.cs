using System.CommandLine;

namespace rose
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            string binaryDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            var rootCommand = new RootCommand("ROSE - RDP Over SSH Executor (ROSE) starts a remoteapp when ssh to a windows computer.");
            var configOption = new Option<FileInfo?>(
            name: "--config",
            description: "The config file to use",
            getDefaultValue: ()=> new FileInfo($"{binaryDirectory}\\config.json"));
            rootCommand.AddGlobalOption(configOption);

            var listCommand = new Command(name:"list", description:"list all the available programs");
            rootCommand.Add(listCommand);

            listCommand.SetHandler((config) => 
            {
                RoseClient client = new RoseClient(config!);
                List<string> availablePrograms = client.ListAvailableShortcuts();
                foreach(string program in availablePrograms)
                {
                    string displayName = System.IO.Path.GetFileNameWithoutExtension(program);
                    Console.WriteLine(displayName);
                }
                Console.WriteLine("");
                Console.WriteLine("----------------------------");
                Console.WriteLine($"Num of programs detected: {availablePrograms.Count}");
                Console.WriteLine(config);
            }, configOption);

            var programArgument = new Argument<string>(name:"program name", description:"program name");
            rootCommand.Add(programArgument);

            rootCommand.SetHandler((config, programName) => 
            {
                RoseClient client = new RoseClient(config!);
                client.StartRdpForProgram(programName);
            }, configOption, programArgument);

            return await rootCommand.InvokeAsync(args);
        }
    }
}