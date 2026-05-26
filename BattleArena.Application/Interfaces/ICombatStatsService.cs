namespace BattleArena.Application.Interfaces;

using Models;
using Core.Entities;

public interface ICombatStatsService
{
    CombatantStats ComputeAttackerStats(Character attacker, Weapon weapon);
    CombatantStats ComputeDefenderStats(Character defender);
}
