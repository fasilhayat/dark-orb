namespace BattleArena.Application.Interfaces;

using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

public interface ICombatService
{
    /// <param name="range">
    /// Distance between attacker and defender. Defaults to <see cref="EngagementRange.Melee"/>.
    /// Ranged weapons in melee range suffer a -2 AP penalty; ranged attacks at distance
    /// reduce the defender's DP by 1 (harder to dodge). When the full distance system is
    /// implemented, the simulator will populate this from position state.
    /// </param>
    AttackResult ResolveAttack(Character attacker, Character defender, IAttackSource source,
        EngagementRange range = EngagementRange.Melee);
    DamageContext ResolveDamage(Character attacker, Character defender, IAttackSource source, bool isCritical = false);
    DamageRollResult RollDamage(IAttackSource source);
    int CalculateAbilityModifier(int score);
}
