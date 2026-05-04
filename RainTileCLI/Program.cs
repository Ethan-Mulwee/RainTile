using System.CommandLine;
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
            Description = "Set a output path, if you provide a path that defines a file then the file will be named as you provided. Otherwise the file will be outputed to the directory"
        };

        Option<bool> yesOption = new("-y", "--yes") {
            Description = "Default to yes when coming to an option for example whether you want to overwrite an existing file"
        };

        Option<bool> noOption = new("-n", "--no") {
            Description = "Default to no when coming to an option"
        };

        Option<bool> shellOption = new("--shell") {
            Description = "Remove all interior voxels from generated model"
        };

        Option<bool> mergeVerticalOption = new("--merge-vertical") {
            Description = "Allowing merging cubes between layers to maximize model optimization, this makes it a little harder to edit the generated model so it is disabled by default"
        };

        RootCommand rootCommand = new("Tool for converting Rain World tile graphics to Minecraft JSON models");
        rootCommand.Add(tilePathArgument);
        rootCommand.Options.Add(initPathOption);
        rootCommand.Options.Add(outputPathOption);
        rootCommand.Options.Add(yesOption);
        rootCommand.Options.Add(noOption);
        rootCommand.Options.Add(shellOption); 
        rootCommand.Options.Add(mergeVerticalOption);

        rootCommand.SetAction(parseResult => {
            FileInfo? tilePathInfo = parseResult.GetValue(tilePathArgument);
            FileInfo? initPathInfoNullable = parseResult.GetValue(initPathOption);
            FileInfo? outputPathInfoNullable = parseResult.GetValue(outputPathOption);
            bool yesOptionValue = parseResult.GetValue(yesOption);
            bool noOptionValue = parseResult.GetValue(noOption);
            bool shellOptionValue = parseResult.GetValue(shellOption);
            bool mergeVerticalOptionValue = parseResult.GetValue(mergeVerticalOption);


            if (tilePathInfo == null || !tilePathInfo.Exists) {
                Console.WriteLine($"Error: Invalid tile image path '{tilePathInfo}'");
                return;
            }

            string tilePath = tilePathInfo.FullName;
            string tileName = Path.GetFileNameWithoutExtension(tilePath);
            string mctileName = tileName.Replace(" ", "_").ToLower();
            string tileDirectory = tilePathInfo.Directory.FullName;
            string initPath;

            if (initPathInfoNullable is FileInfo initPathInfo) {
                initPath = initPathInfo.FullName;
            } else {
                initPath = Path.Join(tileDirectory, "Init.txt");
            }

            Stream tileStream = new FileStream(tilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Png tilePng = Png.Open(tileStream);
            tileStream.Close();

            TileParameters? tileParametersNullable;
            if (!Path.Exists(initPath)) {
                Console.WriteLine($"Error: Could not find Init.txt at ${initPath}");
                TileParameters? parametersNullable = TryDetectParameters(tilePng);
                if (parametersNullable is TileParameters parameters) {
                    tileParametersNullable = parametersNullable;
                } else {
                    Console.WriteLine("Failed to solve for parameters aborting");
                    return;
                }
                
            } else {
                tileParametersNullable = GetTileParameters(tileName, initPath);
                if (tileParametersNullable == null) {
                    Console.WriteLine($"Error: failed to find entry for '{tileName}' in Init.txt");
                    TileParameters? parametersNullable = TryDetectParameters(tilePng);
                    if (parametersNullable is TileParameters parameters) {
                        Console.WriteLine("Attempting to solve for parameters by image dimensions, NOTE: this only works if the image is correctly sized and contains only a one set of sprites and if there is only one solution");
                        tileParametersNullable = parametersNullable;
                    } else {
                        Console.WriteLine("Failed to solve for parameters aborting");
                        return;
                    }
                }
            }

            TileParameters tileParameters = tileParametersNullable.Value;
            if (Path.Exists(initPath)) {
                Console.WriteLine($"Tile '{tileName}', parameters detected from ${initPath}");
            } else {
                Console.WriteLine($"Tile '{tileName}', Warning: parameters auto detected from image dimensions");
            }
            LogParameters(tileParametersNullable.Value);



            TileData tile = CreateTileData(tilePng, tileParameters);
            
            ConversionSettings settings = new ConversionSettings{
                mergeType = MergingType.XY,
                shell = false
            };

            if (mergeVerticalOptionValue)
                settings.mergeType = MergingType.XYZ;
            if (shellOptionValue)
                settings.shell = true;

            VoxelGrid tileGrid = ConvertTileToVoxel(tile, settings);
            string tileJson = ConvertVoxelToJson(tile, $"{mctileName}", tileGrid);

            string outputTilePath = $"{mctileName}.json";
            if (outputPathInfoNullable != null) {
                if (outputPathInfoNullable.Directory.Exists) {
                    if (Path.GetExtension(outputPathInfoNullable.FullName) == "") {
                        // Directory case
                        outputTilePath = Path.Join(outputPathInfoNullable.FullName, outputTilePath);
                    } else {
                        // File case
                        outputTilePath = outputPathInfoNullable.FullName;
                    }
                } else {
                    Console.WriteLine("Error: invalid output path aborting");
                    return;
                }
            }



            if (Path.Exists(outputTilePath) && !yesOptionValue) {
                if (noOptionValue) {
                        Console.WriteLine("File already exists aborting");
                        return;
                }
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