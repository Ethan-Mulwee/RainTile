
using System.Text.RegularExpressions;
using static RainTileLib.VoxelFunctions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RainTileLib;

    public struct PixelCoordinates {
        public int X, Y;
    }

public static class TileConversion {

    public struct ConversionSettings {
        public MergingType mergeType;
        public bool shell;
    }

    public static VoxelGrid ConvertTileToVoxel(TileData tile, ConversionSettings settings) {

        VoxelGrid grid = CreateVoxelGrid(tile);

        if (settings.shell) RemoveInteriorVoxels(grid);

        MergeOptimize(grid, settings.mergeType);

        SetVoxelVisibility(grid);

        return grid;
    }

    public static string ConvertVoxelToJson(TileData tile, string texturePath, VoxelGrid grid) {
        List<Voxel> voxelList = new List<Voxel>();
        List<List<int>> layersIndices = new List<List<int>>();
        int voxelIndex = 0;
        for (int z = 0; z < grid.size; z++) {
            List<int> layerIndices = new List<int>();
            for (int y = 0; y < grid.size; y++) {
                for (int x = 0; x < grid.size; x++) {
                    if (grid.voxels[x, y, z] is Voxel voxel) {
                        voxelList.Add(voxel);
                        layerIndices.Add(voxelIndex);
                        voxelIndex++;
                    }
                }
            }
            if (layerIndices.Count > 0)
                layersIndices.Add(layerIndices);
        }

        List<MinecraftElement> elementList = new List<MinecraftElement>();

        // Convert voxel grid to minecraft elements
        for (int i = 0; i < voxelList.Count; i++) {
            Voxel voxel = voxelList[i];
            // top bottom UV
            float UVScaleFactorU = (16.0f / tile.image.Width);
            float UVScaleFactorV = (16.0f / tile.image.Height);

            int U1i = voxel.span.from.X;
            int U2i = voxel.span.to.X;
            int V1i = ((tile.numLayers - voxel.span.from.Z) * tile.tileY) + voxel.span.from.Y + 1;
            int V2i = ((tile.numLayers - voxel.span.from.Z) * tile.tileY) + voxel.span.to.Y + 1;

            float U1 = U1i * UVScaleFactorU;
            float U2 = U2i * UVScaleFactorU;
            float V1 = V1i * UVScaleFactorV;
            float V2 = V2i * UVScaleFactorV;

            float ModelScaleFactor = 16.0f / 20.0f;

            MinecraftFace? northFace = null;
            if (voxel.visibility.north) {
                northFace = new MinecraftFace {
                    uv = new float[] { U2, V1, U1, V1 + (1 * UVScaleFactorV) },
                    texture = "#0"
                };
            }
            MinecraftFace? eastFace = null;
            if (voxel.visibility.east) {
                eastFace = new MinecraftFace {
                    uv = new float[] { U2, V1, U2 - (1 * UVScaleFactorU), V2 },
                    rotation = 90,
                    texture = "#0"
                };
            }
            MinecraftFace? southFace = null;
            if (voxel.visibility.south) {
                southFace = new MinecraftFace {
                    uv = new float[] { U1, V2 - (1 * UVScaleFactorV), U2, V2 },
                    texture = "#0"
                };
            }
            MinecraftFace? westFace = null;
            if (voxel.visibility.west) {
                westFace = new MinecraftFace {
                    uv = new float[] { U1 + (1 * UVScaleFactorU), V2, U1, V1 },
                    rotation = 90,
                    texture = "#0"
                };
            }
            MinecraftFace? upFace = null;
            if (voxel.visibility.up) {
                upFace = new MinecraftFace {
                    uv = new float[] { U1, V1, U2, V2 },
                    texture = "#0"
                };
            }
            MinecraftFace? downFace = null;
            if (voxel.visibility.down) {
                downFace = new MinecraftFace {
                    uv = new float[] { U1, V2, U2, V1 },
                    texture = "#0"
                };
            }

            elementList.Add(new MinecraftElement {
                from = new float[] { (voxel.span.from.X - 16) * ModelScaleFactor, (voxel.span.from.Z) * ModelScaleFactor, (voxel.span.from.Y - 16) * ModelScaleFactor },
                to = new float[] { (voxel.span.to.X - 16) * ModelScaleFactor, (voxel.span.to.Z) * ModelScaleFactor, (voxel.span.to.Y - 16) * ModelScaleFactor },
                rotation = new MinecraftRotation {
                    angle = 0.0f,
                    axis = "y",
                    origin = new int[] { 0, 0, 0 }
                },
                color = 7,
                faces = new MinecraftFaces {
                    north = northFace,
                    east = eastFace,
                    south = southFace,
                    west = westFace,
                    up = upFace,
                    down = downFace,
                }
            });
        }

        MinecraftGroup[] groups = new MinecraftGroup[layersIndices.Count];
        for (int i = 0; i < groups.Length; i++) {
            groups[i] = new MinecraftGroup {
                name = $"layer{i}",
                origin = new int[] { 0, 0, 0 },
                scope = 0,
                color = 0,
                children = layersIndices[i].ToArray()
            };
        }

        MinecraftJSON model = new MinecraftJSON {
            format_version = MinecraftExportConstants.FORMAT_VERSION,
            credit = MinecraftExportConstants.CREDIT,
            texture_size = new int[] { tile.image.Width, tile.image.Height },
            textures = new MinecraftTextures
            {
                texture0Path = texturePath,
                particlePath = texturePath
            },
            elements = elementList.ToArray(),
            groups = groups
        };

        JsonSerializerOptions options = new JsonSerializerOptions {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(model, options);
    }

    public static TileData CreateTileData(Png image, TileParameters parameters) {
        const int CELL_SIZE = 20;
        const int PREVIEW_SIZE = 16;

        int tileX = (parameters.SZx + parameters.BfTiles * 2) * CELL_SIZE;
        int tileY = (parameters.SZy + parameters.BfTiles * 2) * CELL_SIZE;
        int previewX = parameters.SZx * PREVIEW_SIZE;
        int previewY = parameters.SZy * PREVIEW_SIZE;

        int boundX = tileX;
        int boundY = (tileY * parameters.RepeatL.Length) + previewY + 1;

        return new TileData {
            image = image,
            parameters = parameters,
            tileX = tileX,
            tileY = tileY,
            previewX = previewX,
            previewY = previewY,
            boundX = boundX,
            boundY = boundY,
            numLayers = parameters.RepeatL.Length,
        };
    }



    public static PixelCoordinates CalculateTopLeftLayerCoordiantes(TileData tileInfo, int layerIndex) {
        
        return new PixelCoordinates {
            X = 0,
            Y = (layerIndex * tileInfo.tileY)+1
        };
    }

    public static TileParameters? TryDetectParameters(Png image) {

        int sy = -1;
        int numLayers = -1;
        int bfTiles = -1;
        // guesses different values for bfTiles numLayers and SZy until it finds a set that matches the image height, prefers solutions that use less bfTiles
        for (int bfTilesGuess = 0; bfTilesGuess < 3; bfTilesGuess++) {
            for (int syGuess = 1; syGuess < 10; syGuess++) {
                for (int numLayersGuess = 1; numLayersGuess < 10; numLayersGuess++) {
                    int bound = ((syGuess+(bfTilesGuess*2))*numLayersGuess*20)+(syGuess*16);
                    if (bound == image.Height-1) {
                        sy = syGuess;
                        numLayers = numLayersGuess;
                        bfTiles = bfTilesGuess;
                        goto BreakLoop;
                    }
                }
            }
        }
        BreakLoop:
        if (sy == -1) {
            return null;
        }

        int sx = (image.Width / 20) - bfTiles*2;

        int[] repeatL = new int[numLayers];
        for (int i = 0; i < numLayers; i++) {
            repeatL[i] = 1;
        }
        repeatL[0] = 11-numLayers;

        return new TileParameters {
                SZx = sx,
                SZy = sy,
                BfTiles = bfTiles,
                RepeatL = repeatL
        };
    }

    public static TileParameters? GetTileParameters(string tileName, string initPath) {

        Regex nameRegex = new Regex("#nm\\s*:\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
        Regex szRegex = new Regex(@"#sz\s*:\s*point\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)", RegexOptions.IgnoreCase);
        Regex bfRegex = new Regex(@"#bfTiles\s*:\s*(\d+)", RegexOptions.IgnoreCase);
        Regex repeatLRegex = new Regex(@"#repeatL\s*:\s*\[\s*([^\]]*?)\s*\]", RegexOptions.IgnoreCase);
        
        int[] ParseIntList(string text) {
            List<int> numbers = new List<int>();
            foreach (string part in text.Split(",")) {
                string trimmedPart = part.Trim();
                if (trimmedPart == "") {
                    continue;
                }
                numbers.Add(int.Parse(trimmedPart));
            }
            return numbers.ToArray();
        } 

        foreach (string rawLine in File.ReadLines(initPath)) {
            string line = rawLine.Trim();
            if (line.Length == 0) {
                continue;
            }
            if (!(line.StartsWith('[') && line.EndsWith(']'))) {
                continue;
            }

            Match nameMatch = nameRegex.Match(line);
            if (!nameMatch.Success) {
                continue;
            }

            string name = nameMatch.Groups[1].Value;

            if (!string.Equals(name, tileName, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            Match szMatch = szRegex.Match(line);
            Match bfMatch = bfRegex.Match(line);
            Match repeatLMatch = repeatLRegex.Match(line);

            if (!szMatch.Success) {
                throw new ArgumentException("#sz:point(x,y) parameter could not be found");
            }
            if (!bfMatch.Success) {
                throw new ArgumentException("#bfTiles parameter could not be found");
            }
            if (!repeatLMatch.Success) {
                throw new ArgumentException("#repeatL parameter could not be found");
            }

            int sx = int.Parse(szMatch.Groups[1].Value);
            int sy = int.Parse(szMatch.Groups[2].Value);
            int bfTiles = int.Parse(bfMatch.Groups[1].Value);
            int[] repeatL = ParseIntList(repeatLMatch.Groups[1].Value);

            // Console.WriteLine($"SZ: ({sx}, {sy})");
            return new TileParameters{
                SZx = sx,
                SZy = sy,
                BfTiles = bfTiles,
                RepeatL = repeatL
            };
        }

        return null;
    }

    public static void LogParameters(TileParameters p) {
        Console.WriteLine($"Parameters: ( SZ:({p.SZx}, {p.SZy}), bfTiles: {p.BfTiles}, repeatL: [{string.Join(",", p.RepeatL)}] )");
    }
}

public struct TileParameters {
    public int SZx, SZy;
    public int BfTiles;
    public int[] RepeatL; // should add up to 10
};

public struct TileData {
    public Png image;
    public TileParameters parameters;
    public int tileX;
    public int tileY;
    public int previewX;
    public int previewY;

    public int boundX;
    public int boundY;
    public int numLayers;
}