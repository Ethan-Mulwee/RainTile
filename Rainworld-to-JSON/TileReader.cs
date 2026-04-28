
using BigGustave;


public class TileReader {
    public TileInfo GetTileInfo(TileParameters parameters) {
        return new TileInfo();
    }

    public TileParameters GetTileParameters(string imagePath) {
        return new TileParameters();
    }
}

public struct TileParameters {
    public int SZx, SZy;
    public int bfTiles;
    public int[] repeatL; // should add up to 10
};

public struct TileInfo {
    public int tileX;
    public int tileY;
    public int previewX;
    public int previewY;
}