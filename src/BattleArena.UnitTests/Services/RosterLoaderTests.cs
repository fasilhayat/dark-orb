namespace BattleArena.UnitTests.Services;

using BattleArena.Core.Entities;
using BattleArena.Gui.Data;

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
    public void Load_FullRoster_AllHeroesAndEnemiesMatchExpected()
    {
        var json = """
        {
            "races": [
                {"name":"Human","baseMovementSpeed":30},
                {"name":"Orc","baseMovementSpeed":30},
                {"name":"Elf","baseMovementSpeed":35},
                {"name":"Undead","baseMovementSpeed":30},
                {"name":"Half-Elf","baseMovementSpeed":30}
            ],
            "weapons": [
                {"name":"Longsword","damageDie":"D8","damageCount":1,"damageType":"Slashing","attackType":"Melee","attackBonus":2,"archetype":"Sword","hands":1},
                {"name":"Battle Axe","damageDie":"D8","damageCount":1,"damageType":"Slashing","attackType":"Melee","attackBonus":1,"archetype":"Axe","hands":1},
                {"name":"Orcish Axe","damageDie":"D10","damageCount":1,"damageType":"Slashing","attackType":"Melee","attackBonus":1,"archetype":"Axe","hands":1},
                {"name":"Ceremonial Mace","damageDie":"D6","damageCount":1,"damageType":"Bludgeoning","attackType":"Melee","attackBonus":2,"archetype":"Mace","hands":1},
                {"name":"Arcane Staff","damageDie":"D4","damageCount":1,"damageType":"Bludgeoning","attackType":"Melee","attackBonus":1,"archetype":"Staff","hands":2},
                {"name":"Poisoned Dagger","damageDie":"D4","damageCount":2,"damageType":"Piercing","attackType":"Melee","attackBonus":3,"archetype":"Dagger","hands":1}
            ],
            "spells": [
                {"name":"Fireball","school":"Evocation","damageDie":"D6","damageCount":3,"damageType":"Fire","attackBonus":2,"spellLevel":3,"turnMeterCost":90,"manaCost":50},
                {"name":"Ice Bolt","school":"Evocation","damageDie":"D8","damageCount":2,"damageType":"Ice","attackBonus":2,"spellLevel":2,"turnMeterCost":80,"manaCost":35},
                {"name":"Shock","school":"Evocation","damageDie":"D6","damageCount":2,"damageType":"Lightning","attackBonus":2,"spellLevel":2,"turnMeterCost":75,"manaCost":20},
                {"name":"Smite","school":"Evocation","damageDie":"D8","damageCount":2,"damageType":"Holy","attackBonus":2,"spellLevel":2,"turnMeterCost":80,"manaCost":35},
                {"name":"Heal","school":"Healing","damageDie":"D8","damageCount":2,"damageType":"Holy","spellLevel":2,"turnMeterCost":80,"manaCost":25},
                {"name":"Moonfire","school":"Evocation","damageDie":"D6","damageCount":2,"damageType":"Lightning","attackBonus":1,"spellLevel":2,"turnMeterCost":80,"manaCost":30},
                {"name":"Entangle","school":"CC","damageDie":"D4","damageCount":1,"damageType":"Bludgeoning","spellLevel":2,"turnMeterCost":80,"manaCost":25},
                {"name":"Shadow Bolt","school":"Other","damageDie":"D8","damageCount":2,"damageType":"Ice","attackBonus":2,"spellLevel":2,"turnMeterCost":80,"manaCost":35},
                {"name":"Soul Drain","school":"Other","damageDie":"D10","damageCount":1,"damageType":"Fire","attackBonus":1,"spellLevel":2,"turnMeterCost":80,"manaCost":25},
                {"name":"Root","school":"CC","damageDie":"D4","damageCount":1,"damageType":"Bludgeoning","spellLevel":2,"turnMeterCost":80,"manaCost":30},
                {"name":"Curse","school":"CC","damageDie":"D6","damageCount":1,"damageType":"Shadow","spellLevel":2,"turnMeterCost":70,"manaCost":20}
            ],
            "armors": [
                {"name":"Chain Mail","armorClass":16,"mitigation":2,"maxDexterityBonus":6,"movementPenalty":10},
                {"name":"Leather Armor","armorClass":11,"mitigation":1,"maxDexterityBonus":6,"movementPenalty":5},
                {"name":"Mage Robes","armorClass":14,"mitigation":0,"maxDexterityBonus":6,"turnMeterCostReduction":5},
                {"name":"Scaled Vestments","armorClass":12,"mitigation":1,"maxDexterityBonus":6,"movementPenalty":5},
                {"name":"Druidic Robes","armorClass":14,"mitigation":0,"maxDexterityBonus":6},
                {"name":"Orcish Hide","armorClass":12,"mitigation":2,"maxDexterityBonus":4,"movementPenalty":5},
                {"name":"Worn Leather","armorClass":11,"mitigation":1,"maxDexterityBonus":6,"movementPenalty":5},
                {"name":"Dark Robes","armorClass":14,"mitigation":0,"maxDexterityBonus":6,"turnMeterCostReduction":5},
                {"name":"Shadowweave Robes","armorClass":14,"mitigation":0,"maxDexterityBonus":6,"turnMeterCostReduction":5}
            ],
            "heroes": [
                {"name":"Theron","level":5,"strength":18,"dexterity":12,"intelligence":10,"race":"Human","classId":8,"className":"Fighter","sex":"M","strikeRating":14,"turnSpeed":10,"maxHitPoints":50,"equipment":{"chest":"Chain Mail","rightHand":"Longsword"}},
                {"name":"Gruk","level":3,"strength":16,"dexterity":8,"intelligence":8,"race":"Orc","classId":1,"className":"Barbarian","sex":"M","strikeRating":16,"turnSpeed":6,"maxHitPoints":35,"equipment":{"chest":"Leather Armor","rightHand":"Battle Axe"}},
                {"name":"Lyra","level":5,"strength":8,"dexterity":14,"intelligence":18,"race":"Elf","classId":5,"className":"Mage","sex":"F","strikeRating":13,"turnSpeed":8,"maxHitPoints":30,"maxMana":155,"equipment":{"chest":"Mage Robes"},"memorizedSpells":["Fireball","Ice Bolt","Shock"]},
                {"name":"Sera","level":4,"strength":12,"dexterity":10,"intelligence":16,"race":"Human","classId":4,"className":"Priest","sex":"F","strikeRating":14,"turnSpeed":8,"maxHitPoints":35,"maxMana":100,"equipment":{"chest":"Scaled Vestments","rightHand":"Ceremonial Mace"},"memorizedSpells":["Smite","Heal"]},
                {"name":"Elara","level":4,"strength":8,"dexterity":14,"intelligence":17,"race":"Elf","classId":7,"className":"Druid","sex":"F","strikeRating":14,"turnSpeed":9,"maxHitPoints":28,"maxMana":110,"equipment":{"chest":"Druidic Robes","rightHand":"Arcane Staff"},"memorizedSpells":["Moonfire","Entangle"]}
            ],
            "enemies": [
                {"name":"Krag","level":4,"strength":17,"dexterity":9,"intelligence":6,"race":"Orc","classId":1,"className":"Barbarian","sex":"M","strikeRating":15,"turnSpeed":7,"maxHitPoints":45,"equipment":{"chest":"Orcish Hide","rightHand":"Orcish Axe"}},
                {"name":"Skrix","level":2,"strength":9,"dexterity":16,"intelligence":10,"race":"Human","classId":9,"className":"Rogue","sex":"M","strikeRating":12,"turnSpeed":12,"maxHitPoints":20,"equipment":{"chest":"Worn Leather","rightHand":"Poisoned Dagger"}},
                {"name":"Mordak","level":3,"strength":7,"dexterity":12,"intelligence":16,"race":"Undead","classId":5,"className":"Mage","sex":"M","strikeRating":14,"turnSpeed":9,"maxHitPoints":25,"maxMana":60,"equipment":{"chest":"Dark Robes"},"memorizedSpells":["Shadow Bolt","Soul Drain","Root"]},
                {"name":"Zarath","level":5,"strength":6,"dexterity":12,"intelligence":18,"race":"Undead","classId":5,"className":"Mage","sex":"M","strikeRating":15,"turnSpeed":8,"maxHitPoints":28,"maxMana":85,"equipment":{"chest":"Shadowweave Robes"},"memorizedSpells":["Shadow Bolt","Soul Drain","Curse"]}
            ]
        }
        """;
        var path = WriteJson(json);
        var data = RosterLoader.ForceLoad(path);

        Assert.Equal(5, data.Heroes.Count);
        Assert.Equal(4, data.Enemies.Count);

        // Spot-check hero names
        Assert.Contains(data.Heroes, c => c.Name == "Theron");
        Assert.Contains(data.Heroes, c => c.Name == "Gruk");
        Assert.Contains(data.Heroes, c => c.Name == "Lyra");
        Assert.Contains(data.Heroes, c => c.Name == "Sera");
        Assert.Contains(data.Heroes, c => c.Name == "Elara");

        // Spot-check enemy names
        Assert.Contains(data.Enemies, c => c.Name == "Krag");
        Assert.Contains(data.Enemies, c => c.Name == "Skrix");
        Assert.Contains(data.Enemies, c => c.Name == "Mordak");
        Assert.Contains(data.Enemies, c => c.Name == "Zarath");

        // Check specific stats
        var theron = data.Heroes.First(c => c.Name == "Theron");
        Assert.Equal(18, theron.Strength);
        Assert.Equal(12, theron.Dexterity);
        Assert.Equal(10, theron.Intelligence);
        Assert.Equal(5, theron.Level);
        Assert.Equal("Fighter", theron.ClassName);
        Assert.Equal("Human", theron.Race?.Name);
        Assert.Equal(50, theron.MaxHitPoints);
        Assert.Equal(50, theron.CurrentHitPoints);

        // Check equipment resolution
        Assert.NotNull(theron.Equipment.Chest);
        Assert.Equal("Chain Mail", theron.Equipment.Chest.Name);
        Assert.Equal(16, theron.Equipment.Chest.ArmorClass);

        Assert.NotNull(theron.Equipment.RightHand);
        Assert.Equal("Longsword", theron.Equipment.RightHand.Name);

        // Check spell resolution
        var lyra = data.Heroes.First(c => c.Name == "Lyra");
        Assert.Equal(3, lyra.MemorizedSpells.Count);
        Assert.Contains(lyra.MemorizedSpells, s => s.Name == "Fireball");
        Assert.Contains(lyra.MemorizedSpells, s => s.Name == "Ice Bolt");
        Assert.Contains(lyra.MemorizedSpells, s => s.Name == "Shock");
        Assert.Equal(155, lyra.MaxMana);
        Assert.Equal(155, lyra.CurrentMana);

        // Check spellcaster has null equipment right hand (no weapon equipped in right hand slot)
        Assert.Null(lyra.Equipment.RightHand);
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
