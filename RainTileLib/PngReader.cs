using System;
using StbImageSharp;

// Designed to be basically the same as the BigGustave API but with StbImageSharp as the backend for Adam7 support
public class Png {
    private readonly byte[] data;
    public int Width { get; }
    public int Height { get; }

    private Png(int width, int height, byte[] rgba) {
        Width = width;
        Height = height;
        data = rgba;
    }

    public Pixel GetPixel(int x, int y) {
        int i = (y * Width + x) * 4;
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        // Console.WriteLine($"{data[i+0]}, {data[i+1]}, {data[i+2]}, {data[i+3]}");
        return new Pixel(data[i+0], data[i+1], data[i+2], data[i+3]);
    }

    public static Png Open(Stream stream) {
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        return new Png(image.Width, image.Height, image.Data);
    }
}

public readonly struct Pixel {
    public byte R { get; }

    public byte G { get; }

    public byte B { get; }

    public byte A { get; }

    public Pixel(byte r, byte g, byte b, byte a) {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}