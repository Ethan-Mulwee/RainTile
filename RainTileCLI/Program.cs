using System.CommandLine;
using RainTileLib;
using static RainTileLib.TileConversion;


class Program
{
    static int Main(string[] args) {
        Option<FileInfo> tilePathOption = new("-t", "--tilePath") {
            Description = "Path to png file used to generate the model"
        };

        Option<FileInfo> initPathOption = new("-p", "--initPath") {
            Description = "Set an explict path to Init.txt file used to set tile parameters. Otherwise Init.txt is expected to be in the same directory as the image"
        };

        RootCommand rootCommand = new("Tool for converting Rain World tile graphics to Minecraft JSON models");
        rootCommand.Options.Add(tilePathOption);

        rootCommand.SetAction(parseResult => {
            FileInfo parsedFile = parseResult.GetValue(tilePathOption);
            if (!parsedFile.Exists) {
                Console.WriteLine($"Error: Invalid tile image path '{parsedFile}'");
                return;
            }

            string tileName = parsedFile.Name;
            string tileDirectory = parsedFile.Directory.FullName;
            string initPath = Path.Join(tileDirectory, "Init.txt");
            TileParameters? tileParametersNullable = GetTileParameters(tileName, initPath);
            
            
        });

        if (args.Length == 0) {
            args = new[] {"--help"};
        }

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }
}