namespace BattleArena.Core.Entities;

public class Subrace
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
