namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;

// Default target selector — picks a random living enemy each turn.
// Suitable for 1v1 (only one choice) and NvN (random spread of attacks).
public class RandomTargetSelector : ITargetSelector
{
    public Task<Character> SelectTargetAsync(
        Character actor,
        IEnumerable<Character> livingEnemies,
        CancellationToken ct = default)
    {
        var targets = livingEnemies.ToList();
        if (targets.Count == 0)
            throw new InvalidOperationException($"{actor.Name} has no living enemies to target.");

        return Task.FromResult(targets[Random.Shared.Next(targets.Count)]);
    }
}
