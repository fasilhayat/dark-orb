namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class StatusEffect
{
    public string Name { get; set; } = string.Empty;
    public StatusEffectType Type { get; set; }
    public TriggerCondition TriggerCondition { get; set; }
    public int Duration { get; set; }
    public StackRule StackRule { get; set; }
    public int Magnitude { get; set; }
    public int ResolutionPriority { get; set; }
    public string Source { get; set; } = string.Empty;
    public int AttackPowerModifier { get; set; }
    public int DefensePowerModifier { get; set; }
    public int TurnMeterModifier { get; set; }
    public int DamagePerTurn { get; set; }
    public int ApplicationChance { get; set; } = 100;
    public ResistanceType ResistanceType { get; set; } = ResistanceType.Magic;
    public List<ResistanceBonus> ResistanceBonuses { get; set; } = new();
    public int DoTDamageCount { get; set; }
    public DieType DoTDamageDie { get; set; }
}
