namespace BattleArena.Core.Entities;

using Core.Entities.Enums;

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int RaceId { get; set; }
    public int ClassId { get; set; }
    public int Strength { get; set; } = 10;
    public int StrengthPercentile { get; set; }
    public int Dexterity { get; set; } = 10;
    public int Stamina { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    public int MaxHitPoints { get; set; }
    public int CurrentHitPoints { get; set; }
    public int StrikeRating { get; set; }
    public int TurnSpeed { get; set; }
    public ArmorSlots Equipment { get; set; } = new();
    public Race? Race { get; set; }
    public List<Feat> Feats { get; set; } = new();
    public List<StatusEffect> ActiveStatusEffects { get; set; } = new();
    public List<DamageType> Vulnerabilities { get; set; } = new();
}
