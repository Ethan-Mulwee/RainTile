using System.CommandLine;
using BigGustave;
using RainTileLib;
using static RainTileLib.TileConversion;
using static RainTileLib.VoxelFunctions;


class Program
{
    static int Main(string[] args) {
        Argument<FileInfo> tilePathArgument = new("tilePath") {
            Description = "Path to png file used to generate the model"
        };

        Option<FileInfo> initPathOption = new("-i", "--initPath", "--init") {
            Description = "Set an explict path to Init.txt file used to set tile parameters. Otherwise Init.txt is expected to be in the same directory as the image"
        };

        Option<FileInfo> outputPathOption = new("-o", "--outputPath", "--output") {
            Description = "Set an explict path to Init.txt file used to set tile parameters. Otherwise Init.txt is expected to be in the same directory as the image"
        };

        Option<bool> yesOption = new("-y", "--yes") {
            Description = "Default to yes when coming to an option"
        };

        Option<bool> shellOption = new("--shell") {
            Description = "Remove all interior voxels from generated model"
        };

        Option<bool> mergeVerticalOption = new("--merge-vertical") {
            Description = "Remove all interior voxels from generated model"
        };

        RootCommand rootCommand = new("Tool for converting Rain World tile graphics to Minecraft JSON models");
        rootCommand.Add(tilePathArgument);
        rootCommand.Options.Add(initPathOption);
        rootCommand.Options.Add(yesOption);
        rootCommand.Options.Add(shellOption);
        rootCommand.Options.Add(mergeVerticalOption);

        rootCommand.SetAction(parseResult => {
            FileInfo? tilePathInfo = parseResult.GetValue(tilePathArgument);
            FileInfo? initPathInfoNullable = parseResult.GetValue(initPathOption);
            bool yesOptionValue = parseResult.GetValue(yesOption);

            if (!tilePathInfo.Exists) {
                Console.WriteLine($"Error: Invalid tile image path '{tilePathInfo}'");
                return;
            }

            string tilePath = tilePathInfo.FullName;
            string tileName = Path.GetFileNameWithoutExtension(tilePath);
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
                Console.WriteLine($"Error: failed to find entry for '{tileName}' in Init.txt");
                Console.WriteLine("TODO: add option to detect parameters from image");
                return;
            }
            TileParameters tileParameters = tileParametersNullable.Value;
            Console.WriteLine($"Tile '{tileName}', parameters detected from ${initPath}");
            LogParameters(tileParametersNullable.Value);

            Stream tileStream = new FileStream(tilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Png tilePng = Png.Open(tileStream);
            tileStream.Close();

            Console.WriteLine($"{tilePng.Width}, {tilePng.Height}");

            TileData tile = CreateTileData(tilePng, tileParameters);
            
            ConversionSettings settings = new ConversionSettings{
                mergeType = MergingType.XY,
                shell = false
            };

            VoxelGrid tileGrid = ConvertTileToVoxel(tile, settings);
            string tileJson = ConvertVoxelToJson(tile, $"{tileName}", tileGrid);

            string outputTilePath = $"{tileName}.json";
            if (Path.Exists(outputTilePath) && !yesOptionValue) {
                Console.Write($"'{outputTilePath}' already exists would you like to overwrite it? [y/N]: ");
                string response = Console.ReadLine();
                switch (response) {
                    case "y":
                    case "Y":
                        break;
                    default:
                        Console.WriteLine("Aborting");
                        return;
                }
            }

            FileStream outputTileStream = File.Open(outputTilePath, FileMode.Create);
            StreamWriter outputTileWriter = new StreamWriter(outputTileStream);
            outputTileWriter.Write(tileJson);

            outputTileWriter.Close();
            outputTileStream.Close();

            Console.WriteLine($"Success wrote model to {outputTilePath}");

        });

        if (args.Length == 0) {
            args = new[] {"--help"};
        }

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }
}