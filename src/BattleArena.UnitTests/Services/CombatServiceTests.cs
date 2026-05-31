namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Modifiers;
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
        _sut = new CombatService(_dice, new CombatStatsService(), [new RangeModifier()]);
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
            StrikeRating = 10
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
        Assert.Equal(13, result.AttackPower);
        Assert.Equal(11, result.DefensePower);
        Assert.Equal(9, result.Damage);
    }

    [Fact]
    public void ResolveAttack_WhenHitRollBelowDefensePower_ReturnsMiss()
    {
        var attacker = new Character
        {
            Strength = 10,
            StrikeRating = 4
        };
        var defender = CreateDefender(15);
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

        // Returns: atk=20, def=5 (sequential) → Priority 5: Critical
        _dice.Roll(DieType.D20).Returns(20, 5);
        _dice.Roll(DieType.D8).Returns(6);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsHit);
        Assert.True(result.IsCriticalHit);
        Assert.False(result.IsFumble);
        Assert.False(result.IsClash);
        Assert.Equal(16, result.Damage);
    }

    [Fact]
    public void ResolveAttack_NaturalTwenty_CriticalDamageDoubled()
    {
        var attacker = new Character { Strength = 14 };
        var defender = CreateDefender(99);
        var weapon = new Weapon { Name = "Greatsword", DamageDie = DieType.D6 };

        // Returns: atk=20, def=5 → Priority 5: Critical
        _dice.Roll(DieType.D20).Returns(20, 5);
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

        // atk=1, def=10 → Priority 4: Fumble (not TotalReversal because def != 20)
        _dice.Roll(DieType.D20).Returns(1, 10);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.False(result.IsHit);
        Assert.True(result.IsFumble);
        Assert.False(result.IsCriticalHit);
        Assert.False(result.IsTotalReversal);
        Assert.Equal(0, result.Damage);
    }

    [Fact]
    public void ResolveAttack_NaturalOne_AppliesAttackPowerPenalty()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(1);
        var weapon = new Weapon { Name = "Dagger", DamageDie = DieType.D4 };

        _dice.Roll(DieType.D20).Returns(1, 10);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.Equal(-2, result.AttackPowerPenalty);
    }

    [Fact]
    public void ResolveAttack_HigherStrikeRating_MoreLikelyToHit()
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
        Assert.Equal(19, result.AttackPower);
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

    // ── 7-Case priority matrix tests ────────────────────────────────────────

    [Fact]
    public void ResolveAttack_Atk1Def20_IsTotalReversal()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(5);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D8 };

        _dice.Roll(DieType.D20).Returns(1, 20);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.False(result.IsHit);
        Assert.True(result.IsFumble);
        Assert.True(result.IsTotalReversal);
        Assert.Equal(-4, result.AttackPowerPenalty);
        Assert.Equal(0, result.Damage);
        Assert.True(result.DefenderTmBonus > 0);
    }

    [Fact]
    public void ResolveAttack_Atk20Def1_IsDevastatingStrike()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(5);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D8 };

        _dice.Roll(DieType.D20).Returns(20, 1);
        _dice.Roll(DieType.D8).Returns(4);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsHit);
        Assert.True(result.IsDevastatingStrike);
        Assert.False(result.IsFumble);
        Assert.False(result.IsClash);
        Assert.True(result.Damage > 0);
    }

    [Fact]
    public void ResolveAttack_DevastatingStrike_DamageIsTripleBase()
    {
        var attacker = new Character { Strength = 10, Level = 0 }; // Level=0 removes level scaling for predictability
        var defender = CreateDefender(0);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D8, DamageType = DamageType.Slashing };

        _dice.Roll(DieType.D20).Returns(20, 1);
        _dice.Roll(DieType.D8).Returns(4);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        // BaseDamage = roll(4) + STR mod(0) + flat(0) + level(0) = 4; ×3 × 1.0 − 0 = 12
        Assert.Equal(12, result.Damage);
    }

    [Fact]
    public void ResolveAttack_BothRoll20_IsClash()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(5);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D8 };

        _dice.Roll(DieType.D20).Returns(20, 20);
        _dice.Roll(DieType.D8).Returns(6);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsHit);
        Assert.True(result.IsClash);
        Assert.False(result.IsCriticalHit);       // Clash is NOT a crit
        Assert.False(result.IsDevastatingStrike); // Clash is NOT devastating
    }

    [Fact]
    public void ResolveAttack_Clash_DamageIsHalfNormal()
    {
        var attacker = new Character { Strength = 10, Level = 0 }; // Level=0 removes level scaling for predictability
        var defender = CreateDefender(0);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D8, DamageType = DamageType.Slashing };

        _dice.Roll(DieType.D20).Returns(20, 20);
        _dice.Roll(DieType.D8).Returns(6);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        // BaseDamage=6, FinalDamage=6, /2 = 3
        Assert.Equal(3, result.Damage);
    }

    [Fact]
    public void ResolveAttack_AnyRoll1_Def2to19_IsFumbleNotTotalReversal()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(5);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D4 };

        _dice.Roll(DieType.D20).Returns(1, 15);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsFumble);
        Assert.False(result.IsTotalReversal);
        Assert.Equal(-2, result.AttackPowerPenalty);
    }

    [Fact]
    public void ResolveAttack_Atk20_Def2to19_IsCriticalNotClash()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(5);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D6 };

        _dice.Roll(DieType.D20).Returns(20, 10);
        _dice.Roll(DieType.D6).Returns(3);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.True(result.IsCriticalHit);
        Assert.False(result.IsClash);
        Assert.False(result.IsDevastatingStrike);
    }

    [Fact]
    public void ResolveAttack_Def20_Atk2to19_IsPerfectParry()
    {
        var attacker = new Character { Strength = 10 };
        var defender = CreateDefender(5);
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D6 };

        _dice.Roll(DieType.D20).Returns(10, 20);

        var result = _sut.ResolveAttack(attacker, defender, weapon);

        Assert.False(result.IsHit);
        Assert.True(result.IsPerfectParry);
        Assert.Equal(0, result.Damage);
        Assert.True(result.DefenderTmBonus > 0);
    }

    // ── Regression: exclusive outcome flags ─────────────────────────────────

    [Fact]
    public void ResolveAttack_TotalReversal_IsNotCritAndNotClash()
    {
        var attacker = new Character { Strength = 10 };
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D6 };
        _dice.Roll(DieType.D20).Returns(1, 20);

        var result = _sut.ResolveAttack(attacker, CreateDefender(5), weapon);

        Assert.False(result.IsCriticalHit);
        Assert.False(result.IsClash);
        Assert.False(result.IsPerfectParry);
    }

    [Fact]
    public void ResolveAttack_DevastatingStrike_IsNotFumbleAndNotClash()
    {
        var attacker = new Character { Strength = 10 };
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D6 };
        _dice.Roll(DieType.D20).Returns(20, 1);
        _dice.Roll(DieType.D6).Returns(3);

        var result = _sut.ResolveAttack(attacker, CreateDefender(5), weapon);

        Assert.False(result.IsFumble);
        Assert.False(result.IsClash);
        Assert.False(result.IsCriticalHit);
    }

    [Fact]
    public void ResolveAttack_PerfectParry_AttackPowerPenaltyIsZero()
    {
        var attacker = new Character { Strength = 10 };
        var weapon   = new Weapon { Name = "Sword", DamageDie = DieType.D6 };
        _dice.Roll(DieType.D20).Returns(10, 20);

        var result = _sut.ResolveAttack(attacker, CreateDefender(5), weapon);

        Assert.Equal(0, result.AttackPowerPenalty);
    }

    private static Character CreateDefender(int armorClass)
    {
        return new Character
        {
            Equipment = new ArmorSlots
            {
                Chest = new Armor
                {
                    ArmorClass = armorClass
                }
            }
        };
    }
}
