
using BigGustave;
using System.Text.Json;
using System.Text.Json.Serialization;

String imagePath = "";
if (args.Length > 0)
{
    imagePath = args[0];
}
else
{
    Console.WriteLine("Error: Please provide a file path");
    return;
}

Stream imageStreamSource = null;
try
{
    imageStreamSource = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
}
catch (Exception e)
{
    Console.WriteLine($"Error: Invalid file path: {imagePath}");
    Console.WriteLine(e);
    return;
}

Png image = Png.Open(imageStreamSource);

Console.WriteLine($"{image.Width}x{image.Height}");


string fileName = Path.GetFileNameWithoutExtension(imagePath);
string minecraftSafeName = fileName.Replace(" ", "_").ToLower();

TileParameters? parametersNullable = TileReader.GetTileParameters(imagePath);
if (parametersNullable == null)
    return;
TileParameters parameters = parametersNullable.Value;
TileInfo info = TileReader.CalculateTileInfo(parameters);
Console.WriteLine($"sz({parameters.SZx}, {parameters.SZy}), bfTiles: {parameters.BfTiles}, repeatL: {string.Join(",", parameters.RepeatL)}");
Console.WriteLine($"Correct bounds: {info.boundX}, {info.boundY}");
Console.WriteLine($"Number of layers defined: {info.numLayers}");
Console.WriteLine($"Layer Size: ({info.tileX}, {info.tileY})");

VoxelGrid baseGrid = CreateVoxelGrid(image, info);

VoxelFunctions.MergeX(baseGrid);

VoxelFunctions.MergeY(baseGrid);

// VoxelFunctions.MergeZ(baseGrid);

List<Voxel> optimziedVoxels = new List<Voxel>();
List<List<int>> layersIndicies = new List<List<int>>();
int index = 0;
for (int z = 0; z < baseGrid.size; z++)
{
    List<int> layerIndices = new List<int>();
    for (int y = 0; y < baseGrid.size; y++)
    {
        for (int x = 0; x < baseGrid.size; x++)
        {
            Voxel? voxelN = baseGrid.voxels[x, y, z];
            if (voxelN != null)
            {
                optimziedVoxels.Add(voxelN.Value);
                layerIndices.Add(index);
                index++;
            }
        }
    }
    if (layerIndices.Count > 0)
        layersIndicies.Add(layerIndices);
}

List<MinecraftElement> elementList = new List<MinecraftElement>();

// Convert voxel grid to minecraft elements
for (int i = 0; i < optimziedVoxels.Count; i++)
{
    Voxel voxel = optimziedVoxels[i];
    // top bottom UV
    float UVScaleFactorU = (16.0f / image.Width);
    float UVScaleFactorV = (16.0f / image.Height);

    int U1i = voxel.span.from.X;
    int U2i = voxel.span.to.X;
    int V1i = ((info.numLayers - voxel.span.from.Z) * info.tileY) + voxel.span.from.Y + 1;
    int V2i = ((info.numLayers - voxel.span.from.Z) * info.tileY) + voxel.span.to.Y + 1;

    float U1 = U1i * UVScaleFactorU;
    float U2 = U2i * UVScaleFactorU;
    float V1 = V1i * UVScaleFactorV;
    float V2 = V2i * UVScaleFactorV;



    float ModelScaleFactor = 16.0f / 20.0f;

    elementList.Add(new MinecraftElement
    {
        from = new float[] { (voxel.span.from.X - 16) * ModelScaleFactor, (voxel.span.from.Z) * ModelScaleFactor, (voxel.span.from.Y - 16) * ModelScaleFactor },
        to = new float[] { (voxel.span.to.X - 16) * ModelScaleFactor, (voxel.span.to.Z) * ModelScaleFactor, (voxel.span.to.Y - 16) * ModelScaleFactor },
        rotation = new MinecraftRotation
        {
            angle = 0.0f,
            axis = "y",
            origin = new int[] { 0, 0, 0 }
        },
        color = 7,
        faces = new MinecraftFaces
        {
            north = new MinecraftFace
            {
                uv = new float[] { U2, V1, U1, V1 + (1 * UVScaleFactorV) },
                texture = "#0"
            },
            east = new MinecraftFace
            {
                uv = new float[] { U2 - (1 * UVScaleFactorU), V2, U2, V1 },
                texture = "#0"
            },
            south = new MinecraftFace
            {
                uv = new float[] { U1, V2 - (1 * UVScaleFactorV), U2, V2 },
                texture = "#0"
            },
            west = new MinecraftFace
            {
                uv = new float[] { U1, V2, U1 + (1 * UVScaleFactorU), V1 },
                texture = "#0"
            },
            up = new MinecraftFace
            {
                uv = new float[] { U1, V1, U2, V2 },
                texture = "#0"
            },
            down = new MinecraftFace
            {
                uv = new float[] { U1, V1, U2, V2 },
                texture = "#0"
            },
        }
    });
}

