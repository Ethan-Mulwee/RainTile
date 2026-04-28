
using BigGustave;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

if (args.Length > 0) {
    Console.WriteLine($"First argument: {args[0]}");
}

String testPath = "/home/ethan/Desktop/rained_v2.5.0_linux-x64/Data/Graphics/Background AC Fan.png";
Stream imageStreamSource = new FileStream(testPath, FileMode.Open, FileAccess.Read, FileShare.Read);
Png image = Png.Open(imageStreamSource);

Pixel pixel = image.GetPixel(image.Width - 1, image.Height - 1);

int pixelRedAverage = 0;

pixelRedAverage += pixel.R;

pixel = image.GetPixel(0, 0);

pixelRedAverage += pixel.R;

Console.WriteLine(pixelRedAverage / 2.0);

Console.WriteLine($"{image.Height}x{image.Width}");