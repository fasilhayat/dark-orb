namespace BattleArena.Presentation;

public sealed record DamagePreviewConfig
{
    public int DevastationThresholdPercent { get; init; } = 25;
    public int DamagePreviewDurationMs { get; init; } = 800;
    public double ImpactFlashIntensity { get; init; } = 1.0;
    public bool EnableCardShake { get; init; } = false;

    public static DamagePreviewConfig Default { get; } = new();
}
