namespace BattleArena.Presentation;

using BattleArena.Application.Models;

public static class CombatPlaybackEngine
{
    public static void PlayTurnBased(
        CombatResult result,
        CombatDisplayState state,
        ICombatPresenter presenter,
        Action<CombatLogEntry, CombatDisplayState>? prepareEventState = null)
    {
        var turnEvents = new List<CombatLogEntry>();
        var pendingMessages = new List<CombatLogEntry>();
        var inTurn = false;
        var turnCount = 0;
        var turnTick = 0;
        var bus = presenter.VisualEventBus;

        void FlushTurn()
        {
            if (!inTurn || turnEvents.Count == 0) return;

            var turnStart = turnEvents.First(e => e.EventType == "TurnStart");
            var isHero = state.Layout.HeroNames.Contains(turnStart.ActorName);

            state.ApplyEvent(turnStart);
            presenter.RefreshScreen(state, turnTick, turnStart.ActiveActorName);
            presenter.ShowTurnHeader(turnCount, turnStart.ActorName, turnStart.TargetName, isHero);

            foreach (var entry in turnEvents)
            {
                if (!ReferenceEquals(entry, turnStart))
                    state.ApplyEvent(entry);

                presenter.ShowCombatEvent(entry, state);

                EmitVisualEvents(bus, entry);

                var delay = presenter.GetEventDelayMs(entry.EventType);
                if (delay > 0)
                    presenter.Wait(delay);
            }

            turnEvents.Clear();
            inTurn = false;
        }

        PreSeedTurnMeters(result, state);

        presenter.ShowInitialScreen(state, 0);
        presenter.WaitForCombatStart();

        foreach (var entry in result.Log)
        {
            prepareEventState?.Invoke(entry, state);

            switch (entry.EventType)
            {
                case "TurnMeterGain":
                case "TurnEnd":
                    break;

                case "TurnStart":
                    if (pendingMessages.Count > 0)
                    {
                        turnEvents.InsertRange(0, pendingMessages);
                        pendingMessages.Clear();
                    }

                    FlushTurn();
                    inTurn = true;
                    turnCount++;
                    turnTick = entry.Tick;
                    turnEvents.Add(entry);
                    break;

                case "ApiCall":
                    if (inTurn)
                        turnEvents.Add(entry);
                    break;

                case "RoundStart":
                case "RoundEnd":
                case "PetSummoned":
                case "PetExpired":
                    if (inTurn)
                        turnEvents.Add(entry);
                    else
                        pendingMessages.Add(entry);
                    break;

                case "SkippedTurn":
                    pendingMessages.Add(entry);
                    break;

                default:
                    if (inTurn)
                        turnEvents.Add(entry);
                    break;
            }
        }

        if (pendingMessages.Count > 0)
            turnEvents.InsertRange(0, pendingMessages);

        FlushTurn();

        presenter.ClearAllPersistentEffects();
        var combatOver = result.Log.Any(e => e.EventType is "Death" or "KnockedOut");
        presenter.WaitForNextTurn(combatOver);
    }

    public static void PlayRealTime(
        CombatResult result,
        CombatDisplayState state,
        ICombatPresenter presenter,
        Action<CombatLogEntry, CombatDisplayState>? prepareEventState = null)
    {
        var byTick = result.Log.GroupBy(e => e.Tick).OrderBy(g => g.Key).ToList();
        var quietStart = -1;
        var quietEnd = -1;
        var bus = presenter.VisualEventBus;

        void FlushQuiet()
        {
            if (quietStart < 0)
                return;
            if (quietEnd > quietStart + 1)
                presenter.ShowQuietTicksSummary(quietStart, quietEnd);
            quietStart = quietEnd = -1;
        }

        PreSeedTurnMeters(result, state);

        presenter.ShowInitialScreen(state, 0);
        presenter.Wait(1500);

        foreach (var tickGroup in byTick.Where(g => g.Key >= 1))
        {
            var entries = tickGroup.ToList();
            var hasAction = entries.Any(e => e.EventType == "TurnStart");

            foreach (var entry in entries.Where(e => e.EventType == "TurnMeterGain"))
            {
                prepareEventState?.Invoke(entry, state);
                state.ApplyEvent(entry);
            }

            if (!hasAction)
            {
                if (quietStart < 0)
                    quietStart = tickGroup.Key;
                quietEnd = tickGroup.Key;
                if ((tickGroup.Key - quietStart) % 2 == 0)
                    presenter.RefreshScreen(state, tickGroup.Key, null);
                presenter.Wait(150);
                continue;
            }

            FlushQuiet();

            var turnStart = entries.First(e => e.EventType == "TurnStart");
            prepareEventState?.Invoke(turnStart, state);
            state.ApplyEvent(turnStart);

            presenter.RefreshScreen(state, tickGroup.Key, turnStart.ActiveActorName);
            presenter.Wait(600);

            foreach (var entry in entries)
            {
                if (entry.EventType == "TurnMeterGain")
                    continue;

                if (!ReferenceEquals(entry, turnStart))
                {
                    prepareEventState?.Invoke(entry, state);
                    state.ApplyEvent(entry);
                }

                presenter.ShowCombatEvent(entry, state);

                EmitVisualEvents(bus, entry);

                var delay = presenter.GetEventDelayMs(entry.EventType);
                if (delay > 0)
                    presenter.Wait(delay);
            }

            presenter.Wait(500);
        }

        FlushQuiet();

        presenter.ClearAllPersistentEffects();

        if (result.Log.Any(e => e.EventType is "Death" or "KnockedOut"))
        {
            presenter.RefreshScreen(state, byTick.Last().Key, null);
            presenter.WaitForNextTurn(true);
        }
    }

