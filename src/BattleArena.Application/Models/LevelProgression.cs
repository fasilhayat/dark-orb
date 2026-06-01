using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Models;

public static class LevelProgression
{
    public const int MaxLevel = 12;

    public static readonly int[] XpThresholds =
        [0, 100, 300, 650, 1150, 1850, 2750, 3900, 5300, 7000, 9100, 11600];

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

    public static int SrBonus(int level, ClassArchetype archetype)
    {
        var reduction = archetype switch
        {
            ClassArchetype.Martial => (level - 1) / 2,
            ClassArchetype.Hybrid => (level - 1) / 3,
            ClassArchetype.Caster => (level - 1) / 4,
            _ => 0
        };
        return Math.Min(reduction, 6);
    }

    public static int AccessorySlots(int level, ClassArchetype archetype)
    {
        var common = level switch
        {
            >= 12 => 4,
            >= 9 => 3,
            >= 6 => 2,
            >= 3 => 1,
            _ => 0
        };
        var bonus = archetype switch
        {
            ClassArchetype.Caster => level switch
            {
                >= 11 => 2,
                >= 8 => 1,
                >= 4 => 1,
                _ => 0
            },
            ClassArchetype.Hybrid => level >= 10 ? 1 : 0,
            _ => 0
        };
        return common + bonus;
    }

    public static int XpForLevel(int level) =>
        level < 1 ? 0 :
        level >= 12 ? XpThresholds[^1] :
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
