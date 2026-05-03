using System.CommandLine;
using RainTileLib;
using static RainTileLib.TileConversion;


class Program
{
    static int Main(string[] args) {
        Option<FileInfo> tilePathOption = new("-t", "--tilePath") {
            Description = "Path to png file used to generate the model"
        };

        Option<FileInfo> initPathOption = new("-i", "--initPath") {
            Description = "Set an explict path to Init.txt file used to set tile parameters. Otherwise Init.txt is expected to be in the same directory as the image"
        };

        RootCommand rootCommand = new("Tool for converting Rain World tile graphics to Minecraft JSON models");
        rootCommand.Options.Add(tilePathOption);
        rootCommand.Options.Add(initPathOption);

        rootCommand.SetAction(parseResult => {
            FileInfo? tilePathInfo = parseResult.GetValue(tilePathOption);
            FileInfo? initPathInfoNullable = parseResult.GetValue(initPathOption);

            if (!tilePathInfo.Exists) {
                Console.WriteLine($"Error: Invalid tile image path '{tilePathInfo}'");
                return;
            }

            string tileName = Path.GetFileNameWithoutExtension(tilePathInfo.FullName);
            string tileDirectory = tilePathInfo.Directory.FullName;
            string initPath;

            if (initPathInfoNullable is FileInfo initPathInfo) {
                initPath = initPathInfo.FullName;
            } else {
                initPath = Path.Join(tileDirectory, "Init.txt");
            }

            if (!Path.Exists(initPath)) {
                Console.WriteLine($"Error: Could not find Init.txt at ${initPath}");
                Console.WriteLine("TODO: add option to detect params from image");
                return;
            }
            TileParameters? tileParametersNullable = GetTileParameters(tileName, initPath);
            if (tileParametersNullable == null) {
                Console.WriteLine($"Error: failed to find entry for '${tileName}' in Init.txt");
                Console.WriteLine("TODO: add option to detect params from image");
                return;
            }
            Console.WriteLine($"Tile '{tileName}', parameters detected from ${initPath}");
            LogParameters(tileParametersNullable.Value);


            
        });

        if (args.Length == 0) {
            args = new[] {"--help"};
        }

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }
}