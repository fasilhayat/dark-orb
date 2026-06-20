namespace BattleArena.Gui.Models.World;

public static class TestMapData
{
    public static TileMap CreateArenaMap()
    {
        const int width = 16;
        const int height = 10;
        var tiles = new Tile[width * height];

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            // Pond in the top-left corner
            if (x >= 1 && x <= 3 && y >= 0 && y <= 2)
                tiles[y * width + x] = new Tile(TileType.Water, 0, false);
            // Mountain ridge across the top-right
            else if (x >= 11 && x <= 14 && y <= 1)
                tiles[y * width + x] = new Tile(TileType.Mountain, 0, false);
            // Dirt path down the centre columns
            else if (x == 7 || x == 8)
                tiles[y * width + x] = new Tile(TileType.Road, 1, true);
            // Scattered forest patches (impassable)
            else if ((x + y) % 4 == 0 && x > 3 && x < 11)
                tiles[y * width + x] = new Tile(TileType.Forest, 0, false);
            // Grass everywhere else
            else
                tiles[y * width + x] = new Tile(TileType.Grass, 1, true);
        }

        return new TileMap(width, height, tiles);
    }
}
