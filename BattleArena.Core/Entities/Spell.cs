using BattleArena.Core.Entities.Enums;

namespace BattleArena.Core.Entities;

public class Spell
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SpellSchool School { get; set; }
    public DieType? DamageDie { get; set; }
    public DamageType? DamageType { get; set; }
    public int ManaCost { get; set; }
}
