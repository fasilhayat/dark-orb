namespace BattleArena.Presentation;

using BattleArena.Application.Models;

/// <summary>
/// Merges API dice-call log entries into the main combat log.
/// ApiCall entries are inserted immediately BEFORE the priority event
/// (Attack, SpellQueued, KnockedOut, Death) of the same actor in the same tick,
/// so dice rolls visually precede the outcome they produced.
/// Falls back to appending after the last event of the tick when no matching
/// priority event exists for that actor.
/// </summary>
public static class CombatLogMerger
{
    private static readonly string[] _insertBeforePriority = ["Attack", "SpellQueued", "KnockedOut", "Death", "Healed"];

    public static List<CombatLogEntry> Merge(
        List<CombatLogEntry> log,
        List<CombatLogEntry>? diceLog)
    {
        if (diceLog is not { Count: > 0 }) return log;

        var diceByKey = diceLog
            .GroupBy(d => (d.Tick, d.ActorName ?? ""))
            .ToDictionary(g => g.Key, g => g.ToList());

        var merged = new List<CombatLogEntry>(log.Count + diceLog.Count);
        var insertedKeys = new HashSet<(int Tick, string Actor)>();

        string? currentActor = null;

        for (int i = 0; i < log.Count; i++)
        {
            var entry = log[i];
            if (entry.EventType == "TurnStart")
                currentActor = entry.ActorName;

            var key = (entry.Tick, entry.ActorName ?? "");

            // Healed events store ActorName as the target, but dice are logged
            // under the caster name. Use the current turn's actor instead.
            if (entry.EventType == "Healed" && currentActor != null)
                key = (entry.Tick, currentActor);

            // Insert this actor's dice before their first priority event
            if (Array.IndexOf(_insertBeforePriority, entry.EventType) >= 0
                && diceByKey.TryGetValue(key, out var dice)
                && insertedKeys.Add(key))
            {
                merged.AddRange(dice);
            }

            merged.Add(entry);

            // After the last event of a tick, append any uninserted dice for that tick
            bool isLastOfTick = i == log.Count - 1 || log[i + 1].Tick != entry.Tick;
            if (isLastOfTick)
            {
                foreach (var dk in diceByKey.Keys.Where(k => k.Tick == entry.Tick))
                {
                    if (insertedKeys.Add(dk))
                        merged.AddRange(diceByKey[dk]);
                }
            }
        }

        // Dice for ticks that had no matching log events at all
        foreach (var kvp in diceByKey)
        {
            if (insertedKeys.Add(kvp.Key))
                merged.AddRange(kvp.Value);
        }

        return merged;
    }
}
