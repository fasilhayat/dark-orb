namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

// Step definitions for FullCombat.feature.
// These steps use REAL (live) dice — no mocks — so the combat outcome is
// non-deterministic. Assertions cover structural invariants only.
[Binding]
public class FullCombatSteps
{
    // Combatants indexed by name, in insertion order (first = fighter, second = opponent)
    private readonly Dictionary<string, Character> _combatants = new();
    private readonly Dictionary<string, Weapon> _weapons = new();
    private readonly List<string> _order = new();

    private CombatResult _combatResult = null!;
    private readonly ICombatSimulator _combatSimulator;

    public FullCombatSteps()
    {
        // Wire up real services — no mocking so dice rolls are live.
        var dice = new DiceService();
        var combatStats = new CombatStatsService();
        var combat = new CombatService(dice, combatStats);
        var turnmeter = new TurnmeterService();
        var statusEffect = new StatusEffectService();
        _combatSimulator = new CombatSimulator(combat, turnmeter, statusEffect, dice);
    }

    // ── Character setup ────────────────────────────────────────────────────────

    // Matches both "a Fighter named ..." and "an Orc named ..." via the flexible prefix.
    [Given(@"(?:a|an) \w+ named ""([^""]+)"" with level (\d+), strength (\d+), dexterity (\d+), strike rating (\d+), turn speed (\d+), and (\d+) hit points")]
    public void GivenACombatantNamed(string name, int level, int strength, int dexterity, int strikeRating, int turnSpeed, int hp)
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

