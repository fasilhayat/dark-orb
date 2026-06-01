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
        "Barbarian" or "Fighter" or "Knight" or "Paladin" => ClassArchetype.Martial,
        "Mage" or "Priest" or "Druid" => ClassArchetype.Caster,
        "Rogue" or "Bard" => ClassArchetype.Hybrid,
        _ => ClassArchetype.Martial
    };

    public static ClassArchetype Archetype(int classId) => classId switch
    {
        1 or 2 or 3 or 8 => ClassArchetype.Martial,
        4 or 5 or 7 => ClassArchetype.Caster,
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

    /// <summary>Hit die sides for each class (indexed by ClassId 1-9).</summary>
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
}
