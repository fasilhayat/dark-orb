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
            // Dirt path down the center column
            if (x == 7 || x == 8)
                tiles[y * width + x] = new Tile(TileType.Road, 1, true);
            // Shade variation: every other tile a slightly different green
            else if ((x + y) % 3 == 0)
                tiles[y * width + x] = new Tile(TileType.Grass, 1, true);
            else if ((x + y) % 3 == 1)
                tiles[y * width + x] = new Tile(TileType.Forest, 1, true);
            else
                tiles[y * width + x] = new Tile(TileType.Grass, 1, true);
        }

        return new TileMap(width, height, tiles);
    }
}
