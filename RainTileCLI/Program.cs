using System.CommandLine;
using RainTileLib;


class Program
{
    static int Main(string[] args) {
        Option<FileInfo> fileOption = new("-i", "--input") {
            Description = "Path to png file used to generate the model"
        };

        RootCommand rootCommand = new("Tool for converting Rain World tile graphics to Minecraft JSON models");
        rootCommand.Options.Add(fileOption);

        rootCommand.SetAction(parseResult => {
            FileInfo parsedFile = parseResult.GetValue(fileOption);
            if (parsedFile.Exists)
                ReadFile(parsedFile);
            else 
                Console.WriteLine("Invalid path");
            return 0;
        });

        if (args.Length == 0) {
            args = new[] {"--help"};
        }

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    static void ReadFile(FileInfo file)
    {
        foreach (string line in File.ReadLines(file.FullName))
        {
            Console.WriteLine(line);
        }
    }
}