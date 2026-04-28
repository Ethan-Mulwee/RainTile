
using BigGustave;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;


public static class TileReader {
    public static TileInfo GetTileInfo(TileParameters parameters) {
        return new TileInfo();
    }

    public static TileParameters GetTileParameters(string imagePath) {
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

            if (szMatch == null) {
                Console.WriteLine("#sz:point(x,y) parameter could not be found");
            }
            if (bfMatch == null) {
                Console.WriteLine("#bfTiles parameter could not be found");
            }
            if (repeatLMatch == null) {
                Console.WriteLine("#repeatL parameter could not be found");
            }

            int sx = int.Parse(szMatch.Groups[1].Value);
            int sy = int.Parse(szMatch.Groups[2].Value);

            Console.WriteLine($"SZ: ({sx}, {sy})");
        }

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