    // Equips a weapon into the combatant's right-hand slot.
    // Die count and sides are parsed from the "XdY" notation (e.g. "1d8").
    [Given(@"""([^""]+)"" wields a ""([^""]+)"" dealing (\d+)d(\d+) (\w+) damage with attack bonus (\d+)")]
    public void GivenCombatantWields(string characterName, string weaponName, int dieCount, int dieSides, string damageTypeName, int attackBonus)
    {
        var weapon = new Weapon
        {
            Name = weaponName,
            DamageDie = ParseDieType(dieSides),
            DamageCount = dieCount,
            DamageType = ParseDamageType(damageTypeName),
            AttackType = AttackType.Melee,
            AttackBonus = attackBonus
        };
        _weapons[characterName] = weapon;
        _combatants[characterName].Equipment.RightHand = weapon;
    }

    // Equips a chest-slot armor piece. Higher ArmorClass = better protection.
    // CombatStatsService uses it directly: EffectiveAC = ArmorClass.
    [Given(@"""([^""]+)"" wears ""([^""]+)"" with armor class (\d+) and mitigation (\d+)")]
    public void GivenCombatantWears(string characterName, string armorName, int armorClass, int mitigation)
    {
        _combatants[characterName].Equipment.Chest = new Armor
        {
            Name = armorName,
            ArmorClass = armorClass,
            Mitigation = mitigation,
            MaxDexterityBonus = 6   // No meaningful cap for these scenarios
        };
    }

    // ── Combat execution ───────────────────────────────────────────────────────

    // Runs the combat with real dice. The result is verified by the Then steps below.
    [When(@"the combat is simulated with a maximum of (\d+) ticks")]
    public void WhenTheCombatIsSimulated(int maxTicks)
    {
        var fighterName = _order[0];
        var opponentName = _order[1];

        _combatResult = _combatSimulator.Simulate(
            _combatants[fighterName], _weapons[fighterName],
            _combatants[opponentName], _weapons[opponentName],
            maxTicks);
    }

    // ── Assertions ─────────────────────────────────────────────────────────────

    // The combat must reach a conclusive end — not time out.
    [Then(@"the combat should have ended before the tick limit")]
    public void ThenTheCombatShouldHaveEndedBeforeTheTickLimit()
    {
        Assert.False(_combatResult.MaxTicksReached,
            $"Combat did not finish within the tick limit. Log:\n{_combatResult.FormatLog()}");
    }

    // The winner must still be alive.
    [Then(@"the winning combatant should have hit points above zero")]
    public void ThenTheWinningCombatantShouldHaveHitPointsAboveZero()
    {
        Assert.NotNull(_combatResult.Winner);
        Assert.True(_combatResult.Winner.CurrentHitPoints > 0,
            $"Winner {_combatResult.Winner.Name} has {_combatResult.Winner.CurrentHitPoints} HP — expected > 0.");
    }

    // The loser must be at or below zero HP.
    [Then(@"the losing combatant should have zero or fewer hit points")]
    public void ThenTheLosingCombatantShouldHaveZeroOrFewerHitPoints()
    {
        Assert.NotNull(_combatResult.Loser);
        Assert.True(_combatResult.Loser.CurrentHitPoints <= 0,
            $"Loser {_combatResult.Loser.Name} has {_combatResult.Loser.CurrentHitPoints} HP — expected ≤ 0.");
    }

    // Log must contain at least one entry.
    [Then(@"the combat log should not be empty")]
    public void ThenTheCombatLogShouldNotBeEmpty()
    {
        Assert.NotEmpty(_combatResult.Log);
    }

    // Turnmeter gain is recorded every tick for every combatant.
    [Then(@"the combat log should contain turnmeter gain events")]
    public void ThenTheCombatLogShouldContainTurnMeterGainEvents()
    {
        Assert.Contains(_combatResult.Log, e => e.EventType == "TurnMeterGain");
    }

    // At least one attack must have been made during the combat.
    [Then(@"the combat log should contain at least one attack event")]
    public void ThenTheCombatLogShouldContainAtLeastOneAttackEvent()
    {
        Assert.Contains(_combatResult.Log, e => e.EventType == "Attack");
    }

    // At least one attack must have connected and dealt damage.
    [Then(@"the combat log should contain at least one damage event")]
    public void ThenTheCombatLogShouldContainAtLeastOneDamageEvent()
    {
        Assert.Contains(_combatResult.Log, e => e.EventType == "Damage");
    }

    // Combat result must carry a non-empty GUID used for traceability.
    [Then(@"the combat result should have a combat identifier")]
    public void ThenTheCombatResultShouldHaveACombatIdentifier()
    {
        Assert.NotEqual(Guid.Empty, _combatResult.CombatId);
    }

    // Each simulation run produces a unique combat identifier.
    [Then(@"the combat identifier should be unique per simulation")]
    public void ThenTheCombatIdentifierShouldBeUniquePerSimulation()
    {
        // Run a second simulation with the same combatants
        var fighterName = _order[0];
        var opponentName = _order[1];

        // Reset HP for a fresh fight
        _combatants[fighterName].CurrentHitPoints = _combatants[fighterName].MaxHitPoints;
        _combatants[opponentName].CurrentHitPoints = _combatants[opponentName].MaxHitPoints;

        var secondResult = _combatSimulator.Simulate(
            _combatants[fighterName], _weapons[fighterName],
            _combatants[opponentName], _weapons[opponentName],
            500);

        Assert.NotEqual(_combatResult.CombatId, secondResult.CombatId);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static DieType ParseDieType(int sides) => sides switch
    {
        4   => DieType.D4,
        6   => DieType.D6,
        8   => DieType.D8,
        10  => DieType.D10,
        12  => DieType.D12,
        20  => DieType.D20,
        100 => DieType.D100,
        _   => throw new ArgumentOutOfRangeException(nameof(sides), $"Unknown die size: d{sides}")
    };

    private static DamageType ParseDamageType(string name) => name switch
    {
        "Slashing"     => DamageType.Slashing,
        "Piercing"     => DamageType.Piercing,
        "Bludgeoning"  => DamageType.Bludgeoning,
        "Fire"         => DamageType.Fire,
        "Ice"          => DamageType.Ice,
        "Lightning"    => DamageType.Lightning,
        "Poison"       => DamageType.Poison,
        "Shadow"       => DamageType.Shadow,
        "Holy"         => DamageType.Holy,
        "Acid"         => DamageType.Acid,
        _              => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown damage type: {name}")
    };
}
