namespace BattleArena.Gui.Models.World;

public class TileMap
{
    private readonly Tile[] _tiles;

    public int Width { get; }
    public int Height { get; }

    public TileMap(int width, int height, Tile[] tiles)
    {
        if (tiles.Length != width * height)
            throw new ArgumentException($"Expected {width * height} tiles, got {tiles.Length}");

        Width = width;
        Height = height;
        _tiles = tiles;
    }

    public Tile this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new IndexOutOfRangeException($"Tile ({x}, {y}) is out of bounds ({Width}x{Height})");
            return _tiles[y * Width + x];
        }
    }
}
