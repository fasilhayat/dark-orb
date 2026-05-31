namespace BattleArena.Application.Models;

using Core.Entities;
using Core.Entities.Enums;

public class AttackResult
{
    public int HitRoll { get; set; }
    public bool IsHit { get; set; }
    public bool IsCriticalHit { get; set; }
    public bool IsFumble { get; set; }
    /// <summary>
    /// Applied to attacker's AttackPower on their next turn when a fumble occurs (-2).
    /// </summary>
    public int AttackPowerPenalty { get; set; }
    public int Damage { get; set; }
    public DieType DamageDie { get; set; }
    public string WeaponName { get; set; } = string.Empty;
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public DamageContext? DamageContext { get; set; }
    public List<StatusEffect> AppliedStatusEffects { get; set; } = new();
}
