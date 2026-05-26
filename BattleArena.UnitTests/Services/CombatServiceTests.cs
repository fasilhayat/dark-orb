using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;
using NSubstitute;

namespace BattleArena.UnitTests.Services;

public class CombatServiceTests
{
    private readonly IDiceService _dice = Substitute.For<IDiceService>();
    private readonly CombatService _sut;

    public CombatServiceTests()
    {
        _sut = new CombatService(_dice);
    }

    [Fact]
    public void CalculateAbilityModifier_With10_Returns0()
    {
        var result = _sut.CalculateAbilityModifier(10);

        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(8, -1)]
    [InlineData(10, 0)]
    [InlineData(12, 1)]
    [InlineData(14, 2)]
    [InlineData(18, 4)]
    [InlineData(20, 5)]
    public void CalculateAbilityModifier_VariousScores_ReturnsCorrectModifier(int score, int expected)
    {
        var result = _sut.CalculateAbilityModifier(score);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ResolveAttack_WhenHitRollMeetsAC_ReturnsHit()
    {
        var attacker = new Character
        {
            Strength = 14,
            StrikeRating = 19
        };
        var weapon = new Weapon
        {
            Name = "Longsword",
            DamageDie = DieType.D8,
            DamageType = DamageType.Slashing
        };

        _dice.Roll(DieType.D20).Returns(12);
        _dice.Roll(DieType.D8).Returns(5);

        // Use a higher target armor class so the low roll results in a miss
        var result = _sut.ResolveAttack(attacker, 10, weapon);

        Assert.True(result.IsHit);
        Assert.Equal(12, result.HitRoll);
        Assert.Equal(7, result.Damage); // 5 + 2 (STR mod)
    }

    [Fact]
    public void ResolveAttack_WhenHitRollBelowAC_ReturnsMiss()
    {
        var attacker = new Character
        {
            Strength = 10,
            StrikeRating = 19
        };
        var weapon = new Weapon
        {
            Name = "Dagger",
            DamageDie = DieType.D4
        };

        _dice.Roll(DieType.D20).Returns(5);

        // Use AC 10 so that a roll of 5 + mod 0 = 5 is less than AC 10
        var result = _sut.ResolveAttack(attacker, 10, weapon);

        Assert.False(result.IsHit);
        Assert.Equal(0, result.Damage);
    }

    [Fact]
    public void ResolveAttack_DamageCannotGoBelowZero()
    {
        var attacker = new Character
        {
            Strength = 6,
            StrikeRating = 19
        };
        var weapon = new Weapon
        {
            Name = "Dagger",
            DamageDie = DieType.D4
        };

        _dice.Roll(DieType.D20).Returns(10);
        _dice.Roll(DieType.D4).Returns(1);

        var result = _sut.ResolveAttack(attacker, 5, weapon);

        Assert.True(result.IsHit);
        Assert.Equal(0, result.Damage); // 1 + (-2) = -1, clamped to 0
    }

    [Fact]
    public void ResolveAttack_NaturalTwenty_IsCriticalHit()
    {
        var attacker = new Character { Strength = 10 };
        var weapon = new Weapon { Name = "Longsword", DamageDie = DieType.D8 };

        _dice.Roll(DieType.D20).Returns(20);
        _dice.Roll(DieType.D8).Returns(6);

        var result = _sut.ResolveAttack(attacker, 99, weapon);

        Assert.True(result.IsHit);
        Assert.True(result.IsCriticalHit);
        Assert.False(result.IsFumble);
        Assert.Equal(12, result.Damage); // (6 + 0) * 2 = 12
    }

    [Fact]
    public void ResolveAttack_NaturalTwenty_CriticalDamageDoubled()
    {
        var attacker = new Character { Strength = 14 }; // +2 STR mod
        var weapon = new Weapon { Name = "Greatsword", DamageDie = DieType.D6 };

        _dice.Roll(DieType.D20).Returns(20);
        _dice.Roll(DieType.D6).Returns(4);

        var result = _sut.ResolveAttack(attacker, 99, weapon);

        Assert.True(result.IsCriticalHit);
        Assert.Equal(12, result.Damage); // (4 + 2) * 2 = 12
    }

    [Fact]
    public void ResolveAttack_NaturalOne_IsFumble()
    {
        var attacker = new Character { Strength = 18 }; // strong, but still fumbles
        var weapon = new Weapon { Name = "Battleaxe", DamageDie = DieType.D8 };

        _dice.Roll(DieType.D20).Returns(1);

        var result = _sut.ResolveAttack(attacker, 1, weapon);

        Assert.False(result.IsHit);
        Assert.True(result.IsFumble);
        Assert.False(result.IsCriticalHit);
        Assert.Equal(0, result.Damage);
    }

    [Fact]
    public void ResolveAttack_NaturalOne_AppliesAttackPowerPenalty()
    {
        var attacker = new Character { Strength = 10 };
        var weapon = new Weapon { Name = "Dagger", DamageDie = DieType.D4 };

        _dice.Roll(DieType.D20).Returns(1);

        var result = _sut.ResolveAttack(attacker, 1, weapon);

        Assert.Equal(-2, result.AttackPowerPenalty);
    }

    [Fact]
    public void ResolveAttack_LowerStrikeRatingIsBetter_MoreLikelyToHit()
    {
        var attacker = new Character
        {
            Strength = 16,
            StrikeRating = 15
        };
        var weapon = new Weapon
        {
            Name = "Greatsword",
            DamageDie = DieType.D6,
            DamageType = DamageType.Slashing
        };

        _dice.Roll(DieType.D20).Returns(8);
        _dice.Roll(DieType.D6).Returns(6);

        var result = _sut.ResolveAttack(attacker, 5, weapon);

        // 8 + 3 (STR) = 11 >= 15 - 5 = 10
        Assert.True(result.IsHit);
    }

    [Fact]
    public void RollDamage_ReturnsDieRollResult()
    {
        var weapon = new Weapon
        {
            Name = "Battleaxe",
            DamageDie = DieType.D8
        };

        _dice.Roll(DieType.D8).Returns(7);

        var result = _sut.RollDamage(weapon);

        Assert.Equal(DieType.D8, result.DieType);
        Assert.Equal(7, result.Result);
    }
}
