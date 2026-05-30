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
            E(1, "TurnStart"),
            E(1, "Attack"),
            E(2, "TurnStart"),
            E(2, "Damage")  // no Attack in tick 2 → dice appended at end
        };
        var diceLog = new List<CombatLogEntry>
        {
            E(1, "ApiCall", "D1"),
            E(2, "ApiCall", "D2")
        };

        var merged = CombatLogMerger.Merge(log, diceLog);

        var types = merged.Select(e => e.EventType).ToList();
        var actors = merged.Select(e => e.ActorName).ToList();
        // Tick 1: dice before Attack
        Assert.Equal("TurnStart", types[0]);
        Assert.Equal("D1", actors[1]);
        Assert.Equal("Attack", types[2]);
        // Tick 2: no Attack → dice after last event
        Assert.Equal("TurnStart", types[3]);
        Assert.Equal("Damage", types[4]);
        Assert.Equal("D2", actors[5]);
    }

    [Fact]
    public void Merge_MultipleDiceAtSameTick_AllInsertedBeforeAttack()
    {
        var log = new List<CombatLogEntry> { E(7, "Attack") };
        var diceLog = new List<CombatLogEntry>
        {
            E(7, "ApiCall", "D1"),
            E(7, "ApiCall", "D2"),
            E(7, "ApiCall", "D3")
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
        // Dice inserted before Attack (the first priority event in the tick)
        Assert.Equal(["RoundStart", "TurnStart", "ApiCall", "Attack", "Damage", "TurnEnd"], types);
    }
}
