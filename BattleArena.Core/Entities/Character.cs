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
    public short Npc { get; set; }
    public string Biography { get; set; } = string.Empty;
    public int ExperiencePoints { get; set; }
    public ArmorSlots Equipment { get; set; } = new();
    public Race? Race { get; set; }
    public List<Feat> Feats { get; set; } = new();
    public List<StatusEffect> ActiveStatusEffects { get; set; } = new();
    public List<DamageType> Vulnerabilities { get; set; } = new();
    public List<Spell> MemorizedSpells { get; set; } = new();

    // ── Vital state (computed from CurrentHitPoints) ──────────────────────────
    // Alive      : HP > 0         — actively fighting
    // KnockedOut : HP 0 to -9     — unconscious, out of the fight but not slain
    // Dead       : HP -10 or lower — permanently dead
    public bool IsAlive      => CurrentHitPoints > 0;
    public bool IsKnockedOut => CurrentHitPoints <= 0 && CurrentHitPoints >= -9;
    public bool IsDead       => CurrentHitPoints <= -10;

    public CharacterVitalStatus VitalStatus =>
        IsDead       ? CharacterVitalStatus.Dead :
        IsKnockedOut ? CharacterVitalStatus.KnockedOut :
                       CharacterVitalStatus.Alive;

    private const int _spellTmCostIntFactor = 3;
    private const int _spellTmCostLevelFactor = 1;
    private const int _minSpellTmCost = 10;

    public int ComputeSpellTurnMeterCost(Spell spell)
    {
        var intMod = (Intelligence - 10) / 2;
        var reduction = intMod * _spellTmCostIntFactor
                      + Level * _spellTmCostLevelFactor
                      + Equipment.TotalTurnMeterCostReduction;
        return Math.Max(_minSpellTmCost, spell.TurnMeterCost - reduction);
    }

    /// <summary>
    /// Total resistance (0–100) for the given type, summed from all sources:
    ///   1. Racial feats (e.g. Elf/Dwarf "Magic Resistance" = 25)
    ///   2. Equipped armor pieces
    ///   3. Active protective status effects (spells like Arcane Ward)
    /// Capped at 95 so there is always at least a 5 % infliction chance.
    /// </summary>
    public int ComputeResistance(ResistanceType type)
    {
        var total = 0;

        if (Race is not null)
            foreach (var feat in Race.Feats)
                foreach (var r in feat.Resistances)
                    if (r.Type == type) total += r.Value;

        total += Equipment.TotalResistance(type);

        foreach (var effect in ActiveStatusEffects)
            foreach (var r in effect.ResistanceBonuses)
                if (r.Type == type) total += r.Value;

        return Math.Clamp(total, 0, 95);
    }
}
