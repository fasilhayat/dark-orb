namespace BattleArena.Demo;

using Application.Models;
using BattleArena.Presentation;

internal sealed class ConsoleCombatPresenter : ICombatPresenter
{
    public VisualEventBus VisualEventBus { get; } = new();

    private readonly GuiDisplayConfig _config;
    private readonly IReadOnlyDictionary<string, int> _maxHp;
    private readonly Action<CombatDisplayState, int, string?> _drawScreen;
    private readonly Action<int> _paced;
    private readonly Dictionary<string, Action<CombatLogEntry, CombatDisplayState>> _eventHandlers;

    private static readonly Dictionary<string, int> _delays = new()
    {
        ["TurnStart"] = 900,
        ["Attack"] = 900,
        ["Damage"] = 1100,
        ["TurnEnd"] = 300,
        ["DoTTick"] = 700,
        ["HoTTick"] = 700,
        ["Healed"] = 800,
        ["EffectApplied"] = 700,
        ["EffectResisted"] = 700,
        ["EffectExpired"] = 500,
        ["PetSummoned"] = 700,
        ["PetExpired"] = 700,
        ["RoundStart"] = 700,
        ["RoundEnd"] = 500,
        ["SkippedTurn"] = 900,
        ["FumblePenalty"] = 600,
        ["Death"] = 1800,
        ["KnockedOut"] = 1800,
    };

    public ConsoleCombatPresenter(
        GuiDisplayConfig config,
        IReadOnlyDictionary<string, int> maxHp,
        Action<CombatDisplayState, int, string?> drawScreen,
        Action<int> paced)
    {
        _config = config;
        _maxHp = maxHp;
        _drawScreen = drawScreen;
        _paced = paced;
        _eventHandlers = BuildHandlers();
    }

    public void ShowInitialScreen(CombatDisplayState state, int tick)
    {
        _drawScreen(state, tick, null);
        Console.WriteLine();
    }

    public void WaitForCombatStart()
    {
        Demo.CWL("  Press any key for first action...", ConsoleColor.Gray);
        Console.ReadKey(true);
    }

    public void RefreshScreen(CombatDisplayState state, int tick, string? activeActorName)
        => _drawScreen(state, tick, activeActorName);

    public void ShowCombatEvent(CombatLogEntry entry, CombatDisplayState state)
    {
        if (_eventHandlers.TryGetValue(entry.EventType, out var handler))
            handler(entry, state);
    }

    public int GetEventDelayMs(string eventType) => _delays.GetValueOrDefault(eventType);

    public void Wait(int milliseconds) => _paced(milliseconds);

    public void ShowTurnHeader(int turnNumber, string actorName, string? targetName, bool isHero)
    {
        Console.WriteLine();
        Demo.CW($"  Turn {turnNumber}  ", ConsoleColor.Gray);
        Demo.CW("|  ", ConsoleColor.Gray);
        Demo.CW(actorName.ToUpper(), isHero ? ConsoleColor.Cyan : ConsoleColor.Red);
        Demo.CW("  →  ", ConsoleColor.Gray);
        Demo.CWL(targetName?.ToUpper() ?? "?", ConsoleColor.White);
        Console.WriteLine();
    }

    public void WaitForNextTurn(bool combatOver)
    {
        Console.WriteLine();
        Demo.CWL("  " + new string('-', 77), ConsoleColor.Gray);
        Demo.CWL(combatOver
            ? "  Combat over!  Press any key for results..."
            : "  Press any key for next turn...", ConsoleColor.Gray);
        Console.ReadKey(true);
    }

