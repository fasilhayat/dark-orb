namespace BattleArena.Presentation;

public static class CcVisualConfig
{
    public sealed record CcEffectVisual(string Color, SpellAnimation Animation, string Label, string Icon = "\U0001F463");

    private static readonly Dictionary<string, CcEffectVisual> _effects = new()
    {
        ["Stun"]    = new("#d4a017", SpellAnimation.Blink, "STUNNED"),
        ["Hold"]    = new("#ff88aa", SpellAnimation.Blink, "HELD"),
        ["Freeze"]  = new("#44ccff", SpellAnimation.Blink, "FROZEN"),
        ["Sleep"]   = new("#aa44ff", SpellAnimation.Blink, "ASLEEP"),
        ["Petrify"] = new("#888888", SpellAnimation.Blink, "PETRIFIED"),
        ["Fear"]    = new("#8822aa", SpellAnimation.Blink, "FEARED"),
        ["Root"]    = new("#44cc44", SpellAnimation.Pulse, "ROOTED"),
    };

    public static string GetIcon(string effectName) =>
        _effects.GetValueOrDefault(effectName)?.Icon ?? "\U0001F463";

    private static readonly HashSet<string> _tmLockingEffects = new(StringComparer.OrdinalIgnoreCase)
    {
        "Stun", "Freeze", "Sleep", "Petrify",
    };

    public static bool IsTmLockingEffect(string effectName) =>
        _tmLockingEffects.Contains(effectName);

    private static readonly HashSet<string> _tmLockingCcLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "stunned", "frozen", "asleep", "petrified",
    };

    public static bool IsTmLockingCcLabel(string ccLabel) =>
        _tmLockingCcLabels.Contains(ccLabel);

    public static IReadOnlySet<string> CcEffectNames { get; } = new HashSet<string>(_effects.Keys);

    public static bool IsCcEffect(string effectName) => _effects.ContainsKey(effectName);

    public static string GetColor(string effectName) =>
        _effects.GetValueOrDefault(effectName)?.Color ?? "#88ccff";

    public static string GetLabel(string effectName) =>
        _effects.GetValueOrDefault(effectName)?.Label ?? effectName;

    public static SpellAnimation GetAnimation(string effectName) =>
        _effects.GetValueOrDefault(effectName)?.Animation ?? SpellAnimation.None;
}
