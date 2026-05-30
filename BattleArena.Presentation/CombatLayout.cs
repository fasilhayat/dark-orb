namespace BattleArena.Presentation;

/// <summary>
/// Immutable snapshot of how combatants are arranged on each side.
/// Built once before playback and does not reference live simulator objects.
/// </summary>
public sealed class CombatLayout
{
    public IReadOnlyList<string> HeroNames { get; init; } = [];
    public IReadOnlyList<string> EnemyNames { get; init; } = [];
    public bool IsDuel { get; init; }

    public static CombatLayout From(
        IEnumerable<string> heroNames,
        IEnumerable<string> enemyNames,
        bool isDuel) => new()
    {
        HeroNames = heroNames.ToList(),
        EnemyNames = enemyNames.ToList(),
        IsDuel = isDuel
    };
}
