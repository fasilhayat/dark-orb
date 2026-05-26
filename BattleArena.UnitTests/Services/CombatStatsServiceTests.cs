namespace BattleArena.UnitTests.Services;

using Application.Services;
using Core.Entities;
using Core.Entities.Enums;

public class CombatStatsServiceTests
{
    private readonly CombatStatsService _sut = new();

    [Fact]
    public void ComputeAttackerStats_IncludesWeaponFeatRaceAndStatusBonuses()
    {
        var attacker = new Character
        {
            Level = 3,
            Strength = 16,
            Dexterity = 12,
            StrikeRating = 17,
            Feats = new List<Feat> { new() { AttackBonus = 2 } },
            Race = new Race
            {
                Feats = new List<Feat> { new() { AttackBonus = 1 } }
            },
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { Name = "Battle Cry", AttackPowerModifier = 3, StackRule = StackRule.Stack },
                new() { Name = "Bless", AttackPowerModifier = 4, StackRule = StackRule.HighestWins },
                new() { Name = "Focus", AttackPowerModifier = 2, StackRule = StackRule.HighestWins },
                new() { Name = "Curse", AttackPowerModifier = -2, StackRule = StackRule.NoStack }
            }
        };
        var weapon = new Weapon
        {
            AttackType = AttackType.Melee,
            AttackBonus = 2
        };

        var result = _sut.ComputeAttackerStats(attacker, weapon);

        Assert.Equal(3, result.ClassAccuracyBase);
        Assert.Equal(3, result.LevelScaling);
        Assert.Equal(3, result.AttributeModifier);
        Assert.Equal(2, result.WeaponAttackBonus);
        Assert.Equal(2, result.SkillModifiers);
        Assert.Equal(5, result.BuffModifiers);
        Assert.Equal(1, result.RacialModifiers);
        Assert.Equal(19, result.AttackPower);
    }

    [Fact]
    public void ComputeDefenderStats_AppliesArmorDexCapShieldBuffsAndDefenseBonuses()
    {
        var defender = new Character
        {
            Dexterity = 18,
            Feats = new List<Feat> { new() { DefenseBonus = 1 } },
            Race = new Race
            {
                Feats = new List<Feat> { new() { DefenseBonus = 2 } }
            },
            Equipment = new ArmorSlots
            {
                Chest = new Armor { ArmorClass = 6, MaxDexterityBonus = 2 },
                Head = new Armor { ArmorClass = 2, MaxDexterityBonus = 1 },
                Shield = new Shield { DefenseBonus = 3 }
            },
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { Name = "Ward I", Type = StatusEffectType.Buff, DefensePowerModifier = 4, StackRule = StackRule.HighestWins, Source = "spell-a" },
                new() { Name = "Ward II", Type = StatusEffectType.Buff, DefensePowerModifier = 2, StackRule = StackRule.HighestWins, Source = "spell-a" },
                new() { Name = "Barrier", Type = StatusEffectType.Buff, DefensePowerModifier = 1, StackRule = StackRule.HighestWins, Source = "spell-b" },
                new() { Name = "Expose", Type = StatusEffectType.Debuff, DefensePowerModifier = -2, StackRule = StackRule.Stack, Source = "enemy" }
            }
        };

        var result = _sut.ComputeDefenderStats(defender);

        Assert.Equal(12, result.EffectiveAC);
        Assert.Equal(3, result.DexterityModifier);
        Assert.Equal(3, result.ShieldBonus);
        Assert.Equal(3, result.DefensiveBuffs);
        Assert.Equal(3, result.DefenseRacialModifiers);
        Assert.Equal(24, result.DefensePower);
    }
}
