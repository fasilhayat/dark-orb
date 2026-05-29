namespace BattleArena.Application.Services;

/// <summary>
/// Computes the display severity label for a hit based on damage as a fraction of the target's max HP.
/// Shared between the Demo console renderer and acceptance tests so both use identical label thresholds.
/// </summary>
public static class CombatHitLabelService
{
    public static string GetLabel(int damage, int targetMaxHp)
    {
        var pct = (double)damage / Math.Max(1, targetMaxHp);
        return pct >= 0.25 ? "CRUSHING HIT"
             : pct >= 0.15 ? "HEAVY HIT"
             : pct >= 0.08 ? "SOLID HIT"
             : pct >= 0.03 ? "GLANCING HIT"
             : "GRAZE";
    }
}
