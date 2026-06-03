using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace BattleArena.Gui;

internal static class PortraitResolver
{
    private static readonly Dictionary<string, string> NameToFile = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Kaela Vornskald"] = "kaela-vornskald.png",
        ["Ser Garrick Dawnshield"] = "ser-garrick-dawnshield.png",
        ["Vaelith Moonveil"] = "vaelith-moonveil.png",
        ["Sister Elira Vane"] = "sister-elira-vane.png",
        ["Lord Aethor Valeborn"] = "lord-aethor-valeborn.png",
        ["Finnick Bramblefoot"] = "finnick-quickfingers-bramblefoot.png",
        ["Korg Stonefist"] = "korg-stonefist.png",
        ["Graveworm"] = "graveworm.png",
        ["Shadowmere"] = "shadowmere.png",
        ["Old Man Kael"] = "old-man-kael.png",
        ["Greta Ironhand"] = "greta-ironhand.png",
        ["Merchant Vex"] = "merchant-vex.png",
        ["High Priestess Luna"] = "high-priestess luna.png",
        ["Lysander the Bard"] = "lysander-the-bard.png",
        ["Elder Treant"] = "elder-treant.png",
        ["Infernal Commander Maleth"] = "infernal-commander-maleth.png",
    };

    private static readonly string PortraitDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Portraits");
    private static readonly Dictionary<string, Bitmap?> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<string> KnownNames => NameToFile.Keys;

    public static bool HasPortrait(string characterName) =>
        NameToFile.ContainsKey(characterName);

    public static Bitmap? GetPortrait(string characterName)
    {
        if (Cache.TryGetValue(characterName, out var cached))
            return cached;

        if (!NameToFile.TryGetValue(characterName, out var filename))
        {
            Cache[characterName] = null;
            return null;
        }

        var path = Path.Combine(PortraitDir, filename);
        if (!File.Exists(path))
        {
            Cache[characterName] = null;
            return null;
        }

        try
        {
            var bitmap = new Bitmap(path);
            Cache[characterName] = bitmap;
            return bitmap;
        }
        catch
        {
            Cache[characterName] = null;
            return null;
        }
    }
}
