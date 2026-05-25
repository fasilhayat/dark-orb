using BattleArena.Application.Models;
using BattleArena.Core.Entities;

namespace BattleArena.Application.Interfaces;

public interface ICombatService
{
    AttackResult ResolveAttack(Character attacker, int targetArmorClass, Weapon weapon);
    DamageRollResult RollDamage(Weapon weapon);
    int CalculateAbilityModifier(int score);
}
