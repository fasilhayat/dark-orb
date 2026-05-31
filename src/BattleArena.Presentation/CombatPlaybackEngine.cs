namespace BattleArena.Presentation;

using BattleArena.Application.Models;

/// <summary>
/// Drives turn-based and real-time combat playback.
/// Manages event ordering and state transitions; delegates all rendering
/// to the provided <see cref="ICombatPresenter"/> implementation.
/// </summary>
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

        void FlushTurn()
        {
            if (!inTurn || turnEvents.Count == 0) return;

            var turnStart = turnEvents.First(e => e.EventType == "TurnStart");
            var isHero = state.Layout.HeroNames.Contains(turnStart.ActorName);

            presenter.RefreshScreen(state, turnTick, turnStart.ActiveActorName);
            presenter.ShowTurnHeader(turnCount, turnStart.ActorName, turnStart.TargetName, isHero);

            foreach (var entry in turnEvents)
            {
                presenter.ShowCombatEvent(entry, state);
                var delay = presenter.GetEventDelayMs(entry.EventType);
                if (delay > 0)
                    presenter.Wait(delay);
            }

            turnEvents.Clear();
            inTurn = false;
        }

        var combatOver = turnEvents.Any(e => e.EventType is "Death" or "KnockedOut");
        presenter.WaitForNextTurn(combatOver);
        
        PreSeedTurnMeters(result, state);

        presenter.ShowInitialScreen(state, 0);
        presenter.WaitForCombatStart();

        foreach (var entry in result.Log)
        {
            prepareEventState?.Invoke(entry, state);
            state.ApplyEvent(entry);

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
                var delay = presenter.GetEventDelayMs(entry.EventType);
                if (delay > 0)
                    presenter.Wait(delay);
            }

            presenter.Wait(500);
        }

        FlushQuiet();

        if (result.Log.Any(e => e.EventType is "Death" or "KnockedOut"))
        {
            presenter.RefreshScreen(state, byTick.Last().Key, null);
            presenter.WaitForNextTurn(true);
        }
    }

    /// <summary>
    /// Apply all TurnMeterGain events before the first TurnStart so the
    /// opening screen shows accumulated TM rather than zero bars.
    /// </summary>
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
}
