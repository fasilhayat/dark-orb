namespace BattleArena.Gui.Models.World;

public record Tile(TileType Type, int MovementCost, bool IsPassable);
