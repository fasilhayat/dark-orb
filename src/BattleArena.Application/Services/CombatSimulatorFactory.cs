namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Interfaces;

public sealed class CombatSimulatorFactory : ICombatSimulatorFactory
{
    private readonly ICombatService        _combat;
    private readonly ITurnmeterService     _turnmeter;
    private readonly IStatusEffectService  _statusEffect;
    private readonly IDiceService          _dice;

    public CombatSimulatorFactory(
        ICombatService       combat,
        ITurnmeterService    turnmeter,
        IStatusEffectService statusEffect,
        IDiceService         dice)
    {
        _combat       = combat;
        _turnmeter    = turnmeter;
        _statusEffect = statusEffect;
        _dice         = dice;
    }

    public ICombatSimulator Create(ITargetSelector? heroSelector = null, ITargetSelector? enemySelector = null)
        => new CombatSimulator(_combat, _turnmeter, _statusEffect, _dice, heroSelector, enemySelector);
}
