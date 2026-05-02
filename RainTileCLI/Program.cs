

using RainTileLib;
using BigGustave;
using System.Text.Json;
using System.Text.Json.Serialization;
using static RainTileLib.VoxelFunctions;

String imagePath = "";
if (args.Length > 0) {
    imagePath = args[0];
}
else {
    Console.WriteLine("Error: Please provide a file path");
    return;
}

String outputPath = "";
if (args.Length > 1) {
    outputPath = args[1];
}

Stream imageStreamSource = null;
try {
    imageStreamSource = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
}
catch (Exception e) {
    Console.WriteLine($"Error: Invalid file path: {imagePath}");
    Console.WriteLine(e);
    return;
}

Png image = Png.Open(imageStreamSource);

string fileName = Path.GetFileNameWithoutExtension(imagePath);
string minecraftSafeName = fileName.Replace(" ", "_").ToLower();

FileStream minecraftModelStream = null;
try {
    minecraftModelStream = File.Open($"{minecraftSafeName}.json", FileMode.CreateNew);
} 
catch (Exception e) {
    if (e is IOException) {
        Console.Write($"File '{minecraftSafeName}.json' already exists would you like to overwrite it? [y/N]: ");
        string response = Console.ReadLine();
        switch (response) {
            case "y":
            case "Y":
            minecraftModelStream = File.Open($"{minecraftSafeName}.json", FileMode.Create);
                break;
            default:
                Console.WriteLine("Aborting");
                return;
                break;
        }
    }
}
var minecraftModelWriter = new StreamWriter(minecraftModelStream);

TileParameters? parametersNullable = TileReader.GetTileParameters(imagePath);
if (parametersNullable == null) {
    Console.WriteLine("Failed to read parameters, check if Init.txt is next to the file and the tile you're attempting to convert has a valid entry");
    return;
}

TileParameters parameters = parametersNullable.Value;
TileInfo info = TileReader.CalculateTileInfo(parameters);
VoxelGrid grid = CreateVoxelGrid(image, info);
MergeOptimize(grid, MergingType.XY);

List<Voxel> optimziedVoxels = new List<Voxel>();
List<List<int>> layersIndicies = new List<List<int>>();
int index = 0;
for (int z = 0; z < grid.size; z++) {
    List<int> layerIndices = new List<int>();
    for (int y = 0; y < grid.size; y++) {
        for (int x = 0; x < grid.size; x++) {
            if (grid.voxels[x, y, z] is Voxel voxel) {
                optimziedVoxels.Add(voxel);
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
for (int i = 0; i < optimziedVoxels.Count; i++) {
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
                uv = new float[] { U2, V1, U2 - (1 * UVScaleFactorU), V2 },
                rotation = 90,
                texture = "#0"
            },
            south = new MinecraftFace
            {
                uv = new float[] { U1, V2 - (1 * UVScaleFactorV), U2, V2 },
                texture = "#0"
            },
            west = new MinecraftFace
            {
                uv = new float[] { U1 + (1 * UVScaleFactorU), V2, U1, V1 },
                rotation = 90,
                texture = "#0"
            },
            up = new MinecraftFace
            {
                uv = new float[] { U1, V1, U2, V2 },
                texture = "#0"
            },
            down = new MinecraftFace
            {
                uv = new float[] { U1, V2, U2, V1 },
                texture = "#0"
            },
        }
    });
}

MinecraftGroup[] groups = new MinecraftGroup[layersIndicies.Count];
for (int i = 0; i < groups.Length; i++) {
    groups[i] = new MinecraftGroup {
        name = $"layer{i}",
        origin = new int[] { 0, 0, 0 },
        scope = 0,
        color = 0,
        children = layersIndicies[i].ToArray()
    };
}


MinecraftJSON model = new MinecraftJSON {
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

JsonSerializerOptions options = new JsonSerializerOptions {
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};


string json = JsonSerializer.Serialize(model, options);

minecraftModelWriter.Write(json);
minecraftModelWriter.Close();
minecraftModelStream.Close();

try {
    File.Copy(imagePath, minecraftSafeName+".png");
} 
catch (Exception e) {
    if (e is IOException) {
        Console.Write($"File '{minecraftSafeName}.png' already exists would you like to overwrite it? [y/N]: ");
        string response = Console.ReadLine();
        switch (response) {
            case "y":
            case "Y":
            File.Copy(imagePath, minecraftSafeName+".png", true);
                break;
            default:
                break;
        }
    }
}
Console.WriteLine($"Success, created {minecraftSafeName}.json and {minecraftSafeName}.png");

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

