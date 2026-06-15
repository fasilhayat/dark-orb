namespace BattleArena.Gui.Rendering;

public readonly record struct Viewport(
    int MinTileX, int MinTileY,
    int MaxTileX, int MaxTileY)
{
    public bool Contains(int tileX, int tileY) =>
        tileX >= MinTileX && tileX <= MaxTileX &&
        tileY >= MinTileY && tileY <= MaxTileY;
}
