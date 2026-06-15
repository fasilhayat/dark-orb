namespace BattleArena.Presentation;

using System;
using System.Collections.Generic;
using BattleArena.Core.Entities.Enums;

[Flags]
public enum EffectVisualTarget
{
    None    = 0,
    HpBar   = 1,
    ManaBar = 2,
    TmBar   = 4,
}

public static class EffectVisualConfig
{
    private static readonly HashSet<string> _displayed = new(StringComparer.OrdinalIgnoreCase)
    {
        "Burning", "Ignite", "Frozen", "Freeze", "Shocked", "Stun",
        "Sleep", "Fear", "Petrify", "Poisoned", "Bleeding", "Leech", "LeechMana",
        "Electrified",
        "Confused",
        "Charmed",
    };

    private static readonly Dictionary<string, EffectVisualTarget> _targets = new(StringComparer.OrdinalIgnoreCase)
    {
        // HP-affecting effects -> blink HP bar border
        ["Burning"]   = EffectVisualTarget.HpBar,
        ["Ignite"]    = EffectVisualTarget.HpBar,
        ["Poisoned"]  = EffectVisualTarget.HpBar,
        ["Bleeding"]  = EffectVisualTarget.HpBar,

        // TM-affecting effects -> blink TM bar border
        ["Electrified"] = EffectVisualTarget.TmBar,
        ["Confused"]    = EffectVisualTarget.TmBar,
        ["Charmed"]     = EffectVisualTarget.ManaBar,
        ["Shocked"]   = EffectVisualTarget.TmBar,
        ["Stun"]      = EffectVisualTarget.None,
        ["Frozen"]    = EffectVisualTarget.None,
        ["Freeze"]    = EffectVisualTarget.None,
        ["Sleep"]     = EffectVisualTarget.None,
        ["Fear"]      = EffectVisualTarget.None,
        ["Petrify"]   = EffectVisualTarget.None,
        ["Root"]      = EffectVisualTarget.None,

        // Leech — HP only
        ["Leech"]     = EffectVisualTarget.HpBar,
        // LeechMana — Mana only
        ["LeechMana"] = EffectVisualTarget.ManaBar,
    };

    public static bool IsDisplayed(string effectName) => _displayed.Contains(effectName);

    public static string GetColor(string effectName)
    {
        if (CcVisualConfig.IsCcEffect(effectName))
            return CcVisualConfig.GetColor(effectName);
        return effectName switch
        {
            "Burning"   => "#ff6600",
            "Ignite"    => "#ff4400",
            "Frozen"    => "#44ccff",
            "Freeze"    => "#44ccff",
            "Shocked"   => "#ffff44",
            "Electrified" => "#88ddff",
            "Poisoned"  => "#44ff44",
            "Bleeding"  => "#ff4444",
            "Leech" or "LeechMana" => TransferEffectRegistry.GetConfig(effectName).TransferColor,
            "Confused"  => "#aaaaaa",
            "Charmed"   => "#ff88aa",
            _           => "#88ccff",
        };
    }

    public static string GetElementColor(ElementalType type) => type switch
    {
        ElementalType.Fire => "#ff6600",
        ElementalType.Ice => "#44ccff",
        ElementalType.Lightning => "#ffff44",
        ElementalType.Poison => "#44ff44",
        ElementalType.Holy => "#ffffaa",
        ElementalType.Shadow => "#aa44aa",
        ElementalType.Acid => "#44ff44",
        _ => "#ffffff",
    };

    public static string? GetElementDoTName(ElementalType type) => type switch
    {
        ElementalType.Fire => "Burning",
        ElementalType.Ice => "Chilled",
        ElementalType.Lightning => "Shocked",
        ElementalType.Poison => "Poisoned",
        _ => null,
    };

    public static string GetSpellOverlayColor(string spellName)
    {
        var lower = spellName.ToLowerInvariant();
        if (lower.Contains("fire") || lower.Contains("flame") || lower.Contains("burn") || lower.Contains("inferno"))
            return "#ff6600";
        if (lower.Contains("ice") || lower.Contains("frost") || lower.Contains("freeze") || lower.Contains("cold"))
            return "#44ccff";
        if (lower.Contains("shock") || lower.Contains("lightning") || lower.Contains("thunder") || lower.Contains("spark"))
            return "#ffff44";
        if (lower.Contains("heal") || lower.Contains("cure") || lower.Contains("bless") || lower.Contains("restore"))
            return "#44cc44";
        if (lower.Contains("stun") || lower.Contains("sleep") || lower.Contains("fear") || lower.Contains("charm"))
            return "#cc44cc";
        if (lower.Contains("poison") || lower.Contains("acid"))
            return "#44ff44";
        if (lower.Contains("arcane") || lower.Contains("magic") || lower.Contains("mystic"))
            return "#cc88ff";
        if (lower.Contains("shield") || lower.Contains("armor") || lower.Contains("ward") || lower.Contains("protect"))
            return "#88aaff";
        return "#ffffff";
    }

    public static EffectVisualTarget GetVisualTarget(string effectName)
        => _targets.TryGetValue(effectName, out var target) ? target : EffectVisualTarget.None;

    public static bool AffectsHpBar(string effectName)
        => (GetVisualTarget(effectName) & EffectVisualTarget.HpBar) == EffectVisualTarget.HpBar;

    public static bool AffectsManaBar(string effectName)
        => (GetVisualTarget(effectName) & EffectVisualTarget.ManaBar) == EffectVisualTarget.ManaBar;

    public static bool AffectsTmBar(string effectName)
        => (GetVisualTarget(effectName) & EffectVisualTarget.TmBar) == EffectVisualTarget.TmBar;
}