    public void ShowQuietTicksSummary(int fromTick, int toTick)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"\n  ... {toTick - fromTick + 1} quiet ticks (TM building)");
        Console.ResetColor();
        _paced(300);
    }

    public void ClearAllPersistentEffects() { }

    public void ShowCombatEventOverlay(string actorName, string? targetName, string effectType)
    {
        var color = effectType switch
        {
            "PerfectParry"      => ConsoleColor.Green,
            "DevastatingStrike" => ConsoleColor.Magenta,
            "TotalReversal"     => ConsoleColor.Yellow,
            _                   => ConsoleColor.White,
        };
        Console.WriteLine();
        Demo.CWL($"  \u2726 {effectType.ToUpper()} \u2726", color);
        Console.WriteLine($"    {actorName}  \u2194  {targetName ?? "?"}");
        Console.ResetColor();
    }

    private void HandleRoundStart(CombatLogEntry entry, CombatDisplayState _)
    {
        Console.WriteLine();
        Demo.CWL($"  ══════════════════ ROUND {entry.RoundNumber} ══════════════════", ConsoleColor.Yellow);
    }

    private void HandleRoundEnd(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CWL($"  ── end of round {entry.RoundNumber} ──────────────────────────", ConsoleColor.Gray);
        Console.WriteLine();
    }

    private void HandleTurnStart(CombatLogEntry entry, CombatDisplayState state)
    {
        var verb = entry.IsSpell == true ? "conjures" : "readies";

        Console.WriteLine();
        Demo.CWL("  " + new string('·', 77), ConsoleColor.Gray);
        Demo.CW("  ▶ ", ConsoleColor.White);
        Demo.CW(entry.ActorName.ToUpper(), state.IsHeroSide(entry.ActorName) ? ConsoleColor.Cyan : ConsoleColor.Red);
        Demo.CW($"  {verb}  ", ConsoleColor.Gray);
        Demo.CW($"[{entry.AttackSourceName}]", entry.IsSpell == true ? ConsoleColor.Magenta : ConsoleColor.Yellow);
        Demo.CW("  →  ", ConsoleColor.Gray);
        Demo.CWL(entry.TargetName ?? "?", state.IsHeroSide(entry.TargetName) ? ConsoleColor.Cyan : ConsoleColor.Red);
    }

    private static void HandleAttack(CombatLogEntry entry, CombatDisplayState _) => Demo.PrintAttack(entry);

    private void HandleDamage(CombatLogEntry entry, CombatDisplayState _)
    {
        var maxHp = _maxHp.GetValueOrDefault(entry.ActorName, 1);
        Demo.CW($"     {entry.ActorName}", ConsoleColor.White);
        Demo.CW("  takes  ");
        if (_config.IsFieldEnabled("damageEvent", "DamageDealt"))
            Demo.CW($"{entry.DamageDealt}", ConsoleColor.Red);
        Demo.CW("  damage   ");
        if (_config.IsFieldEnabled("damageEvent", "TargetHpBefore") ||
            _config.IsFieldEnabled("damageEvent", "TargetHpAfter"))
        {
            Demo.CW("[", ConsoleColor.Gray);
            if (_config.IsFieldEnabled("damageEvent", "TargetHpBefore"))
            {
                Demo.CW($"{entry.TargetHpBefore}", ConsoleColor.Gray);
                Demo.CW(" → ", ConsoleColor.Gray);
            }

            if (_config.IsFieldEnabled("damageEvent", "TargetHpAfter"))
            {
                Demo.CW($"{Math.Max(0, entry.TargetHpAfter ?? 0)}", Demo.HpColorInline(entry.TargetHpAfter ?? 0, maxHp));
                Demo.CW("/", ConsoleColor.Gray);
                Demo.CW($"{maxHp}", ConsoleColor.Gray);
            }

            Demo.CWL(" HP]", ConsoleColor.Gray);
        }
        else
        {
            Console.WriteLine();
        }
    }

    private static void HandleFumblePenalty(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ⚠ ", ConsoleColor.Yellow);
        Demo.CWL(entry.Message, ConsoleColor.Yellow);
    }

    private static void HandleDoTTick(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ↓ ", ConsoleColor.Yellow);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CW("  suffers  ");
        Demo.CW($"{entry.DamageDealt}", ConsoleColor.Red);
        Demo.CW($"  {entry.StatusEffectName ?? "DoT"} damage", ConsoleColor.Yellow);
        Console.WriteLine();
    }

    private static void HandleHoTTick(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ↑ ", ConsoleColor.Green);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CW("  recovers  ");
        Demo.CW($"{entry.DamageDealt}", ConsoleColor.Green);
        Demo.CW($"  HP from {entry.StatusEffectName ?? "HoT"}", ConsoleColor.Green);
        Console.WriteLine();
    }

    private static void HandleHealed(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ♥ ", ConsoleColor.Green);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CW("  healed for  ");
        Demo.CW($"{entry.DamageDealt}", ConsoleColor.Green);
        Demo.CW($"  HP by {entry.AttackSourceName ?? "spell"}", ConsoleColor.Green);
        Console.WriteLine();
    }

    private static void HandleEffectApplied(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ★ ", ConsoleColor.Yellow);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CWL($"  is afflicted with  {entry.StatusEffectName}!", ConsoleColor.Yellow);
    }

    private static void HandleEffectResisted(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ✓ ", ConsoleColor.Green);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CW("  resists  ");
        Demo.CW(entry.StatusEffectName ?? "the effect", ConsoleColor.Green);
        Demo.CWL($"   (rolled {entry.ResistRoll} vs {entry.ResistThreshold})", ConsoleColor.Gray);
    }

    private static void HandleEffectExpired(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ○ ", ConsoleColor.Gray);
        Demo.CW(entry.StatusEffectName ?? string.Empty, ConsoleColor.Green);
        Demo.CW("  has worn off  ");
        Demo.CWL(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
    }

    private static void HandlePetSummoned(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ✦ ", ConsoleColor.Magenta);
        Demo.CW(entry.SummonedPetName ?? "Unknown pet", ConsoleColor.White);
        Demo.CWL(" has been summoned!", ConsoleColor.Magenta);
    }

    private static void HandlePetExpired(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ✦ ", ConsoleColor.Gray);
        Demo.CWL($"{entry.SummonedPetName} fades away...", ConsoleColor.Gray);
    }

    private static void HandleSkippedTurn(CombatLogEntry entry, CombatDisplayState _)
    {
        Console.WriteLine();
        Demo.CWL("  " + new string('·', 77), ConsoleColor.Gray);
        Demo.CW("  ⊘ ", ConsoleColor.Yellow);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CW("  ");
        Demo.CWL(entry.Message.Split("is ")[^1], ConsoleColor.Yellow);
    }

    private static void HandleTurnEnd(CombatLogEntry _, CombatDisplayState _1)
    {
    }

    private static void HandleDeath(CombatLogEntry entry, CombatDisplayState _)
    {
        Console.WriteLine();
        Demo.CWL("  " + new string('*', 65), ConsoleColor.Red);
        Demo.CWL($"  ✝  {entry.Message}", ConsoleColor.Red);
        Demo.CWL("  " + new string('*', 65), ConsoleColor.Red);
    }

    private static void HandleKnockedOut(CombatLogEntry entry, CombatDisplayState _)
    {
        Console.WriteLine();
        Demo.CWL("  " + new string('~', 65), ConsoleColor.Yellow);
        Demo.CWL($"  ⊘  {entry.Message}", ConsoleColor.Yellow);
        Demo.CWL("  " + new string('~', 65), ConsoleColor.Yellow);
    }

    private static void HandleApiCall(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("     ⚡ ", ConsoleColor.Cyan);
        Demo.CWL(entry.Message, ConsoleColor.Cyan);
    }

    private static void HandleManaRegen(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ♪ ", ConsoleColor.Magenta);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CW("  regen  ");
        Demo.CWL($" +{entry.ManaRegen} mana", ConsoleColor.Magenta);
    }

    private static void HandleManaDeduct(CombatLogEntry entry, CombatDisplayState _)
    {
        Demo.CW("  ◆ ", ConsoleColor.Magenta);
        Demo.CW(entry.ActorName, Demo.CharColor(entry.ActorName, entry.ActiveActorName));
        Demo.CW("  casts  ");
        Demo.CW(entry.AttackSourceName ?? "unknown", ConsoleColor.Magenta);
        Demo.CWL($"  (-{entry.ManaCost} mana)", ConsoleColor.Magenta);
    }

    private Dictionary<string, Action<CombatLogEntry, CombatDisplayState>> BuildHandlers() => new()
    {
        ["RoundStart"] = HandleRoundStart,
        ["RoundEnd"] = HandleRoundEnd,
        ["TurnStart"] = HandleTurnStart,
        ["Attack"] = HandleAttack,
        ["Damage"] = HandleDamage,
        ["FumblePenalty"] = HandleFumblePenalty,
        ["DoTTick"] = HandleDoTTick,
        ["HoTTick"] = HandleHoTTick,
        ["Healed"] = HandleHealed,
        ["EffectApplied"] = HandleEffectApplied,
        ["EffectResisted"] = HandleEffectResisted,
        ["EffectExpired"] = HandleEffectExpired,
        ["PetSummoned"] = HandlePetSummoned,
        ["PetExpired"] = HandlePetExpired,
        ["SkippedTurn"] = HandleSkippedTurn,
        ["TurnEnd"] = HandleTurnEnd,
        ["Death"] = HandleDeath,
        ["KnockedOut"] = HandleKnockedOut,
        ["ApiCall"] = HandleApiCall,
        ["ManaRegen"] = HandleManaRegen,
        ["ManaDeduct"] = HandleManaDeduct,
    };
}
