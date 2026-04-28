
public static class MinecraftExportConstants {
    public const string FORMAT_VERSION = "1.21.11";
    public const string CREDIT = "Rainworld to JSON";
}

public struct MinecraftJSON {
    public string format_version { get; set; }
    public string credit { get; set; }
    public MinecraftElement[] elements { get; set; }
}

public struct MinecraftElement {
    public int[] from { get; set; }
    public int[] to { get; set; }
    public MinecraftRotation rotation { get; set; }
    public int color { get; set; }
    public MinecraftFaces faces { get; set; }
}

public struct MinecraftRotation {
    public float angle { get; set; }
    public string axis { get; set; }
    public int[] origin { get; set; }
}

public struct MinecraftFaces {
    public MinecraftFace north { get; set; }
    public MinecraftFace east { get; set; }
    public MinecraftFace south { get; set; }
    public MinecraftFace west { get; set; }
    public MinecraftFace up { get; set; }
    public MinecraftFace down { get; set; }
}

public struct MinecraftFace {
    public int[] uv { get; set; }
    public string texture { get; set; }
}