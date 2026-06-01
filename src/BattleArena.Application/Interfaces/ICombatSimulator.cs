namespace BattleArena.Application.Interfaces;

using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

public interface ICombatSimulator
{
    Task<CombatResult> SimulateAsync(
        Party heroParty, Party enemyParty,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains);

    Task<CombatResult> SimulateAsync(
        Character fighter,  IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000,
        ICombatObserver? observer = null,
        CancellationToken ct = default,
        TerrainType terrain = TerrainType.Plains);

    CombatResult Simulate(Party heroParty, Party enemyParty, int maxTicks = 1000,
        TerrainType terrain = TerrainType.Plains);

    CombatResult Simulate(
        Character fighter, IAttackSource? fighterAttack,
        Character opponent, IAttackSource? opponentAttack,
        int maxTicks = 1000,
        TerrainType terrain = TerrainType.Plains);
}
