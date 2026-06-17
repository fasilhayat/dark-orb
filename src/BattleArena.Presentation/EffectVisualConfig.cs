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

    /// <summary>Base color for a spell school — used when no elemental type is present.</summary>
    public static string GetSchoolColor(SpellSchool school) => school switch
    {
        SpellSchool.Aegis      => "#4488cc",
        SpellSchool.Stormcraft => "#ff8800",
        SpellSchool.Verdancy   => "#44aa44",
        SpellSchool.Umbramancy => "#882288",
        SpellSchool.Mirage     => "#cc88ff",
        SpellSchool.Dominion   => "#ffcc44",
        SpellSchool.Deity      => "#ffffaa",
        _                      => "#ffffff",
    };

    /// <summary>
    /// Spell border/overlay color. Element color takes precedence over school
    /// when the spell has an elemental type.
    /// </summary>
    public static string GetSpellColor(SpellSchool school, ElementalType element) =>
        element == ElementalType.None ? GetSchoolColor(school) : GetElementColor(element);

    /// <summary>
    /// Animation applied to character card borders and bar borders during and after a
    /// spell event. Element-specific animation takes precedence over school animation.
    /// </summary>
    public static SpellAnimation GetSpellAnimation(SpellSchool school, ElementalType element)
    {
        if (element != ElementalType.None)
            return element switch
            {
                ElementalType.Fire      => SpellAnimation.Flicker,
                ElementalType.Ice       => SpellAnimation.Blink,
                ElementalType.Lightning => SpellAnimation.Flash,
                ElementalType.Poison    => SpellAnimation.Flicker,
                ElementalType.Holy      => SpellAnimation.Pulse,
                ElementalType.Shadow    => SpellAnimation.Drain,
                ElementalType.Acid      => SpellAnimation.Flicker,
                _                       => SpellAnimation.Flash,
            };

        return school switch
        {
            SpellSchool.Aegis      => SpellAnimation.Pulse,
            SpellSchool.Stormcraft => SpellAnimation.Flash,
            SpellSchool.Verdancy   => SpellAnimation.Pulse,
            SpellSchool.Umbramancy => SpellAnimation.Drain,
            SpellSchool.Mirage     => SpellAnimation.Shimmer,
            SpellSchool.Dominion   => SpellAnimation.Flash,
            SpellSchool.Deity      => SpellAnimation.HealGlow,
            _                      => SpellAnimation.Flash,
        };
    }

    /// <summary>
    /// Animation for a persistent status effect (DoT, CC, Leech) by effect name.
    /// Used to drive border flicker timers on character cards.
    /// </summary>
    public static SpellAnimation GetEffectAnimation(string effectName) => effectName switch
    {
        "Burning" or "Ignite"    => SpellAnimation.Flicker,
        "Poisoned" or "Bleeding" => SpellAnimation.Flicker,
        "Electrified"            => SpellAnimation.Flicker,
        "Leech" or "LeechMana"   => SpellAnimation.Drain,
        "Confused"               => SpellAnimation.Shimmer,
        "Charmed"                => SpellAnimation.Pulse,
        _ when CcVisualConfig.IsCcEffect(effectName) => CcVisualConfig.GetAnimation(effectName),
        _                        => SpellAnimation.Flicker,
    };

    /// <summary>
    /// Legacy overlay color lookup by spell name. Used when only the spell name is
    /// available (e.g. <see cref="T:BattleArena.Presentation.CombatPlaybackEngine"/>).
    /// Prefer <see cref="GetSpellColor"/> when school and element are known.
    /// </summary>
    public static string GetSpellOverlayColor(string spellName)
    {
        var lower = spellName.ToLowerInvariant();
        if (lower.Contains("fire") || lower.Contains("flame") || lower.Contains("burn") || lower.Contains("inferno"))
            return GetElementColor(ElementalType.Fire);
        if (lower.Contains("ice") || lower.Contains("frost") || lower.Contains("freeze") || lower.Contains("cold"))
            return GetElementColor(ElementalType.Ice);
        if (lower.Contains("shock") || lower.Contains("lightning") || lower.Contains("thunder") || lower.Contains("spark"))
            return GetElementColor(ElementalType.Lightning);
        if (lower.Contains("heal") || lower.Contains("cure") || lower.Contains("bless") || lower.Contains("restore"))
            return GetSchoolColor(SpellSchool.Deity);
        if (lower.Contains("stun") || lower.Contains("sleep") || lower.Contains("fear") || lower.Contains("charm"))
            return GetSchoolColor(SpellSchool.Umbramancy);
        if (lower.Contains("poison") || lower.Contains("acid"))
            return GetElementColor(ElementalType.Poison);
        if (lower.Contains("arcane") || lower.Contains("magic") || lower.Contains("mystic"))
            return GetSchoolColor(SpellSchool.Mirage);
        if (lower.Contains("shield") || lower.Contains("armor") || lower.Contains("ward") || lower.Contains("protect"))
            return GetSchoolColor(SpellSchool.Aegis);
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
