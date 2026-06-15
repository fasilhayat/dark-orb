using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Models;

public static class LevelProgression
{
    public const int MaxLevel = 20;

    // D&D 5e official XP thresholds (levels 1–20).
    public static readonly int[] XpThresholds =
        [0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000,
         85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000];

    public enum ClassArchetype
    {
        Martial,
        Caster,
        Hybrid
    }

    public static ClassArchetype Archetype(string className) => className switch
    {
        "Barbarian" or "Fighter" or "Knight" or "Paladin" or "Ranger" => ClassArchetype.Martial,
        "Mage" or "Priest" or "Druid" or "Tempest" => ClassArchetype.Caster,
        "Rogue" or "Bard" => ClassArchetype.Hybrid,
        _ => ClassArchetype.Martial
    };

    public static ClassArchetype Archetype(int classId) => classId switch
    {
        1 or 2 or 3 or 8 or 10 => ClassArchetype.Martial,
        4 or 5 or 7 or 11 => ClassArchetype.Caster,
        6 or 9 => ClassArchetype.Hybrid,
        _ => ClassArchetype.Martial
    };

    // SR increases with level — martial classes gain fastest (modern D&D: higher SR = better attacker).
    public static int SrLevelGain(int level, ClassArchetype archetype)
    {
        var gain = archetype switch
        {
            ClassArchetype.Martial => (level - 1) / 2,
            ClassArchetype.Hybrid => (level - 1) / 3,
            ClassArchetype.Caster => (level - 1) / 4,
            _ => 0
        };
        // Per-archetype SR caps at level 20: Martial=9, Hybrid=6, Caster=4.
        var cap = archetype switch
        {
            ClassArchetype.Martial => 9,
            ClassArchetype.Hybrid  => 6,
            ClassArchetype.Caster  => 4,
            _ => 6
        };
        return Math.Min(gain, cap);
    }

    public static int AccessorySlots(int level, ClassArchetype archetype)
    {
        var common = level switch
        {
            >= 18 => 6,
            >= 15 => 5,
            >= 12 => 4,
            >= 9  => 3,
            >= 6  => 2,
            >= 3  => 1,
            _ => 0
        };
        var bonus = archetype switch
        {
            ClassArchetype.Caster => level switch
            {
                >= 18 => 3,
                >= 11 => 2,
                >= 8  => 1,
                >= 4  => 1,
                _ => 0
            },
            ClassArchetype.Hybrid => level switch
            {
                >= 16 => 2,
                >= 10 => 1,
                _ => 0
            },
            _ => 0
        };
        return common + bonus;
    }

    public static int XpForLevel(int level) =>
        level < 1 ? 0 :
        level >= 20 ? XpThresholds[^1] :
        XpThresholds[level - 1];

    public static int XpToNextLevel(int currentLevel) =>
        currentLevel < 1 ? XpThresholds[0] :
        currentLevel >= MaxLevel ? 0 :
        XpThresholds[currentLevel] - XpThresholds[currentLevel - 1];

    public static int LevelFromXp(int totalXp)
    {
        for (var i = XpThresholds.Length - 1; i >= 0; i--)
            if (totalXp >= XpThresholds[i])
                return i + 1;
        return 1;
    }

    /// <summary>Hit die sides for each class (indexed by ClassId 1-11).</summary>
    public static int HitDieSides(int classId) => classId switch
    {
        1 => 12,  // Barbarian (unchanged)
        2 => 10,  // Knight (unchanged)
        3 => 10,  // Paladin (unchanged)
        4 => 10,  // Priest (was d8)
        5 => 6,   // Mage (was d4)
        6 => 8,   // Bard (was d6)
        7 => 10,  // Druid (was d8)
        8 => 10,  // Fighter (unchanged)
        9 => 8,   // Rogue (was d6)
        11 => 10, // Tempest (battle-priest, d10)
        _ => 8
    };

