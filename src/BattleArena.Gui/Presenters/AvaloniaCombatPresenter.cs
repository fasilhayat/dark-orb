using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Application.Services;
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
    private readonly ISoundPlayer? _soundPlayer;
    private volatile bool _stopped;

    private readonly VisualEventBus _visualEventBus = new();
    private readonly Dictionary<string, DispatcherTimer> _effectFlickerTimers = new();

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

    VisualEventBus ICombatPresenter.VisualEventBus => _visualEventBus;

    private static readonly Dictionary<string, int> _delays = new()
    {
        ["TurnStart"] = 600, ["Attack"] = 600, ["Damage"] = 800,
        ["DoTTick"] = 500, ["HoTTick"] = 500, ["Healed"] = 600,
        ["EffectApplied"] = 500, ["EffectResisted"] = 500,
        ["EffectExpired"] = 400, ["PetSummoned"] = 500, ["PetExpired"] = 500,
        ["RoundStart"] = 500, ["RoundEnd"] = 400, ["SkippedTurn"] = 600,
        ["FumblePenalty"] = 500, ["Death"] = 1200, ["KnockedOut"] = 1200,
        ["PerfectParry"] = 800, ["DevastatingStrike"] = 1000, ["TotalReversal"] = 1000,
    };

    public AvaloniaCombatPresenter(
        MainWindowViewModel vm,
        GuiDisplayConfig config,
        ManualResetEventSlim waitHandle,
        Dispatcher dispatcher,
        ISoundPlayer? soundPlayer = null)
    {
        _vm = vm;
        _config = config;
        _waitHandle = waitHandle;
        _dispatcher = dispatcher;
        _soundPlayer = soundPlayer;
        _pacingMultiplier = 1.0;

        _visualEventBus.NormalEventPublished += OnNormalVisualEvent;
        _visualEventBus.IncredibleEventPublished += OnIncredibleVisualEvent;
        if (_soundPlayer is not null)
            _visualEventBus.SoundRequested += OnSoundRequested;
    }

    private void OnSoundRequested(SoundEvent e)
    {
        if (_pacingMultiplier < 0.3)
            return;
        _dispatcher.Post(() => _soundPlayer?.Play(e.SoundId));
    }

    /// <summary>
    /// Stop all active timers, animations, and event subscriptions.
    /// Called when the user navigates away from combat.
    /// </summary>
    public void Stop()
    {
        _stopped = true;
        ClearAllPersistentEffects();
        _dispatcher.Post(() => _vm.ClearOverlay());
        _visualEventBus.NormalEventPublished -= OnNormalVisualEvent;
        _visualEventBus.IncredibleEventPublished -= OnIncredibleVisualEvent;
        _visualEventBus.SoundRequested -= OnSoundRequested;
    }

    private void OnNormalVisualEvent(VisualEvent ev)
    {
        _dispatcher.Post(() =>
        {
            if (ev.IsPersistent && ev.EffectName is not null)
            {
                var target = ev.TargetName ?? ev.ActorName;
                StartPersistentEffect(target, ev.EffectName, ev.Color);
                return;
            }

            if (ev.EffectName == "ClearPersistent")
            {
                var target = ev.TargetName ?? ev.ActorName;
                if (!string.IsNullOrEmpty(target))
                    StopPersistentEffect(target);
                return;
            }

            if (ev.HealAmount > 0 && ev.TargetName is not null)
            {
                var card = FindCard(ev.TargetName);
                if (card is not null)
                {
                    var maxHp = Math.Max(1, card.MaxHp);
                    var healAmount = Math.Min(ev.HealAmount, card.MaxHp);
                    var preHealHp = Math.Max(0, card.Hp - healAmount);
                    var start = (double)preHealHp / maxHp;
                    var width = (double)healAmount / maxHp;
                    AnimateHealGlow(card, start, width);
                }
            }

            FlashBorder(ev.ActorName, ev.Color);
            if (ev.TargetName is not null)
                FlashBorder(ev.TargetName, ev.Color);

            _vm.TriggerOverlay(ev.OverlayText);
            AnimateOverlay();
        });
    }

    private void OnIncredibleVisualEvent(VisualEvent ev)
    {
        _dispatcher.Post(() =>
        {
            FlashBorder(ev.ActorName, ev.Color);
            if (ev.TargetName is not null)
                FlashBorder(ev.TargetName, ev.Color);

            _vm.TriggerOverlay(ev.OverlayText);
            AnimateOverlay();
        });

        var adjusted = (int)(ev.DurationMs * _pacingMultiplier);
        if (adjusted > 0)
            Thread.Sleep(adjusted);

        _visualEventBus.SignalIncredibleComplete();
    }

    public void ShowInitialScreen(CombatDisplayState state, int tick)
    {
        if (_stopped) return;
        _dispatcher.Post(() =>
        {
            _vm.IsApiMode = state.IsApiMode;
            _vm.UpdateFromState(state, tick);
            _vm.AddLogEntry([Seg("\u2550\u2550\u2550\u2550\u2550\u2550\u2550 Combat starting \u2550\u2550\u2550\u2550\u2550\u2550\u2550", Dim)]);
        });
    }

    public void WaitForCombatStart()
    {
        if (_stopped) return;
        Thread.Sleep(500);
    }

    public void RefreshScreen(CombatDisplayState state, int tick, string? activeActorName)
    {
        if (_stopped) return;
        _dispatcher.Post(() =>
        {
            _vm.ActiveActorName = activeActorName ?? "";
            _vm.UpdateFromState(state, tick);
        });
    }

    public void ShowCombatEvent(CombatLogEntry entry, CombatDisplayState state)
    {
        if (_stopped) return;
        var rows = BuildRows(entry, state);
        if (rows.Count == 0) return;

        _dispatcher.Post(() =>
        {
            _vm.UpdateFromState(state, entry.Tick);
            foreach (var row in rows)
                if (row.Count > 0)
                    _vm.AddLogEntry(row);
        });
    }

    public int GetEventDelayMs(string eventType) =>
        _delays.GetValueOrDefault(eventType, 300);

    public void Wait(int milliseconds)
    {
        if (_stopped) return;
        var adjusted = (int)(milliseconds * _pacingMultiplier);
        if (adjusted > 0) Thread.Sleep(adjusted);
    }

    public void ShowTurnHeader(int turnNumber, string actorName, string? targetName, bool isHero)
    {
        if (_stopped) return;
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
        if (_stopped) return;
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
        if (_stopped) return;
        _dispatcher.Post(() =>
            _vm.AddLogEntry([Seg($"  ... {toTick - fromTick + 1} quiet ticks (TM building)", Dim)]));
    }

    public void ShowCombatEventOverlay(string actorName, string? targetName, string effectType)
    {
        if (_stopped) return;
        _dispatcher.Post(() =>
        {
            var color = effectType switch
            {
                "PerfectParry"      => "#44ff44",
                "DevastatingStrike" => "#ff44ff",
                "TotalReversal"     => "#ffff44",
                _                   => "#ffffff",
            };
            var text = effectType switch
            {
                "PerfectParry"      => "PERFECT PARRY!",
                "DevastatingStrike" => "DEVASTATING STRIKE!",
                "TotalReversal"     => "TOTAL REVERSAL!",
                _                   => effectType,
            };

            // Flash borders on both characters
            FlashBorder(actorName, color);
            if (targetName is not null)
                FlashBorder(targetName, color);

            // Show centered overlay text with zoom+dissolve
            _vm.TriggerOverlay(text);
            AnimateOverlay();
        });
    }

    private void FlashBorder(string characterName, string color)
    {
        var card = FindCard(characterName);
        if (card is null) return;
        card.BorderFlashColor = color;
        StartBorderResetTimer(card);
    }

    private CharCardViewModel? FindCard(string name)
    {
        foreach (var h in _vm.Heroes)
            if (h.Name == name) return h;
        foreach (var e in _vm.Enemies)
            if (e.Name == name) return e;
        return null;
    }

    private void StartBorderResetTimer(CharCardViewModel card)
    {
        Task.Delay(1800).ContinueWith(_ =>
        {
            if (_stopped) return;
            _dispatcher.Post(() => card.BorderFlashColor = null);
        });
    }

    private void StartPersistentEffect(string characterName, string effectName, string color)
    {
        StopPersistentEffect(characterName);

        var card = FindCard(characterName);
        if (card is null) return;

        var darkColor = DarkenColor(color);

        switch (effectName)
        {
            case "Burning":
            case "Frozen":
            case "Freeze":
            case "Shocked":
                // Fast flicker (300ms) for elemental effects
                card.PersistentBorderColor = color;
                StartFlickerTimer(characterName, color, darkColor, 300);
                break;

            case "Stun":
                // Slow pulse (800ms) for stun — distinct from elemental flicker
                card.PersistentBorderColor = color;
                StartFlickerTimer(characterName, color, darkColor, 800);
                break;

            default:
                card.PersistentBorderColor = color;
                break;
        }
    }

    private void StartFlickerTimer(string characterName, string color, string darkColor, int intervalMs)
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
        timer.Tick += (_, _) =>
        {
            var c = FindCard(characterName);
            if (c is null)
            {
                timer.Stop();
                _effectFlickerTimers.Remove(characterName);
                return;
            }
            c.PersistentBorderColor = c.PersistentBorderColor == color ? darkColor : color;
        };
        timer.Start();
        _effectFlickerTimers[characterName] = timer;
    }

    public void ClearAllPersistentEffects()
    {
        foreach (var kvp in _effectFlickerTimers)
        {
            kvp.Value.Stop();
            var card = FindCard(kvp.Key);
            if (card is not null)
                card.PersistentBorderColor = null;
        }
        _effectFlickerTimers.Clear();
    }

    private void StopPersistentEffect(string characterName)
    {
        if (_effectFlickerTimers.TryGetValue(characterName, out var timer))
        {
            timer.Stop();
            _effectFlickerTimers.Remove(characterName);
        }

        var card = FindCard(characterName);
        if (card is not null)
            card.PersistentBorderColor = null;
    }

    private void AnimateHealGlow(CharCardViewModel card, double start, double width)
    {
        card.HealGlowOpacity = 0.7;
        card.HealGlowStart = start;
        card.HealGlowFraction = width;

        const int durationMs = 800;
        const int intervalMs = 30;
        var steps = durationMs / intervalMs;

        Task.Run(async () =>
        {
            for (var i = 0; i < steps && !_stopped; i++)
            {
                await Task.Delay(intervalMs);
                if (_stopped) return;
                var t = (double)(i + 1) / steps;
                var opacity = 0.7 * (1.0 - t * t);
                _dispatcher.Post(() =>
                {
                    if (_stopped) return;
                    card.HealGlowOpacity = opacity;
                });
            }
            if (!_stopped)
            {
                _dispatcher.Post(() =>
                {
                    card.HealGlowOpacity = 0;
                    card.HealGlowStart = 0;
                    card.HealGlowFraction = 0;
                });
            }
        });
    }

    private static string DarkenColor(string hex)
    {
        var c = Color.Parse(hex);
        return $"#{(byte)(c.R * 0.5):x2}{(byte)(c.G * 0.5):x2}{(byte)(c.B * 0.5):x2}";
    }

    private void AnimateOverlay()
    {
        const int durationMs = 1500;
        const int intervalMs = 30;
        var steps = durationMs / intervalMs;
        var elapsed = 0;

        Task.Run(async () =>
        {
            for (var i = 0; i < steps && !_stopped; i++)
            {
                await Task.Delay(intervalMs);
                if (_stopped) return;
                elapsed += intervalMs;
                var t = (double)elapsed / durationMs;
                var eased = 1.0 - (1.0 - t) * (1.0 - t);
                var mainScale = 0.25 + eased * 1.95;
                var mainOpacity = Math.Max(0, 1.0 - (eased - 0.08) / 0.92);
                _dispatcher.Post(() =>
                {
                    if (_stopped) return;
                    _vm.OverlayScale = mainScale;
                    _vm.OverlayOpacity = mainOpacity;
                    _vm.OverlayTrailScale1 = mainScale * 0.78;
                    _vm.OverlayTrailOpacity1 = mainOpacity * 0.35;
                    _vm.OverlayTrailScale2 = mainScale * 0.56;
                    _vm.OverlayTrailOpacity2 = mainOpacity * 0.15;
                });
            }
            if (!_stopped)
                _dispatcher.Post(() => _vm.ClearOverlay());
        });
    }

    private List<List<LogSegment>> BuildRows(CombatLogEntry e, CombatDisplayState state)
    {
        var rows = e.EventType switch
        {
            "TurnStart"          => [BuildTurnStartRow(e, state)],
            "Attack"             => BuildAttackRows(e, state),
            "Damage"             => [BuildDamageRow(e)],
            "DoTTick"            => [BuildDoTTickRow(e, state)],
            "HoTTick"            => [BuildHoTTickRow(e, state)],
            "Healed"             => [BuildHealedRow(e, state)],
            "EffectApplied"      => [BuildEffectAppliedRow(e, state)],
            "EffectResisted"     => [BuildEffectResistedRow(e, state)],
            "EffectExpired"      => [BuildEffectExpiredRow(e, state)],
            "FumblePenalty"      => [[Seg($"  \u26a0 {e.Message}", Yellow)]],
            "PetSummoned"        => [[Seg($"  \u2726 {e.SummonedPetName ?? "Unknown"} has been summoned!", Magenta)]],
            "PetExpired"         => [[Seg($"  \u2726 {e.SummonedPetName} fades away...", Gray)]],
            "SkippedTurn"        => [BuildSkippedTurnRow(e, state)],
            "RoundStart"         => [BuildRoundStartRow(e)],
            "RoundEnd"           => [],
            "ManaRegen"          => [BuildManaRegenRow(e, state)],
            "ManaDeduct"         => [BuildManaDeductRow(e, state)],
            "ApiCall"            => [[Seg("  \u26a1 ", Cyan), Seg(e.Message, DarkGray)]],
            _                    => [[Seg($"  [{e.EventType}] {e.Message}", Gray)]]
        };

        if (e.SoundDescription is not null && rows.Count > 0 && rows[0].Count > 0)
            rows[0].Add(Seg($"  \u266a {e.SoundDescription}", Gray));

        return rows;
    }

    private static List<LogSegment> BuildTurnStartRow(CombatLogEntry e, CombatDisplayState state)
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

    private List<List<LogSegment>> BuildAttackRows(CombatLogEntry e, CombatDisplayState state)
    {
        var rows = new List<List<LogSegment>>();
        var total  = (e.DieRoll ?? 0) + (e.AttackPower ?? 0);
        var margin = total - (e.DefensePower ?? 0);
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        var srcColor = e.IsSpell == true ? Magenta : Yellow;

        // Row 1: attacker + weapon + inline roll summary
        var row1 = new List<LogSegment>
        {
            Seg("  ", White),
            Seg(e.ActorName, actorColor),
            Seg(e.IsSpell ? "  casts  " : "  attacks with  ", Gray),
            Seg($"[{e.AttackSourceName ?? "?"}]", srcColor),
        };
        if (_config.IsFieldEnabled("attackEvent", "DieRoll") ||
            _config.IsFieldEnabled("attackEvent", "AttackPower") ||
            _config.IsFieldEnabled("attackEvent", "DefensePower"))
        {
            row1.Add(Seg("   d20=", DarkGray));
            if (_config.IsFieldEnabled("attackEvent", "DieRoll"))
                row1.Add(Seg($"{e.DieRoll}", DarkGray));
            if (_config.IsFieldEnabled("attackEvent", "AttackPower"))
            {
                row1.Add(Seg("+", DarkGray));
                row1.Add(Seg($"{e.AttackPower}", DarkGray));
            }
            row1.Add(Seg($"={total}", DarkGray));
            if (_config.IsFieldEnabled("attackEvent", "DefensePower"))
            {
                row1.Add(Seg("  vs ", DarkGray));
                row1.Add(Seg($"{e.DefensePower}", DarkGray));
            }
            row1.Add(Seg("  ", DarkGray));
            row1.Add(Seg(margin >= 0 ? $"+{margin}" : $"{margin}", margin >= 0 ? Green : Red));
        }
        rows.Add(row1);

        // Row 2: outcome (only when enabled)
        if (_config.IsFieldEnabled("attackEvent", "IsHit"))
        {
            var row2 = new List<LogSegment> { Seg("     ", White) };
            if (_config.IsFieldEnabled("attackEvent", "IsCritical") && e.IsCritical == true)
                row2.Add(Seg("\u26a1 CRITICAL HIT", Magenta));
            else if (_config.IsFieldEnabled("attackEvent", "IsFumble") && e.IsFumble == true)
                row2.Add(Seg("\u26a0 FUMBLE", Yellow));
            else if (e.IsHit == true)
            {
                var dmg = e.DamageDealt ?? 0;
                var targetMaxHp = e.TargetName is not null ? state.TryGet(e.TargetName)?.MaxHp ?? 1 : 1;
                var label = CombatHitLabelService.GetLabel(dmg, targetMaxHp);
                var labelColor = label switch
                {
                    "CRUSHING HIT" => Magenta,
                    "HEAVY HIT"    => Yellow,
                    "SOLID HIT"    => Green,
                    "GLANCING HIT" => White,
                    _              => Gray,
                };
                row2.Add(Seg(label, labelColor));
                if (_config.IsFieldEnabled("attackEvent", "DamageDealt"))
                {
                    row2.Add(Seg("   \u2502   ", Gray));
                    row2.Add(Seg($"Dmg: {dmg}", Cyan));
                }
            }
            else
                row2.Add(Seg(margin >= -3 ? "\u25cb NEAR MISS" : "\u25cb MISS", Red));

            rows.Add(row2);
        }

        // Row 3 (optional): flavour phrase
        if (!string.IsNullOrEmpty(e.Phrase))
            rows.Add([Seg($"     \"{e.Phrase}\"", Cyan)]);

        return rows;
    }

    private List<LogSegment> BuildDamageRow(CombatLogEntry e)
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
                list.Add(Seg("/HP", Gray));
            }
            list.Add(Seg("]", Gray));
        }
        return list;
    }

    private static List<LogSegment> BuildDoTTickRow(CombatLogEntry e, CombatDisplayState state)
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

    private static List<LogSegment> BuildEffectAppliedRow(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2605 ", Yellow),
            Seg(e.ActorName, actorColor),
            Seg($"  is afflicted with  {e.StatusEffectName}!", Yellow),
        ];
    }

    private static List<LogSegment> BuildEffectResistedRow(CombatLogEntry e, CombatDisplayState state)
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

    private static List<LogSegment> BuildEffectExpiredRow(CombatLogEntry e, CombatDisplayState state)
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

    private static List<LogSegment> BuildSkippedTurnRow(CombatLogEntry e, CombatDisplayState state)
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

    private static List<LogSegment> BuildRoundStartRow(CombatLogEntry e) =>
    [
        Seg($"  \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550 ROUND {e.RoundNumber} \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550", Yellow),
    ];

    private static List<LogSegment> BuildManaRegenRow(CombatLogEntry e, CombatDisplayState state)
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

    private static List<LogSegment> BuildManaDeductRow(CombatLogEntry e, CombatDisplayState state)
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

    private static List<LogSegment> BuildPerfectParryRow(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u29df ", Green),
            Seg(e.ActorName, actorColor),
            Seg("  PERFECT PARRY!  (both rolled 20)", Green),
        ];
    }

    private static List<LogSegment> BuildDevastatingStrikeRow(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2620 ", Magenta),
            Seg(e.ActorName, actorColor),
            Seg("  DEVASTATING STRIKE!  triple damage!", Magenta),
        ];
    }

    private static List<LogSegment> BuildTotalReversalRow(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u21bb ", Yellow),
            Seg(e.ActorName, actorColor),
            Seg("  TOTAL REVERSAL!  fumble flipped!", Yellow),
        ];
    }

    private static List<LogSegment> BuildDeathRow(CombatLogEntry e) =>
    [
        Seg("  \u2020  ", Red),
        Seg(e.Message.ToUpper(), Red),
    ];

    private static List<LogSegment> BuildKnockedOutRow(CombatLogEntry e) =>
    [
        Seg("  \u2298  ", Yellow),
        Seg(e.Message.ToUpper(), Yellow),
    ];

    private static List<LogSegment> BuildHoTTickRow(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2191 ", Green),
            Seg(e.ActorName, actorColor),
            Seg("  recovers  ", Gray),
            Seg($"{e.DamageDealt}", Green),
            Seg($"  HP from {e.StatusEffectName ?? "HoT"}", Green),
        ];
    }

    private static List<LogSegment> BuildHealedRow(CombatLogEntry e, CombatDisplayState state)
    {
        var actorColor = NameBrush(state.IsHeroSide(e.ActorName), e.ActorName, null);
        return
        [
            Seg("  \u2665 ", Green),
            Seg(e.ActorName, actorColor),
            Seg("  healed for  ", Gray),
            Seg($"{e.DamageDealt}", Green),
            Seg($"  HP by {e.AttackSourceName ?? "spell"}", Green),
        ];
    }

    private static IBrush NameBrush(bool isHero, string? name, IBrush? fallback)
    {
        if (name is null) return fallback ?? White;
        return isHero ? HeroName : EnemyName;
    }

    private static IBrush MakeBrush(string hex) => new SolidColorBrush(Color.Parse(hex));
    private static LogSegment Seg(string text, IBrush brush) => new() { Text = text, Brush = brush };
}
