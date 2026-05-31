namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;

public class CombatServiceTests
{
    private readonly IDiceService _dice = Substitute.For<IDiceService>();
    private readonly CombatService _sut;

    public CombatServiceTests()
    {
        _sut = new CombatService(_dice, new CombatStatsService());
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
    public void ResolveAttack_WhenHitRollMeetsDefensePower_ReturnsHit()
    {
        var attacker = new Character
        {
            Strength = 14,
            StrikeRating = 19
        };
        var defender = CreateDefender(10);
        var weapon = new Weapon
        {
            Name = "Longsword",
            DamageDie = DieType.D8,
            DamageType = DamageType.Slashing
        };

        _dice.Roll(DieType.D20).Returns(12);
        _dice.Roll(DieType.D8).Returns(5);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsHit);
        Assert.Equal(12, result.HitRoll);
        Assert.Equal(4, result.AttackPower);
        Assert.Equal(11, result.DefensePower);
        Assert.Equal(9, result.Damage);
    }

    [Fact]
    public void ResolveAttack_WhenHitRollBelowDefensePower_ReturnsMiss()
    {
        var attacker = new Character
        {
            Strength = 10,
            StrikeRating = 19
        };
        var defender = CreateDefender(10);
        var weapon = new Weapon
        {
            Name = "Dagger",
            DamageDie = DieType.D4
        };

        _dice.Roll(DieType.D20).Returns(5);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.False(result.IsHit);
        Assert.Equal(0, result.Damage);
        Assert.Null(result.DamageContext);
    }

    [Fact]
    public void ResolveAttack_DamageCannotGoBelowZero()
    {
        var attacker = new Character
        {
            Strength = 6,
            StrikeRating = 19
        };
        var defender = CreateDefender(5);
        var weapon = new Weapon
        {
            Name = "Dagger",
            DamageDie = DieType.D4
        };

        _dice.Roll(DieType.D20).Returns(10);
        _dice.Roll(DieType.D4).Returns(1);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsHit);
        Assert.Equal(1, result.Damage);
        Assert.NotNull(result.DamageContext);
        Assert.Equal(1, result.DamageContext!.BaseDamage);
        Assert.Equal(1, result.DamageContext.FinalDamage);
    }

    [Fact]
    public void ResolveAttack_NaturalTwenty_IsCriticalHit()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(99);
        var weapon = new Weapon { Name = "Longsword", DamageDie = DieType.D8 };

        _dice.Roll(DieType.D20).Returns(20);
        _dice.Roll(DieType.D8).Returns(6);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsHit);
        Assert.True(result.IsCriticalHit);
        Assert.False(result.IsFumble);
        Assert.Equal(16, result.Damage);
    }

    [Fact]
    public void ResolveAttack_NaturalTwenty_CriticalDamageDoubled()
    {
        var attacker = new Character { Strength = 14 };
        var defender = CreateDefender(99);
        var weapon = new Weapon { Name = "Greatsword", DamageDie = DieType.D6 };

        _dice.Roll(DieType.D20).Returns(20);
        _dice.Roll(DieType.D6).Returns(4);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsCriticalHit);
        Assert.Equal(16, result.Damage);
    }

    [Fact]
    public void ResolveAttack_NaturalOne_IsFumble()
    {
        var attacker = new Character { Strength = 18 };
        var defender = CreateDefender(1);
        var weapon = new Weapon { Name = "Battleaxe", DamageDie = DieType.D8 };

        _dice.Roll(DieType.D20).Returns(1);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.False(result.IsHit);
        Assert.True(result.IsFumble);
        Assert.False(result.IsCriticalHit);
        Assert.Equal(0, result.Damage);
    }

    [Fact]
    public void ResolveAttack_NaturalOne_AppliesAttackPowerPenalty()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(1);
        var weapon = new Weapon { Name = "Dagger", DamageDie = DieType.D4 };

        _dice.Roll(DieType.D20).Returns(1);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

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
        var defender = CreateDefender(5);
        var weapon = new Weapon
        {
            Name = "Greatsword",
            DamageDie = DieType.D6,
            DamageType = DamageType.Slashing
        };

        _dice.Roll(DieType.D20).Returns(8);
        _dice.Roll(DieType.D6).Returns(6);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsHit);
        Assert.Equal(9, result.AttackPower);
    }

    [Fact]
    public void ResolveDamage_AppliesVulnerabilityMitigationAndElementalDamage()
    {
        var attacker = new Character { Strength = 14 };
        var defender = CreateDefender(10);
        defender.Vulnerabilities.Add(DamageType.Slashing);
        defender.Equipment.Head = new Armor { Mitigation = 2 };
        var weapon = new Weapon
        {
            Name = "Flameblade",
            DamageDie = DieType.D8,
            DamageType = DamageType.Slashing,
            FlatDamageBonus = 1,
            ElementalDamage = 3
        };

        _dice.Roll(DieType.D8).Returns(5);

        var result = _sut.ResolveDamage(attacker, defender, weapon);

        Assert.Equal(5, result.WeaponDiceRoll);
        Assert.Equal(2, result.AttributeModifier);
        Assert.Equal(1, result.FlatBonuses);
        Assert.Equal(10, result.BaseDamage);
        Assert.Equal(1.5f, result.TypeMultiplier);
        Assert.Equal(2, result.ArmorMitigation);
        Assert.Equal(3, result.ElementalModifiers);
        Assert.Equal(16, result.FinalDamage);
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

    private static Character CreateDefender(int targetArmorClass)
    {
        return new Character
        {
            Equipment = new ArmorSlots
            {
                Chest = new Armor
                {
                    ArmorClass = Math.Max(0, 20 - targetArmorClass)
                }
            }
        };
    }
}
