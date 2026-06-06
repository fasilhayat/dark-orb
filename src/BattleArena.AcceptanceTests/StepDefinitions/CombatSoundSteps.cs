namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;
using BattleArena.Presentation;
using Reqnroll;
using Xunit;

[Binding]
public class CombatSoundSteps
{
    // ── Single-attack sound scenarios ──────────────────────────────────────────

    private readonly IDiceService _dice = Substitute.For<IDiceService>();
    private readonly ICombatService _combat;
    private Character _character = new();
    private Weapon _weapon = new();
    private AttackResult? _attackResult;

    public CombatSoundSteps()
    {
        _combat = new CombatService(_dice, new CombatStatsService(), [new Application.Modifiers.RangeModifier()]);
    }

    [Given(@"a sound fighter with strength (\d+) and strike rating (\d+)")]
    public void GivenSoundFighter(int strength, int strikeRating)
    {
        _character = new Character
        {
            Strength = strength,
            StrikeRating = strikeRating,
            Dexterity = 10
        };
    }

    [Given(@"sound fighter wields ""([^""]+)"" dealing (\w+) damage with ([+-]\d+) bonus")]
    public void GivenSoundFighterWields(string name, string dieName, int bonus)
    {
        _weapon = new Weapon
        {
            Name = name,
            DamageDie = ParseDieType(dieName),
            AttackBonus = bonus,
            DamageType = DamageType.Slashing,
            AttackType = AttackType.Melee,
            DamageCount = 1
        };
    }

    [Given(@"the sound D20 rolls are (\d+) and (\d+)")]
    public void GivenSoundD20Rolls(int attackRoll, int defenseRoll)
    {
        _dice.Roll(DieType.D20).Returns(attackRoll, defenseRoll);
    }

    [Given(@"the sound damage die roll is (\d+)")]
    public void GivenSoundDamageDieRoll(int roll)
    {
        _dice.Roll(Arg.Is<DieType>(d => d != DieType.D20)).Returns(roll);
    }

    [When(@"the sound fighter attacks a target with armor class (\d+) and strike rating (\d+)")]
    public void WhenSoundFighterAttacks(int armorClass, int strikeRating)
    {
        var defender = new Character
        {
            StrikeRating = strikeRating,
            Dexterity = 10,
            Equipment = new ArmorSlots
            {
                Chest = new Armor
                {
                    ArmorClass = armorClass,
                    MaxDexterityBonus = 10
                }
            }
        };
        _attackResult = _combat.ResolveAttack(_character, defender, _weapon);
    }

    [Then(@"the sound attack should hit")]
    public void ThenSoundAttackShouldHit()
    {
        Assert.NotNull(_attackResult);
        Assert.True(_attackResult.IsHit);
    }

    [Then(@"the sound attack should miss")]
    public void ThenSoundAttackShouldMiss()
    {
        Assert.NotNull(_attackResult);
        Assert.False(_attackResult.IsHit);
    }

    [Then(@"the sound attack should be a critical hit")]
    public void ThenSoundAttackIsCriticalHit()
    {
        Assert.NotNull(_attackResult);
        Assert.True(_attackResult.IsCriticalHit);
    }

    [Then(@"the sound attack should be a fumble")]
    public void ThenSoundAttackIsFumble()
    {
        Assert.NotNull(_attackResult);
        Assert.True(_attackResult.IsFumble);
    }

    [Then(@"the attack result has PerfectParry")]
    public void ThenAttackResultHasPerfectParry()
    {
        Assert.NotNull(_attackResult);
        Assert.True(_attackResult.IsPerfectParry);
    }

    [Then(@"the attack result has DevastatingStrike")]
    public void ThenAttackResultHasDevastatingStrike()
    {
        Assert.NotNull(_attackResult);
        Assert.True(_attackResult.IsDevastatingStrike);
    }

    [Then(@"the attack result has TotalReversal")]
    public void ThenAttackResultHasTotalReversal()
    {
        Assert.NotNull(_attackResult);
        Assert.True(_attackResult.IsTotalReversal);
    }

    // ── Full combat sound scenarios ────────────────────────────────────────────

    private readonly Dictionary<string, Character> _combatants = new();
    private readonly Dictionary<string, Weapon> _weapons = new();
    private readonly List<string> _order = new();
    private CombatResult? _combatResult;

    [Given(@"a sound combatant named ""([^""]+)"" with level (\d+), strength (\d+), dexterity (\d+), strike rating (\d+), turn speed (\d+), and (\d+) hit points")]
    public void GivenSoundCombatant(string name, int level, int strength, int dexterity, int strikeRating, int turnSpeed, int hp)
    {
        var character = new Character
        {
            Name = name,
            Level = level,
            Strength = strength,
            Dexterity = dexterity,
            StrikeRating = strikeRating,
            TurnSpeed = turnSpeed,
            MaxHitPoints = hp,
            CurrentHitPoints = hp
        };
        _combatants[name] = character;
        _order.Add(name);
    }