    /// <summary>Turnmeter bonus from level, scaled by archetype.</summary>
    public static int TurnMeterLevelBonus(int level, ClassArchetype archetype) => archetype switch
    {
        ClassArchetype.Martial => level / 3,
        ClassArchetype.Hybrid  => level / 4,
        ClassArchetype.Caster  => level / 5,
        _ => 0
    };

    /// <summary>Spell memorization slots based on Intelligence + equipment bonus.</summary>
    public static int SpellMemorizationSlots(int intelligence, int equipmentBonus = 0)
    {
        var mod = (intelligence - 10) / 2;
        return Math.Max(1, 2 + mod + equipmentBonus);
    }

    /// <summary>Computes expected MaxHp for a character at the given level, using average (not rolled) HD values.</summary>
    public static int ComputeExpectedMaxHp(int level, int classId, int stamina, int raceHitPointBonus = 0)
    {
        var staminaMod = (stamina - 10) / 2;
        var hitDieSides = HitDieSides(classId);
        var hp = Math.Max(1, hitDieSides + staminaMod + raceHitPointBonus);
        if (level > 1)
        {
            var avgPerLevel = Math.Max(1, (hitDieSides + 1) / 2 + staminaMod + raceHitPointBonus);
            hp += (level - 1) * avgPerLevel;
        }
        return hp;
    }

    /// <summary>Converts hit die sides to DieType for rolling on level-up.</summary>
    public static DieType HitDieToDieType(int sides) => sides switch
    {
        4 => DieType.D4,
        6 => DieType.D6,
        8 => DieType.D8,
        10 => DieType.D10,
        12 => DieType.D12,
        _ => DieType.D8
    };

    // ── Class combat data (mirrors PlayerClass DB values) ─────────────────────

    /// <summary>Base attacks per turn for each class (indexed by ClassId).</summary>
    private static readonly int[] _attacksPerTurn =
        [1,  // 0 unused
         3,  // 1 Barbarian
         2,  // 2 Knight
         2,  // 3 Paladin
         1,  // 4 Priest
         1,  // 5 Mage
         1,  // 6 Bard
         1,  // 7 Druid
         2,  // 8 Fighter
         1,  // 9 Rogue
         2,  // 10 Ranger
         1,  // 11 Tempest
        ];

