namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;
using Reqnroll;
using Xunit;

[Binding]
[Scope(Feature = "Combat — Elemental Afterburn")]
public class ElementalAfterburnSteps
{
    private readonly IDiceService _dice = Substitute.For<IDiceService>();
    private Character _caster = null!;
    private Character _target = null!;
    private Spell _spell = null!;
    private CombatResult? _result;

    [Given(@"a spellcaster named ""([^""]+)"" with intelligence (\d+)")]
    public void GivenASpellcasterNamed(string name, int intelligence)
    {
        _caster = new Character
        {
            Name = name,
            ClassId = 5,
            Level = 5,
            Strength = 8,
            Dexterity = 14,
            Intelligence = intelligence,
            StrikeRating = 13,
            TurnSpeed = 100,
            MaxHitPoints = 30,
            CurrentHitPoints = 30,
            MaxMana = 300,
            CurrentMana = 300
        };
    }

    [Given(@"a spell ""([^""]+)"" dealing (\d+)d(\d+) (\w+) damage with (fire|ice|lightning|poison|acid|holy|shadow) elemental type")]
    public void GivenASpell(string name, int count, int sides, string damageTypeName, string elementalTypeName)
    {
        _spell = new Spell
        {
            Name = name,
            School = SpellSchool.Evocation,
            DamageDie = ParseDieType(sides),
            DamageCount = count,
            DamageType = ParseDamageType(damageTypeName),
            AttackBonus = 2,
            SpellLevel = 3,
            TurnMeterCost = 90,
            ManaCost = 10,
            ElementalType = Enum.TryParse<ElementalType>(elementalTypeName, true, out var et) ? et : ElementalType.None
        };
        _caster.MemorizedSpells = [_spell];
    }

    [Given(@"a spell ""([^""]+)"" dealing (\d+)d(\d+) (\w+) damage with no elemental type")]
    public void GivenASpellWithNoElementalType(string name, int count, int sides, string damageTypeName)
    {
        _spell = new Spell
        {
            Name = name,
            School = SpellSchool.Evocation,
            DamageDie = ParseDieType(sides),
            DamageCount = count,
            DamageType = ParseDamageType(damageTypeName),
            AttackBonus = 2,
            SpellLevel = 1,
            TurnMeterCost = 60,
            ManaCost = 5,
            ElementalType = ElementalType.None
        };
        _caster.MemorizedSpells = [_spell];
    }

    [Given(@"a target ""([^""]+)"" with (\d+) hit points")]
    public void GivenATarget(string name, int hp)
    {
        GivenATarget(name, hp, 1);
    }

    [Given(@"a target ""([^""]+)"" with (\d+) hit points and turn speed (\d+)")]
    public void GivenATargetWithTurnSpeed(string name, int hp, int turnSpeed)
    {
        GivenATarget(name, hp, turnSpeed);
    }

    private void GivenATarget(string name, int hp, int turnSpeed)
    {
        _target = new Character
        {
            Name = name,
            ClassId = 8,
            Level = 1,
            Strength = 10,
            Dexterity = 10,
            Intelligence = 10,
            StrikeRating = 10,
            TurnSpeed = turnSpeed,
            MaxHitPoints = hp,
            CurrentHitPoints = hp
        };
    }

    [Given(@"the D20 roll is (\d+)")]
    public void GivenTheD20RollIs(int roll)
    {
        _dice.Roll(DieType.D20).Returns(roll);
    }

    [Given(@"the D100 roll is (\d+)")]
    public void GivenTheD100RollIs(int roll)
    {
        _dice.Roll(DieType.D100).Returns(roll);
    }

    [Given(@"the damage die roll is (\d+)")]
    public void GivenTheDamageDieRollIs(int roll)
    {
        _dice.Roll(Arg.Is<DieType>(d => d != DieType.D20 && d != DieType.D100)).Returns(roll);
    }

    [When(@"the combat is simulated for (\d+) ticks")]
    public void WhenTheCombatIsSimulated(int maxTicks)
    {
        var simulator = new CombatSimulator(
            new CombatService(_dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            _dice);

        _result = simulator.Simulate(Party.Solo(_caster), Party.Solo(_target), maxTicks);
    }

    [Then(@"the combat log should contain an EffectApplied event for ""([^""]+)"" on ""([^""]+)""")]
    public void ThenTheCombatLogShouldContainEffectApplied(string effectName, string targetName)
    {
        Assert.NotNull(_result);
        var matches = _result.Log
            .Where(e => e.EventType == "EffectApplied"
                     && e.StatusEffectName == effectName
                     && e.ActorName == targetName)
            .ToList();
        Assert.NotEmpty(matches);
    }

    [Then(@"the combat log should contain DoTTick events for ""([^""]+)"" on ""([^""]+)"" with damage dealt")]
    public void ThenTheCombatLogShouldContainDoTTicks(string effectName, string targetName)
    {
        Assert.NotNull(_result);
        var ticks = _result.Log
            .Where(e => e.EventType == "DoTTick"
                     && e.StatusEffectName == effectName
                     && e.ActorName == targetName)
            .ToList();
        Assert.NotEmpty(ticks);
        Assert.All(ticks, t => Assert.True(t.DamageDealt > 0,
            $"DoTTick for {effectName} on tick {t.Tick} should deal > 0 damage, got {t.DamageDealt}"));
    }

    [Then(@"the DoTTick messages should read like ""([^""]+)""")]
    public void ThenTheDoTTickMessagesShouldReadLike(string pattern)
    {
        Assert.NotNull(_result);
        var ticks = _result.Log.Where(e => e.EventType == "DoTTick").ToList();
        Assert.NotEmpty(ticks);
        Assert.All(ticks, t => Assert.Matches(pattern, t.Message));
    }

    [Then(@"the combat log should contain no afterburn EffectApplied events")]
    public void ThenTheCombatLogShouldContainNoAfterburnEffectApplied()
    {
        Assert.NotNull(_result);
        var applied = _result.Log
            .Where(e => e.EventType == "EffectApplied"
                     && e.StatusEffectName is "Burning" or "Chilled" or "Shocked" or "Poisoned")
            .ToList();
        Assert.Empty(applied);
    }

    // ── helpers ─────────────────────────────────────────────────────────────────

    private static DieType ParseDieType(int sides) => sides switch
    {
        4 => DieType.D4,
        6 => DieType.D6,
        8 => DieType.D8,
        10 => DieType.D10,
        12 => DieType.D12,
        20 => DieType.D20,
        100 => DieType.D100,
        _ => throw new ArgumentOutOfRangeException(nameof(sides), $"Unknown die type: d{sides}")
    };

    private static DamageType ParseDamageType(string name) => name.ToLowerInvariant() switch
    {
        "fire" => DamageType.Fire,
        "ice" => DamageType.Ice,
        "lightning" => DamageType.Lightning,
        "poison" => DamageType.Poison,
        "acid" => DamageType.Acid,
        "slashing" => DamageType.Slashing,
        "piercing" => DamageType.Piercing,
        "bludgeoning" => DamageType.Bludgeoning,
        "psychic" => DamageType.Psychic,
        "holy" => DamageType.Holy,
        "shadow" => DamageType.Shadow,
        _ => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown damage type: {name}")
    };
}
