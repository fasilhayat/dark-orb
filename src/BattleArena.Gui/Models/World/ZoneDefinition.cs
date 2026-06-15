namespace BattleArena.Gui.Models.World;

public record ZoneDefinition(string Name, string MapId, string AmbientColor)
{
    public static readonly ZoneDefinition World = new("World", "world", "#00000000");
    public static readonly ZoneDefinition Dungeon = new("Cave", "dungeon", "#40000000");
}
