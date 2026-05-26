namespace BattleArena.Application.Models;

using Core.Entities;
using Core.Entities.Enums;

// The outcome of a full battle simulation run by BattleSimulator.
public class BattleResult
{
    // The party that won and the party that lost.
    // Set for both party and 1v1 battles (1v1 creates single-member parties internally).
    public Party? WinningParty { get; set; }
    public Party? LosingParty  { get; set; }

    // How the last defeated combatant left the fight.
    public CharacterVitalStatus LoserStatus { get; set; } = CharacterVitalStatus.Alive;

    public int TotalTicks { get; set; }
    public List<BattleLogEntry> Log { get; set; } = new();
    public bool MaxTicksReached { get; set; }

    // Convenience accessors for 1v1 results (single-member parties).
    public Character? Winner => WinningParty?.Members.FirstOrDefault()?.Character;
    public Character? Loser  => LosingParty?.Members.FirstOrDefault()?.Character;

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
        if (!MaxTicksReached && WinningParty is not null && LosingParty is not null)
        {
            var loserTag = LoserStatus == CharacterVitalStatus.Dead ? "[DEAD]" : "[KO]";
            sb.AppendLine($"  WINNER: {WinningParty.Name}  |  LOSER: {LosingParty.Name} {loserTag}  |  Ticks: {TotalTicks}");
        }
        else
            sb.AppendLine($"  MAX TICKS REACHED ({TotalTicks}) — battle inconclusive");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        return sb.ToString();
    }
}
