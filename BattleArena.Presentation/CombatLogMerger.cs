namespace BattleArena.Presentation;

using BattleArena.Application.Models;

/// <summary>
/// Merges API dice-call log entries into the main combat log.
/// ApiCall entries are inserted immediately BEFORE the Attack event of the same tick
/// so dice rolls visually precede the outcome they produced (hit/miss, damage calc).
/// Falls back to before the first terminal event (KnockedOut, Death) if no Attack exists,
/// or appends after the last event of the tick when neither is present.
/// </summary>
public static class CombatLogMerger
{
    // Insert dice calls before the first of these events in the tick.
    private static readonly string[] _insertBeforePriority = ["Attack", "KnockedOut", "Death"];

    public static List<CombatLogEntry> Merge(
        List<CombatLogEntry> log,
        List<CombatLogEntry>? diceLog)
    {
        if (diceLog is not { Count: > 0 }) return log;

        var diceByTick = diceLog
            .GroupBy(d => d.Tick)
            .ToDictionary(g => g.Key, g => g.ToList());

        // For each tick with dice, find the index to insert BEFORE.
        // -1 means append after the last event of that tick.
        var insertBeforeIndex = new Dictionary<int, int>();
        for (int i = 0; i < log.Count; i++)
        {
            int tick = log[i].Tick;
            if (!diceByTick.ContainsKey(tick)) continue;

            if (!insertBeforeIndex.ContainsKey(tick))
                insertBeforeIndex[tick] = -1;

            // Only set once: first matching priority event wins
            if (insertBeforeIndex[tick] == -1
                && Array.IndexOf(_insertBeforePriority, log[i].EventType) >= 0)
            {
                insertBeforeIndex[tick] = i;
            }
        }

        var merged = new List<CombatLogEntry>(log.Count + diceLog.Count);
        var insertedTicks = new HashSet<int>();

        for (int i = 0; i < log.Count; i++)
        {
            // Insert dice BEFORE this event if it's the designated insertion point
            int tick = log[i].Tick;
            if (insertBeforeIndex.TryGetValue(tick, out int beforeIdx)
                && beforeIdx == i
                && insertedTicks.Add(tick)
                && diceByTick.TryGetValue(tick, out var diceEarly))
            {
                merged.AddRange(diceEarly);
            }

            merged.Add(log[i]);

            // Append dice after the last event of the tick when no priority event was found
            bool isLastOfTick = i == log.Count - 1 || log[i + 1].Tick != tick;
            if (isLastOfTick
                && insertBeforeIndex.TryGetValue(tick, out int endIdx)
                && endIdx == -1
                && insertedTicks.Add(tick)
                && diceByTick.TryGetValue(tick, out var diceEnd))
            {
                merged.AddRange(diceEnd);
            }
        }

        // Dice for ticks that had no matching log events at all
        foreach (var kvp in diceByTick.OrderBy(kv => kv.Key))
        {
            if (!insertedTicks.Contains(kvp.Key))
                merged.AddRange(kvp.Value);
        }

        return merged;
    }
}
