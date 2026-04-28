
using BigGustave;

using System.Text.Json;
using System.Text.Json.Serialization;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

if (args.Length > 0) {
    Console.WriteLine($"First argument: {args[0]}");
}

String testPath = "/home/ethan/Desktop/rained_v2.5.0_linux-x64/Data/Graphics/Background AC Fan.png";
Stream imageStreamSource = new FileStream(testPath, FileMode.Open, FileAccess.Read, FileShare.Read);
Png image = Png.Open(imageStreamSource);

Pixel pixel = image.GetPixel(image.Width - 1, image.Height - 1);

Console.WriteLine($"{image.Height}x{image.Width}");

string folder = Path.GetDirectoryName(testPath);
Console.WriteLine(folder);
string initPath = folder + "/Init.txt";

// Stream initStream = new FileStream(initPath, FileMode.Open, FileAccess.Read, FileShare.Read);
string initText = File.ReadAllText(initPath);
Console.WriteLine(initText);


MinecraftElement[] minecraftElements = {
    new MinecraftElement {
        from = new int[]{2, 0, 5},
        to = new int[]{4, 2, 7},
        rotation = new MinecraftRotation {
            angle = 0.0f,
            axis = "y",
            origin = new int[]{2, 0, 5}
        },
        color = 7,
        faces = new MinecraftFaces {
            north = new MinecraftFace {
                uv = new int[]{0, 0, 2, 2},
                texture = "#missing"
            },
            east = new MinecraftFace {
                uv = new int[]{0, 0, 2, 2},
                texture = "#missing"
            },
            south = new MinecraftFace {
                uv = new int[]{0, 0, 2, 2},
                texture = "#missing"
            },
            west = new MinecraftFace {
                uv = new int[]{0, 0, 2, 2},
                texture = "#missing"
            },
            up = new MinecraftFace {
                uv = new int[]{0, 0, 2, 2},
                texture = "#missing"
            },
            down = new MinecraftFace {
                uv = new int[]{0, 0, 2, 2},
                texture = "#missing"
            },
        }
    }
};

MinecraftJSON model = new MinecraftJSON {
    format_version = MinecraftExportConstants.FORMAT_VERSION,
    credit = MinecraftExportConstants.CREDIT,
    elements = minecraftElements
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