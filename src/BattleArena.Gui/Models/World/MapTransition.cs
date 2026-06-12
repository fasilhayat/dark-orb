namespace BattleArena.Gui.Models.World;

public readonly record struct MapTransition(
    string TargetMapId,
    TilePosition SpawnPosition);
