namespace BattleArena.Application.Interfaces;

public interface ICombatSimulatorFactory
{
    ICombatSimulator Create(ITargetSelector? heroSelector = null, ITargetSelector? enemySelector = null);
}
