using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia.Media;
using Avalonia.Threading;
using BattleArena.Application.Models;
using BattleArena.Gui.ViewModels;
using BattleArena.Presentation;

namespace BattleArena.Gui.Presenters;

internal sealed class AvaloniaCombatPresenter : ICombatPresenter
{
    private readonly MainWindowViewModel _vm;
    private readonly GuiDisplayConfig _config;
    private double _pacingMultiplier;
    private readonly ManualResetEventSlim _waitHandle;
    private readonly Dispatcher _dispatcher;

    public bool AutoMode { get; set; }

    public double PacingMultiplier
    {
        get => _pacingMultiplier;
        set => _pacingMultiplier = Math.Max(0.1, value);
    }

    private static readonly Dictionary<string, int> _delays = new()
    {
        ["TurnStart"] = 600,
        ["Attack"] = 600,
        ["Damage"] = 800,
        ["DoTTick"] = 500,
        ["EffectApplied"] = 500,
        ["EffectResisted"] = 500,
        ["EffectExpired"] = 400,
        ["PetSummoned"] = 500,
        ["PetExpired"] = 500,
        ["RoundStart"] = 500,
        ["RoundEnd"] = 400,
        ["SkippedTurn"] = 600,
        ["FumblePenalty"] = 500,
        ["Death"] = 1200,
        ["KnockedOut"] = 1200,
        ["PerfectParry"] = 800,
        ["DevastatingStrike"] = 1000,
        ["TotalReversal"] = 1000,
    };

    private static readonly Dictionary<string, string> _eventColors = new()
    {
        ["TurnStart"] = "#888",
        ["Attack"] = "#ffffff",
        ["Damage"] = "#ff4444",
        ["DoTTick"] = "#d4a017",
        ["EffectApplied"] = "#d4a017",
        ["EffectResisted"] = "#44cc44",
        ["EffectExpired"] = "#888",
        ["PetSummoned"] = "#cc44cc",
        ["PetExpired"] = "#888",
        ["RoundStart"] = "#d4a017",
        ["RoundEnd"] = "#888",
        ["SkippedTurn"] = "#d4a017",
        ["FumblePenalty"] = "#d4a017",
        ["Death"] = "#ff4444",
        ["KnockedOut"] = "#d4a017",
        ["ManaRegen"] = "#cc44cc",
        ["ManaDeduct"] = "#cc44cc",
        ["ApiCall"] = "#00bfff",
        ["PerfectParry"] = "#44cc44",
        ["DevastatingStrike"] = "#cc44cc",
        ["TotalReversal"] = "#d4a017",
    };

    public AvaloniaCombatPresenter(
        MainWindowViewModel vm,
        GuiDisplayConfig config,
        ManualResetEventSlim waitHandle,
        Dispatcher dispatcher)
    {
        _vm = vm;
        _config = config;
        _waitHandle = waitHandle;
        _dispatcher = dispatcher;
        _pacingMultiplier = 1.0;
    }

    public void ShowInitialScreen(CombatDisplayState state, int tick)
    {
        _dispatcher.Post(() =>
        {
            _vm.UpdateFromState(state, tick);
            _vm.AddLogEntry("═══════ Combat starting ═══════", new SolidColorBrush(Color.Parse("#888")));
        });
    }

    public void WaitForCombatStart()
    {
        Thread.Sleep(500);
    }

    public void RefreshScreen(CombatDisplayState state, int tick, string? activeActorName)
    {
        _dispatcher.Post(() =>
        {
            _vm.ActiveActorName = activeActorName ?? "";
            _vm.UpdateFromState(state, tick);
        });
    }

    public void ShowCombatEvent(CombatLogEntry entry, CombatDisplayState state)
    {
        var text = FormatEntry(entry);
        if (string.IsNullOrEmpty(text)) return;

        _dispatcher.Post(() =>
        {
            _vm.UpdateFromState(state, entry.Tick);
            var hex = _eventColors.GetValueOrDefault(entry.EventType, "#ffffff");
            var brush = new SolidColorBrush(Color.Parse(hex));
            _vm.AddLogEntry(text, brush);
        });
    }

    public int GetEventDelayMs(string eventType) =>
        (int)(_delays.GetValueOrDefault(eventType, 300) * _pacingMultiplier);

    public void Wait(int milliseconds)
    {
        var adjusted = (int)(milliseconds * _pacingMultiplier);
        if (adjusted > 0)
            Thread.Sleep(adjusted);
    }

    public void ShowTurnHeader(int turnNumber, string actorName, string? targetName, bool isHero)
    {
        _dispatcher.Post(() =>
        {
            var arrow = isHero ? "\u2192" : "\u2190";
            _vm.AddLogEntry(
                $"  \u2500 Turn {turnNumber}  \u2502  {actorName.ToUpper()}  {arrow}  {targetName?.ToUpper() ?? "?"}",
                new SolidColorBrush(Color.Parse("#888")));
        });
    }

    public void WaitForNextTurn(bool combatOver)
    {
        _dispatcher.Post(() => _vm.CombatOver = combatOver);
        if (AutoMode)
        {
            Thread.Sleep((int)(600 * PacingMultiplier));
        }
        else
        {
            _waitHandle.Wait();
            _waitHandle.Reset();
        }
    }

