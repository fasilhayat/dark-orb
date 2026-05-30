namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

public class CombatService : ICombatService
{
    private readonly IDiceService _dice;
    private readonly ICombatStatsService _combatStats;

    public CombatService(IDiceService dice, ICombatStatsService combatStats)
    {
        _dice = dice;
        _combatStats = combatStats;
    }

    public int CalculateAbilityModifier(int score)
    {
        return (score - 10) / 2;
    }

    public DamageRollResult RollDamage(IAttackSource source)
    {
        var result = _dice.Roll(source.DamageDie);
        return new DamageRollResult
        {
            DieType = source.DamageDie,
            Result = result
        };
    }

    public AttackResult ResolveAttack(Character attacker, Character defender, IAttackSource source)
    {
        var attackerStats = _combatStats.ComputeAttackerStats(attacker, source);
        var defenderStats = _combatStats.ComputeDefenderStats(defender);
        var hitRoll = _dice.Roll(DieType.D20);

        if (hitRoll == 1)
        {
            return new AttackResult
            {
                HitRoll = hitRoll,
                IsHit = false,
                IsFumble = true,
                AttackPowerPenalty = -2,
                Damage = 0,
                DamageDie = source.DamageDie,
                WeaponName = source.Name,
                AttackPower = attackerStats.AttackPower,
                DefensePower = defenderStats.DefensePower
            };
        }

        if (hitRoll == 20)
        {
            var damageContext = ResolveDamage(attacker, defender, source, isCritical: true);
            return new AttackResult
            {
                HitRoll = hitRoll,
                IsHit = true,
                IsCriticalHit = true,
                Damage = damageContext.FinalDamage,
                DamageDie = source.DamageDie,
                WeaponName = source.Name,
                AttackPower = attackerStats.AttackPower,
                DefensePower = defenderStats.DefensePower,
                DamageContext = damageContext
            };
        }

        var totalAttack = hitRoll + attackerStats.AttackPower;
        var isHit = totalAttack >= defenderStats.DefensePower;
        var damageContextOnHit = isHit ? ResolveDamage(attacker, defender, source) : null;

        return new AttackResult
        {
            HitRoll = hitRoll,
            IsHit = isHit,
            Damage = damageContextOnHit?.FinalDamage ?? 0,
            DamageDie = source.DamageDie,
            WeaponName = source.Name,
            AttackPower = attackerStats.AttackPower,
            DefensePower = defenderStats.DefensePower,
            DamageContext = damageContextOnHit
        };
    }

    public DamageContext ResolveDamage(Character attacker, Character defender, IAttackSource source, bool isCritical = false)
    {
        var abilityScore = source.UsesIntelligence
            ? attacker.Intelligence
            : source.AttackType == AttackType.Ranged ? attacker.Dexterity : attacker.Strength;
        var attributeModifier = CalculateAbilityModifier(abilityScore);
        var weaponDiceRoll = RollAttackDamageTotal(source);
        var levelScaling = attacker.Level * 2;
        var baseDamage = weaponDiceRoll + attributeModifier + source.FlatDamageBonus + levelScaling;
        var typeMultiplier = defender.Vulnerabilities.Contains(source.DamageType) ? 1.5f : 1.0f;
        var scaledBaseDamage = isCritical ? baseDamage * 2 : baseDamage;
        var finalDamage = Math.Max(0, (int)(scaledBaseDamage * typeMultiplier) - defender.Equipment.TotalMitigation + source.ElementalDamage);

        return new DamageContext
        {
            WeaponDiceRoll = weaponDiceRoll,
            AttributeModifier = attributeModifier,
            FlatBonuses = source.FlatDamageBonus,
            LevelScaling = levelScaling,
            BaseDamage = baseDamage,
            TypeMultiplier = typeMultiplier,
            ArmorMitigation = defender.Equipment.TotalMitigation,
            ElementalModifiers = source.ElementalDamage,
            FinalDamage = finalDamage
        };
    }

    private int RollAttackDamageTotal(IAttackSource source)
    {
        var total = 0;
        for (var i = 0; i < source.DamageCount; i++)
            total += _dice.Roll(source.DamageDie);
        return total;
    }
}
