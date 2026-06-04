namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
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

    public EffectApplicationResult TryApply(Character target, StatusEffect effect, int resistance, IDiceService dice)
    {
        var chanceRoll = dice.Roll(DieType.D100);
        if (chanceRoll > effect.ApplicationChance)
            return new EffectApplicationResult(false, false, chanceRoll, resistance, effect.Name);

        if (resistance > 0)
        {
            var resistRoll = dice.Roll(DieType.D100);
            if (resistRoll <= resistance)
                return new EffectApplicationResult(false, true, resistRoll, resistance, effect.Name);
        }

        Apply(target, effect);
        return new EffectApplicationResult(true, false, chanceRoll, resistance, effect.Name);
    }

    public IReadOnlyList<string> TickAll(Character target)
    {
        var expired = new List<string>();
        foreach (var effect in target.ActiveStatusEffects.ToList())
        {
            if (effect.Duration > 0)
            {
                effect.Duration--;
                if (effect.Duration == 0)
                {
                    expired.Add(effect.Name);
                    target.ActiveStatusEffects.Remove(effect);
                }
            }
        }
        return expired;
    }

    public int TickDoT(Character target)
    {
        var total = 0;
        foreach (var effect in target.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.DamageOverTime && e.DamagePerTurn > 0)
            .OrderBy(e => e.ResolutionPriority)
            .ToList())
        {
            target.CurrentHitPoints -= effect.DamagePerTurn;
            total += effect.DamagePerTurn;
        }
        return total;
    }

    public int TickHoT(Character target)
    {
        var total = 0;
        foreach (var effect in target.ActiveStatusEffects
            .Where(e => e.Type == StatusEffectType.HealOverTime && e.HealingPerTurn > 0)
            .OrderBy(e => e.ResolutionPriority)
            .ToList())
        {
            var before = target.CurrentHitPoints;
            target.CurrentHitPoints = Math.Min(target.MaxHitPoints, before + effect.HealingPerTurn);
            total += target.CurrentHitPoints - before;
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
