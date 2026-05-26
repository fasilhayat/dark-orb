namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

// Step definitions for FullBattle.feature.
// These steps use REAL (live) dice — no mocks — so the battle outcome is
// non-deterministic. Assertions cover structural invariants only.
[Binding]
public class FullBattleSteps
{
    // Combatants indexed by name, in insertion order (first = fighter, second = opponent)
    private readonly Dictionary<string, Character> _combatants = new();
    private readonly Dictionary<string, Weapon> _weapons = new();
    private readonly List<string> _order = new();

    private BattleResult _result = null!;
    private readonly IBattleSimulator _simulator;

    public FullBattleSteps()
    {
        // Wire up real services — no mocking so dice rolls are live.
        var dice = new DiceService();
        var combatStats = new CombatStatsService();
        var combat = new CombatService(dice, combatStats);
        var turnmeter = new TurnmeterService();
        var statusEffect = new StatusEffectService();
        _simulator = new BattleSimulator(combat, turnmeter, statusEffect, dice);
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

    // Equips a chest-slot armor piece. ArmorClass is the raw AD&D value (lower = better).
    // CombatStatsService converts it: EffectiveAC = 20 - ArmorClass.
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

    // ── Battle execution ───────────────────────────────────────────────────────

    // Runs the battle with real dice. The result is verified by the Then steps below.
    [When(@"the battle is simulated with a maximum of (\d+) ticks")]
    public void WhenTheBattleIsSimulated(int maxTicks)
    {
        var fighterName = _order[0];
        var opponentName = _order[1];

        _result = _simulator.Simulate(
            _combatants[fighterName], _weapons[fighterName],
            _combatants[opponentName], _weapons[opponentName],
            maxTicks);
    }

    // ── Assertions ─────────────────────────────────────────────────────────────

    // The battle must reach a conclusive end — not time out.
    [Then(@"the battle should have ended before the tick limit")]
    public void ThenTheBattleShouldHaveEndedBeforeTheTickLimit()
    {
        Assert.False(_result.MaxTicksReached,
            $"Battle did not finish within the tick limit. Log:\n{_result.FormatLog()}");
    }

    // The winner must still be alive.
    [Then(@"the winning combatant should have hit points above zero")]
    public void ThenTheWinningCombatantShouldHaveHitPointsAboveZero()
    {
        Assert.NotNull(_result.Winner);
        Assert.True(_result.Winner.CurrentHitPoints > 0,
            $"Winner {_result.Winner.Name} has {_result.Winner.CurrentHitPoints} HP — expected > 0.");
    }

    // The loser must be at or below zero HP.
    [Then(@"the losing combatant should have zero or fewer hit points")]
    public void ThenTheLosingCombatantShouldHaveZeroOrFewerHitPoints()
    {
        Assert.NotNull(_result.Loser);
        Assert.True(_result.Loser.CurrentHitPoints <= 0,
            $"Loser {_result.Loser.Name} has {_result.Loser.CurrentHitPoints} HP — expected ≤ 0.");
    }

    // Log must contain at least one entry.
    [Then(@"the battle log should not be empty")]
    public void ThenTheBattleLogShouldNotBeEmpty()
    {
        Assert.NotEmpty(_result.Log);
    }

    // Turnmeter gain is recorded every tick for every combatant.
    [Then(@"the battle log should contain turnmeter gain events")]
    public void ThenTheBattleLogShouldContainTurnMeterGainEvents()
    {
        Assert.Contains(_result.Log, e => e.EventType == "TurnMeterGain");
    }

    // At least one attack must have been made during the battle.
    [Then(@"the battle log should contain at least one attack event")]
    public void ThenTheBattleLogShouldContainAtLeastOneAttackEvent()
    {
        Assert.Contains(_result.Log, e => e.EventType == "Attack");
    }

    // At least one attack must have connected and dealt damage.
    [Then(@"the battle log should contain at least one damage event")]
    public void ThenTheBattleLogShouldContainAtLeastOneDamageEvent()
    {
        Assert.Contains(_result.Log, e => e.EventType == "Damage");
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
        _              => throw new ArgumentOutOfRangeException(nameof(name), $"Unknown damage type: {name}")
    };
}