    /// <summary>Bow attacks per turn (0 = no special bonus).</summary>
    private static readonly int[] _bowAttacksPerTurn =
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // 0-9
         3,  // 10 Ranger (3 attacks with bow)
         0,  // 11 Tempest
        ];

    /// <summary>
    /// Armor restrictions: null = unrestricted, "Light" = light only.
    /// </summary>
    private static readonly string?[] _armorRestrictions =
        [null,
         "Light",   // 1 Barbarian
         null,      // 2 Knight
         null,      // 3 Paladin
         null,      // 4 Priest
         null,      // 5 Mage
         null,      // 6 Bard
         null,      // 7 Druid
         null,      // 8 Fighter
         null,      // 9 Rogue
         null,      // 10 Ranger
         null,      // 11 Tempest (unrestricted — the special exception)
        ];

    /// <summary>Whether each class can dual-wield.</summary>
    private static readonly bool[] _canDualWield =
        [false,
         false,  // 1 Barbarian (two-handers only)
         false,  // 2 Knight (shield specialist)
         false,  // 3 Paladin (two-handers or shield)
         false,  // 4 Priest
         false,  // 5 Mage
         false,  // 6 Bard
         false,  // 7 Druid
         true,   // 8 Fighter
         true,   // 9 Rogue (short sword + dagger or 2 daggers)
         true,   // 10 Ranger (2 shortswords or shortsword + dagger)
         false,  // 11 Tempest
        ];

    /// <summary>
    /// Weapon switch turnmeter cost multiplier.
    /// 0 = no cost, 1 = full cost, 0.5 = half cost.
    /// </summary>
    private static readonly double[] _weaponSwitchCostMultiplier =
        [1.0,
         0.0,  // 1 Barbarian (no cost — restricted to melee)
         0.5,  // 2 Knight (half cost)
         0.5,  // 3 Paladin (half cost)
         1.0,  // 4 Priest
         1.0,  // 5 Mage
         1.0,  // 6 Bard
         1.0,  // 7 Druid
         0.5,  // 8 Fighter (half cost)
         1.0,  // 9 Rogue
         0.0,  // 10 Ranger (no cost)
         1.0,  // 11 Tempest
        ];

    /// <summary>Two-handed weapon attack bonus by class.</summary>
    private static readonly int[] _twoHandedWeaponBonus =
        [0,
         2,  // 1 Barbarian (+2 bonus)
         0,  // 2 Knight
         2,  // 3 Paladin (+2 bonus for two-handed sword/warhammer)
         0,  // 4 Priest
         0,  // 5 Mage
         0,  // 6 Bard
         0,  // 7 Druid
         0,  // 8 Fighter
         0,  // 9 Rogue
         0,  // 10 Ranger
         2,  // 11 Tempest (battle-priest, mace/warhammer training)
        ];

    /// <summary>Shield bonus damage by class.</summary>
    private static readonly int[] _shieldBonusDamage =
        [0,
         0,  // 1 Barbarian
         2,  // 2 Knight (+2 bonus with shield)
         0,  // 3 Paladin
         0,  // 4 Priest
         0,  // 5 Mage
         0,  // 6 Bard
         0,  // 7 Druid
         0,  // 8 Fighter
         0,  // 9 Rogue
         0,  // 10 Ranger
         0,  // 11 Tempest
        ];

    /// <summary>Ranged attack bonus by class.</summary>
    private static readonly int[] _rangedAttackBonus =
        [0,
         0,  // 1 Barbarian
         0,  // 2 Knight
         0,  // 3 Paladin
         0,  // 4 Priest
         0,  // 5 Mage
         0,  // 6 Bard
         0,  // 7 Druid
         0,  // 8 Fighter
         0,  // 9 Rogue
         1,  // 10 Ranger (+1 ranged bonus)
        ];

    public static int AttacksPerTurn(int classId) =>
        classId >= 0 && classId < _attacksPerTurn.Length ? _attacksPerTurn[classId] : 1;

    public static int BowAttacksPerTurn(int classId) =>
        classId >= 0 && classId < _bowAttacksPerTurn.Length ? _bowAttacksPerTurn[classId] : 0;

    public static string? ArmorRestriction(int classId) =>
        classId >= 0 && classId < _armorRestrictions.Length ? _armorRestrictions[classId] : null;

    public static bool CanDualWield(int classId) =>
        classId >= 0 && classId < _canDualWield.Length && _canDualWield[classId];

    public static double WeaponSwitchCostMultiplier(int classId) =>
        classId >= 0 && classId < _weaponSwitchCostMultiplier.Length ? _weaponSwitchCostMultiplier[classId] : 1.0;

    public static int TwoHandedWeaponBonus(int classId) =>
        classId >= 0 && classId < _twoHandedWeaponBonus.Length ? _twoHandedWeaponBonus[classId] : 0;

    public static int ShieldBonusDamage(int classId) =>
        classId >= 0 && classId < _shieldBonusDamage.Length ? _shieldBonusDamage[classId] : 0;

    public static int RangedAttackBonus(int classId) =>
        classId >= 0 && classId < _rangedAttackBonus.Length ? _rangedAttackBonus[classId] : 0;

    /// <summary>Returns true if the given weapon archetype is a two-handed weapon.</summary>
    public static bool IsTwoHandedArchetype(ArchetypeWeapon archetype) =>
        archetype is ArchetypeWeapon.TwoHandedSword
            or ArchetypeWeapon.TwoHandedBattleAxe
            or ArchetypeWeapon.TwoHandedWarhammer;

    /// <summary>Returns true if the given weapon archetype is a ranged bow.</summary>
    public static bool IsBowArchetype(ArchetypeWeapon archetype) =>
        archetype is ArchetypeWeapon.Bow;
}
