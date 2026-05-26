namespace BattleArena.Application.Interfaces;

using Application.Models;
using Core.Entities;

public interface ICombatService
{
    AttackResult ResolveAttack(Character attacker, Character defender, Weapon weapon);
    DamageContext ResolveDamage(Character attacker, Character defender, Weapon weapon, bool isCritical = false);
    DamageRollResult RollDamage(Weapon weapon);
    int CalculateAbilityModifier(int score);
}
