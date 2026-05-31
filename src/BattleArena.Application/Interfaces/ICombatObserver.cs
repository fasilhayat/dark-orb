namespace BattleArena.Application.Interfaces;

using Application.Models;

// Receives combat events in real time as they are generated during a simulation.
//
// A GUI subscribes to this interface to animate each attack, damage hit, and
// death as it happens — no need to replay a pre-computed log after the fact.
//
// A console demo can simply write to stdout here.
// An audio layer can play sound effects.
// An analytics layer can stream events to a server.
//
// Pass an implementation to ICombatSimulator.SimulateAsync via the observer
// parameter. Leave it null to skip notifications (pure log-only mode).
public interface ICombatObserver
{
    /// <summary>
    /// Called once for every event as the simulation generates it.
    /// Invoked before the event is added to the CombatResult log, so the observer
    /// always sees events in the same order they will appear in the log.
    /// </summary>
    Task OnEventAsync(CombatLogEntry entry, CancellationToken ct = default);
}