    public static void PreSeedTurnMeters(CombatResult result, CombatDisplayState state)
    {
        foreach (var entry in result.Log)
        {
            if (entry.EventType == "TurnStart")
                break;
            if (entry.EventType == "TurnMeterGain")
                state.ApplyEvent(entry);
        }
    }

    private static readonly HashSet<string> PersistentEffectNames =
    [
        "Burning", "Ignite", "Frozen", "Freeze", "Shocked", "Stun",
        "Sleep", "Fear", "Petrify", "Poisoned", "Bleeding"
    ];

    private static string GetPersistentColor(string effectName) => effectName switch
    {
        "Burning" => "#ff6600",
        "Ignite" => "#ff4400",
        "Frozen" => "#44ccff",
        "Freeze" => "#44ccff",
        "Shocked" => "#ffff44",
        "Stun" => "#aa66ff",
        "Sleep" => "#aa44ff",
        "Fear" => "#8822aa",
        "Petrify" => "#888888",
        "Poisoned" => "#44ff44",
        "Bleeding" => "#ff4444",
        _ => "#44ff44",
    };

    private static void EmitVisualEvents(VisualEventBus bus, CombatLogEntry entry)
    {
        switch (entry.EventType)
        {
            case "PerfectParry":
                bus.PublishNormal(new VisualEvent
                {
                    EventType = entry.EventType,
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    OverlayText = "PERFECT PARRY",
                    Color = "#44ff44"
                });
                break;

            case "DevastatingStrike":
                bus.PublishNormal(new VisualEvent
                {
                    EventType = entry.EventType,
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    OverlayText = "DEVASTATING STRIKE",
                    Color = "#ff44ff"
                });
                break;

            case "TotalReversal":
                bus.PublishNormal(new VisualEvent
                {
                    EventType = entry.EventType,
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    OverlayText = "TOTAL REVERSAL",
                    Color = "#ffff44"
                });
                break;

            case "FumblePenalty":
                bus.PublishNormal(new VisualEvent
                {
                    EventType = entry.EventType,
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    OverlayText = "FUMBLE",
                    Color = "#ff6644",
                    DurationMs = 1000
                });
                break;

            case "EffectApplied":
                if (entry.CcLabel is not null)
                {
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.CcLabel,
                        ActorName = entry.TargetName ?? entry.ActorName,
                        OverlayText = entry.CcLabel,
                        Color = "#ff8844",
                        DurationMs = 1000
                    });
                }
                if (entry.StatusEffectName is not null && PersistentEffectNames.Contains(entry.StatusEffectName))
                {
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.TargetName,
                        EffectName = entry.StatusEffectName,
                        IsPersistent = true,
                        Color = GetPersistentColor(entry.StatusEffectName),
                    });
                }
                break;

            case "EffectExpired":
                if (entry.StatusEffectName is not null && PersistentEffectNames.Contains(entry.StatusEffectName))
                {
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.TargetName,
                        EffectName = "ClearPersistent",
                    });
                }
                break;

            case "Healed":
                if (entry.TargetName is not null && (entry.DamageDealt ?? 0) > 0)
                {
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.TargetName,
                        HealAmount = entry.DamageDealt ?? 0,
                        Color = "#ffffff",
                    });
                }
                break;

            case "Death":
            case "KnockedOut":
                bus.PublishNormal(new VisualEvent
                {
                    EventType = "ClearPersistent",
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    EffectName = "ClearPersistent",
                });
                break;

            case "IncredibleEvent":
                bus.PublishIncredible(new VisualEvent
                {
                    EventType = entry.EventType,
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    OverlayText = entry.Message,
                    Color = "#ffdd00",
                    DurationMs = 2500
                });
                break;
        }
    }
}
