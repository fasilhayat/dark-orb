namespace BattleArena.Application.Interfaces;

using Models;
using Core.Entities;

public interface ICombatStatsService
{
    CombatantStats ComputeAttackerStats(Character attacker, IAttackSource source);
    /// <param name="source">
    /// The attack source being used against this defender. When <c>null</c>, the physical
    /// defense formula is used (suitable for character-sheet display).
    /// When <see cref="AttackType.Spell"/>, uses Wisdom + magic resistance instead of AC + DEX.
    /// </param>
    CombatantStats ComputeDefenderStats(Character defender, IAttackSource? source = null);
}