    [Given(@"sound combatant ""([^""]+)"" wields a ""([^""]+)"" dealing (\d+)d(\d+) (\w+) damage with attack bonus (\d+)")]
    public void GivenSoundCombatantWields(string characterName, string weaponName, int dieCount, int dieSides, string damageTypeName, int attackBonus)
    {
        var weapon = new Weapon
        {
            Name = weaponName,
            DamageDie = ParseDieTypeSides(dieSides),
            DamageCount = dieCount,
            DamageType = ParseDamageType(damageTypeName),
            AttackType = AttackType.Melee,
            AttackBonus = attackBonus
        };
        _weapons[characterName] = weapon;
        _combatants[characterName].Equipment.RightHand = weapon;
    }

    [Given(@"sound combatant ""([^""]+)"" wears ""([^""]+)"" with armor class (\d+) and mitigation (\d+)")]
    public void GivenSoundCombatantWears(string characterName, string armorName, int armorClass, int mitigation)
    {
        _combatants[characterName].Equipment.Chest = new Armor
        {
            Name = armorName,
            ArmorClass = armorClass,
            Mitigation = mitigation,
            MaxDexterityBonus = 6
        };
    }

    [When(@"the sound combat is simulated with a maximum of (\d+) ticks")]
    public void WhenSoundCombatIsSimulated(int maxTicks)
    {
        var dice = new DiceService();
        var combatStats = new CombatStatsService();
        var combat = new CombatService(dice, combatStats);
        var turnmeter = new TurnmeterService();
        var statusEffect = new StatusEffectService();
        var simulator = new CombatSimulator(combat, turnmeter, statusEffect, dice);

        var fighterName = _order[0];
        var opponentName = _order[1];

        _combatResult = simulator.Simulate(
            _combatants[fighterName], _weapons[fighterName],
            _combatants[opponentName], _weapons[opponentName],
            maxTicks);
    }

    [Then(@"the sound combat should have ended before the tick limit")]
    public void ThenSoundCombatShouldHaveEnded()
    {
        Assert.NotNull(_combatResult);
        Assert.False(_combatResult.MaxTicksReached,
            $"Combat did not finish within the tick limit. Log:\n{_combatResult.FormatLog()}");
    }

    [Then(@"the sound combat log contains ""([^""]+)""(?: or ""([^""]+)"")?")]
    public void ThenSoundCombatLogContains(string eventType, string? eventType2 = null)
    {
        Assert.NotNull(_combatResult);
        var found = _combatResult.Log.Any(e =>
            e.EventType == eventType || (eventType2 is not null && e.EventType == eventType2));
        Assert.True(found, $"Expected log to contain '{eventType}'" +
            (eventType2 is not null ? $" or '{eventType2}'" : ""));
    }

    // ── CombatSoundRegistry scenarios ──────────────────────────────────────────

    [Given(@"the combat sound registry is loaded")]
    public void GivenCombatSoundRegistryLoaded()
    {
        // No-op; CombatSoundRegistry is static
    }

    [When(@"effect sound mappings are verified")]
    public void WhenEffectSoundMappingsVerified()
    {
        // No-op; verification happens in Then steps
    }

    [When(@"event sound mappings are verified")]
    public void WhenEventSoundMappingsVerified()
    {
        // No-op; verification happens in Then steps
    }

    [Then(@"""([^""]+)"" should map to sound ""([^""]+)""")]
    public void ThenMapsToSound(string key, string expectedSoundId)
    {
        var fromEffect = CombatSoundRegistry.GetEffectSoundId(key);
        var fromEvent = CombatSoundRegistry.GetEventSoundId(key);

        var match = fromEffect == expectedSoundId || fromEvent == expectedSoundId;
        Assert.True(match,
            $"Expected '{key}' to map to sound '{expectedSoundId}', " +
            $"but GetEffectSoundId returned '{fromEffect}' and GetEventSoundId returned '{fromEvent}'");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

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

    private static DieType ParseDieTypeSides(int sides) => sides switch
    {
        4 => DieType.D4,
        6 => DieType.D6,
        8 => DieType.D8,
        10 => DieType.D10,
        12 => DieType.D12,
        20 => DieType.D20,
        100 => DieType.D100,
        _ => throw new ArgumentOutOfRangeException(nameof(sides), $"Unknown die size: d{sides}")
    };

    private static DamageType ParseDamageType(string name) => name switch
    {
        "Slashing" => DamageType.Slashing,
        "Piercing" => DamageType.Piercing,
        "Bludgeoning" => DamageType.Bludgeoning,
        "Fire" => DamageType.Fire,
        "Ice" => DamageType.Ice,
        "Lightning" => DamageType.Lightning,
        "Poison" => DamageType.Poison,
        "Shadow" => DamageType.Shadow,
        "Holy" => DamageType.Holy,
        "Acid" => DamageType.Acid,
        _ => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown damage type: {name}")
    };
}
