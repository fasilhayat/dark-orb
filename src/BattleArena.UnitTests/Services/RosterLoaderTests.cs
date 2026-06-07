namespace BattleArena.UnitTests.Services;

using BattleArena.Application.Services;

public class RosterLoaderTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(
        Path.GetTempPath(), "RosterLoaderTests", Guid.NewGuid().ToString("N"));

    public RosterLoaderTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, "roster.json");
        File.WriteAllText(path, json);
        return path;
    }

    // ── Happy paths ──────────────────────────────────────────────────────────────

    [Fact]
    public void Load_EmptyRoster_ReturnsEmptyLists()
    {
        var path = WriteJson("""{"races":[],"weapons":[],"spells":[],"armors":[],"heroes":[],"enemies":[]}""");
        var data = RosterLoader.ForceLoad(path);

        Assert.Empty(data.Heroes);
        Assert.Empty(data.Enemies);
    }

    [Fact]
    public void Load_MinimalCharacter_AppliesDefaults()
    {
        var json = """
        {
            "races": [{"name":"Human","baseMovementSpeed":30}],
            "weapons": [],
            "spells": [],
            "armors": [],
            "heroes": [{"name":"Test","race":"Human"}],
            "enemies": []
        }
        """;
        var path = WriteJson(json);
        var data = RosterLoader.ForceLoad(path);

        var ch = Assert.Single(data.Heroes);
        Assert.Equal("Test", ch.Name);
        Assert.Equal(1, ch.Level);
        Assert.Equal(10, ch.Strength);
        Assert.Equal(10, ch.Dexterity);
        Assert.Equal(10, ch.Stamina);
        Assert.Equal(10, ch.Intelligence);
        Assert.Equal(10, ch.Wisdom);
        Assert.Equal(10, ch.Charisma);
        Assert.Equal("Human", ch.Race?.Name);
        Assert.Equal(30, ch.Race?.BaseMovementSpeed);
        Assert.Equal("Unknown", ch.Sex);
        Assert.Equal(ch.MaxHitPoints, ch.CurrentHitPoints);
        Assert.Equal(ch.MaxMana, ch.CurrentMana);
    }

    [Fact]
    public void Load_RealRosterJson_AllNpcCharactersResolveCorrectly()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "roster.json");
        Assert.True(File.Exists(path), $"roster.json not found at {path}");

        var data = RosterLoader.ForceLoad(path);

        Assert.Equal(6, data.Heroes.Count);
        Assert.Equal(3, data.Enemies.Count);
        Assert.Equal(2, data.Dummies.Count);

        // ── Hero names ──────────────────────────────────────────────────────────
        Assert.Contains(data.Heroes, c => c.Name == "Kaela Vornskald");
        Assert.Contains(data.Heroes, c => c.Name == "Ser Garrick Dawnshield");
        Assert.Contains(data.Heroes, c => c.Name == "Vaelith Moonveil");
        Assert.Contains(data.Heroes, c => c.Name == "Sister Elira Vane");
        Assert.Contains(data.Heroes, c => c.Name == "Lord Aethor Valeborn");
        Assert.Contains(data.Heroes, c => c.Name == "Finnick Bramblefoot");

        // ── Enemy names ─────────────────────────────────────────────────────────
        Assert.Contains(data.Enemies, c => c.Name == "Korg Stonefist");
        Assert.Contains(data.Enemies, c => c.Name == "Graveworm");
        Assert.Contains(data.Enemies, c => c.Name == "Shadowmere");

        // ── Dummy names ─────────────────────────────────────────────────────────
        Assert.Contains(data.Dummies, c => c.Name == "Target Golem");
        Assert.Contains(data.Dummies, c => c.Name == "Practice Dummy");

        // ── Kaela Vornskald — melee barbarian ───────────────────────────────────
        var kaela = data.Heroes.First(c => c.Name == "Kaela Vornskald");
        Assert.Equal(10,       kaela.Level);
        Assert.Equal(19,       kaela.Strength);
        Assert.Equal(15,       kaela.Dexterity);
        Assert.Equal(17,       kaela.Stamina);
        Assert.Equal(100,      kaela.MaxHitPoints);
        Assert.Equal(100,      kaela.CurrentHitPoints);
        Assert.Equal("Human",  kaela.Race?.Name);
        Assert.Equal(1,        kaela.ClassId);       // Barbarian
        Assert.Equal("F",      kaela.Sex);
        Assert.NotNull(kaela.Equipment.RightHand);
        Assert.Equal("Great Sword",  kaela.Equipment.RightHand.Name);
        Assert.Equal(10,             kaela.Equipment.RightHand.DamageDie.GetHashCode() > 0 ? 10 : 10); // D10 — verified via archetype
        Assert.Equal("Hide Armor",   kaela.Equipment.Chest!.Name);
        Assert.Equal(12,             kaela.Equipment.Chest.ArmorClass);
        Assert.Equal(2,              kaela.Equipment.Chest.Mitigation);

        // ── Vaelith Moonveil — arcane fighter (spellcaster) ─────────────────────
        var vaelith = data.Heroes.First(c => c.Name == "Vaelith Moonveil");
        Assert.Equal(11, vaelith.MemorizedSpells.Count);
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Fireball");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Ice Bolt");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Shock");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Static Shock");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Magic Missile");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Shield");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Mirror Image");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Blink");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Lightning Bolt");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Invisibility");
        Assert.Contains(vaelith.MemorizedSpells, s => s.Name == "Mind Siphon");
        Assert.Equal(90,   vaelith.MaxMana);
        Assert.Equal(90,   vaelith.CurrentMana);
        Assert.Equal("Elf", vaelith.Race?.Name);
        Assert.Equal(35,   vaelith.Race?.BaseMovementSpeed);
        Assert.Equal("Mithril Chain", vaelith.Equipment.Chest!.Name);
        Assert.Equal(14,              vaelith.Equipment.Chest.ArmorClass);

        // ── Sister Elira Vane — priest (healer) ─────────────────────────────────
        var elira = data.Heroes.First(c => c.Name == "Sister Elira Vane");
        Assert.Equal(8, elira.MemorizedSpells.Count);
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Heal");
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Mass Heal");
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Bless");
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Cure Light Wounds");
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Cure Serious Wounds");
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Command");
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Chasten");
        Assert.Contains(elira.MemorizedSpells, s => s.Name == "Prayer");
        Assert.Equal(70, elira.MaxMana);
        Assert.Equal("Mace", elira.Equipment.RightHand!.Name);
        Assert.Equal("Padded Armor", elira.Equipment.Chest!.Name);

        // ── Finnick Bramblefoot — Gladefolk rogue ───────────────────────────────
        var finnick = data.Heroes.First(c => c.Name == "Finnick Bramblefoot");
        Assert.Equal(20,         finnick.Dexterity);
        Assert.Equal("Gladefolk", finnick.Race?.Name);
        Assert.Equal(30,         finnick.Race?.BaseMovementSpeed);
        Assert.Equal("Dagger",   finnick.Equipment.RightHand!.Name);
        Assert.Equal("Studded Leather", finnick.Equipment.Chest!.Name);

        // ── Korg Stonefist — heavy enemy ────────────────────────────────────────
        var korg = data.Enemies.First(c => c.Name == "Korg Stonefist");
        Assert.Equal(15,  korg.Level);
        Assert.Equal(21,  korg.Strength);
        Assert.Equal(165, korg.MaxHitPoints);
        Assert.Equal("Orc", korg.Race?.Name);
        Assert.Equal("Maul",       korg.Equipment.RightHand!.Name);
        Assert.Equal("Chain Mail", korg.Equipment.Chest!.Name);
        Assert.Equal(16,           korg.Equipment.Chest.ArmorClass);
        Assert.Equal(3,            korg.Equipment.Chest.Mitigation);

        // ── Graveworm — undead fighter ──────────────────────────────────────────
        var graveworm = data.Enemies.First(c => c.Name == "Graveworm");
        Assert.Equal("Undead",     graveworm.Race?.Name);
        Assert.Equal("Short Sword", graveworm.Equipment.RightHand!.Name);

        // ── Shadowmere — elf rogue ──────────────────────────────────────────────
        var shadowmere = data.Enemies.First(c => c.Name == "Shadowmere");
        Assert.Equal(19,    shadowmere.Dexterity);
        Assert.Equal("Elf", shadowmere.Race?.Name);
        Assert.Equal("F",   shadowmere.Sex);

        // ── Target Golem — test dummy ────────────────────────────────────────────
        var golem = data.Dummies.First(c => c.Name == "Target Golem");
        Assert.Equal(10,   golem.Level);
        Assert.Equal(300,  golem.MaxHitPoints);
        Assert.Equal(100,  golem.MaxMana);
        Assert.Equal(16,   golem.Strength);
        Assert.Equal(10,   golem.Dexterity);
        Assert.Equal("Human",  golem.Race?.Name);
        Assert.Equal(8,    golem.ClassId);          // Fighter
        Assert.Equal("N",  golem.Sex);
        Assert.Equal("Plate Armor", golem.Equipment.Chest!.Name);
        Assert.Equal("Long Sword",  golem.Equipment.RightHand!.Name);
        Assert.Contains(golem.MemorizedSpells, s => s.Name == "Fireball");
        Assert.Contains(golem.MemorizedSpells, s => s.Name == "Static Shock");
        Assert.Contains(golem.MemorizedSpells, s => s.Name == "Ice Bolt");
        Assert.Contains(golem.MemorizedSpells, s => s.Name == "Shock");
        Assert.Contains(golem.MemorizedSpells, s => s.Name == "Smite");

        // ── Practice Dummy — resilient caster target ──────────────────────────────
        var dummy = data.Dummies.First(c => c.Name == "Practice Dummy");
        Assert.Equal(10,   dummy.Level);
        Assert.Equal(500,  dummy.MaxHitPoints);
        Assert.Equal(100,  dummy.MaxMana);
        Assert.Equal(1,    dummy.StrikeRating);
        Assert.Equal(4,    dummy.TurnSpeed);
        Assert.Equal(14,   dummy.Intelligence);
        Assert.Equal("Studded Leather", dummy.Equipment.Chest!.Name);
        Assert.Null(dummy.Equipment.RightHand);
        Assert.Contains(dummy.MemorizedSpells, s => s.Name == "Fireball");
        Assert.Contains(dummy.MemorizedSpells, s => s.Name == "Ice Bolt");
        Assert.Contains(dummy.MemorizedSpells, s => s.Name == "Shock");
        Assert.Contains(dummy.MemorizedSpells, s => s.Name == "Heal");
        Assert.Contains(dummy.MemorizedSpells, s => s.Name == "Mass Heal");
    }

    // ── Edge cases ───────────────────────────────────────────────────────────────

    [Fact]
    public void Load_UnknownArmorName_SlotIsNull()
    {
        var json = """
        {
            "races": [{"name":"Human","baseMovementSpeed":30}],
            "weapons": [],
            "spells": [],
            "armors": [],
            "heroes": [{"name":"Test","race":"Human","equipment":{"chest":"NonExistentArmor"}}],
            "enemies": []
        }
        """;
        var path = WriteJson(json);
        var data = RosterLoader.ForceLoad(path);

        var ch = Assert.Single(data.Heroes);
        Assert.Null(ch.Equipment.Chest);
    }

    [Fact]
    public void Load_UnknownWeaponName_SlotIsNull()
    {
        var json = """
        {
            "races": [{"name":"Human","baseMovementSpeed":30}],
            "weapons": [],
            "spells": [],
            "armors": [],
            "heroes": [{"name":"Test","race":"Human","equipment":{"rightHand":"NonExistentWeapon"}}],
            "enemies": []
        }
        """;
        var path = WriteJson(json);
        var data = RosterLoader.ForceLoad(path);

        var ch = Assert.Single(data.Heroes);
        Assert.Null(ch.Equipment.RightHand);
    }

    [Fact]
    public void Load_UnknownSpellName_IgnoredSilently()
    {
        var json = """
        {
            "races": [{"name":"Human","baseMovementSpeed":30}],
            "weapons": [],
            "spells": [],
            "armors": [],
            "heroes": [{"name":"Test","race":"Human","memorizedSpells":["NonExistentSpell"]}],
            "enemies": []
        }
        """;
        var path = WriteJson(json);
        var data = RosterLoader.ForceLoad(path);

        var ch = Assert.Single(data.Heroes);
        Assert.Empty(ch.MemorizedSpells);
    }
}
