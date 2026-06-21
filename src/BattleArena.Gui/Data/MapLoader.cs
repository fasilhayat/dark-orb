namespace BattleArena.Gui.Data;

using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Models.World;

public sealed class MapData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Terrain { get; init; } = "Plains";
    public int Width { get; init; }
    public int Height { get; init; }
    public List<string> Tiles { get; init; } = [];
}

public static class MapLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private static List<MapData>? _cache;

    private static string MapsDir =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "World", "maps");

    public static IReadOnlyList<MapData> ListMaps()
    {
        if (_cache is not null)
            return _cache;

        var dir = MapsDir;
        if (!Directory.Exists(dir))
        {
            _cache = [];
            return _cache;
        }

        _cache = [];
        foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<MapData>(json, JsonOpts);
                if (data is not null)
                    _cache.Add(data);
            }
            catch { /* skip malformed files */ }
        }

        return _cache;
    }

    public static TileMap LoadMap(MapData data)
    {
        var tiles = new Tile[data.Width * data.Height];
        for (var i = 0; i < tiles.Length; i++)
        {
            var typeName = i < data.Tiles.Count ? data.Tiles[i] : "Grass";
            TileType type;
            try { type = Enum.Parse<TileType>(typeName, ignoreCase: true); }
            catch { type = TileType.Grass; }

            var passable = type switch
            {
                TileType.Water => false,
                TileType.Mountain => false,
                TileType.Forest => false,
                TileType.DungeonWall => false,
                _ => true,
            };
            var cost = type switch
            {
                TileType.Road => 1,
                TileType.Grass => 1,
                TileType.DungeonFloor => 1,
                TileType.Bridge => 1,
                _ => 0,
            };
            tiles[i] = new Tile(type, cost, passable);
        }

        return new TileMap(data.Width, data.Height, tiles);
    }
}
