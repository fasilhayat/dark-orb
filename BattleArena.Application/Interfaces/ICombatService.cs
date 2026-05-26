namespace BattleArena.Application.Interfaces;

using Application.Models;
using Core.Entities;

public interface ICombatService
{
    AttackResult ResolveAttack(Character attacker, Character defender, IAttackSource source);
    DamageContext ResolveDamage(Character attacker, Character defender, IAttackSource source, bool isCritical = false);
    DamageRollResult RollDamage(IAttackSource source);
    int CalculateAbilityModifier(int score);
}
