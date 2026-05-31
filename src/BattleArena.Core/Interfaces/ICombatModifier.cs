namespace BattleArena.Core.Interfaces;

using Core.Entities.Enums;
using Core.Models;

/// <summary>
/// A single pluggable modifier that adjusts combat values for a specific phase.
/// Register implementations via DI; they are injected into <c>CombatService</c>
/// and executed in <see cref="Priority"/> order.
/// </summary>
public interface ICombatModifier
{
    /// <summary>Human-readable name used in logs and diagnostics.</summary>
    string Name { get; }

    /// <summary>
    /// Execution order within the phase — lower values run first.
    /// Recommended bands: 10 = positional, 20 = environmental, 30 = item/set bonuses.
    /// </summary>
    int Priority { get; }

    /// <summary>Which combat phase this modifier participates in.</summary>
    CombatPhase Phase { get; }

    /// <summary>
    /// Apply the modifier to <paramref name="ctx"/>.
    /// Implementations mutate the delta/flag fields on the context; they must
    /// not read dice or mutate <see cref="CombatModifierContext.Attacker"/> /
    /// <see cref="CombatModifierContext.Defender"/> directly.
    /// </summary>
    void Apply(CombatModifierContext ctx);
}
