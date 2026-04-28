

public struct VoxelGrid {
    public Voxel?[,,] voxels;
    public int size;
}

public struct VoxelCoordinates {
    public int X, Y, Z;
};

public struct VoxelSpan {
    public VoxelCoordinates from;
    public VoxelCoordinates to;
}

public struct Voxel {
    public VoxelSpan span;
}