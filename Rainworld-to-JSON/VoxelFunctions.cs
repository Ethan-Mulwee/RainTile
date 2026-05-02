public static class VoxelFunctions {

    public static void MergeX(VoxelGrid inputGrid)
    {
        int size = inputGrid.size;

        for (int z = 0; z < size; z++) {
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    // note that voxel is a copy here due to the nullable type
                    if (inputGrid.voxels[x, y, z] is Voxel voxel)
                    {
                        int walkIdx = 1;
                        while (x + 1 < size && inputGrid.voxels[x + walkIdx, y, z] is Voxel neighborVoxel)
                        {
                            if (walkIdx < 16)
                            {
                                voxel.span = new VoxelSpan
                                {
                                    from = voxel.span.from,
                                    to = neighborVoxel.span.to,
                                };
                                inputGrid.voxels[x + walkIdx, y, z] = null;
                            }
                            else
                            {
                                break;
                            }

                            walkIdx++;
                        }
                        inputGrid.voxels[x, y, z] = voxel;
                    }
                }
            }
        }
    }

    public static void MergeY(VoxelGrid grid) {

        for (int z = 0; z < grid.size; z++) {
            for (int x = 0; x < grid.size; x++) {
                for (int y = 0; y < grid.size; y++) {

                    if (grid.voxels[x, y, z] is Voxel voxel) {

                        int walkIdx = 1;

                        while (y + 1 < grid.size && grid.voxels[x, y + walkIdx, z] != null) {

                            Voxel neighborVoxel = grid.voxels[x, y + walkIdx, z].Value;
                            if (voxel.span.to.X == neighborVoxel.span.to.X && walkIdx < 16) {
                                voxel.span = new VoxelSpan {
                                    from = voxel.span.from,
                                    to = neighborVoxel.span.to,
                                };
                                grid.voxels[x, y + walkIdx, z] = null;
                            }
                            else
                            {
                                break;
                            }

                            walkIdx++;
                        }

                        grid.voxels[x, y, z] = voxel;

                    }
                }

            }
        }
    }
    
    public static void MergeZ(VoxelGrid grid) {

        int size = grid.size;

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {

                    if (grid.voxels[x, y, z] is Voxel voxel) {

                        int walkIdx = 1;

                        while (z+1 < size && grid.voxels[x, y, z+walkIdx] is Voxel neighborVoxel) {
                            if (voxel.span.to.X == neighborVoxel.span.to.X && voxel.span.to.Y == neighborVoxel.span.to.Y) {
                                voxel.span = new VoxelSpan {
                                    from = voxel.span.from,
                                    to = neighborVoxel.span.to,
                                };
                                grid.voxels[x, y, z+walkIdx] = null;
                            } 
                            else {
                                break;
                            }

                            walkIdx++;
                        }

                        grid.voxels[x,y,z] = voxel;

                    }

                }
            }
        }
    }
}