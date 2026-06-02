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

    private static readonly IBrush Gray = MakeBrush("#888");
    private static readonly IBrush DarkGray = MakeBrush("#666");
    private static readonly IBrush White = MakeBrush("#fff");
    private static readonly IBrush Cyan = MakeBrush("#00bfff");
    private static readonly IBrush Yellow = MakeBrush("#d4a017");
    private static readonly IBrush Red = MakeBrush("#ff4444");
    private static readonly IBrush Green = MakeBrush("#44cc44");
    private static readonly IBrush Magenta = MakeBrush("#cc44cc");
    private static readonly IBrush Dim = MakeBrush("#555");
    private static readonly IBrush HeroName = MakeBrush("#88bbff");
    private static readonly IBrush EnemyName = MakeBrush("#ff8888");

    public bool AutoMode { get; set; }

    public double PacingMultiplier
    {
        get => _pacingMultiplier;
        set => _pacingMultiplier = Math.Max(0.1, value);
    }

    private static readonly Dictionary<string, int> _delays = new()
    {
        ["TurnStart"] = 600, ["Attack"] = 600, ["Damage"] = 800,
        ["DoTTick"] = 500, ["EffectApplied"] = 500, ["EffectResisted"] = 500,
        ["EffectExpired"] = 400, ["PetSummoned"] = 500, ["PetExpired"] = 500,
        ["RoundStart"] = 500, ["RoundEnd"] = 400, ["SkippedTurn"] = 600,
        ["FumblePenalty"] = 500, ["Death"] = 1200, ["KnockedOut"] = 1200,
        ["PerfectParry"] = 800, ["DevastatingStrike"] = 1000, ["TotalReversal"] = 1000,
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
            _vm.AddLogEntry([Seg("\u2550\u2550\u2550\u2550\u2550\u2550\u2550 Combat starting \u2550\u2550\u2550\u2550\u2550\u2550\u2550", Dim)]);
        });
    }

    public void WaitForCombatStart() => Thread.Sleep(500);

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
        var segments = BuildSegments(entry, state);
        if (segments.Count == 0) return;

        _dispatcher.Post(() =>
        {
            _vm.UpdateFromState(state, entry.Tick);
            _vm.AddLogEntry(segments);
        });
    }

    public int GetEventDelayMs(string eventType) =>
        (int)(_delays.GetValueOrDefault(eventType, 300) * _pacingMultiplier);

    public void Wait(int milliseconds)
    {
        var adjusted = (int)(milliseconds * _pacingMultiplier);
        if (adjusted > 0) Thread.Sleep(adjusted);
    }

    public void ShowTurnHeader(int turnNumber, string actorName, string? targetName, bool isHero)
    {
        _dispatcher.Post(() =>
        {
            var nameColor = NameBrush(isHero, actorName, null);
            var targetColor = NameBrush(!isHero, targetName, null);
            var arrow = isHero ? "\u2192" : "\u2190";
            _vm.AddLogEntry([
                Seg($"  \u2500 Turn {turnNumber}  \u2502  ", Gray),
                Seg(actorName.ToUpper(), nameColor),
                Seg($"  {arrow}  ", Gray),
                Seg(targetName?.ToUpper() ?? "?", targetColor),
            ]);
        });
    }

    public void WaitForNextTurn(bool combatOver)
    {
        _dispatcher.Post(() => _vm.CombatOver = combatOver);
        if (AutoMode)
            Thread.Sleep((int)(600 * PacingMultiplier));
        else
        {
            _waitHandle.Wait();
            _waitHandle.Reset();
        }
    }

    public void ShowQuietTicksSummary(int fromTick, int toTick)
    {
        _dispatcher.Post(() =>
            _vm.AddLogEntry([Seg($"  ... {toTick - fromTick + 1} quiet ticks (TM building)", Dim)]));
    }

    private List<LogSegment> BuildSegments(CombatLogEntry e, CombatDisplayState state) => e.EventType switch
    {
        "TurnStart" => BuildTurnStart(e, state),
        "Attack" => BuildAttack(e, state),
        "Damage" => BuildDamage(e),
        "DoTTick" => BuildDoTTick(e, state),
        "EffectApplied" => BuildEffectApplied(e, state),
        "EffectResisted" => BuildEffectResisted(e, state),
        "EffectExpired" => BuildEffectExpired(e, state),
        "FumblePenalty" => [Seg($"  \u26a0 {e.Message}", Yellow)],
        "PetSummoned" => [Seg($"  \u2726 {e.SummonedPetName ?? "Unknown"} has been summoned!", Magenta),
                          _eol, Seg(e.SummonedPetName ?? "", White)],
        "PetExpired" => [Seg($"  \u2726 {e.SummonedPetName} fades away...", Gray)],
        "SkippedTurn" => BuildSkippedTurn(e, state),
        "RoundStart" => BuildRoundStart(e),
        "RoundEnd" => BuildRoundEnd(e),
        "ManaRegen" => BuildManaRegen(e, state),
        "ManaDeduct" => BuildManaDeduct(e, state),
        "ApiCall" => [Seg("  \u26a1 ", Cyan), Seg(e.Message, DarkGray)],
        "PerfectParry" => BuildPerfectParry(e, state),
        "DevastatingStrike" => BuildDevastatingStrike(e, state),
        "TotalReversal" => BuildTotalReversal(e, state),
        "TurnEnd" => [],
        _ => [Seg($"  [{e.EventType}] {e.Message}", Gray)]
    };

    private List<LogSegment> BuildTurnStart(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        var targetColor = NameBrush(state.IsHeroSide(e.TargetName), e.TargetName, actorColor);
        var srcColor = e.IsSpell == true ? Magenta : Yellow;
        var verb = e.IsSpell == true ? "conjures" : "readies";
        return
        [
            Seg("  \u25b6 ", White),
            Seg((e.ActorName ?? "?").ToUpper(), actorColor),
            Seg($"  {verb}  ", Gray),
            Seg($"[{e.AttackSourceName}]", srcColor),
            Seg("  \u2192  ", Gray),
            Seg(e.TargetName?.ToUpper() ?? "?", targetColor),
        ];
    }

    private List<LogSegment> BuildAttack(CombatLogEntry e, CombatDisplayState state)
    {
        var list = new List<LogSegment>();
        var total = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
        var margin = total - (e.DefensePower ?? 0);
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        var srcColor = e.IsSpell == true ? Magenta : Yellow;

        list.Add(Seg("  ", White));
        list.Add(Seg(e.ActorName, actorColor));
        list.Add(Seg(e.IsSpell ? "  casts  " : "  attacks with  ", Gray));
        list.Add(Seg($"[{e.AttackSourceName ?? "?"}]", srcColor));

        if (_config.IsFieldEnabled("attackEvent", "DieRoll") ||
            _config.IsFieldEnabled("attackEvent", "AttackPower") ||
            _config.IsFieldEnabled("attackEvent", "DefensePower"))
        {
            list.Add(_eol);
            if (_config.IsFieldEnabled("attackEvent", "DieRoll"))
            {
                list.Add(Seg("     d20=", DarkGray));
                list.Add(Seg($"{e.DieRoll,2}", DarkGray));
            }
            if (_config.IsFieldEnabled("attackEvent", "AttackPower"))
            {
                list.Add(Seg("  ATK ", DarkGray));
                list.Add(Seg($"{e.AttackPower}", DarkGray));
            }
            list.Add(Seg("  \u2192  total ", DarkGray));
            list.Add(Seg($"{total,2}", DarkGray));
            if (_config.IsFieldEnabled("attackEvent", "DefensePower"))
            {
                list.Add(Seg("   vs  DEF ", DarkGray));
                list.Add(Seg($"{e.DefensePower}", DarkGray));
            }
            list.Add(Seg("   \u2502  margin ", DarkGray));
            list.Add(Seg(margin >= 0 ? $"+{margin}" : $"{margin}", margin >= 0 ? Green : Red));
        }

        if (_config.IsFieldEnabled("attackEvent", "IsHit"))
        {
            list.Add(_eol);
            list.Add(Seg("     ", White));
            if (_config.IsFieldEnabled("attackEvent", "IsCritical") && e.IsCritical == true)
                list.Add(Seg("\u26a1 CRITICAL HIT", Magenta));
            else if (_config.IsFieldEnabled("attackEvent", "IsFumble") && e.IsFumble == true)
                list.Add(Seg("\u26a0 FUMBLE", Yellow));
            else if (e.IsHit == true)
            {
                var dmg = e.DamageDealt ?? 0;
                var label = DamageLabel(dmg);
                var labelColor = label switch
                {
                    "CRUSHING HIT" => Magenta,
                    "HEAVY HIT" => Yellow,
                    "SOLID HIT" => Green,
                    "GLANCING HIT" => White,
                    _ => Gray,
                };
                list.Add(Seg(label, labelColor));
                if (_config.IsFieldEnabled("attackEvent", "DamageDealt"))
                {
                    list.Add(Seg("   \u2502   ", Gray));
                    list.Add(Seg($"Dmg: {dmg}", Cyan));
                }
            }
            else
            {
                list.Add(Seg(margin >= -3 ? "\u25cb NEAR MISS" : "\u25cb MISS", Red));
            }
        }

        if (!string.IsNullOrEmpty(e.Phrase))
        {
            list.Add(_eol);
            list.Add(Seg($"     \"{e.Phrase}\"", Cyan));
        }

        return list;
    }

    private List<LogSegment> BuildDamage(CombatLogEntry e)
    {
        var list = new List<LogSegment>
        {
            Seg($"  {e.ActorName}  takes  ", White),
            Seg($"{e.DamageDealt}", Red),
            Seg("  damage", White),
        };
        if (_config.IsFieldEnabled("damageEvent", "TargetHpBefore") ||
            _config.IsFieldEnabled("damageEvent", "TargetHpAfter"))
        {
            list.Add(Seg("  [", Gray));
            if (_config.IsFieldEnabled("damageEvent", "TargetHpBefore"))
                list.Add(Seg($"{e.TargetHpBefore}", Gray));
            if (_config.IsFieldEnabled("damageEvent", "TargetHpAfter"))
            {
                list.Add(Seg(" \u2192 ", Gray));
                list.Add(Seg($"{Math.Max(0, e.TargetHpAfter ?? 0)}", White));
                list.Add(Seg("/", Gray));
                list.Add(Seg($"HP", Gray));
            }
            list.Add(Seg("]", Gray));
        }
        return list;
    }

    private static List<LogSegment> BuildDoTTick(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2193 ", Yellow),
            Seg(e.ActorName, actorColor),
            Seg("  suffers  ", Gray),
            Seg($"{e.DamageDealt}", Red),
            Seg($"  {e.StatusEffectName ?? "DoT"} damage", Yellow),
        ];
    }

    private static List<LogSegment> BuildEffectApplied(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2605 ", Yellow),
            Seg(e.ActorName, actorColor),
            Seg($"  is afflicted with  {e.StatusEffectName}!", Yellow),
        ];
    }

    private static List<LogSegment> BuildEffectResisted(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2713 ", Green),
            Seg(e.ActorName, actorColor),
            Seg("  resists  ", Gray),
            Seg(e.StatusEffectName ?? "the effect", Green),
            Seg($"   (rolled {e.ResistRoll} vs {e.ResistThreshold})", Gray),
        ];
    }

    private static List<LogSegment> BuildEffectExpired(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u25cb ", Gray),
            Seg(e.StatusEffectName ?? string.Empty, Green),
            Seg("  has worn off  ", Gray),
            Seg(e.ActorName, actorColor),
        ];
    }

    private static List<LogSegment> BuildSkippedTurn(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2298 ", Yellow),
            Seg(e.ActorName, actorColor),
            Seg("  ", Gray),
            Seg(e.Message.Split("is ")[^1], Yellow),
        ];
    }

    private static List<LogSegment> BuildRoundStart(CombatLogEntry e) =>
    [
        Seg($"  \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550 ROUND {e.RoundNumber} \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550", Yellow),
    ];

    private static List<LogSegment> BuildRoundEnd(CombatLogEntry e) =>
    [
        Seg($"  \u2500\u2500 end of round {e.RoundNumber} \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500", Gray),
    ];

    private static List<LogSegment> BuildManaRegen(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u266a ", Magenta),
            Seg(e.ActorName, actorColor),
            Seg("  regen  ", Gray),
            Seg($"+{e.ManaRegen} mana", Magenta),
        ];
    }

    private static List<LogSegment> BuildManaDeduct(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u25c6 ", Magenta),
            Seg(e.ActorName, actorColor),
            Seg("  casts  ", Gray),
            Seg(e.AttackSourceName ?? "unknown", Magenta),
            Seg($"  (-{e.ManaCost} mana)", Magenta),
        ];
    }

    private static List<LogSegment> BuildPerfectParry(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u29df ", Green),
            Seg(e.ActorName, actorColor),
            Seg("  PERFECT PARRY!  (both rolled 20)", Green),
        ];
    }

    private static List<LogSegment> BuildDevastatingStrike(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2620 ", Magenta),
            Seg(e.ActorName, actorColor),
            Seg("  DEVASTATING STRIKE!  triple damage!", Magenta),
        ];
    }

    private static List<LogSegment> BuildTotalReversal(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u21bb ", Yellow),
            Seg(e.ActorName, actorColor),
            Seg("  TOTAL REVERSAL!  fumble flipped!", Yellow),
        ];
    }

    private static string DamageLabel(int damage) => damage switch
    {
        <= 0 => "GRAZE",
        < 3 => "GRAZE",
        < 8 => "GLANCING HIT",
        < 15 => "SOLID HIT",
        < 25 => "HEAVY HIT",
        _ => "CRUSHING HIT"
    };

    private static IBrush NameBrush(bool isHero, string? name, IBrush? fallback)
    {
        if (name is null) return fallback ?? White;
        return isHero ? HeroName : EnemyName;
    }

    private static IBrush MakeBrush(string hex) => new SolidColorBrush(Color.Parse(hex));
    private static LogSegment Seg(string text, IBrush brush) => new() { Text = text, Brush = brush };
    private static readonly LogSegment _eol = new() { Text = "\n", Brush = Gray };
}
