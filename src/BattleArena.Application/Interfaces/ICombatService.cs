namespace BattleArena.Application.Interfaces;

using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

public interface ICombatService
{
    AttackResult ResolveAttack(Character attacker, Character defender, IAttackSource source,
        EngagementRange range = EngagementRange.Melee,
        TerrainType terrain = TerrainType.Plains);
    DamageContext ResolveDamage(Character attacker, Character defender, IAttackSource source, bool isCritical = false,
        EngagementRange range = EngagementRange.Melee,
        TerrainType terrain = TerrainType.Plains);
    int ResolveHealing(Character healer, Character target, Spell spell,
        TerrainType terrain = TerrainType.Plains);
    DamageRollResult RollDamage(IAttackSource source);
    int CalculateAbilityModifier(int score);
}
