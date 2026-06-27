namespace BattleArena.UnitTests.Services;

using BattleArena.Application.Models;
using BattleArena.Presentation;

public class CombatLogMergerTests
{
    private static CombatLogEntry E(int tick, string type, string actor = "A") =>
        new() { Tick = tick, EventType = type, ActorName = actor };

    [Fact]
    public void Merge_NullDiceLog_ReturnsOriginalLog()
    {
        var log = new List<CombatLogEntry> { E(1, "TurnStart") };

        var result = CombatLogMerger.Merge(log, null);

        Assert.Same(log, result);
    }

    [Fact]
    public void Merge_EmptyDiceLog_ReturnsOriginalLog()
    {
        var log = new List<CombatLogEntry> { E(1, "TurnStart") };

        var result = CombatLogMerger.Merge(log, []);

        Assert.Same(log, result);
    }

    [Fact]
    public void Merge_ApiCallAppearsBeforeAttack()
    {
        // Dice rolls happen first; Attack shows the computed outcome.
        var log = new List<CombatLogEntry>
        {
            E(5, "TurnStart"),
            E(5, "Attack"),
            E(5, "Damage")
        };
        var diceLog = new List<CombatLogEntry> { E(5, "ApiCall") };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        Assert.Equal(["TurnStart", "ApiCall", "Attack", "Damage"], types);
    }

    [Fact]
    public void Merge_ApiCallAppearsBeforeKnockedOut()
    {
        // When no Attack in tick, dice should still precede the terminal outcome.
        var log = new List<CombatLogEntry>
        {
            E(7, "TurnStart"),
            E(7, "Attack"),
            E(7, "Damage"),
            E(7, "KnockedOut")
        };
        var diceLog = new List<CombatLogEntry> { E(7, "ApiCall") };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        // Dice inserted before Attack (highest priority), so before everything except TurnStart
        Assert.Equal(["TurnStart", "ApiCall", "Attack", "Damage", "KnockedOut"], types);
    }

    [Fact]
    public void Merge_ApiCallAppearsBeforeDeath()
    {
        var log = new List<CombatLogEntry>
        {
            E(9, "Attack"),
            E(9, "Damage"),
            E(9, "Death")
        };
        var diceLog = new List<CombatLogEntry> { E(9, "ApiCall") };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        Assert.Equal(["ApiCall", "Attack", "Damage", "Death"], types);
    }

    [Fact]
    public void Merge_ApiCallNotInsertedBeforeTurnStart()
    {
        var log = new List<CombatLogEntry>
        {
            E(3, "TurnMeterGain"),
            E(3, "TurnStart"),
            E(3, "Attack")
        };
        var diceLog = new List<CombatLogEntry> { E(3, "ApiCall") };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var apiIdx = merged.FindIndex(e => e.EventType == "ApiCall");
        var turnStartIdx = merged.FindIndex(e => e.EventType == "TurnStart");
        Assert.True(apiIdx > turnStartIdx, "ApiCall must appear after TurnStart");
    }

    [Fact]
    public void Merge_MultipleTicksEachGetTheirDiceBeforeAttack()
    {
        var log = new List<CombatLogEntry>
        {
            E(1, "TurnStart", "Hero"),
            E(1, "Attack", "Hero"),
            E(2, "TurnStart", "Villain"),
            E(2, "Damage", "Villain")  // no Attack in tick 2
        };
        var diceLog = new List<CombatLogEntry>
        {
            E(1, "ApiCall", "Hero"),     // matches Attack actor in tick 1 → before Attack
            E(2, "ApiCall", "Villain")   // no Attack for Villain → appended at end of tick 2
        };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        var actors = merged.Select(e => e.ActorName).ToList();
        // Tick 1: dice (Hero) before Attack (Hero)
        Assert.Equal("TurnStart", types[0]);
        Assert.Equal("ApiCall", types[1]);
        Assert.Equal("Hero", actors[1]);
        Assert.Equal("Attack", types[2]);
        // Tick 2: no Attack → dice after last event
        Assert.Equal("TurnStart", types[3]);
        Assert.Equal("Damage", types[4]);
        Assert.Equal("ApiCall", types[5]);
        Assert.Equal("Villain", actors[5]);
    }

    [Fact]
    public void Merge_MultipleDiceAtSameTick_AllInsertedBeforeAttack()
    {
        var log = new List<CombatLogEntry> { E(7, "Attack", "Hero") };
        var diceLog = new List<CombatLogEntry>
        {
            E(7, "ApiCall", "Hero"),
            E(7, "ApiCall", "Hero"),
            E(7, "ApiCall", "Hero")
        };

        var merged = CombatLogMerger.Merge(log, diceLog);

        Assert.Equal(4, merged.Count);
        Assert.All(merged.Take(3), e => Assert.Equal("ApiCall", e.EventType));
        Assert.Equal("Attack", merged[3].EventType);
    }

    [Fact]
    public void Merge_DiceTickNotInMainLog_AppendedAtEnd()
    {
        var log = new List<CombatLogEntry> { E(1, "TurnStart") };
        var diceLog = new List<CombatLogEntry> { E(99, "ApiCall") };

        var merged = CombatLogMerger.Merge(log, diceLog);

        Assert.Equal(2, merged.Count);
        Assert.Equal("ApiCall", merged[1].EventType);
    }

