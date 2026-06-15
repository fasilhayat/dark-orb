namespace BattleArena.Gui.Rendering;

using Models.World;

public readonly record struct PathResult(
    IReadOnlyList<TilePosition> Waypoints,
    bool IsReachable,
    int TotalCost);
