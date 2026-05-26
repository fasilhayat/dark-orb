namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Shield
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DefenseBonus { get; set; }
    public GearQuality Quality { get; set; } = GearQuality.Common;
}
