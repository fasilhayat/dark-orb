namespace BattleArena.Presentation;

public static class CcVisualConfig
{
    public sealed record CcEffectVisual(string Color, string Animation, string Label);

    private static readonly Dictionary<string, CcEffectVisual> _effects = new()
    {
        ["Stun"]    = new("#d4a017", "blink", "STUNNED"),
        ["Freeze"]  = new("#44ccff", "blink", "FROZEN"),
        ["Sleep"]   = new("#aa44ff", "blink", "ASLEEP"),
        ["Petrify"] = new("#888888", "blink", "PETRIFIED"),
        ["Fear"]    = new("#8822aa", "blink", "FEARED"),
        ["Root"]    = new("#44cc44", "blink", "ROOTED"),
    };

    public static IReadOnlySet<string> CcEffectNames { get; } = new HashSet<string>(_effects.Keys);

    public static bool IsCcEffect(string effectName) => _effects.ContainsKey(effectName);

    public static string GetColor(string effectName) =>
        _effects.GetValueOrDefault(effectName)?.Color ?? "#88ccff";

    public static string GetLabel(string effectName) =>
        _effects.GetValueOrDefault(effectName)?.Label ?? effectName;

    public static string GetAnimation(string effectName) =>
        _effects.GetValueOrDefault(effectName)?.Animation ?? "none";
}
