namespace BattleArena.Application.Modifiers;

using Core.Entities.Enums;
using Core.Interfaces;
using Core.Models;

/// <summary>
/// Adjusts attack and defense power based on the engagement range and the
/// attack type of the weapon being used.
///
/// Rules:
/// - Ranged weapon at <see cref="EngagementRange.Melee"/> range: -2 AP
///   (awkward to swing a bow or crossbow at close quarters).
/// - Ranged weapon at Short or Long range: -1 DP on the defender
///   (arrows from distance are harder to dodge).
/// - All other combinations produce no modifier.
/// </summary>
public sealed class RangeModifier : ICombatModifier
{
    public string      Name     => "Range";
    public int         Priority => 10;
    public CombatPhase Phase    => CombatPhase.AttackRoll;

    public void Apply(CombatModifierContext ctx)
    {
        if (ctx.Source.AttackType != AttackType.Ranged)
            return;

        if (ctx.Range == EngagementRange.Melee)
            ctx.AttackPowerDelta -= 2;
        else
            ctx.DefensePowerDelta -= 1;
    }
}
