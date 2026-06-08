namespace BattleArena.Demo;

using Application.Interfaces;
using Application.Models;

internal sealed class CombatConsoleObserver : ICombatObserver
{
    private readonly Action<int> _paced;

    public CombatConsoleObserver(Action<int>? paced = null)
    {
        _paced = paced ?? (_ => { });
    }

    public Task OnEventAsync(CombatLogEntry entry, CancellationToken ct = default)
    {
        switch (entry.EventType)
        {
            case "TurnStart":
                Demo.CW($"\n  ▶ {entry.ActorName}", ConsoleColor.White);
                Demo.CW($"  [{entry.AttackSourceName}]", entry.IsSpell == true ? ConsoleColor.Magenta : ConsoleColor.Yellow);
                Demo.CWL($"  →  {entry.TargetName}", ConsoleColor.Gray);
                break;
            case "Attack":
                Demo.PrintAttack(entry);
                break;
            case "Damage":
                Demo.CW($"     {entry.ActorName}  takes  ");
                Demo.CWL($"{entry.DamageDealt}  damage", ConsoleColor.Red);
                break;
            case "TurnEnd":
                Demo.CWL($"     TM: {entry.TurnMeterBefore} → {entry.TurnMeterAfter}", ConsoleColor.Gray);
                _paced(800);
                break;
            case "Death":
                Demo.CWL($"\n  ✝  {entry.Message}", ConsoleColor.Red);
                _paced(1200);
                break;
            case "KnockedOut":
                Demo.CWL($"\n  ⊘  {entry.Message}", ConsoleColor.Yellow);
                _paced(800);
                break;
            case "SkippedTurn":
                Demo.CWL($"\n  ⊘  {entry.ActorName}  {entry.Message}", ConsoleColor.Yellow);
                break;
            case "EffectApplied":
                Demo.CW("     ★ ", ConsoleColor.Yellow);
                Demo.CWL($"{entry.ActorName}  afflicted with  {entry.StatusEffectName}", ConsoleColor.Yellow);
                break;
            case "EffectResisted":
                Demo.CW("     ✓ ", ConsoleColor.Green);
                Demo.CWL($"{entry.ActorName}  resists  {entry.StatusEffectName}", ConsoleColor.Green);
                break;
            case "DoTTick":
                Demo.CW("     ↓ ", ConsoleColor.Yellow);
                Demo.CWL($"{entry.ActorName}  suffers {entry.DamageDealt}  {entry.StatusEffectName} damage", ConsoleColor.Yellow);
                break;
            case "Move":
                Demo.CW("     ➤ ", ConsoleColor.Cyan);
                Demo.CWL(entry.Message, ConsoleColor.Cyan);
                break;
            case "RoundStart":
                Demo.CWL($"\n  ════════════  ROUND {entry.RoundNumber}  ════════════\n", ConsoleColor.Yellow);
                break;
            case "RoundEnd":
                Demo.CWL($"  ── end of round {entry.RoundNumber} ──\n", ConsoleColor.Gray);
                break;
            case "ManaDeduct":
                Demo.CW("     ◆ ", ConsoleColor.Magenta);
                Demo.CWL($"{entry.ActorName}  casts {entry.AttackSourceName}  (-{entry.ManaCost} mana)", ConsoleColor.Magenta);
                break;
            case "LeechTick":
                var leechRes = entry.LeechResourceType == "Mana" ? "mana" : "HP";
                Demo.CW("     ◇ ", ConsoleColor.Magenta);
                Demo.CW($"{entry.ActorName}  loses {entry.LeechAmount} {leechRes}  →  ");
                Demo.CW($"{entry.LeechCasterName}", ConsoleColor.Green);
                Demo.CWL($"  [+{entry.LeechAmount} {leechRes}]", ConsoleColor.Magenta);
                break;
            case "SpellQueued":
                Demo.CW("     ⏳ ", ConsoleColor.Magenta);
                Demo.CWL(entry.Message, ConsoleColor.Magenta);
                break;
            case "SpellDisrupted":
                Demo.CW("     ⚡ ", ConsoleColor.Yellow);
                Demo.CWL(entry.Message, ConsoleColor.Yellow);
                break;
            case "SpellLost":
                Demo.CW("     ✘ ", ConsoleColor.Red);
                Demo.CWL(entry.Message, ConsoleColor.Red);
                break;
        }
        return Task.CompletedTask;
    }
}
