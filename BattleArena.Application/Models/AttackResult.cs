using BattleArena.Core.Entities.Enums;

namespace BattleArena.Application.Models;

public class AttackResult
{
    public int HitRoll { get; set; }
    public bool IsHit { get; set; }
    public int Damage { get; set; }
    public DieType DamageDie { get; set; }
    public string WeaponName { get; set; } = string.Empty;
}
