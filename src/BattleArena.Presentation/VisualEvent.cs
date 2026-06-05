namespace BattleArena.Presentation;

public sealed class VisualEvent
{
    public string EventType { get; init; } = "";
    public string ActorName { get; init; } = "";
    public string? TargetName { get; init; }
    public string OverlayText { get; init; } = "";
    public string Color { get; init; } = "#44ff44";
    public int DurationMs { get; init; } = 1500;
}
