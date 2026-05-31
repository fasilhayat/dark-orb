namespace BattleArena.Application.Models;

// Classifies an attack event for narrative phrase selection.
// Determined from the d20 roll, attack/defense margin, and special flags.
public enum NarrativeContext
{
    CriticalHit,   // natural 20 — auto-hit, double damage
    CrushingHit,   // margin >= 8 above DefensePower
    SolidHit,      // margin 4–7
    GlancingHit,   // margin 0–3 (barely made it)
    NearMiss,      // missed by 1–3
    WideMiss,      // missed by 4+
    Fumble         // natural 1 — auto-miss, self-debuff
}
