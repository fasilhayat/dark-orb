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
}
