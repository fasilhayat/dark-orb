namespace BattleArena.Application.Interfaces;

using Core.Entities;
using Core.Entities.Enums;

public interface IActionDecisionSource
{
    Task<IAttackSource?> ChooseAttackAsync(
        Character actor,
        IAttackSource? defaultAttack,
        IReadOnlyList<Character> enemies,
        IReadOnlyList<Character> allies,
        int currentTick,
        CancellationToken ct,
        EngagementRange engagementRange = EngagementRange.Melee);
}
