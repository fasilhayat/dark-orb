namespace BattleArena.Application.Interfaces;

using Core.Entities;

public interface IStatusEffectService
{
    void Apply(Character target, StatusEffect effect);
    void TickAll(Character target);
    void Remove(Character target, string effectName);
    IReadOnlyList<StatusEffect> GetActive(Character target);
    int SumAttackModifiers(Character character);
    int SumDefenseModifiers(Character character);
    int SumTurnMeterModifiers(Character character);
}
