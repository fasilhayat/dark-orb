namespace BattleArena.Gui.Rendering.Sprites;

using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Models.World;

public sealed class SpriteSheet
{
    /// <summary>
    /// Maps a sprite sheet tile name to a TileType.
    /// Multiple sprite tiles can map to the same TileType.
    /// </summary>
    private static readonly Dictionary<string, TileType> TileNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["grass_plains"] = TileType.Grass,
        ["farmland"]     = TileType.Grass,
        ["forest"]       = TileType.Forest,
        ["mountain"]     = TileType.Mountain,
        ["rocky"]        = TileType.Mountain,
        ["volcano"]      = TileType.Mountain,
        ["desert"]       = TileType.Grass,
        ["snow"]         = TileType.Grass,
        ["coast"]        = TileType.Water,
        ["swamp"]        = TileType.Water,
        ["river"]        = TileType.Water,
        ["city"]         = TileType.Road,
    };

    private readonly Bitmap _sheet;
    private readonly Bitmap?[] _frames;

    public SpriteSheet(string path, string jsonPath)
    {
        _sheet = new Bitmap(path);
        var json = File.ReadAllText(jsonPath);
        var doc = JsonDocument.Parse(json);

        var tiles = doc.RootElement.GetProperty("tiles");
        _frames = new Bitmap[(int)TileType.DungeonEntrance + 1];

        foreach (var tile in tiles.EnumerateArray())
        {
            var name = tile.GetProperty("name").GetString() ?? "";
            if (!TileNameMap.TryGetValue(name, out var tileType))
                continue;

            var x = tile.GetProperty("x").GetInt32();
            var y = tile.GetProperty("y").GetInt32();
            var w = tile.GetProperty("width").GetInt32();
            var h = tile.GetProperty("height").GetInt32();

            var frame = new RenderTargetBitmap(new PixelSize(w, h));
            using var ctx = frame.CreateDrawingContext();
            ctx.DrawImage(_sheet, new Rect(x, y, w, h), new Rect(0, 0, w, h));
            ctx.Dispose();
            _frames[(int)tileType] = frame;
        }
    }

    public Bitmap? GetTile(TileType type)
    {
        var i = (int)type;
        return i >= 0 && i < _frames.Length ? _frames[i] : null;
    }

    public void Dispose() => _sheet.Dispose();
}
