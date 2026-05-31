namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxHitPoints { get; set; }
    public int ArmorClass { get; set; }
    public int TurnSpeed { get; set; }
    public int Strength { get; set; }
    public int StrikeRating { get; set; }
    public int AttackBonus { get; set; }
    public int DamageCount { get; set; } = 1;
    public DieType DamageDie { get; set; }
    public DamageType DamageType { get; set; }
    public int SummonDurationRounds { get; set; }
}
