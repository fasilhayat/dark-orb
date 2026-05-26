namespace BattleArena.Application.Interfaces;

using Core.Entities;
using Core.Entities.Enums;

public interface IStatusEffectService
{
    void Apply(Character target, StatusEffect effect);
    IReadOnlyList<string> TickAll(Character target);
    int TickDoT(Character target);
    bool HasEffectType(Character target, StatusEffectType type);
    void Remove(Character target, string effectName);
    IReadOnlyList<StatusEffect> GetActive(Character target);
    int SumAttackModifiers(Character character);
    int SumDefenseModifiers(Character character);
    int SumTurnMeterModifiers(Character character);
}
