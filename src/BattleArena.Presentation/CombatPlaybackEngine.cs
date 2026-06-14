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

            var config = presenter.DamagePreviewConfig;

            for (var i = 0; i < turnEvents.Count; i++)
            {
                var entry = turnEvents[i];

                if (!ReferenceEquals(entry, turnStart))
                {
                    if (entry.EventType == "Damage")
                    {
                        var wasCrit = WasFromCriticalHit(turnEvents, i);
                        var showPreview = wasCrit
                            || MeetsDevastationThreshold(entry, state, config.DevastationThresholdPercent);

                        if (showPreview)
                        {
                            var totalDamage = entry.DamageDealt ?? 0;
                            var targetName = entry.ActorName;

                            while (i + 1 < turnEvents.Count
                                && turnEvents[i + 1].EventType == "Damage"
                                && turnEvents[i + 1].ActorName == targetName)
                            {
                                i++;
                                totalDamage += turnEvents[i].DamageDealt ?? 0;
                            }

                            var targetState = state.TryGet(targetName);
                            if (targetState is not null && totalDamage > 0)
                            {
                                var previewAmount = Math.Min(totalDamage, Math.Max(0, targetState.Hp));
                                var overlay = wasCrit ? "\u00d72 CRIT!" : "";
                                bus.PublishNormal(new VisualEvent
                                {
                                    EventType = "DamagePreview",
                                    ActorName = targetName,
                                    TargetName = targetName,
                                    OverlayText = overlay,
                                    Color = wasCrit ? "#ff44ff" : "#ffffff",
                                    DamagePreviewAmount = previewAmount,
                                    TargetMaxHp = targetState.MaxHp,
                                    HpBefore = targetState.Hp,
                                });

                                var previewDelay = presenter.GetEventDelayMs("DamagePreview");
                                if (previewDelay > 0)
                                    presenter.Wait(previewDelay);
                            }
                        }
                    }

                    state.ApplyEvent(entry);
                }

                EmitVisualEvents(bus, entry);
                EmitCombatSounds(bus, entry);

                try
                {
                    presenter.ShowCombatEvent(entry, state);
                }
                catch
                {
                    // Swallow rendering exceptions to prevent playback from halting
                }

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

                    if (inTurn)
                    {
                        FlushTurn();
                        presenter.WaitForNextTurn(false);
                    }
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

        if (inTurn)
            FlushTurn();

        var combatOver = result.Log.Any(e => e.EventType is "Death" or "KnockedOut");
        presenter.WaitForNextTurn(combatOver);
        presenter.ClearAllPersistentEffects();
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

    private static bool WasFromCriticalHit(IReadOnlyList<CombatLogEntry> turnEvents, int damageIndex)
    {
        for (var j = damageIndex - 1; j >= 0; j--)
        {
            var prev = turnEvents[j];
            if (prev.EventType == "Attack" && prev.TargetName == turnEvents[damageIndex].ActorName)
                return prev.IsCritical == true;
            if (prev.EventType == "TurnStart")
                break;
        }
        return false;
    }

    private static bool MeetsDevastationThreshold(CombatLogEntry entry, CombatDisplayState state, int devastationThresholdPercent)
    {
        var targetState = state.TryGet(entry.ActorName);
        if (targetState is null) return false;

        var damageAmount = entry.DamageDealt ?? 0;
        if (damageAmount <= 0) return false;

        return damageAmount * 100 >= targetState.MaxHp * devastationThresholdPercent;
    }

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
            "Electrified" => "#88ddff",
            "Confused" => "#aaaaaa",
            "Charmed" => "#ff88aa",
            _ => TryGetTransferColor(effectName),
        };
    }

    private static string TryGetTransferColor(string effectName)
    {
        var config = TransferEffectRegistry.GetConfig(effectName);
        return config.TransferColor;
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
                else if (IsUpgradedSpell(entry))
                    soundId = CombatSoundRegistry.GetEventSoundId("SpellUpgrade");
                else if (entry.IsSpell)
                    soundId = CombatSoundRegistry.GetSpellCastSoundId(entry.AttackSourceName);
                break;

            case "Healed":
                if (entry.IsSpell)
                    soundId = CombatSoundRegistry.GetHealCastSoundId(entry.AttackSourceName);
                break;

            case "EffectApplied":
                if (entry.StatusEffectName is not null)
                    soundId = CombatSoundRegistry.GetEffectSoundId(entry.StatusEffectName);
                break;

            case "LeechTick":
                soundId = CombatSoundRegistry.GetTransferSoundId(
                    entry.StatusEffectName ?? "Leech", entry.EventType);
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

    private const int UpgradeThreshold = 5;

    private static bool IsUpgradedSpell(CombatLogEntry entry) =>
        entry.IsSpell &&
        entry.SpellLevel.HasValue &&
        entry.CasterLevel.HasValue &&
        entry.CasterLevel.Value >= entry.SpellLevel.Value + UpgradeThreshold;

    private static void EmitVisualEvents(VisualEventBus bus, CombatLogEntry entry)
    {
        switch (entry.EventType)
        {
            case "Attack":
                if (entry.IsSpell && !string.IsNullOrEmpty(entry.AttackSourceName))
                {
                    var isUpgraded = IsUpgradedSpell(entry);
                    var ev = new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.TargetName,
                        OverlayText = entry.AttackSourceName!.ToUpperInvariant(),
                        Color = isUpgraded ? "#ffdd44" : SpellOverlayColor(entry.AttackSourceName),
                        DurationMs = isUpgraded ? 1800 : 1200,
                    };
                    if (isUpgraded)
                        bus.PublishMajor(ev);
                    else
                        bus.PublishNormal(ev);
                }
                else if (entry.IsCritical == true)
                {
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.TargetName,
                        OverlayText = "CRITICAL HIT!",
                        Color = "#ff44ff",
                        DurationMs = 1200,
                    });
                }
                break;

            case "DoTTick":
                if (entry.StatusEffectName is not null)
                {
                    var targetName = entry.TargetName ?? entry.ActorName;
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = targetName,
                        OverlayText = entry.StatusEffectName.ToUpperInvariant(),
                        Color = GetPersistentColor(entry.StatusEffectName),
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
                if (entry.StatusEffectName is not null && EffectVisualConfig.IsDisplayed(entry.StatusEffectName))
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
                if (entry.StatusEffectName is not null && EffectVisualConfig.IsDisplayed(entry.StatusEffectName))
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
                bus.PublishNormal(new VisualEvent
                {
                    EventType = entry.EventType,
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    OverlayText = entry.EventType == "Death" ? "SLAIN" : "KNOCKED OUT",
                    Color = "#ff4444",
                    DurationMs = 2000,
                });
                break;

            case "Resurrection":
                bus.PublishNormal(new VisualEvent
                {
                    EventType = entry.EventType,
                    ActorName = entry.ActorName,
                    TargetName = entry.TargetName,
                    OverlayText = "RESURRECTION",
                    Color = "#44cc44",
                    DurationMs = 2000,
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

            case "LeechTick":
                if (entry.LeechAmount > 0 && entry.LeechCasterName is not null)
                {
                    var effectName = entry.StatusEffectName ?? "Leech";
                    var resourceLabel = entry.LeechResourceType == "Mana" ? "MANA" : "HP";
                    var config = TransferEffectRegistry.GetConfig(
                        entry.LeechResourceType == "Mana" ? "LeechMana" : effectName);
                    bus.PublishNormal(new VisualEvent
                    {
                        EventType = entry.EventType,
                        ActorName = entry.ActorName,
                        TargetName = entry.LeechCasterName,
                        OverlayText = $"{resourceLabel} {config.OverlayLabel}",
                        Color = config.TransferColor,
                        DurationMs = config.DurationMs,
                        LeechAmount = entry.LeechAmount ?? 0,
                        LeechCasterName = entry.LeechCasterName,
                        LeechResourceType = entry.LeechResourceType ?? "HP",
                        EffectName = effectName
                    });
                }
                break;
        }
    }
}
