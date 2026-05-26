namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

/// <summary>
/// Step definitions for the CombatStats.feature.
/// Exercises CombatStatsService directly to verify that AttackPower and
/// DefensePower are derived correctly from all character sources.
/// </summary>
[Binding]
public class CombatStatsSteps
{
    private readonly CombatStatsService _sut = new();
    private Character _attacker = new();
    private Character _defender = new();
    private Weapon _weapon = new();
    private CombatantStats? _result;

    [Given(@"an attacker at level (\d+) with strength (\d+) and strike rating (\d+)")]
    public void GivenAnAttackerAtLevelWithStrengthAndStrikeRating(int level, int strength, int strikeRating)
    {
        _attacker = new Character
        {
            Level = level,
            Strength = strength,
            StrikeRating = strikeRating
        };
    }

    [Given(@"the attacker has a combat feat granting \+(\d+) attack bonus")]
    public void GivenTheAttackerHasACombatFeatGrantingAttackBonus(int bonus)
    {
        _attacker.Feats.Add(new Feat { AttackBonus = bonus });
    }

    [Given(@"the attacker's race has a combat feat granting \+(\d+) attack bonus")]
    public void GivenTheAttackersRaceHasACombatFeatGrantingAttackBonus(int bonus)
    {
        _attacker.Race ??= new Race();
        _attacker.Race.Feats.Add(new Feat { AttackBonus = bonus });
    }

    [Given(@"the attacker uses a melee weapon with attack bonus (\d+)")]
    public void GivenTheAttackerUsesAMeleeWeaponWithAttackBonus(int bonus)
    {
        _weapon = new Weapon { AttackType = AttackType.Melee, AttackBonus = bonus };
    }

    [Given(@"the attacker has a stacking attack buff with \+(\d+) modifier")]
    public void GivenTheAttackerHasAStackingAttackBuffWithModifier(int modifier)
    {
        _attacker.ActiveStatusEffects.Add(new StatusEffect
        {
            Name = $"StackBuff_{modifier}_{_attacker.ActiveStatusEffects.Count}",
            Type = StatusEffectType.Buff,
            AttackPowerModifier = modifier,
            StackRule = StackRule.Stack,
            Source = $"source_{_attacker.ActiveStatusEffects.Count}"
        });
    }

    [Given(@"the attacker has a highest-wins attack buff with \+(\d+) modifier")]
    public void GivenTheAttackerHasAHighestWinsAttackBuffWithModifier(int modifier)
    {
        _attacker.ActiveStatusEffects.Add(new StatusEffect
        {
            Name = $"HWBuff_{modifier}",
            Type = StatusEffectType.Buff,
            AttackPowerModifier = modifier,
            StackRule = StackRule.HighestWins,
            Source = $"source_{_attacker.ActiveStatusEffects.Count}"
        });
    }

    [Given(@"the attacker has an attack debuff with (-?\d+) modifier")]
    public void GivenTheAttackerHasAnAttackDebuffWithModifier(int modifier)
    {
        _attacker.ActiveStatusEffects.Add(new StatusEffect
        {
            Name = $"Debuff_{modifier}",
            Type = StatusEffectType.Debuff,
            AttackPowerModifier = modifier,
            StackRule = StackRule.Stack,
            Source = "debuff-source"
        });
    }

    [When(@"attack power is computed")]
    public void WhenAttackPowerIsComputed()
    {
        _result = _sut.ComputeAttackerStats(_attacker, _weapon);
    }

    [Then(@"the total attack power should be (\d+)")]
    public void ThenTheTotalAttackPowerShouldBe(int expected)
    {
        Assert.NotNull(_result);
        Assert.Equal(expected, _result.AttackPower);
    }

    // ── Defender steps ─────────────────────────────────────────────────────────

    [Given(@"a stats defender with dexterity (\d+)")]
    public void GivenAStatsDefenderWithDexterity(int dexterity)
    {
        _defender = new Character { Dexterity = dexterity };
    }

    [Given(@"the stats defender wears chest armor with class (\d+) and max dex bonus (\d+)")]
    public void GivenTheStatsDefenderWearsChestArmorWithClassAndMaxDexBonus(int armorClass, int maxDex)
    {
        _defender.Equipment.Chest = new Armor { ArmorClass = armorClass, MaxDexterityBonus = maxDex };
    }

    [Given(@"the stats defender wears head armor with class (\d+) and max dex bonus (\d+)")]
    public void GivenTheStatsDefenderWearsHeadArmorWithClassAndMaxDexBonus(int armorClass, int maxDex)
    {
        _defender.Equipment.Head = new Armor { ArmorClass = armorClass, MaxDexterityBonus = maxDex };
    }

    [Given(@"the stats defender carries a shield with \+(\d+) defense bonus")]
    public void GivenTheStatsDefenderCarriesAShieldWithDefenseBonus(int bonus)
    {
        _defender.Equipment.Shield = new Shield { DefenseBonus = bonus };
    }

    [When(@"defense power is computed")]
    public void WhenDefensePowerIsComputed()
    {
        _result = _sut.ComputeDefenderStats(_defender);
    }

    [Then(@"the total defense power should be (\d+)")]
    public void ThenTheTotalDefensePowerShouldBe(int expected)
    {
        Assert.NotNull(_result);
        Assert.Equal(expected, _result.DefensePower);
    }

    [Then(@"the computed effective armor class should be (\d+)")]
    public void ThenTheComputedEffectiveArmorClassShouldBe(int expected)
    {
        Assert.NotNull(_result);
        Assert.Equal(expected, _result.EffectiveAC);
    }

    [Then(@"the computed dexterity modifier should be (\d+)")]
    public void ThenTheComputedDexterityModifierShouldBe(int expected)
    {
        Assert.NotNull(_result);
        Assert.Equal(expected, _result.DexterityModifier);
    }
}
