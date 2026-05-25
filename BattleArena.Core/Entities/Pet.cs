using BattleArena.Core.Entities.Enums;

namespace BattleArena.Core.Entities;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DieType? DamageDie { get; set; }
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
}
