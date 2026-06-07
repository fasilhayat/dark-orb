namespace BattleArena.Presentation;

/// <summary>
/// Visual and audio configuration for a resource-transfer effect (Leech, Life Drain, Soul Siphon, etc.).
/// Every transfer spell references one of these; the same renderer handles all of them.
/// </summary>
public sealed class TransferEffectConfig
{
    /// <summary>Unique identifier matching <see cref="StatusEffect.Name"/> or spell name.</summary>
    public string EffectName { get; init; } = "";

    /// <summary>Primary color of the energy stream.</summary>
    public string TransferColor { get; init; } = "#ff4444";

    /// <summary>Color of emitted particles along the stream.</summary>
    public string ParticleColor { get; init; } = "#ff4444";

    /// <summary>Glow color on the source (resource-giver).</summary>
    public string SourceGlowColor { get; init; } = "#ff4444";

    /// <summary>Glow color on the destination (resource-receiver).</summary>
    public string DestinationGlowColor { get; init; } = "#ff4444";

    /// <summary>Particle intensity (0.0 – 1.0).</summary>
    public double ParticleIntensity { get; init; } = 0.6;

    /// <summary>Thickness of the energy stream in logical units.</summary>
    public double StreamThickness { get; init; } = 3.0;

    /// <summary>Pulse speed of the stream (1.0 = normal).</summary>
    public double StreamPulseSpeed { get; init; } = 1.0;

    /// <summary>Sound effect ID played during the transfer.</summary>
    public string SoundId { get; init; } = "LeechTick";

    /// <summary>Display overlay text (e.g. "HP LEECH", "MANA DRAIN").</summary>
    public string OverlayLabel { get; init; } = "LEECH";

    /// <summary>Duration of the visual effect in milliseconds.</summary>
    public int DurationMs { get; init; } = 1000;

    /// <summary>
    /// True when the effect should render a persistent border on the affected character card.
    /// </summary>
    public bool IsPersistent { get; init; }
}