    [Fact]
    public void Merge_PreservesRelativeOrderWithinTick()
    {
        var log = new List<CombatLogEntry>
        {
            E(2, "RoundStart"),
            E(2, "TurnStart"),
            E(2, "Attack"),
            E(2, "Damage"),
            E(2, "TurnEnd")
        };
        var diceLog = new List<CombatLogEntry> { E(2, "ApiCall") };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        // Dice inserted before Attack (the first priority event of the matching actor in the tick)
        Assert.Equal(["RoundStart", "TurnStart", "ApiCall", "Attack", "Damage", "TurnEnd"], types);
    }

    [Fact]
    public void Merge_MultipleActorsSameTick_DiceBeforeEachOwnAttack()
    {
        // Two actors act in same tick. Each actor's dice should precede their own Attack.
        var log = new List<CombatLogEntry>
        {
            E(1, "TurnStart", "Finnick"),
            E(1, "Attack", "Finnick"),
            E(1, "Damage"),
            E(1, "TurnStart", "Merchant Vex"),
            E(1, "Attack", "Merchant Vex"),
            E(1, "Damage")
        };
        var diceLog = new List<CombatLogEntry>
        {
            E(1, "ApiCall", "Finnick"),
            E(1, "ApiCall", "Finnick"),
            E(1, "ApiCall", "Finnick"),
            E(1, "ApiCall", "Merchant Vex"),
            E(1, "ApiCall", "Merchant Vex")
        };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        var actors = merged.Select(e => e.ActorName).ToList();

        // Expected: [TurnStart(F), FinnickDice(3), Attack(F), Damage, TurnStart(MV), MVDice(2), Attack(MV), Damage]
        Assert.Equal("TurnStart", types[0]);
        Assert.Equal("Finnick", actors[0]);

        Assert.Equal("ApiCall", types[1]);
        Assert.Equal("ApiCall", types[2]);
        Assert.Equal("ApiCall", types[3]);
        Assert.Equal("Finnick", actors[1]);

        Assert.Equal("Attack", types[4]);
        Assert.Equal("Finnick", actors[4]);

        Assert.Equal("Damage", types[5]);

        Assert.Equal("TurnStart", types[6]);
        Assert.Equal("Merchant Vex", actors[6]);

        Assert.Equal("ApiCall", types[7]);
        Assert.Equal("ApiCall", types[8]);
        Assert.Equal("Merchant Vex", actors[7]);

        Assert.Equal("Attack", types[9]);
        Assert.Equal("Merchant Vex", actors[9]);

        Assert.Equal("Damage", types[10]);
    }

    [Fact]
    public void Merge_Healed_MismatchedActor_UsesCurrentTurnActor()
    {
        // Healed events store ActorName as the target, but dice are under the caster.
        // The merger should use the current turn's actor (tracked from TurnStart) for the key.
        var log = new List<CombatLogEntry>
        {
            E(1, "TurnStart", "Priestess"),
            // Healed with ActorName=Elira (target), but dice under Priestess (caster)
            new() { Tick = 1, EventType = "Healed", ActorName = "Elira" },
            E(1, "TurnEnd", "Priestess")
        };
        var diceLog = new List<CombatLogEntry>
        {
            E(1, "ApiCall", "Priestess"),
            E(1, "ApiCall", "Priestess")
        };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        var actors = merged.Select(e => e.ActorName).ToList();

        // Expected: [TurnStart(Priestess), ApiCall, ApiCall, Healed(Elira), TurnEnd]
        Assert.Equal("TurnStart", types[0]);
        Assert.Equal("Priestess", actors[0]);

        Assert.Equal("ApiCall", types[1]);
        Assert.Equal("ApiCall", types[2]);
        Assert.Equal("Priestess", actors[1]);

        Assert.Equal("Healed", types[3]);
        Assert.Equal("Elira", actors[3]);

        Assert.Equal("TurnEnd", types[4]);
    }

    [Fact]
    public void Merge_SpellQueued_DiceBeforeQueuedEvent()
    {
        // A spellcaster's dice should appear before their SpellQueued event.
        var log = new List<CombatLogEntry>
        {
            E(1, "TurnStart", "Vaelith"),
            E(1, "SpellQueued", "Vaelith"),
            E(1, "TurnStart", "Finnick"),
            E(1, "Attack", "Finnick")
        };
        var diceLog = new List<CombatLogEntry>
        {
            E(1, "ApiCall", "Vaelith"),
            E(1, "ApiCall", "Vaelith"),
            E(1, "ApiCall", "Finnick")
        };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        var actors = merged.Select(e => e.ActorName).ToList();

        // Expected: [TurnStart(V), VaelithDice(2), SpellQueued(V), TurnStart(F), FinnickDice(1), Attack(F)]
        Assert.Equal("TurnStart", types[0]);
        Assert.Equal("Vaelith", actors[0]);

        Assert.Equal("ApiCall", types[1]);
        Assert.Equal("ApiCall", types[2]);
        Assert.Equal("Vaelith", actors[1]);

        Assert.Equal("SpellQueued", types[3]);
        Assert.Equal("Vaelith", actors[3]);

        Assert.Equal("TurnStart", types[4]);
        Assert.Equal("Finnick", actors[4]);

        Assert.Equal("ApiCall", types[5]);
        Assert.Equal("Finnick", actors[5]);

        Assert.Equal("Attack", types[6]);
        Assert.Equal("Finnick", actors[6]);
    }
}
