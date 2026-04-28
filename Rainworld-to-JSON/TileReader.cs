
using BigGustave;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;


public static class TileReader {
    public static TileInfo CalculateTileInfo(TileParameters parameters) {
        const int CELL_SIZE = 20;
        const int PREVIEW_SIZE = 16;

        int tileX = (parameters.SZx + parameters.BfTiles * 2) * CELL_SIZE;
        int tileY = (parameters.SZy + parameters.BfTiles * 2) * CELL_SIZE;
        int previewX = parameters.SZx * PREVIEW_SIZE;
        int previewY = parameters.SZy * PREVIEW_SIZE;

        int boundX = tileX;
        int boundY = (tileY * parameters.RepeatL.Length) + previewY + 1;

        return new TileInfo {
            tileX = tileX,
            tileY = tileY,
            previewX = previewX,
            previewY = previewY,
            boundX = boundX,
            boundY = boundY,
            numLayers = parameters.RepeatL.Length,
        };
    }

    public struct PixelCoordinates {
        public int X, Y;
    }

    public static PixelCoordinates CalculateTopLeftLayerCoordiantes(TileInfo tileInfo, int layerIndex) {
        
        return new PixelCoordinates {
            X = 0,
            Y = (layerIndex * tileInfo.tileY)+1
        };
    }

    public static TileParameters? GetTileParameters(string imagePath) {
        string folder = Path.GetDirectoryName(imagePath);
        string initPath = folder + "/Init.txt";
        string initText = File.ReadAllText(initPath);

        string imageName = Path.GetFileNameWithoutExtension(imagePath);

        Regex nameRegex = new Regex("#nm\\s*:\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
        Regex szRegex = new Regex(@"#sz\s*:\s*point\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)", RegexOptions.IgnoreCase);
        Regex bfRegex = new Regex(@"#bfTiles\s*:\s*(\d+)", RegexOptions.IgnoreCase);
        Regex repeatLRegex = new Regex(@"#repeatL\s*:\s*\[\s*([^\]]*?)\s*\]", RegexOptions.IgnoreCase);
        
        int[] ParseIntList(string text) {
            List<int> numbers = new List<int>();
            foreach (string part in text.Split(",")) {
                string trimmedPart = part.Trim();
                if (trimmedPart == "") {
                    continue;
                }
                numbers.Add(int.Parse(trimmedPart));
            }
            return numbers.ToArray();
        } 

        foreach (string rawLine in initText.Split(Environment.NewLine)) {
            string line = rawLine.Trim();
            if (line.Length == 0) {
                continue;
            }
            if (!(line.StartsWith('[') && line.EndsWith(']'))) {
                continue;
            }

            Match nameMatch = nameRegex.Match(line);
            if (!nameMatch.Success) {
                continue;
            }

            string name = nameMatch.Groups[1].Value;

            if (!string.Equals(name, imageName, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            Match szMatch = szRegex.Match(line);
            Match bfMatch = bfRegex.Match(line);
            Match repeatLMatch = repeatLRegex.Match(line);

            if (!szMatch.Success) {
                throw new ArgumentException("#sz:point(x,y) parameter could not be found");
            }
            if (!bfMatch.Success) {
                throw new ArgumentException("#bfTiles parameter could not be found");
            }
            if (!repeatLMatch.Success) {
                throw new ArgumentException("#repeatL parameter could not be found");
            }

            int sx = int.Parse(szMatch.Groups[1].Value);
            int sy = int.Parse(szMatch.Groups[2].Value);
            int bfTiles = int.Parse(bfMatch.Groups[1].Value);
            int[] repeatL = ParseIntList(repeatLMatch.Groups[1].Value);

            // Console.WriteLine($"SZ: ({sx}, {sy})");
            return new TileParameters{
                SZx = sx,
                SZy = sy,
                BfTiles = bfTiles,
                RepeatL = repeatL
            };
        }

        return null;
    }
}

public struct TileParameters {
    public int SZx, SZy;
    public int BfTiles;
    public int[] RepeatL; // should add up to 10
};

public struct TileInfo {
    public int tileX;
    public int tileY;
    public int previewX;
    public int previewY;

    public int boundX;
    public int boundY;
    public int numLayers;
}