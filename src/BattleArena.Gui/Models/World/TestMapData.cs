namespace BattleArena.Gui.Models.World;

public static class TestMapData
{
    public static TileMap CreateArenaMap(string terrain = "grass")
    {
        const int width = 12;
        const int height = 8;

        var type = terrain.ToLowerInvariant() switch
        {
            "desert" => TileType.Road,
            _ => TileType.Grass,
        };

        var tiles = new Tile[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            tiles[y * width + x] = new Tile(type, 1, true);

        return new TileMap(width, height, tiles);
    }
}