MinecraftGroup[] groups = new MinecraftGroup[layersIndicies.Count];
for (int i = 0; i < groups.Length; i++)
{
    groups[i] = new MinecraftGroup
    {
        name = $"layer{i}",
        origin = new int[] { 0, 0, 0 },
        scope = 0,
        color = 0,
        children = layersIndicies[i].ToArray()
    };
}


MinecraftJSON model = new MinecraftJSON
{
    format_version = MinecraftExportConstants.FORMAT_VERSION,
    credit = MinecraftExportConstants.CREDIT,
    texture_size = new int[] { image.Width, image.Height },
    textures = new MinecraftTextures
    {
        texture0Path = minecraftSafeName,
        particlePath = minecraftSafeName
    },
    elements = elementList.ToArray(),
    groups = groups
};

Console.WriteLine(minecraftSafeName);

JsonSerializerOptions options = new JsonSerializerOptions
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var minecraftModelStream = File.Open($"{minecraftSafeName}.json", FileMode.Create);
var minecraftModelWriter = new StreamWriter(minecraftModelStream);
string json = JsonSerializer.Serialize(model, options);

minecraftModelWriter.Write(json);
minecraftModelWriter.Close();
minecraftModelStream.Close();

static VoxelGrid CreateVoxelGrid(Png image, TileInfo info)
{
    VoxelGrid voxelGrid = new VoxelGrid
    {
        voxels = new Voxel?[256, 256, 256],
        size = 256
    };

    // create voxel grid
    for (int i = 0; i < info.numLayers; i++)
    {
        for (int x = 0; x < info.tileX; x++)
        {
            for (int y = 0; y < info.tileY; y++)
            {
                PixelCoordinates layerOffset = TileReader.CalculateTopLeftLayerCoordiantes(info, i);
                PixelCoordinates imageCoords = new PixelCoordinates { X = layerOffset.X + x, Y = layerOffset.Y + y };
                PixelCoordinates modelCoords = new PixelCoordinates { X = x - 16, Y = y - 16 };
                Pixel pixel = image.GetPixel(imageCoords.X, imageCoords.Y);
                if ((pixel.R * pixel.G * pixel.B) == 0)
                {
                    voxelGrid.voxels[x, y, (info.numLayers - i)] = new Voxel
                    {
                        span = new VoxelSpan
                        {
                            from = new Vector3Int
                            {
                                X = x,
                                Y = y,
                                Z = (info.numLayers - i)
                            },
                            to = new Vector3Int
                            {
                                X = x + 1,
                                Y = y + 1,
                                Z = (info.numLayers - i) + 1
                            },
                        }
                    };
                }
            }
        }
    }

    return voxelGrid;
}

// remove interior voxels NOTE: doing this before mergin will raise cube count this raises cube count
// for (int z = 0; z < voxelGrid.size; z++) {
//     for (int y = 0; y < voxelGrid.size; y++) {
//         for (int x = 0; x < voxelGrid.size; x++) {
//             Voxel? voxelN = voxelGrid.voxels[x,y,z];
//             bool up = false;
//             bool down = false;
//             bool north = false;
//             bool south = false;
//             bool east = false;
//             bool west = false;
//             if (voxelN != null) {
//                 if (x+1 < voxelGrid.size && voxelGrid.voxels[x+1, y, z] != null) east = true;
//                 if (x-1 > 0 && voxelGrid.voxels[x-1, y, z] != null) west = true;
//                 if (y+1 < voxelGrid.size && voxelGrid.voxels[x, y+1, z] != null) north = true;
//                 if (y-1 > 0 && voxelGrid.voxels[x, y-1, z] != null) south = true;
//                 if (z+1 < voxelGrid.size && voxelGrid.voxels[x, y, z+1] != null) up = true;
//                 if (z-1 > 0 && voxelGrid.voxels[x, y, z-1] != null) down = true;
//             }

//             if (up && down && north && south && east && west) {
//                 voxelGrid_NoInterior.voxels[x,y,z] = null;
//             } else {
//                 voxelGrid_NoInterior.voxels[x,y,z] = voxelN;
//             }
//         }
//     }
// }

// File.Copy(testPath, minecraftSafeName+".png");