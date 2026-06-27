namespace BattleArena.Presentation;

using System.Collections.Generic;

public static class SpellSymbolRegistry
{
    private static readonly Dictionary<string, SpellSymbolEffect> Exact = new()
    {
        ["Hold Person"] = new() { Symbol = "\u26d3", Color = "#ff88cc", FontFamily = "Segoe UI Symbol" },
        ["Hold Animal"] = new() { Symbol = "\u26d3", Color = "#ff88cc", FontFamily = "Segoe UI Symbol" },
        ["Charm Person"] = new() { Symbol = "\u2665", Color = "#ff4488", FontFamily = "Times New Roman" },
        ["Charm Enemy"] = new() { Symbol = "\u2665", Color = "#ff4488", FontFamily = "Times New Roman" },
        ["Goodberry"] = new() { Symbol = "\u2618", Color = "#66dd66", FontFamily = "Segoe UI Symbol" },
        ["Barkskin"] = new() { Symbol = "\u2618", Color = "#66dd66", FontFamily = "Segoe UI Symbol" },
        ["Vampiric Touch"] = new() { Symbol = "\u2e38", Color = "#cc44cc", FontFamily = "Times New Roman" },
        ["Mind Siphon"] = new() { Symbol = "\u2e38", Color = "#cc44cc", FontFamily = "Times New Roman" },
    };

    private sealed record Rule(System.Func<string, bool> Match, SpellSymbolEffect Effect);

    private static readonly Rule[] KeywordRules =
    {
        new(n => n.Contains("Fire") || n.Contains("Flame") || n.Contains("Inferno") || n.Contains("Burn"),
            new() { Symbol = "\u2604", Color = "#ff6600", FontFamily = "Segoe UI Symbol" }),
        new(n => n.Contains("Siphon") || n.Contains("Syphon") || n.Contains("Leech") || n.Contains("Drain") || n.Contains("Vampiric"),
            new() { Symbol = "\u2e38", Color = "#cc44cc", FontFamily = "Times New Roman" }),
        new(n => n.Contains("Charm") || n.Contains("Dominate"),
            new() { Symbol = "\u2665", Color = "#ff4488", FontFamily = "Times New Roman" }),
        new(n => n.Contains("Hold") || n.Contains("Paralyze"),
            new() { Symbol = "\u26d3", Color = "#ff88cc", FontFamily = "Segoe UI Symbol" }),
        new(n => (n.Contains("Heal") || n.Contains("Cure") || n.Contains("Restor")) && !n.Contains("Harm"),
            new() { Symbol = "\u271a", Color = "#44cc44", FontFamily = "Times New Roman" }),
        new(n => n.Contains("Regenerat") || n.Contains("Rejuvenat"),
            new() { Symbol = "\u2618", Color = "#66dd66", FontFamily = "Segoe UI Symbol" }),
    };

    public static SpellSymbolEffect? Lookup(string spellName)
    {
        if (string.IsNullOrEmpty(spellName))
            return null;

        if (Exact.TryGetValue(spellName, out var exact))
            return exact;

        foreach (var rule in KeywordRules)
            if (rule.Match(spellName))
                return rule.Effect;

        return null;
    }
}
