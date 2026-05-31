namespace BattleArena.Application.Interfaces;

using Core.Entities;

// Chooses which enemy a character attacks this turn.
// Inject a custom implementation for AI targeting, taunt effects, focus-fire, etc.
//
// The async signature allows GUI implementations to suspend here while waiting
// for the player's input (e.g. a mouse click on a target). AI implementations
// (LowestHp, Random) return Task.FromResult immediately — zero overhead.
public interface ITargetSelector
{
    Task<Character> SelectTargetAsync(
        Character actor,
        IEnumerable<Character> livingEnemies,
        CancellationToken ct = default);
}
