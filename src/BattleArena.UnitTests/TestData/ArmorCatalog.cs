namespace BattleArena.UnitTests.TestData;

using Core.Entities;

// ─────────────────────────────────────────────────────────────────────────────
// Mirrors a representative spread of armor rows from 02-seed-data.sql.
// Values must match the SQL: armor_class, mitigation, max_dexterity_bonus.
// Update here when the seed changes.
// ─────────────────────────────────────────────────────────────────────────────
public static class ArmorCatalog
{
    private static readonly Dictionary<string, Armor> _entries = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── Light ─────────────────────────────────────── AC  MIT  MaxDex
        ["Leather Armor"]   = Make("Leather Armor",   11,  1,  99),
        ["Studded Leather"] = Make("Studded Leather", 12,  1,  99),

        // ── Medium ────────────────────────────────────── AC  MIT  MaxDex
        ["Hide Armor"]      = Make("Hide Armor",      12,  2,   2),
        ["Scale Mail"]      = Make("Scale Mail",      14,  2,   2),

        // ── Heavy ─────────────────────────────────────── AC  MIT  MaxDex
        ["Chain Mail"]      = Make("Chain Mail",      16,  3,   0),
        ["Plate Armor"]     = Make("Plate Armor",     18,  5,   0),

        // ── Caster / unarmored ────────────────────────── AC  MIT  MaxDex
        ["Robes"]           = Make("Robes",           10,  0,   6),
    };

    public static Armor Get(string name)
    {
        if (!_entries.TryGetValue(name, out var armor))
            throw new KeyNotFoundException(
                $"Armor '{name}' not found in ArmorCatalog. " +
                $"Valid entries: {string.Join(", ", _entries.Keys)}");

        // Return a copy so tests cannot mutate the catalog
        return new Armor
        {
            Name              = armor.Name,
            ArmorClass        = armor.ArmorClass,
            Mitigation        = armor.Mitigation,
            MaxDexterityBonus = armor.MaxDexterityBonus,
        };
    }

    private static Armor Make(string name, int ac, int mit, int maxDex) => new()
    {
        Name              = name,
        ArmorClass        = ac,
        Mitigation        = mit,
        MaxDexterityBonus = maxDex,
    };
}
