using System.Text.Json.Serialization;

namespace RainTileLib;

public static class MinecraftExportConstants {
    public const string FORMAT_VERSION = "1.21.11";
    public const string CREDIT = "RainTile 1.0.0";
}

public struct MinecraftJSON {
    public string format_version { get; set; }
    public string credit { get; set; }
    public int[] texture_size { get; set; }
    public MinecraftTextures textures { get; set; }
    public MinecraftElement[] elements { get; set; }
    public MinecraftGroup[] groups { get; set; }
}

public struct MinecraftTextures {
    [JsonPropertyName("0")]
    public string texture0Path { get; set; }
    [JsonPropertyName("particle")]
    public string particlePath { get; set; }
}

public struct MinecraftElement {
    public float[] from { get; set; }
    public float[] to { get; set; }
    public MinecraftRotation rotation { get; set; }
    public int color { get; set; }
    public MinecraftFaces faces { get; set; }
}

public struct MinecraftGroup {
    public string name { get; set; }
    public int[] origin { get; set; }
    public int scope { get; set; }
    public int color { get; set; }
    public int[] children { get; set; }
}

public struct MinecraftRotation {
    public float angle { get; set; }
    public string axis { get; set; }
    public int[] origin { get; set; }
}

public struct MinecraftFaces {
    public MinecraftFace? north { get; set; }
    public MinecraftFace? east { get; set; }
    public MinecraftFace? south { get; set; }
    public MinecraftFace? west { get; set; }
    public MinecraftFace? up { get; set; }
    public MinecraftFace? down { get; set; }
}

public struct MinecraftFace {
    public float[] uv { get; set; }
    public int rotation { get; set; }
    public string texture { get; set; }
}