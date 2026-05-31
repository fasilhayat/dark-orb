namespace BattleArena.Application.Models;

/// <summary>
/// Outcome of a single on-hit effect application attempt.
/// Applied=true  → effect landed and was added to target's active effects.
/// Applied=false, WasResisted=true  → target's resistance shook off the effect (roll shown in log).
/// Applied=false, WasResisted=false → base ApplicationChance roll failed (no message needed — effect simply didn't trigger).
/// </summary>
public record EffectApplicationResult(
    bool Applied,
    bool WasResisted,
    int Roll,
    int TotalResistance,
    string EffectName);
