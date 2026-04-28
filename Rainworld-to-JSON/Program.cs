
using BigGustave;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;


if (args.Length > 0) {
    Console.WriteLine($"First argument: {args[0]}");
}

String testPath = "/home/ethan/Desktop/rained_v2.5.0_linux-x64/Data/Graphics/Less Giant Screw.png";
Stream imageStreamSource = new FileStream(testPath, FileMode.Open, FileAccess.Read, FileShare.Read);
Png image = Png.Open(imageStreamSource);

Console.WriteLine($"{image.Width}x{image.Height}");

List<MinecraftElement> elementList = new List<MinecraftElement>();

TileParameters? parametersNullable = TileReader.GetTileParameters(testPath);
if (parametersNullable == null)
    return; 
TileParameters parameters = parametersNullable.Value;
TileInfo info = TileReader.CalculateTileInfo(parameters);
Console.WriteLine($"sz({parameters.SZx}, {parameters.SZy}), bfTiles: {parameters.BfTiles}, repeatL: {string.Join(",", parameters.RepeatL)}");
Console.WriteLine($"Correct bounds: {info.boundX}, {info.boundY}");
Console.WriteLine($"Number of layers defined: {info.numLayers}");
Console.WriteLine($"Layer Size: ({info.tileX}, {info.tileY})");

for (int i = 0; i < info.numLayers; i++) {
    for (int x = 0; x < info.tileX; x++) {
        for (int y = 0; y < info.tileY; y++) {
            PixelCoordinates layerOffset = TileReader.CalculateTopLeftLayerCoordiantes(info, i);
            PixelCoordinates imageCoords = new PixelCoordinates{X=layerOffset.X + x, Y=layerOffset.Y + y};
            PixelCoordinates modelCoords = new PixelCoordinates{X = x - 16, Y = y - 16};
            Pixel pixel = image.GetPixel(imageCoords.X, imageCoords.Y);
            if ((pixel.R * pixel.G * pixel.B) == 0) {
                elementList.Add(new MinecraftElement {
                    from = new int[]{modelCoords.X, (info.numLayers-i)+0, modelCoords.Y},
                    to = new int[]{modelCoords.X+1, (info.numLayers-i)+1, modelCoords.Y+1},
                    rotation = new MinecraftRotation {
                        angle = 0.0f,
                        axis = "y",
                        origin = new int[]{0,0,0}
                    },
                    color = 7,
                    faces = new MinecraftFaces {
                        north = new MinecraftFace {
                            uv = new int[]{0, 0, 1, 1},
                            texture = "#missing"
                        },
                        east = new MinecraftFace {
                            uv = new int[]{0, 0, 1, 1},
                            texture = "#missing"
                        },
                        south = new MinecraftFace {
                            uv = new int[]{0, 0, 1, 1},
                            texture = "#missing"
                        },
                        west = new MinecraftFace {
                            uv = new int[]{0, 0, 1, 1},
                            texture = "#missing"
                        },
                        up = new MinecraftFace {
                            uv = new int[]{0, 0, 1, 1},
                            texture = "#missing"
                        },
                        down = new MinecraftFace {
                            uv = new int[]{0, 0, 1, 1},
                            texture = "#missing"
                        },
                    }
                });
            }
        }
    }
}

MinecraftJSON model = new MinecraftJSON {
    format_version = MinecraftExportConstants.FORMAT_VERSION,
    credit = MinecraftExportConstants.CREDIT,
    elements = elementList.ToArray()
};

JsonSerializerOptions options = new JsonSerializerOptions {
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var minecraftModelStream = File.Open("Model.json", FileMode.Create);
var minecraftModelWriter = new StreamWriter(minecraftModelStream);
string json = JsonSerializer.Serialize(model, options);

minecraftModelWriter.Write(json);
minecraftModelWriter.Close();
minecraftModelStream.Close();