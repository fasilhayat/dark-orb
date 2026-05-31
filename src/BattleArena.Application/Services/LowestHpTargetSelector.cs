namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;

// Targets the living enemy with the lowest current HP — focus-fire strategy.
// Used in Auto mode for both hero and enemy parties.
public class LowestHpTargetSelector : ITargetSelector
{
    public Task<Character> SelectTargetAsync(
        Character actor,
        IEnumerable<Character> livingEnemies,
        CancellationToken ct = default)
    {
        var targets = livingEnemies.ToList();
        if (targets.Count == 0)
            throw new InvalidOperationException($"{actor.Name} has no living enemies to target.");

        return Task.FromResult(targets.MinBy(c => c.CurrentHitPoints)!);
    }
}
