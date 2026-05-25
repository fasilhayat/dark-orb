using BattleArena.Core.Entities.Enums;

namespace BattleArena.Core.Entities;

public class Deity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DeityAlignment Alignment { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
}
