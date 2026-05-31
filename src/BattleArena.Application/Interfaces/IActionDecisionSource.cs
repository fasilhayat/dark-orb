namespace BattleArena.Application.Interfaces;

using Core.Entities;

public interface IActionDecisionSource
{
    Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        int currentTick,
        CancellationToken ct);
}
