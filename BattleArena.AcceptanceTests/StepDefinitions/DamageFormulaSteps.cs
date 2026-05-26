namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;
using Reqnroll;
using Xunit;

/// <summary>
/// Step definitions for the DamageFormula.feature.
/// Exercises ResolveDamage directly, bypassing the d20 hit check, to isolate
/// the §9 damage pipeline: BaseDamage × TypeMultiplier - ArmorMitigation + ElementalDamage.
/// </summary>
[Binding]
public class DamageFormulaSteps
{
    private readonly IDiceService _dice = Substitute.For<IDiceService>();
    private readonly ICombatService _combat;
    private Character _attacker = new();
    private Character _defender = new();
    private Weapon _weapon = new() { DamageDie = DieType.D8, DamageType = DamageType.Slashing };
    private DamageContext? _result;

    public DamageFormulaSteps()
    {
        _combat = new CombatService(_dice, new CombatStatsService());
    }

    [Given(@"a damage formula attacker with strength (\d+)")]
    public void GivenADamageFormulaAttackerWithStrength(int strength)
    {
        _attacker = new Character { Strength = strength };
    }

    [Given(@"a (\w+) damage weapon with (\w+) die")]
    public void GivenADamageTypeWeaponWithDie(string damageType, string dieName)
    {
        _weapon = new Weapon
        {
            DamageDie = ParseDieType(dieName),
            DamageType = Enum.Parse<DamageType>(damageType),
            AttackType = AttackType.Melee
        };
    }

    [Given(@"the weapon has a flat damage bonus of (\d+)")]
    public void GivenTheWeaponHasAFlatDamageBonusOf(int bonus)
    {
        _weapon.FlatDamageBonus = bonus;
    }

    [Given(@"the weapon deals (\d+) elemental bonus damage")]
    public void GivenTheWeaponDealsElementalBonusDamage(int elemental)
    {
        _weapon.ElementalDamage = elemental;
    }

    [Given(@"a damage formula target with no modifiers")]
    public void GivenADamageFormulaTargetWithNoModifiers()
    {
        _defender = new Character();
    }

    [Given(@"a damage formula target with armor mitigation of (\d+)")]
    public void GivenADamageFormulaTargetWithArmorMitigationOf(int mitigation)
    {
        _defender = new Character
        {
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Mitigation = mitigation }
            }
        };
    }

    [Given(@"the target is vulnerable to (\w+) damage")]
    public void GivenTheTargetIsVulnerableTo(string damageType)
    {
        _defender.Vulnerabilities.Add(Enum.Parse<DamageType>(damageType));
    }

    [Given(@"the weapon damage die rolls (\d+)")]
    public void GivenTheWeaponDamageDieRolls(int roll)
    {
        _dice.Roll(Arg.Is<DieType>(d => d != DieType.D20)).Returns(roll);
    }

    [When(@"damage is resolved against the target")]
    public void WhenDamageIsResolvedAgainstTheTarget()
    {
        _result = _combat.ResolveDamage(_attacker, _defender, _weapon);
    }

    [Then(@"the base damage should be (\d+)")]
    public void ThenTheBaseDamageShouldBe(int expected)
    {
        Assert.NotNull(_result);
        Assert.Equal(expected, _result.BaseDamage);
    }

    [Then(@"the final damage should be (\d+)")]
    public void ThenTheFinalDamageShouldBe(int expected)
    {
        Assert.NotNull(_result);
        Assert.Equal(expected, _result.FinalDamage);
    }

    private static DieType ParseDieType(string name) => name switch
    {
        "D4" => DieType.D4,
        "D6" => DieType.D6,
        "D8" => DieType.D8,
        "D10" => DieType.D10,
        "D12" => DieType.D12,
        "D20" => DieType.D20,
        _ => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown die type: {name}")
    };
}
