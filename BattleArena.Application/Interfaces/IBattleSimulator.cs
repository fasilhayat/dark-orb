namespace BattleArena.Application.Interfaces;

using Application.Models;
using Core.Entities;

// Simulates a full turn-based battle between two combatants, driving the
// turnmeter, resolving attacks each turn, tracking HP, and recording every
// event in a structured battle log.
public interface IBattleSimulator
{
    /// <summary>
    /// Run the battle loop until one combatant's HP reaches zero or maxTicks is exceeded.
    /// </summary>
    BattleResult Simulate(
        Character fighter, Weapon fighterWeapon,
        Character opponent, Weapon opponentWeapon,
        int maxTicks = 1000);
}
