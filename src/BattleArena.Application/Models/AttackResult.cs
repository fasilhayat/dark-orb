namespace BattleArena.Application.Models;

using Core.Entities;
using Core.Entities.Enums;

public class AttackResult
{
    public int HitRoll { get; set; }
    /// <summary>The defender's d20 roll (0 when no defense roll was made, e.g. old-style tests).</summary>
    public int DefenseRoll { get; set; }
    public bool IsHit { get; set; }
    public bool IsCriticalHit { get; set; }
    public bool IsFumble { get; set; }

    // ── Special outcomes (mutually exclusive) ─────────────────────────────────
    /// <summary>Attacker rolled 20, defender rolled 1 — triple damage auto-hit.</summary>
    public bool IsDevastatingStrike { get; set; }
    /// <summary>Both rolled 20 — mutual 50% weapon damage each.</summary>
    public bool IsClash { get; set; }
    /// <summary>Defender rolled 20 (non-special attacker) — auto-miss, defender gains TM.</summary>
    public bool IsPerfectParry { get; set; }
    /// <summary>Attacker rolled 1, defender rolled 20 — auto-miss, −4 AP, defender gains TM.</summary>
    public bool IsTotalReversal { get; set; }

    /// <summary>
    /// Applied to attacker's AttackPower on their next turn.
    /// -2 for a normal Fumble, -4 for a TotalReversal.
    /// </summary>
    public int AttackPowerPenalty { get; set; }

    /// <summary>
    /// TM awarded to the defender on PerfectParry or TotalReversal.
    /// Computed by CombatService based on attack type and engagement range.
    /// 0 for all other outcomes.
    /// </summary>
    public int DefenderTmBonus { get; set; }

    public int Damage { get; set; }
    public DieType DamageDie { get; set; }
    public string WeaponName { get; set; } = string.Empty;
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public DamageContext? DamageContext { get; set; }
    public List<StatusEffect> AppliedStatusEffects { get; set; } = new();
}
