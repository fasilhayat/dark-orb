namespace BattleArena.Core.Entities;

using Core.Entities.Enums;
using Core.Models;

// ReSharper disable once ArrangeModifiers

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int RaceId { get; set; }
    public int? SubraceId { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public PlayerClass? Class { get; set; }
    public Subrace? Subrace { get; set; }
    public string Sex { get; set; } = "Unknown";
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
    public int MaxMana { get; set; }
    public int CurrentMana { get; set; }
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

    public int ManaRegenPerTick =>
        Math.Max(0,
            (Intelligence - 10) / 2
            + Equipment.TotalManaRegenBonus
            + ActiveStatusEffects.Sum(e => e.ManaRegenModifier));

    // MaxMana including any gear bonuses (e.g. arcane robes, mana-crystal amulet).
    public int EffectiveMaxMana => MaxMana + Equipment.TotalMaxManaBonus;

    public int EffectiveMovementSpeed
    {
        get
        {
            var baseSpeed = Race?.BaseMovementSpeed ?? 30;
            var classBonus = Class?.MovementBonus ?? 0;
            var penalty = Equipment.TotalMovementPenalty;
            var buff = ActiveStatusEffects.Sum(e => e.MovementModifier);
            return Math.Max(10, baseSpeed + classBonus - penalty + buff);
        }
    }

    /// <summary>Total spell casts available per combat. Reset at combat start.</summary>
    public int RemainingCasts { get; set; } = int.MaxValue;

    public int MaxCastsPerCombat => 2 + Level / 3 + Equipment.TotalSpellSlotsBonus;

    public int SpellMemorizationSlots
    {
        get
        {
            var mod = (Intelligence - 10) / 2;
            return Math.Max(1, 2 + mod + Equipment.TotalSpellSlotsBonus);
        }
    }

    /// <summary>
    /// Base Strength score plus any Strength bonuses from equipped gear
    /// (e.g., belts of giant strength, gauntlets of ogre power).
    /// </summary>
    public int EffectiveStrength => Strength + Equipment.TotalStrengthBonus;

    private const int MinStrengthForTwoHanded = 16;
    private const int MinStrengthForDualWield = 15;

    /// <summary>Returns true if this character's class may wield the given weapon archetype (AD&amp;D 2e rules).</summary>
    public bool CanEquip(ArchetypeWeapon archetype) => archetype.IsUsableByClass(ClassId);

    /// <summary>
    /// Returns true if this character has the minimum Strength (including gear bonuses)
    /// to wield the given weapon. Two-handed weapons require STR 16.
    /// </summary>
    public bool HasSufficientStrength(Weapon weapon)
    {
        var implicitMin = (weapon.Hands >= 2 && ClassCombatData.IsTwoHandedArchetype(weapon.Archetype))
            ? MinStrengthForTwoHanded
            : 0;
        var required = Math.Max(weapon.MinimumStrength, implicitMin);
        return required <= 0 || EffectiveStrength >= required;
    }

    /// <summary>Returns true if this character's class may wield the given weapon (inherits from its archetype) and has sufficient Strength.</summary>
    public bool CanEquip(Weapon weapon) => CanEquip(weapon.Archetype) && HasSufficientStrength(weapon);

    // ── Attacks per turn ───────────────────────────────────────────────────────

    /// <summary>
    /// Number of attacks this character makes per turn.
    /// Base from class, modified by weapon type (e.g., Ranger bows = 3).
    /// </summary>
    public int AttacksPerTurn
    {
        get
        {
            var baseAttacks = Class?.AttacksPerTurn ?? ClassCombatData.AttacksPerTurn(ClassId);
            var bowAttacks  = Class?.BowAttacksPerTurn ?? ClassCombatData.BowAttacksPerTurn(ClassId);

            // Ranger wielding a bow gets extra attacks
            if (bowAttacks > 0
                && Equipment.RightHand?.AttackType == AttackType.Ranged
                && Equipment.RightHand?.Archetype == ArchetypeWeapon.Bow)
            {
                return Math.Max(baseAttacks, bowAttacks);
            }

            return baseAttacks;
        }
    }

    private const int _fullTurnThreshold = 100;

    /// <summary>
    /// Turnmeter cost for switching weapon types (e.g. from ranged to melee).
    /// 0 = no cost, full value = one full turn cost.
    /// </summary>
    public int WeaponSwitchTurnMeterCost
    {
        get
        {
            var multiplier = Class?.WeaponSwitchCostMultiplier ?? ClassCombatData.WeaponSwitchCostMultiplier(ClassId);
            if (multiplier <= 0) return 0;
            return (int)(_fullTurnThreshold * multiplier);
        }
    }

    /// <summary>
    /// Returns true if the character's equipped armor violates class restrictions.
    /// E.g., Barbarians cannot wear heavy armor.
    /// </summary>
    public bool HasArmorViolation
    {
        get
        {
            var restriction = Class?.ArmorRestriction ?? ClassCombatData.ArmorRestriction(ClassId);
            if (string.IsNullOrEmpty(restriction)) return false;

            foreach (var armor in new[] { Equipment.Chest })
            {
                if (armor is null) continue;
                var category = armor.CategoryName;
                if (restriction == "Light" && category != "Light")
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Returns true if dual-wielding is valid for this character's class.
    /// Requires STR 15 minimum.
    /// Rogues: short sword + dagger or 2 daggers.
    /// Rangers: 2 shortswords or shortsword + dagger.
    /// Fighters: any combination of one-handed weapons.
    /// </summary>
    public bool CanDualWield
    {
        get
        {
            if (EffectiveStrength < MinStrengthForDualWield) return false;
            var canDW = Class?.CanDualWield ?? ClassCombatData.CanDualWield(ClassId);
            if (!canDW) return false;
            if (Equipment.RightHand is null || Equipment.LeftHand is null) return false;

            var right = Equipment.RightHand;
            var left = Equipment.LeftHand;

            // Both must be one-handed
            if (right.Hands > 1 || left.Hands > 1) return false;

            // Rogue restrictions: short sword + dagger, or 2 daggers
            if (ClassId == 9)
            {
                var validDaggers = right.Archetype == ArchetypeWeapon.Dagger && left.Archetype == ArchetypeWeapon.Dagger;
                var validShortSwordDagger = (right.Archetype == ArchetypeWeapon.ShortSword && left.Archetype == ArchetypeWeapon.Dagger)
                    || (right.Archetype == ArchetypeWeapon.Dagger && left.Archetype == ArchetypeWeapon.ShortSword);
                return validDaggers || validShortSwordDagger;
            }

            // Ranger restrictions: 2 shortswords or shortsword + dagger
            if (ClassId == 10)
            {
                var bothShortSwords = right.Archetype == ArchetypeWeapon.ShortSword && left.Archetype == ArchetypeWeapon.ShortSword;
                var shortSwordDagger = (right.Archetype == ArchetypeWeapon.ShortSword && left.Archetype == ArchetypeWeapon.Dagger)
                    || (right.Archetype == ArchetypeWeapon.Dagger && left.Archetype == ArchetypeWeapon.ShortSword);
                return bothShortSwords || shortSwordDagger;
            }

            // Fighter: any one-handed weapons
            return true;
        }
    }

    /// <summary>
    /// Bonus attack power from class-specific two-handed weapon training.
    /// Barbarian: +2 with two-handed swords and battle-axes.
    /// Paladin: +2 with two-handed swords and warhammers.
    /// </summary>
    public int TwoHandedWeaponBonus
    {
        get
        {
            var weapon = Equipment.RightHand;
            if (weapon is null || weapon.Hands < 2) return 0;
            if (!ClassCombatData.IsTwoHandedArchetype(weapon.Archetype)) return 0;
            return Class?.TwoHandedWeaponBonus ?? ClassCombatData.TwoHandedWeaponBonus(ClassId);
        }
    }

    /// <summary>
    /// Bonus damage when wielding a shield.
    /// Knight: +2 damage with shield equipped.
    /// </summary>
    public int ShieldBonusDamage
    {
        get
        {
            if (Equipment.Shield is null) return 0;
            return Class?.ShieldBonusDamage ?? ClassCombatData.ShieldBonusDamage(ClassId);
        }
    }

    /// <summary>
    /// Ranged attack bonus (Ranger: +1 with ranged weapons).
    /// </summary>
    public int RangedAttackBonus
    {
        get
        {
            var weapon = Equipment.RightHand;
            if (weapon is null || weapon.AttackType != AttackType.Ranged) return 0;
            return Class?.RangedAttackBonus ?? ClassCombatData.RangedAttackBonus(ClassId);
        }
    }

    /// <summary>
    /// Elven Ranger Dexterity bonus: Elven rangers get extra hit bonus based on DEX modifier.
    /// </summary>
    public int ElvenRangerDexBonus
    {
        get
        {
            if (ClassId != 10) return 0; // Must be Ranger
            var isElf = Race?.Name == "Elf";
            var isHalfElf = Race?.Name == "Half-Elf";
            if (!isElf && !isHalfElf) return 0;

            var weapon = Equipment.RightHand;
            if (weapon is null) return 0;

            // Bonus applies to ranged attacks
            if (weapon.AttackType != AttackType.Ranged) return 0;

            return CalculateAbilityModifier(Dexterity);
        }
    }

    private static int CalculateAbilityModifier(int score) => (score - 10) / 2;

    private const int _spellTmCostIntPctPerMod = 3;
    private const int _spellTmCostLevelPct = 1;
    private const int _minSpellTmCostPct = 10;

    /// <summary>
    /// Returns the spell's turn-meter cost as a percentage of a full turn
    /// (100 = 100 % = one full turn). Reduced by INT, level, and equipment.
    /// </summary>
    private static readonly Dictionary<string, HashSet<string>> SpellClassRestrictions = new()
    {
        ["Smite"] = ["Paladin", "Knight"],
        ["Mind Game"] = ["Mage", "Bard", "Priest"],
        ["Charm Person"] = ["Mage", "Bard", "Priest"],
        ["Turn Undead"] = ["Priest", "Paladin", "Knight"],
    };

    /// <summary>
    /// Returns true if this character's class can cast the given spell.
    /// Some spells (e.g., Smite) are restricted to specific classes.
    /// </summary>
    public bool CanCast(Spell spell) =>
        RemainingCasts > 0
        && (!SpellClassRestrictions.TryGetValue(spell.Name, out var allowed)
        || allowed.Contains(ClassName ?? string.Empty));

    public int ComputeSpellTurnMeterCost(Spell spell)
    {
        var intMod = (Intelligence - 10) / 2;
        var reduction = intMod * _spellTmCostIntPctPerMod
                      + Level * _spellTmCostLevelPct
                      + Equipment.TotalTurnMeterCostReduction;
        return Math.Max(_minSpellTmCostPct, spell.TurnMeterCost - reduction);
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

        if (Subrace is not null)
            foreach (var feat in Subrace.Feats)
                foreach (var r in feat.Resistances)
                    if (r.Type == type) total += r.Value;

        total += Equipment.TotalResistance(type);

        foreach (var effect in ActiveStatusEffects)
            foreach (var r in effect.ResistanceBonuses)
                if (r.Type == type) total += r.Value;

        return Math.Clamp(total, 0, 95);
    }
}
