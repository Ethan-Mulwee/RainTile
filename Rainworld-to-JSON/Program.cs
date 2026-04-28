
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

VoxelGrid voxelGrid = new VoxelGrid{
    voxels = new Voxel?[48,48,48],
    size = 48
};

// create voxel grid
for (int i = 0; i < info.numLayers; i++) {
    for (int x = 0; x < info.tileX; x++) {
        for (int y = 0; y < info.tileY; y++) {
            PixelCoordinates layerOffset = TileReader.CalculateTopLeftLayerCoordiantes(info, i);
            PixelCoordinates imageCoords = new PixelCoordinates{X=layerOffset.X + x, Y=layerOffset.Y + y};
            PixelCoordinates modelCoords = new PixelCoordinates{X = x - 16, Y = y - 16};
            Pixel pixel = image.GetPixel(imageCoords.X, imageCoords.Y);
            if ((pixel.R * pixel.G * pixel.B) == 0) {
                voxelGrid.voxels[x,y,(info.numLayers-i)] = new Voxel {
                    span = new VoxelSpan {
                        from = new VoxelCoordinates {
                            X = x,
                            Y = y,
                            Z = (info.numLayers-i)
                        },
                        to = new VoxelCoordinates {
                            X = x+1,
                            Y = y+1,
                            Z = (info.numLayers-i)+1
                        },
                    }
                };
                // elementList.Add(new MinecraftElement {
                //     from = new int[]{modelCoords.X, (info.numLayers-i)+0, modelCoords.Y},
                //     to = new int[]{modelCoords.X+1, (info.numLayers-i)+1, modelCoords.Y+1},
                //     rotation = new MinecraftRotation {
                //         angle = 0.0f,
                //         axis = "y",
                //         origin = new int[]{0,0,0}
                //     },
                //     color = 7,
                //     faces = new MinecraftFaces {
                //         north = new MinecraftFace {
                //             uv = new int[]{0, 0, 1, 1},
                //             texture = "#missing"
                //         },
                //         east = new MinecraftFace {
                //             uv = new int[]{0, 0, 1, 1},
                //             texture = "#missing"
                //         },
                //         south = new MinecraftFace {
                //             uv = new int[]{0, 0, 1, 1},
                //             texture = "#missing"
                //         },
                //         west = new MinecraftFace {
                //             uv = new int[]{0, 0, 1, 1},
                //             texture = "#missing"
                //         },
                //         up = new MinecraftFace {
                //             uv = new int[]{0, 0, 1, 1},
                //             texture = "#missing"
                //         },
                //         down = new MinecraftFace {
                //             uv = new int[]{0, 0, 1, 1},
                //             texture = "#missing"
                //         },
                //     }
                // });
            }
        }
    }
}

List<Voxel> optimziedVoxels = new List<Voxel>();

// optimize elements
for (int z = 0; z < voxelGrid.size; z++) {
    for (int y = 0; y < voxelGrid.size; y++) {
        for (int x = 0; x < voxelGrid.size; x++) {
            Voxel? voxelN = voxelGrid.voxels[x, y, z];
            if (x+1 >= voxelGrid.size) {
                if (voxelN != null) {
                    optimziedVoxels.Add(voxelN.Value);
                }
                continue;
            }
            
            int i = 1;
            if (voxelN != null) {
                Voxel voxel = voxelN.Value;
                while (x+1 < voxelGrid.size && voxelGrid.voxels[x+i, y, z] != null) {

                    Voxel neighborVoxel = voxelGrid.voxels[x+i, y, z].Value;
                    voxel.span = new VoxelSpan {
                        from = voxel.span.from,
                        to = neighborVoxel.span.to,
                    };
                    voxelGrid.voxels[x+i, y, z] = null;
                    
                    i++;
                }
                optimziedVoxels.Add(voxel);
            }
        }
    }
}
// for (int z = 0; z < voxelGrid.size; z++) {
//     for (int y = 0; y < voxelGrid.size; y++) {
//         for (int x = 0; x < voxelGrid.size; x++) {
//             if (voxelGrid.voxels[x,y,z] != null)
//                 optimziedVoxels.Add(voxelGrid.voxels[x,y,z].Value);
//         }
//     }
// }

// Convert voxel grid to minecraft elements
for (int i = 0; i < optimziedVoxels.Count; i++) {
    Voxel voxel = optimziedVoxels[i];
    elementList.Add(new MinecraftElement {
        from = new int[]{voxel.span.from.X-16, voxel.span.from.Z-16, voxel.span.from.Y-16},
        to = new int[]{voxel.span.to.X-16, voxel.span.to.Z-16, voxel.span.to.Y-16},
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