namespace BattleArena.ReqnrollTests.StepDefinitions;

using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;
using NSubstitute;
using Reqnroll;
using Xunit;

[Binding]
public class CombatSteps
{
    private readonly IDiceService _dice;
    private readonly ICombatService _combat;
    private Character _character = new();
    private Weapon _weapon = new();
    private AttackResult? _attackResult;
    private DamageRollResult? _damageResult;
    private int _modifierResult;

    public CombatSteps()
    {
        _dice = Substitute.For<IDiceService>();
        _combat = new CombatService(_dice);
    }

    [Given(@"a character with strength (\d+) and strike rating (\d+)")]
    public void GivenACharacterWithStrengthAndStrikeRating(int strength, int strikeRating)
    {
        _character = new Character
        {
            Strength = strength,
            StrikeRating = strikeRating
        };
    }

    [Given(@"a weapon named ""([^""]+)"" with (\w+) damage die and \+(\d+) attack bonus")]
    public void GivenAWeaponWithDamageDieAndAttackBonus(string name, string dieName, int attackBonus)
    {
        _weapon = new Weapon
        {
            Name = name,
            DamageDie = ParseDieType(dieName),
            AttackBonus = attackBonus
        };
    }

    [Given(@"the D20 roll is (\d+)")]
    public void GivenTheD20RollIs(int roll)
    {
        _dice.Roll(DieType.D20).Returns(roll);
    }

    [Given(@"the damage die roll is (\d+)")]
    public void GivenTheDamageDieRollIs(int roll)
    {
        // Only return this value for damage dice (not the D20 used for hit rolls).
        _dice.Roll(Arg.Is<DieType>(d => d != DieType.D20)).Returns(roll);
    }

    [When(@"the ability modifier is calculated for score (\d+)")]
    public void WhenTheAbilityModifierIsCalculated(int score)
    {
        _modifierResult = _combat.CalculateAbilityModifier(score);
    }

    [When(@"the character attacks a target with armor class (\d+)")]
    public void WhenTheCharacterAttacksATargetWithArmorClass(int armorClass)
    {
        _attackResult = _combat.ResolveAttack(_character, armorClass, _weapon);
    }

    [When(@"the character rolls damage for the weapon")]
    public void WhenTheCharacterRollsDamageForTheWeapon()
    {
        _damageResult = _combat.RollDamage(_weapon);
    }

    [Then(@"the modifier should be (-?\d+)")]
    public void ThenTheModifierShouldBe(int expected)
    {
        Assert.Equal(expected, _modifierResult);
    }

    [Then(@"the attack should hit")]
    public void ThenTheAttackShouldHit()
    {
        Assert.NotNull(_attackResult);
        Assert.True(_attackResult.IsHit);
    }

    [Then(@"the attack should miss")]
    public void ThenTheAttackShouldMiss()
    {
        Assert.NotNull(_attackResult);
        Assert.False(_attackResult.IsHit);
    }

    [Then(@"the hit roll should be (\d+)")]
    public void ThenTheHitRollShouldBe(int expected)
    {
        Assert.NotNull(_attackResult);
        Assert.Equal(expected, _attackResult.HitRoll);
    }

    [Then(@"the damage should be (\d+)")]
    public void ThenTheDamageShouldBe(int expected)
    {
        Assert.NotNull(_attackResult);
        Assert.Equal(expected, _attackResult.Damage);
    }

    [Then(@"the weapon used should be ""([^""]+)""")]
    public void ThenTheWeaponUsedShouldBe(string expected)
    {
        Assert.NotNull(_attackResult);
        Assert.Equal(expected, _attackResult.WeaponName);
    }

    [Then(@"the damage result should be (\d+)")]
    public void ThenTheDamageResultShouldBe(int expected)
    {
        Assert.NotNull(_damageResult);
        Assert.Equal(expected, _damageResult.Result);
    }

    [Then(@"the damage die type should be (\w+)")]
    public void ThenTheDamageDieTypeShouldBe(string dieName)
    {
        Assert.NotNull(_damageResult);
        Assert.Equal(ParseDieType(dieName), _damageResult.DieType);
    }

    private static DieType ParseDieType(string name) => name switch
    {
        "D4" => DieType.D4,
        "D6" => DieType.D6,
        "D8" => DieType.D8,
        "D10" => DieType.D10,
        "D12" => DieType.D12,
        "D20" => DieType.D20,
        "D100" => DieType.D100,
        _ => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown die type: {name}")
    };
}