    public void ShowQuietTicksSummary(int fromTick, int toTick)
    {
        _dispatcher.Post(() =>
            _vm.AddLogEntry($"  ... {toTick - fromTick + 1} quiet ticks (TM building)", new SolidColorBrush(Color.Parse("#555"))));
    }

    private string FormatEntry(CombatLogEntry e) => e.EventType switch
    {
        "TurnStart" => $"  \u25b6 {e.ActorName}  readies  [{e.AttackSourceName}]  \u2192  {e.TargetName}",
        "Attack" => FormatAttack(e),
        "Damage" => FormatDamage(e),
        "DoTTick" => $"  \u2193 {e.ActorName}  suffers {e.DamageDealt}  {e.StatusEffectName ?? "DoT"} damage",
        "EffectApplied" => $"  \u2605 {e.ActorName}  is afflicted with  {e.StatusEffectName}!",
        "EffectResisted" => $"  \u2713 {e.ActorName}  resists  {e.StatusEffectName}  (rolled {e.ResistRoll} vs {e.ResistThreshold})",
        "EffectExpired" => $"  \u25cb {e.StatusEffectName}  has worn off  {e.ActorName}",
        "FumblePenalty" => $"  \u26a0 {e.Message}",
        "PetSummoned" => $"  \u2726 {e.SummonedPetName} has been summoned!",
        "PetExpired" => $"  \u2726 {e.SummonedPetName} fades away...",
        "SkippedTurn" => $"  \u2298 {e.ActorName}  {e.Message.Split("is ")[^1]}",
        "RoundStart" => $"  \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550 ROUND {e.RoundNumber} \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550",
        "RoundEnd" => $"  \u2500\u2500 end of round {e.RoundNumber} \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
        "ManaRegen" => $"  \u266a {e.ActorName}  regen  +{e.ManaRegen} mana",
        "ManaDeduct" => $"  \u25c6 {e.ActorName}  casts  {e.AttackSourceName}  (-{e.ManaCost} mana)",
        "ApiCall" => $"  \u26a1 {e.Message}",
        "PerfectParry" => $"  \u29df {e.ActorName}  PERFECT PARRY!  (both rolled 20)",
        "DevastatingStrike" => $"  \u2620 {e.ActorName}  DEVASTATING STRIKE!  triple damage!",
        "TotalReversal" => $"  \u21bb {e.ActorName}  TOTAL REVERSAL!  fumble flipped!",
        "TurnEnd" => "",
        _ => $"  [{e.EventType}] {e.Message}"
    };

    private string FormatAttack(CombatLogEntry e)
    {
        var total = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
        var margin = total - (e.DefensePower ?? 0);

        var line = $"  {e.ActorName}  attacks with  [{e.AttackSourceName ?? "?"}]";

        if (_config.IsFieldEnabled("attackEvent", "DieRoll") ||
            _config.IsFieldEnabled("attackEvent", "AttackPower") ||
            _config.IsFieldEnabled("attackEvent", "DefensePower"))
        {
            var detail = "     d20=";
            if (_config.IsFieldEnabled("attackEvent", "DieRoll"))
                detail += $"{e.DieRoll,2}";
            if (_config.IsFieldEnabled("attackEvent", "AttackPower"))
                detail += $"  ATK {e.AttackPower}";
            detail += $"  =  total {total,2}";
            if (_config.IsFieldEnabled("attackEvent", "DefensePower"))
                detail += $"  vs DEF {e.DefensePower}";
            var marginStr = margin >= 0 ? $"+{margin}" : $"{margin}";
            detail += $"  |  margin {marginStr}";

            line += $"\n  {detail}";
        }

        if (_config.IsFieldEnabled("attackEvent", "IsHit") && e.IsHit == true)
        {
            var dmg = e.DamageDealt ?? 0;
            line += $"\n     {DamageLabel(dmg)}   Dmg: {dmg}";
        }
        else if (e.IsFumble == true)
        {
            line += "\n     \u26a0 FUMBLE";
        }
        else
        {
            var hitLabel = margin >= -3 ? "NEAR MISS" : "MISS";
            line += $"\n     {hitLabel}";
        }

        if (!string.IsNullOrEmpty(e.Phrase))
            line += $"\n     \"{e.Phrase}\"";

        return line;
    }

    private string FormatDamage(CombatLogEntry e)
    {
        var line = $"  {e.ActorName}  takes  {e.DamageDealt}  damage";
        if (_config.IsFieldEnabled("damageEvent", "TargetHpBefore") ||
            _config.IsFieldEnabled("damageEvent", "TargetHpAfter"))
        {
            line += "  [";
            if (_config.IsFieldEnabled("damageEvent", "TargetHpBefore"))
                line += $"{e.TargetHpBefore}";
            if (_config.IsFieldEnabled("damageEvent", "TargetHpAfter"))
                line += $" \u2192 {Math.Max(0, e.TargetHpAfter ?? 0)} HP";
            line += "]";
        }
        return line;
    }

    private static string DamageLabel(int damage)
    {
        return damage switch
        {
            <= 0 => "GRAZE",
            < 3 => "GRAZE",
            < 8 => "GLANCING HIT",
            < 15 => "SOLID HIT",
            < 25 => "HEAVY HIT",
            _ => "CRUSHING HIT"
        };
    }

}
