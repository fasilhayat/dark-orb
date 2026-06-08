namespace BattleArena.Presentation;

using System;
using System.Collections.Generic;

[Flags]
public enum EffectVisualTarget
{
    None    = 0,
    HpBar   = 1,
    ManaBar = 2,
}

public static class EffectVisualConfig
{
    private static readonly HashSet<string> _displayed = new(StringComparer.OrdinalIgnoreCase)
    {
        "Burning", "Ignite", "Frozen", "Freeze", "Shocked", "Stun",
        "Sleep", "Fear", "Petrify", "Poisoned", "Bleeding", "Leech", "LeechMana",
        "Electrified",
    };

    private static readonly Dictionary<string, EffectVisualTarget> _targets = new(StringComparer.OrdinalIgnoreCase)
    {
        // HP-affecting effects -> blink HP bar border
        ["Burning"]   = EffectVisualTarget.HpBar,
        ["Ignite"]    = EffectVisualTarget.HpBar,
        ["Poisoned"]  = EffectVisualTarget.HpBar,
        ["Bleeding"]  = EffectVisualTarget.HpBar,

        // TM-affecting effects -> card border only (HP bar border stays neutral)
        ["Electrified"] = EffectVisualTarget.None,
        ["Shocked"]   = EffectVisualTarget.None,
        ["Stun"]      = EffectVisualTarget.None,
        ["Frozen"]    = EffectVisualTarget.None,
        ["Freeze"]    = EffectVisualTarget.None,
        ["Sleep"]     = EffectVisualTarget.None,
        ["Fear"]      = EffectVisualTarget.None,
        ["Petrify"]   = EffectVisualTarget.None,
        ["Root"]      = EffectVisualTarget.None,

        // Leech — HP + mana
        ["Leech"]     = EffectVisualTarget.HpBar | EffectVisualTarget.ManaBar,
        ["LeechMana"] = EffectVisualTarget.ManaBar,
    };

    public static bool IsDisplayed(string effectName) => _displayed.Contains(effectName);

    public static EffectVisualTarget GetVisualTarget(string effectName)
        => _targets.TryGetValue(effectName, out var target) ? target : EffectVisualTarget.None;

    public static bool AffectsHpBar(string effectName)
        => (GetVisualTarget(effectName) & EffectVisualTarget.HpBar) == EffectVisualTarget.HpBar;

    public static bool AffectsManaBar(string effectName)
        => (GetVisualTarget(effectName) & EffectVisualTarget.ManaBar) == EffectVisualTarget.ManaBar;
}
