using BattleArena.Application.Interfaces;
using BattleArena.Application.Models;
using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Services;

public class CombatService : ICombatService
{
    private readonly IDiceService _dice;

    public CombatService(IDiceService dice)
    {
        _dice = dice;
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

    public AttackResult ResolveAttack(Character attacker, int targetArmorClass, Weapon weapon)
    {
        var hitRoll = _dice.Roll(DieType.D20);
        var strMod = CalculateAbilityModifier(attacker.Strength);
        var totalAttack = hitRoll + strMod + weapon.AttackBonus;
        // An attack hits when the total attack roll meets or exceeds the target's armor class.
        var isHit = totalAttack >= targetArmorClass;

        var damage = 0;
        if (isHit)
        {
            var damageRoll = RollDamage(weapon);
            damage = damageRoll.Result + strMod;
        }

        return new AttackResult
        {
            HitRoll = hitRoll,
            IsHit = isHit,
            Damage = Math.Max(0, damage),
            DamageDie = weapon.DamageDie,
            WeaponName = weapon.Name
        };
    }
}
