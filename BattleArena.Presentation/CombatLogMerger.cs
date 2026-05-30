namespace BattleArena.Presentation;

using BattleArena.Application.Models;

/// <summary>
/// Merges API dice-call log entries into the main combat log.
/// ApiCall entries are inserted AFTER the last main event of the same tick,
/// so dice rolls appear chronologically after the action they belong to.
/// </summary>
public static class CombatLogMerger
{
    public static List<CombatLogEntry> Merge(
        List<CombatLogEntry> log,
        List<CombatLogEntry>? diceLog)
    {
        if (diceLog is not { Count: > 0 }) return log;

        var diceByTick = diceLog
            .GroupBy(d => d.Tick)
            .ToDictionary(g => g.Key, g => g.ToList());

        var merged = new List<CombatLogEntry>(log.Count + diceLog.Count);
        var insertedTicks = new HashSet<int>();

        for (int i = 0; i < log.Count; i++)
        {
            merged.Add(log[i]);

            bool isLastOfTick = i == log.Count - 1 || log[i + 1].Tick != log[i].Tick;
            if (isLastOfTick
                && insertedTicks.Add(log[i].Tick)
                && diceByTick.TryGetValue(log[i].Tick, out var dice))
            {
                merged.AddRange(dice);
            }
        }

        foreach (var kvp in diceByTick.OrderBy(kv => kv.Key))
        {
            if (!insertedTicks.Contains(kvp.Key))
                merged.AddRange(kvp.Value);
        }

        return merged;
    }
}
