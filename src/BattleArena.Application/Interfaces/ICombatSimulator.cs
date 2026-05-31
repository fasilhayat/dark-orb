namespace BattleArena.Application.Interfaces;

using Application.Models;
using Core.Entities;

// Simulates a full turn-based combat between two parties, driving the
// turnmeter, resolving attacks each turn, tracking HP, and recording every
// event in a structured combat log.
//
// Supports 1v1, 1vN, and up to 6vN (hero party max = 6 characters).
public interface ICombatSimulator
{
    // ── Async API (primary) ────────────────────────────────────────────────────
    // Preferred for GUI and any context where the simulation must not block the
    // calling thread. Pass an ICombatObserver to receive events in real time
    // (e.g. for animation). Pass a CancellationToken to allow the player to
    // forfeit or the system to time out.

    /// <summary>Party-vs-party async combat. Ends when one party is defeated or maxTicks is reached.</summary>
    Task<CombatResult> SimulateAsync(
        Party heroParty, Party enemyParty,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default);

    /// <summary>1v1 async convenience overload — wraps both characters in single-member parties.</summary>
    Task<CombatResult> SimulateAsync(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default);

    // ── Sync convenience wrappers (tests / console demo) ─────────────────────
    // These call the async path via GetAwaiter().GetResult(). Safe for console
    // and test contexts (no synchronisation context). Do not call from a UI thread.

    /// <summary>Run a party-vs-party combat. Ends when one party is fully defeated or maxTicks is reached.</summary>
    CombatResult Simulate(Party heroParty, Party enemyParty, int maxTicks = 1000);

    /// <summary>Convenience 1v1 overload — wraps both characters in single-member parties.</summary>
    CombatResult Simulate(
        Character fighter, IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000);
}
