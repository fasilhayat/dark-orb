namespace BattleArena.Application.Models;

using Core.Entities;
using Core.Entities.Enums;

// The outcome of a full battle simulation run by BattleSimulator.
public class BattleResult
{
    // Null only when MaxTicksReached = true
    public Character? Winner { get; set; }
    public Character? Loser  { get; set; }

    // How the loser left the fight: KnockedOut (HP 0 to -9) or Dead (HP <= -10).
    public CharacterVitalStatus LoserStatus { get; set; } = CharacterVitalStatus.Alive;

    public int TotalTicks { get; set; }
    public List<BattleLogEntry> Log { get; set; } = new();
    public bool MaxTicksReached { get; set; }

    // Formats the full event log as a human-readable string suitable for test output.
    public string FormatLog()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("  BATTLE LOG");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        foreach (var entry in Log)
        {
            sb.AppendLine($"[Tick {entry.Tick:D3}] {entry.Message}");
            if (!string.IsNullOrEmpty(entry.Phrase))
                sb.AppendLine($"           >> {entry.Phrase}");
        }
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        if (!MaxTicksReached && Winner is not null && Loser is not null)
        {
            var loserTag = LoserStatus == CharacterVitalStatus.Dead ? "[DEAD]" : "[KO]";
            sb.AppendLine($"  WINNER: {Winner.Name} (HP: {Winner.CurrentHitPoints})  |  LOSER: {Loser.Name} (HP: {Loser.CurrentHitPoints}) {loserTag}  |  Ticks: {TotalTicks}");
        }
        else
            sb.AppendLine($"  MAX TICKS REACHED ({TotalTicks}) — battle inconclusive");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        return sb.ToString();
    }
}
