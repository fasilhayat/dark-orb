namespace BattleArena.UnitTests.Services;

using Application.Modifiers;
using Core.Entities;
using Core.Entities.Enums;
using Core.Models;

public class TerrainModifierTests
{
    private readonly TerrainModifier _sut = new();

    private static CombatModifierContext MakeCtx(string raceName, TerrainType terrain)
    {
        var source = new Weapon { Name = "Test", AttackType = AttackType.Melee, DamageDie = DieType.D6 };
        var race   = new Race { Name = raceName };
        var attacker = new Character { Name = "Attacker", Strength = 10, Race = race };
        var defender = new Character { Name = "Defender", Strength = 10, Race = race };
        return new CombatModifierContext
        {
            Attacker         = attacker,
            Defender         = defender,
            Source           = source,
            Range            = EngagementRange.Melee,
            Terrain          = terrain,
            BaseAttackPower  = 10,
            BaseDefensePower = 8
        };
    }

    [Fact]
    public void ElfInForest_GainsTwoAttackPower()
    {
        var ctx = MakeCtx("Elf", TerrainType.Forest);
        _sut.Apply(ctx);
        Assert.Equal(2, ctx.AttackPowerDelta);
        Assert.Equal(0, ctx.DefensePowerDelta);
    }

    [Fact]
    public void ElfInDesert_LosesOneAttackPower()
    {
        var ctx = MakeCtx("Elf", TerrainType.Desert);
        _sut.Apply(ctx);
        Assert.Equal(-1, ctx.AttackPowerDelta);
    }

    [Fact]
    public void DwarfInMountain_GainsTwoDefensePower()
    {
        var ctx = MakeCtx("Dwarf", TerrainType.Mountain);
        _sut.Apply(ctx);
        Assert.Equal(0, ctx.AttackPowerDelta);
        Assert.Equal(2, ctx.DefensePowerDelta);
    }

    [Fact]
    public void LizardInDesert_GainsOneAttackAndOneDefense()
    {
        var ctx = MakeCtx("Lizard", TerrainType.Desert);
        _sut.Apply(ctx);
        Assert.Equal(1, ctx.AttackPowerDelta);
        Assert.Equal(1, ctx.DefensePowerDelta);
    }

    [Fact]
    public void LizardInIcy_LosesOneAttackAndOneDefense()
    {
        var ctx = MakeCtx("Lizard", TerrainType.Icy);
        _sut.Apply(ctx);
        Assert.Equal(-1, ctx.AttackPowerDelta);
        Assert.Equal(-1, ctx.DefensePowerDelta);
    }

    [Fact]
    public void HumanInAnyTerrain_NoModifiers()
    {
        var terrains = new[] { TerrainType.Plains, TerrainType.Desert, TerrainType.Mountain, TerrainType.Forest, TerrainType.Swamp };
        foreach (var t in terrains)
        {
            var ctx = MakeCtx("Human", t);
            _sut.Apply(ctx);
            Assert.Equal(0, ctx.AttackPowerDelta);
            Assert.Equal(0, ctx.DefensePowerDelta);
        }
    }

    [Fact]
    public void PlainsTerrain_NoModifierForAnyRace()
    {
        var races = new[] { "Elf", "Dwarf", "Lizard", "Orc", "Ogre", "Kobold", "Gladefolk", "Human", "Undead", "Demon" };
        foreach (var r in races)
        {
            var ctx = MakeCtx(r, TerrainType.Plains);
            _sut.Apply(ctx);
            Assert.Equal(0, ctx.AttackPowerDelta);
            Assert.Equal(0, ctx.DefensePowerDelta);
        }
    }

    [Fact]
    public void DifferentRaces_AffectedIndependently()
    {
        var source = new Weapon { Name = "Test", AttackType = AttackType.Melee, DamageDie = DieType.D6 };
        var elf   = new Character { Name = "Elf",  Strength = 10, Race = new Race { Name = "Elf" } };
        var dwarf = new Character { Name = "Dwarf", Strength = 10, Race = new Race { Name = "Dwarf" } };
        var ctx = new CombatModifierContext
        {
            Attacker         = elf,
            Defender         = dwarf,
            Source           = source,
            Range            = EngagementRange.Melee,
            Terrain          = TerrainType.Mountain,
            BaseAttackPower  = 10,
            BaseDefensePower = 8
        };
        _sut.Apply(ctx);
        Assert.Equal(0, ctx.AttackPowerDelta);   // Elf has no mountain bonus
        Assert.Equal(2, ctx.DefensePowerDelta);  // Dwarf gains +2 DP in mountain
    }

    [Fact]
    public void Priority_IsEnvironmentalBand()
    {
        Assert.Equal(20, _sut.Priority);
    }
}
