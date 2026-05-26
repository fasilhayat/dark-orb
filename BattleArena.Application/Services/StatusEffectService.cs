namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;
using Core.Entities.Enums;

public class StatusEffectService : IStatusEffectService
{
    public void Apply(Character target, StatusEffect effect)
    {
        var existing = target.ActiveStatusEffects.FirstOrDefault(e => e.Name == effect.Name);
        if (existing is null)
        {
            target.ActiveStatusEffects.Add(effect);
            return;
        }

        switch (effect.StackRule)
        {
            case StackRule.NoStack:
                break;
            case StackRule.HighestWins:
                if (effect.Magnitude > existing.Magnitude)
                {
                    target.ActiveStatusEffects.Remove(existing);
                    target.ActiveStatusEffects.Add(effect);
                }
                break;
            case StackRule.Stack:
                if (effect.Source != existing.Source)
                    target.ActiveStatusEffects.Add(effect);
                break;
        }
    }

    public void TickAll(Character target)
    {
        foreach (var effect in target.ActiveStatusEffects.ToList())
        {
            if (effect.Duration > 0)
            {
                effect.Duration--;
                if (effect.Duration == 0)
                    target.ActiveStatusEffects.Remove(effect);
            }
        }
    }

    public int TickDoT(Character target)
    {
        var total = 0;
        foreach (var effect in target.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.DamageOverTime && e.DamagePerTurn > 0)
            .ToList())
        {
            target.CurrentHitPoints -= effect.DamagePerTurn;
            total += effect.DamagePerTurn;
            effect.Duration--;
            if (effect.Duration <= 0)
                target.ActiveStatusEffects.Remove(effect);
        }
        return total;
    }

    public bool HasEffectType(Character target, StatusEffectType type)
    {
        return target.ActiveStatusEffects.Any(e => e.Type == type);
    }

    public void Remove(Character target, string effectName)
    {
        target.ActiveStatusEffects.RemoveAll(e => e.Name == effectName);
    }

    public IReadOnlyList<StatusEffect> GetActive(Character target) => target.ActiveStatusEffects.AsReadOnly();

    public int SumAttackModifiers(Character character) => character.ActiveStatusEffects.Sum(e => e.AttackPowerModifier);

    public int SumDefenseModifiers(Character character) => character.ActiveStatusEffects.Sum(e => e.DefensePowerModifier);

    public int SumTurnMeterModifiers(Character character) => character.ActiveStatusEffects.Sum(e => e.TurnMeterModifier);
}
