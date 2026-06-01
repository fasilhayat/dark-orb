namespace BattleArena.Application.Modifiers;

using Core.Entities.Enums;
using Core.Interfaces;
using Core.Models;

/// <summary>
/// Adjusts damage output based on the defender's active protective buffs.
/// Reads <c>StatusEffect.DefensePowerModifier</c> buffs and converts them
/// into flat damage reduction, and applies a stacking multiplier for each
/// active protective effect.
///
/// Priority band 30 — item/set / spell-buff bonuses.
/// This demonstrates the <see cref="CombatPhase.DamageCalculation"/> slot.
/// </summary>
public sealed class DamageModifier : ICombatModifier
{
    public string      Name     => "DamageModifier";
    public int         Priority => 30;
    public CombatPhase Phase    => CombatPhase.DamageCalculation;

    public void Apply(CombatModifierContext ctx)
    {
        var defender = ctx.Defender;

        foreach (var effect in defender.ActiveStatusEffects)
        {
            if (effect.Type != StatusEffectType.Buff)
                continue;

            // Protective buffs with DefensePowerModifier also reduce incoming
            // damage by half that value (e.g. +6 DP buff = -3 damage).
            if (effect.DefensePowerModifier > 0)
                ctx.DamageDelta -= effect.DefensePowerModifier / 2;

            // Each ResistanceBonus on the defender reduces corresponding
            // elemental damage by the bonus value.
            foreach (var rb in effect.ResistanceBonuses)
            {
                var source = ctx.Source;
                if (source.ElementalType != ElementalType.None &&
                    MatchingResistance(source.ElementalType, rb.Type))
                {
                    ctx.DamageDelta -= rb.Value / 2;
                }
            }

            // Each protective buff provides 5 % damage reduction (multiplicative).
            if (effect.DefensePowerModifier > 0)
                ctx.DamageMultiplier *= 0.95;
        }
    }

    private static bool MatchingResistance(ElementalType elemental, ResistanceType resistance)
    {
        return (elemental, resistance) switch
        {
            (ElementalType.Fire, ResistanceType.Fire) => true,
            (ElementalType.Ice, ResistanceType.Cold) => true,
            (ElementalType.Lightning, ResistanceType.Lightning) => true,
            (ElementalType.Poison, ResistanceType.Poison) => true,
            (ElementalType.Shadow, ResistanceType.Shadow) => true,
            (ElementalType.Holy, ResistanceType.Holy) => true,
            (ElementalType.Acid, ResistanceType.Acid) => true,
            _ => false
        };
    }
}
