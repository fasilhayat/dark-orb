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

    public DamageRollResult RollDamage(Weapon weapon)
    {
        var result = _dice.Roll(weapon.DamageDie);
        return new DamageRollResult
        {
            DieType = weapon.DamageDie,
            Result = result
        };
    }

    public AttackResult ResolveAttack(Character attacker, Character defender, Weapon weapon)
    {
        var attackerStats = _combatStats.ComputeAttackerStats(attacker, weapon);
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
                DamageDie = weapon.DamageDie,
                WeaponName = weapon.Name,
                AttackPower = attackerStats.AttackPower,
                DefensePower = defenderStats.DefensePower
            };
        }

        if (hitRoll == 20)
        {
            var damageContext = ResolveDamage(attacker, defender, weapon, isCritical: true);
            return new AttackResult
            {
                HitRoll = hitRoll,
                IsHit = true,
                IsCriticalHit = true,
                Damage = damageContext.FinalDamage,
                DamageDie = weapon.DamageDie,
                WeaponName = weapon.Name,
                AttackPower = attackerStats.AttackPower,
                DefensePower = defenderStats.DefensePower,
                DamageContext = damageContext
            };
        }

        var totalAttack = hitRoll + attackerStats.AttackPower;
        var isHit = totalAttack >= defenderStats.DefensePower;
        var damageContextOnHit = isHit ? ResolveDamage(attacker, defender, weapon) : null;

        return new AttackResult
        {
            HitRoll = hitRoll,
            IsHit = isHit,
            Damage = damageContextOnHit?.FinalDamage ?? 0,
            DamageDie = weapon.DamageDie,
            WeaponName = weapon.Name,
            AttackPower = attackerStats.AttackPower,
            DefensePower = defenderStats.DefensePower,
            DamageContext = damageContextOnHit
        };
    }

    public DamageContext ResolveDamage(Character attacker, Character defender, Weapon weapon, bool isCritical = false)
    {
        var attributeModifier = CalculateAbilityModifier(weapon.AttackType == AttackType.Ranged ? attacker.Dexterity : attacker.Strength);
        var weaponDiceRoll = RollWeaponDamageTotal(weapon);
        var baseDamage = weaponDiceRoll + attributeModifier + weapon.FlatDamageBonus;
        var typeMultiplier = defender.Vulnerabilities.Contains(weapon.DamageType) ? 1.5f : 1.0f;
        var scaledBaseDamage = isCritical ? baseDamage * 2 : baseDamage;
        var finalDamage = Math.Max(0, (int)(scaledBaseDamage * typeMultiplier) - defender.Equipment.TotalMitigation + weapon.ElementalDamage);

        return new DamageContext
        {
            WeaponDiceRoll = weaponDiceRoll,
            AttributeModifier = attributeModifier,
            FlatBonuses = weapon.FlatDamageBonus,
            BaseDamage = baseDamage,
            TypeMultiplier = typeMultiplier,
            ArmorMitigation = defender.Equipment.TotalMitigation,
            ElementalModifiers = weapon.ElementalDamage,
            FinalDamage = finalDamage
        };
    }

    private int RollWeaponDamageTotal(Weapon weapon)
    {
        var total = 0;
        for (var i = 0; i < weapon.DamageCount; i++)
            total += _dice.Roll(weapon.DamageDie);
        return total;
    }
}
