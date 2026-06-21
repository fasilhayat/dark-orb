namespace BattleArena.Gui.Rendering;

using Models.World;

public static class FogOfWar
{
    /// <summary>Tile fog state: true = currently visible to the player's side.</summary>
    public static bool[]? CurrentFog { get; private set; }
    private static int _mapWidth;
    private static int _mapHeight;

    /// <summary>
    /// Recalculate visibility for all tiles based on friendly unit positions
    /// and their sight radii.  Call before each RenderMap.
    /// </summary>
    public static void Recompute(TileMap map, IReadOnlyList<CombatantTile> combatants)
    {
        _mapWidth = map.Width;
        _mapHeight = map.Height;
        CurrentFog = new bool[_mapWidth * _mapHeight];

        // Visible tiles: all tiles within sight radius of any friendly unit
        foreach (var c in combatants)
        {
            if (!c.IsHero) continue;
            var sight = c.SightRadius;
            for (var y = 0; y < _mapHeight; y++)
            for (var x = 0; x < _mapWidth; x++)
            {
                if (HexGrid.TileDistance(c.Position, new TilePosition(x, y)) <= sight)
                    CurrentFog[y * _mapWidth + x] = true;
            }
        }
    }

    public static bool IsVisible(int x, int y) =>
        CurrentFog is not null && x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight
            && CurrentFog[y * _mapWidth + x];

    public static bool IsVisible(TilePosition pos) =>
        IsVisible(pos.TileX, pos.TileY);
}
