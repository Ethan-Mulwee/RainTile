
using System.Runtime.CompilerServices;

namespace RainTileLib;


public static class VoxelFunctions {


    public static VoxelGrid CreateVoxelGrid(TileData tile) {
        int size = Math.Max(Math.Max(tile.tileX, tile.tileY), tile.numLayers);

        VoxelGrid voxelGrid = new VoxelGrid {
            voxels = new Voxel?[size, size, size],
            size = size
        };

        PixelCoordinates topLeft = new PixelCoordinates{X=0, Y=0};
        Pixel topLeftPixel = tile.image.GetPixel(0,0);
        if (topLeftPixel.R + topLeftPixel.G + topLeftPixel.B != 0) {
            Console.WriteLine("Warning: black pixel is not in the top left, some tiles are like this for some reason");
        }


        // create voxel grid
        for (int i = 0; i < tile.numLayers; i++) {
            for (int x = 0; x < tile.tileX; x++) {
                for (int y = 0; y < tile.tileY; y++) {
                    PixelCoordinates layerOffset = TileConversion.CalculateTopLeftLayerCoordiantes(tile, i);
                    PixelCoordinates imageCoords = new PixelCoordinates { X = layerOffset.X + x, Y = layerOffset.Y + y };
                    Pixel pixel = tile.image.GetPixel(imageCoords.X, imageCoords.Y);
                    if ((pixel.R * pixel.G * pixel.B) == 0) {
                        voxelGrid.voxels[x, y, (tile.numLayers - i)] = new Voxel {
                            span = new VoxelSpan {
                                from = new Vector3Int {
                                    X = x,
                                    Y = y,
                                    Z = (tile.numLayers - i)
                                },
                                to = new Vector3Int {
                                    X = x + 1,
                                    Y = y + 1,
                                    Z = (tile.numLayers - i) + 1
                                },
                            }
                        };
                    }
                }
            }
        }

        return voxelGrid;
    }

    public static void RemoveInteriorVoxels(VoxelGrid grid) {
        VoxelGrid newGrid = new VoxelGrid{
            voxels = new Voxel?[grid.size, grid.size, grid.size],
            size = grid.size
        };

        // remove interior voxels NOTE: doing this before mergin will raise cube count this raises cube count
        for (int z = 0; z < grid.size; z++) {
            for (int y = 0; y < grid.size; y++) {
                for (int x = 0; x < grid.size; x++) {
                    Voxel? voxelN = grid.voxels[x,y,z];
                    bool up = false;
                    bool down = false;
                    bool north = false;
                    bool south = false;
                    bool east = false;
                    bool west = false;
                    if (voxelN != null) {
                        if (x+1 < grid.size && grid.voxels[x+1, y, z] != null) east = true;
                        if (x-1 > 0 && grid.voxels[x-1, y, z] != null) west = true;
                        if (y+1 < grid.size && grid.voxels[x, y+1, z] != null) north = true;
                        if (y-1 > 0 && grid.voxels[x, y-1, z] != null) south = true;
                        if (z+1 < grid.size && grid.voxels[x, y, z+1] != null) up = true;
                        if (z-1 > 0 && grid.voxels[x, y, z-1] != null) down = true;
                    }

                    if (up && down && north && south && east && west) {
                        newGrid.voxels[x,y,z] = null;
                    } else {
                        newGrid.voxels[x,y,z] = voxelN;
                    }
                }
            }
        }
        
        grid.voxels = newGrid.voxels; 
    }


    /* ------------------------------ Optimization ------------------------------ */

    public enum MergingType {
        XY,
        None,
        XYZ
    }

    public static void MergeOptimize(VoxelGrid grid, MergingType type) {
        switch (type) {
            case MergingType.XY:
                MergeX(grid);
                MergeY(grid);
                break;
            case MergingType.XYZ:
                MergeX(grid);
                MergeY(grid);
                MergeZ(grid);
                break;
        }
    }

    public const int MERGE_LIMIT = 16;

    static void MergeX(VoxelGrid grid) {
        for (int z = 0; z < grid.size; z++) {
            for (int y = 0; y < grid.size; y++) {
                for (int x = 0; x < grid.size; x++) {
                    MergeWalk(grid, new Vector3Int(x, y, z), Axis.X);
                }
            }
        }
    }

    static void MergeY(VoxelGrid grid) {
        for (int z = 0; z < grid.size; z++) {
            for (int x = 0; x < grid.size; x++) {
                for (int y = 0; y < grid.size; y++) {
                    MergeWalk(grid, new Vector3Int(x,y,z), Axis.Y);
                }
            }
        }
    }

    static void MergeZ(VoxelGrid grid) {
        for (int x = 0; x < grid.size; x++) {
            for (int y = 0; y < grid.size; y++) {
                for (int z = 0; z < grid.size; z++) {
                    MergeWalk(grid, new Vector3Int(x,y,z), Axis.Z);
                }
            }
        }
    }


    private static void MergeWalk(VoxelGrid grid, Vector3Int coords, Axis axis) {
        if (grid.voxels[coords.X, coords.Y, coords.Z] is Voxel voxel) {

            int walkIdx = 1;

            Vector3Int neighborCoords = new Vector3Int(coords);
            neighborCoords[(int)axis] += 1;

            while (neighborCoords[(int)axis] < grid.size && grid.voxels[neighborCoords.X, neighborCoords.Y, neighborCoords.Z] is Voxel neighborVoxel) {
                if (walkIdx < MERGE_LIMIT && MatchingSpans(voxel, neighborVoxel, axis)) {
                    voxel.span = new VoxelSpan {
                        from = voxel.span.from,
                        to = neighborVoxel.span.to,
                    };
                    grid.voxels[neighborCoords.X, neighborCoords.Y, neighborCoords.Z] = null;
                    neighborCoords[(int)axis] += 1;
                }
                else {
                    break;
                }

                walkIdx++;
            }

            grid.voxels[coords.X, coords.Y, coords.Z] = voxel;
        }
    }

    // Assumes that voxels are next to each other, i.e. doesn't check from only to
    private static bool MatchingSpans(Voxel a, Voxel b, Axis axis) {
        switch (axis) {
            case Axis.X:
                return a.span.to.Y == b.span.to.Y && a.span.to.Z == b.span.to.Z;
            case Axis.Y:
                return a.span.to.X == b.span.to.X && a.span.to.Z == b.span.to.Z;
            case Axis.Z:
                return a.span.to.X == b.span.to.X && a.span.to.Y == b.span.to.Y;
        }
        return false;
    }
}