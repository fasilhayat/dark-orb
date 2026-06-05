namespace BattleArena.Presentation;

using BattleArena.Application.Models;

/// <summary>
/// Contract between the combat playback engine and a concrete front-end renderer.
///
/// Call order per event (engine guarantees this):
///   1. CombatDisplayState.ApplyEvent(entry)
///   2. ShowCombatEvent(entry, state)
///
/// Implement this interface in any front-end: console, WPF, Avalonia, web.
/// For async UIs, wrap the engine call in Task.Run and make blocking calls
/// from synchronised UI dispatch.
/// </summary>
public interface ICombatPresenter
{
    VisualEventBus VisualEventBus { get; }

    void ShowInitialScreen(CombatDisplayState state, int tick);
    void WaitForCombatStart();
    void RefreshScreen(CombatDisplayState state, int tick, string? activeActorName);
    void ShowCombatEvent(CombatLogEntry entry, CombatDisplayState state);
    void ShowCombatEventOverlay(string actorName, string? targetName, string effectType);
    int GetEventDelayMs(string eventType);
    void Wait(int milliseconds);
    void ShowTurnHeader(int turnNumber, string actorName, string? targetName, bool isHero);
    void WaitForNextTurn(bool combatOver);
    void ShowQuietTicksSummary(int fromTick, int toTick);

    /// <summary>
    /// Stop all persistent-effect timers (flicker borders, etc.) and clear their visual state.
    /// Called by the playback engine after combat ends.
    /// </summary>
    void ClearAllPersistentEffects();
}
