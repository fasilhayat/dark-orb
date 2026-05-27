namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

/// <summary>
/// Step definitions for Resistance.feature.
/// Tests verify that ComputeResistance sums sources correctly and that
/// TryApply respects both ApplicationChance and resistance rolls.
/// </summary>
[Binding]
public class ResistanceSteps
{
    private Character _character = new();
    private int _appliedCount;
    private int _resistedCount;

    // ── Character setup ────────────────────────────────────────────────────────

    [Given(@"a character with no resistance sources")]
    public void GivenACharacterWithNoResistanceSources()
    {
        _character = new Character();
    }

    [Given(@"a target with (\d+) magic resistance")]
    public void GivenATargetWithMagicResistance(int resistance)
    {
        _character = new Character
        {
            Race = resistance > 0
                ? new Race { Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Magic, resistance)] }] }
                : null
        };
    }

    [Given(@"a character with a racial feat granting (\d+) magic resistance")]
    public void GivenACharacterWithRacialFeatGrantingMagicResistance(int value)
    {
        _character = new Character
        {
            Race = new Race { Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Magic, value)] }] }
        };
    }

    [Given(@"a character with a racial feat granting (\d+) fire resistance")]
    public void GivenACharacterWithRacialFeatGrantingFireResistance(int value)
    {
        _character = new Character
        {
            Race = new Race { Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Fire, value)] }] }
        };
    }

    [Given(@"a character wearing armor with (\d+) fire resistance")]
    public void GivenACharacterWearingArmorWithFireResistance(int value)
    {
        _character = new Character
        {
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Resistances = [new ResistanceBonus(ResistanceType.Fire, value)] }
            }
        };
    }

    [Given(@"the character also wears boots with (\d+) fire resistance")]
    public void GivenTheCharacterAlsoWearsBootsWithFireResistance(int value)
    {
        _character.Equipment.Boots = new Armor { Resistances = [new ResistanceBonus(ResistanceType.Fire, value)] };
    }

    [Given(@"the character wears chest armor with (\d+) magic resistance")]
    public void GivenTheCharacterWearsChestArmorWithMagicResistance(int value)
    {
        _character.Equipment.Chest = new Armor { Resistances = [new ResistanceBonus(ResistanceType.Magic, value)] };
    }

    [Given(@"the character has an active buff granting (\d+) magic resistance")]
    public void GivenTheCharacterHasAnActiveBuffGrantingMagicResistance(int value)
    {
        _character.ActiveStatusEffects.Add(new StatusEffect
        {
            Name = "Arcane Ward",
            ResistanceBonuses = [new ResistanceBonus(ResistanceType.Magic, value)]
        });
    }

    // ── Actions ────────────────────────────────────────────────────────────────

    [When(@"an Arcane Ward buff granting (\d+) magic resistance is applied to the character")]
    public void WhenAnArcaneWardBuffGrantingMagicResistanceIsApplied(int value)
    {
        _character.ActiveStatusEffects.Add(new StatusEffect
        {
            Name = "Arcane Ward",
            ResistanceBonuses = [new ResistanceBonus(ResistanceType.Magic, value)]
        });
    }

    [When(@"a magical effect with (\d+)% application chance is applied (\d+) times")]
    public void WhenAMagicalEffectIsAppliedNTimes(int applicationChance, int attempts)
    {
        var sut = new StatusEffectService();
        var dice = new DiceService();
        var resistance = _character.ComputeResistance(ResistanceType.Magic);

        _appliedCount = 0;
        _resistedCount = 0;

        for (var i = 0; i < attempts; i++)
        {
            var target = new Character();
            var effect = new StatusEffect
            {
                Name = "Test Effect",
                Type = StatusEffectType.Debuff,
                Duration = 1,
                ApplicationChance = applicationChance,
                StackRule = StackRule.NoStack
            };

            var result = sut.TryApply(target, effect, resistance, dice);
            if (result.Applied)    _appliedCount++;
            if (result.WasResisted) _resistedCount++;
        }
    }

    // ── Assertions ─────────────────────────────────────────────────────────────

    [Then(@"the character's computed magic resistance should be (\d+)")]
    public void ThenComputedMagicResistanceShouldBe(int expected)
    {
        Assert.Equal(expected, _character.ComputeResistance(ResistanceType.Magic));
    }

    [Then(@"the character's computed fire resistance should be (\d+)")]
    public void ThenComputedFireResistanceShouldBe(int expected)
    {
        Assert.Equal(expected, _character.ComputeResistance(ResistanceType.Fire));
    }

    [Then(@"the character's computed cold resistance should be (\d+)")]
    public void ThenComputedColdResistanceShouldBe(int expected)
    {
        Assert.Equal(expected, _character.ComputeResistance(ResistanceType.Cold));
    }

    [Then(@"all (\d+) applications should have landed")]
    public void ThenAllApplicationsShouldHaveLanded(int expected)
    {
        Assert.Equal(expected, _appliedCount);
    }

    [Then(@"at least (\d+) of the (\d+) applications should have been resisted")]
    public void ThenAtLeastNApplicationsShouldHaveBeenResisted(int minimumResisted, int total)
    {
        Assert.True(_resistedCount >= minimumResisted,
            $"Expected at least {minimumResisted} resists out of {total} attempts, but got {_resistedCount}. " +
            $"Applied: {_appliedCount}");
    }

    [Then(@"at least 1 application should have landed")]
    public void ThenAtLeastOneApplicationShouldHaveLanded()
    {
        Assert.True(_appliedCount >= 1,
            $"Expected at least 1 application to land, but got {_appliedCount} (resisted {_resistedCount} times).");
    }
}
