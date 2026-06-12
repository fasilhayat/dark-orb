namespace BattleArena.Gui.Rendering;

using Models.World;

public static class IsometricCoordinateTranslator
{
    public static PixelPosition TileToScreen(TilePosition tile, int tileWidth, int tileHeight)
    {
        var x = (tile.TileX - tile.TileY) * (tileWidth / 2.0);
        var y = (tile.TileX + tile.TileY) * (tileHeight / 2.0);
        return new PixelPosition(x, y);
    }

    public static TilePosition ScreenToTile(PixelPosition screen, int tileWidth, int tileHeight)
    {
        var tileX = (int)Math.Floor((screen.X / (tileWidth / 2.0) + screen.Y / (tileHeight / 2.0)) / 2.0);
        var tileY = (int)Math.Floor((screen.Y / (tileHeight / 2.0) - screen.X / (tileWidth / 2.0)) / 2.0);
        return new TilePosition(tileX, tileY);
    }
}
