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
    public void Merge_ApiCallAppearsAfterLastMainEventAtSameTick()
    {
        var log = new List<CombatLogEntry>
        {
            E(5, "TurnStart"),
            E(5, "Attack"),
            E(5, "Damage")
        };
        var diceLog = new List<CombatLogEntry> { E(5, "ApiCall") };

        var merged = CombatLogMerger.Merge(log, diceLog);

        Assert.Equal(4, merged.Count);
        Assert.Equal("Damage", merged[2].EventType);
        Assert.Equal("ApiCall", merged[3].EventType);
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
        var attackIdx = merged.FindIndex(e => e.EventType == "Attack");
        Assert.True(apiIdx > attackIdx, "ApiCall must appear after Attack");
    }

    [Fact]
    public void Merge_MultipleTicksEachGetTheirDiceAfterLastEvent()
    {
        var log = new List<CombatLogEntry>
        {
            E(1, "TurnStart"),
            E(1, "Attack"),
            E(2, "TurnStart"),
            E(2, "Damage")
        };
        var diceLog = new List<CombatLogEntry>
        {
            E(1, "ApiCall", "D1"),
            E(2, "ApiCall", "D2")
        };

        var merged = CombatLogMerger.Merge(log, diceLog);

        Assert.Equal(6, merged.Count);
        Assert.Equal("Attack", merged[1].EventType);
        Assert.Equal("D1", merged[2].ActorName);
        Assert.Equal("Damage", merged[4].EventType);
        Assert.Equal("D2", merged[5].ActorName);
    }

    [Fact]
    public void Merge_MultipleDiceAtSameTick_AllAppendedAfterMainEvents()
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
        Assert.Equal("Attack", merged[0].EventType);
        Assert.All(merged.Skip(1), e => Assert.Equal("ApiCall", e.EventType));
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
        Assert.Equal(["RoundStart", "TurnStart", "Attack", "Damage", "TurnEnd", "ApiCall"], types);
    }
}
