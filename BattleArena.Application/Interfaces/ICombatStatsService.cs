namespace BattleArena.Application.Interfaces;

using Models;
using Core.Entities;

public interface ICombatStatsService
{
    CombatantStats ComputeAttackerStats(Character attacker, IAttackSource source);
    CombatantStats ComputeDefenderStats(Character defender);
}
