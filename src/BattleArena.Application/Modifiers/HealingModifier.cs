namespace BattleArena.Application.Modifiers;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Core.Models;

/// <summary>
/// Adjusts healing output based on caster buffs and target state.
/// Checks the caster's active status effects for healing-power bonuses
/// and the target's debuffs (e.g. Wounded reduces healing received).
///
/// Priority band 10 — base healing adjustments.
/// This demonstrates the <see cref="CombatPhase.Healing"/> slot.
/// </summary>
public sealed class HealingModifier : ICombatModifier
{
    public string      Name     => "HealingModifier";
    public int         Priority => 10;
    public CombatPhase Phase    => CombatPhase.Healing;

    public void Apply(CombatModifierContext ctx)
    {
        // Caster buffs that boost healing output.
        foreach (var effect in ctx.Attacker.ActiveStatusEffects)
        {
            if (effect.Type == StatusEffectType.Buff && effect.AttackPowerModifier > 0)
                ctx.HealingPowerDelta += effect.AttackPowerModifier / 2;
        }

        // Target debuffs that reduce healing received (e.g. "Wounded").
        foreach (var effect in ctx.Defender.ActiveStatusEffects)
        {
            if (effect.Type == StatusEffectType.Debuff && effect.DefensePowerModifier < 0)
                ctx.HealingMultiplier *= 0.8;
        }

        // Group healing is less potent per target.
        if (ctx.Source is Spell { IsGroupHeal: true })
            ctx.HealingMultiplier *= 0.6;
    }
}
