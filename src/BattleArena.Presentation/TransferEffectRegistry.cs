namespace BattleArena.Presentation;

using System.Collections.Generic;

/// <summary>
/// Central registry of all transfer-effect visual configurations.
/// Add new entries here as new drain / siphon spells are introduced;
/// the renderer requires zero code changes.
/// </summary>
public static class TransferEffectRegistry
{
    private static readonly Dictionary<string, TransferEffectConfig> Configs = new()
    {
        ["Leech"] = new TransferEffectConfig
        {
            EffectName          = "Leech",
            TransferColor       = "#cc0000",
            ParticleColor       = "#ff4444",
            SourceGlowColor     = "#ff0000",
            DestinationGlowColor = "#ff0000",
            ParticleIntensity   = 0.6,
            StreamThickness     = 3.0,
            SoundId             = "LeechTick",
            OverlayLabel        = "LEECH",
            DurationMs          = 1000,
            IsPersistent        = true
        },
        ["LeechMana"] = new TransferEffectConfig
        {
            EffectName          = "LeechMana",
            TransferColor       = "#bb0044",
            ParticleColor       = "#ff4444",
            SourceGlowColor     = "#ff0000",
            DestinationGlowColor = "#ff0000",
            ParticleIntensity   = 0.5,
            StreamThickness     = 2.5,
            SoundId             = "LeechTick",
            OverlayLabel        = "MANA LEECH",
            DurationMs          = 1000,
            IsPersistent        = true
        }
    };

    /// <summary>Look up a config by effect name. Falls back to a default red transfer config.</summary>
    public static TransferEffectConfig GetConfig(string effectName)
    {
        if (Configs.TryGetValue(effectName, out var config))
            return config;
        return new TransferEffectConfig
        {
            EffectName = effectName,
            OverlayLabel = effectName.ToUpperInvariant()
        };
    }

    /// <summary>
    /// Register a new or override an existing config at runtime.
    /// Used by spells to inject their visual theme during setup.
    /// </summary>
    public static void Register(string effectName, TransferEffectConfig config)
    {
        Configs[effectName] = config;
    }
}