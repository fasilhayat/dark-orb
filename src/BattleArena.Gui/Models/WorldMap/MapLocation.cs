namespace BattleArena.Gui.Models.WorldMap;

public class MapLocation
{
    public string Name { get; set; } = "";
    public LocationType Type { get; set; }
    public double ScreenX { get; set; }
    public double ScreenY { get; set; }
    public string? TargetMapId { get; set; }
    public string? Description { get; set; }
}
