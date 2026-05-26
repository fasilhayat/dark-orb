using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Models;

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
}
