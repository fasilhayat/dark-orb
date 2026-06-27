namespace BattleArena.Presentation;

public sealed class VisualEvent
{
    public string EventType { get; init; } = "";
    public string ActorName { get; init; } = "";
    public string? TargetName { get; init; }
    public string OverlayText { get; init; } = "";
    public string Color { get; init; } = "#44ff44";
    public int DurationMs { get; init; } = 1500;

    public string MainForeground { get; init; } = "#ffffff";

    public string? EffectName { get; init; }
    public bool IsPersistent { get; init; }
    public int HealAmount { get; init; }
    public int DamagePreviewAmount { get; init; }
    public int TargetMaxHp { get; init; }
    public int HpBefore { get; init; }

    // Leech transfer visualization
    public int LeechAmount { get; init; }
    public string? LeechCasterName { get; init; }
    public string LeechResourceType { get; init; } = "HP";
    public SpellSymbolEffect? SpellSymbol { get; init; }
}
