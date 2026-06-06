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

                EmitVisualEvents(bus, entry);
                EmitCombatSounds(bus, entry);

                presenter.ShowCombatEvent(entry, state);

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

                EmitVisualEvents(bus, entry);
                EmitCombatSounds(bus, entry);

                presenter.ShowCombatEvent(entry, state);

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

    private static string SpellOverlayColor(string spellName)
    {
        var lower = spellName.ToLowerInvariant();
        if (lower.Contains("fire") || lower.Contains("flame") || lower.Contains("burn") || lower.Contains("inferno"))
            return "#ff6600";
        if (lower.Contains("ice") || lower.Contains("frost") || lower.Contains("freeze") || lower.Contains("cold"))
            return "#44ccff";
        if (lower.Contains("shock") || lower.Contains("lightning") || lower.Contains("thunder") || lower.Contains("spark"))
            return "#ffff44";
        if (lower.Contains("heal") || lower.Contains("cure") || lower.Contains("bless") || lower.Contains("restore"))
            return "#44cc44";
        if (lower.Contains("stun") || lower.Contains("sleep") || lower.Contains("fear") || lower.Contains("charm"))
            return "#cc44cc";
        if (lower.Contains("poison") || lower.Contains("acid"))
            return "#44ff44";
        if (lower.Contains("arcane") || lower.Contains("magic") || lower.Contains("mystic"))
            return "#cc88ff";
        if (lower.Contains("shield") || lower.Contains("armor") || lower.Contains("ward") || lower.Contains("protect"))
            return "#88aaff";
        return "#ffffff";
    }

    private static readonly HashSet<string> PersistentEffectNames =
    [
        "Burning", "Ignite", "Frozen", "Freeze", "Shocked", "Stun",
        "Sleep", "Fear", "Petrify", "Poisoned", "Bleeding"
    ];

    private static string GetPersistentColor(string effectName)
    {
        if (CcVisualConfig.IsCcEffect(effectName))
            return CcVisualConfig.GetColor(effectName);
        return effectName switch
        {
            "Burning" => "#ff6600",
            "Ignite" => "#ff4400",
            "Frozen" => "#44ccff",
            "Freeze" => "#44ccff",
            "Shocked" => "#ffff44",
            "Poisoned" => "#44ff44",
            "Bleeding" => "#ff4444",
            _ => "#44ff44",
        };
    }

    private static void EmitCombatSounds(VisualEventBus bus, CombatLogEntry entry)
    {
        string? soundId = null;

        switch (entry.EventType)
        {
            case "DoTTick":
                if (entry.StatusEffectName is not null)
                    soundId = CombatSoundRegistry.GetEffectSoundId(entry.StatusEffectName);
                break;

            case "Attack":
                if (entry.IsCritical == true)
                    soundId = CombatSoundRegistry.GetCriticalHitSoundId();
                else if (entry.IsSpell)
                    soundId = CombatSoundRegistry.GetSpellCastSoundId();
                break;

            case "Healed":
                if (entry.IsSpell)
                    soundId = CombatSoundRegistry.GetHealCastSoundId();
                break;

            case "EffectApplied":
                if (entry.StatusEffectName is not null)
                    soundId = CombatSoundRegistry.GetEffectSoundId(entry.StatusEffectName);
                break;

            case "PerfectParry":
                soundId = CombatSoundRegistry.GetEventSoundId("PerfectParry");
                break;

            case "DevastatingStrike":
                soundId = CombatSoundRegistry.GetCriticalHitSoundId();
                break;

            case "FumblePenalty":
            case "TotalReversal":
                soundId = CombatSoundRegistry.GetEventSoundId("FumblePenalty");
                break;

            case "Death":
                soundId = CombatSoundRegistry.GetEventSoundId("Death");
                break;

            case "Resurrection":
                soundId = CombatSoundRegistry.GetEventSoundId("Resurrection");
                break;
        }

        if (string.IsNullOrEmpty(soundId))
            return;

        var desc = CombatSoundRegistry.GetSoundDescription(soundId);
        entry.SoundDescription = desc;
        bus.PublishSound(new SoundEvent { SoundId = soundId, Description = desc });
    }

    private static void EmitVisualEvents(VisualEventBus bus, CombatLogEntry entry)
    {
        switch (entry.EventType)
        {
            case "Attack":
                if (entry.IsSpell && !string.IsNullOrEmpty(entry.AttackSourceName))
                {
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.TargetName,
                        OverlayText = entry.AttackSourceName!.ToUpperInvariant(),
                        Color = SpellOverlayColor(entry.AttackSourceName),
                        DurationMs = 1200,
                    });
                }
                break;

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
                    var ccColor = CcVisualConfig.GetColor(entry.CcLabel);
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.CcLabel,
                        ActorName = entry.TargetName ?? entry.ActorName,
                        OverlayText = entry.CcLabel.ToUpperInvariant(),
                        Color = ccColor,
                        DurationMs = 1000
                    });
                }
                else if (entry.StatusEffectName is not null && CcVisualConfig.IsCcEffect(entry.StatusEffectName))
                {
                    var label = CcVisualConfig.GetLabel(entry.StatusEffectName);
                    var ccColor = CcVisualConfig.GetColor(entry.StatusEffectName);
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.StatusEffectName,
                        ActorName = entry.TargetName ?? entry.ActorName,
                        OverlayText = label,
                        Color = ccColor,
                        DurationMs = 1000,
                    });
                }
                else if (entry.IsBuff == true && !string.IsNullOrEmpty(entry.AttackSourceName))
                {
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.TargetName,
                        OverlayText = entry.AttackSourceName.ToUpperInvariant(),
                        Color = SpellOverlayColor(entry.AttackSourceName),
                        DurationMs = 1000,
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

            case "HoTTick":
                if ((entry.DamageDealt ?? 0) > 0)
                {
                    var targetName = entry.TargetName ?? entry.ActorName;
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = targetName,
                        HealAmount = entry.DamageDealt ?? 0,
                        Color = "#ffffff",
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
                if ((entry.DamageDealt ?? 0) > 0)
                {
                    var targetName = entry.TargetName ?? entry.ActorName;
                    var overlay = entry.IsSpell && !string.IsNullOrEmpty(entry.AttackSourceName)
                        ? entry.AttackSourceName.ToUpperInvariant()
                        : $"HEALED +{entry.DamageDealt}";
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = targetName,
                        OverlayText = overlay,
                        HealAmount = entry.DamageDealt ?? 0,
                        Color = "#44cc44",
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
