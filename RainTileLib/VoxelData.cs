namespace RainTileLib;

public class VoxelGrid {
    public Voxel?[,,] voxels;
    public int size;
}

public struct VoxelSpan {
    public Vector3Int from;
    public Vector3Int to;
}

public struct Voxel {
    public VoxelSpan span;
}

public struct Vector3Int {
    public int X, Y, Z;

    public Vector3Int(int x, int y, int z) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public Vector3Int(Vector3Int v) {
        this.X = v.X;
        this.Y = v.Y;
        this.Z = v.Z;
    }

    // This is the Indexer that allows the [] syntax
    public int this[int index] {
        get {
            return index switch {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new System.IndexOutOfRangeException("Invalid Vector3Int index")
            };
        }
        set {
            switch (index) {
                case 0: X = value; break;
                case 1: Y = value; break;
                case 2: Z = value; break;
                default: throw new System.IndexOutOfRangeException("Invalid Vector3Int index");
            }
        }
    }
}

public enum Axis {
    X = 0,
    Y = 1,
    Z = 2
}
