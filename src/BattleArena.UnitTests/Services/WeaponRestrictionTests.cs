namespace BattleArena.UnitTests.Services;

using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Tests for AD&amp;D 2e weapon proficiency rules encoded in <see cref="ArchetypeWeaponExtensions"/>.
///
/// Class IDs:
///   1=Barbarian  2=Knight  3=Paladin  4=Priest  5=Mage  6=Bard  7=Druid  8=Fighter  9=Rogue  10=Ranger
/// </summary>
public class WeaponRestrictionTests
{
    private static Character Of(int classId) => new() { ClassId = classId };

    // ── Universal weapons ───────────────────────────────────────────────────

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    public void Dagger_AllClasses_CanEquip(int classId)
    {
        Assert.True(Of(classId).CanEquip(ArchetypeWeapon.Dagger));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    public void Staff_AllClasses_CanEquip(int classId)
    {
        Assert.True(Of(classId).CanEquip(ArchetypeWeapon.Staff));
    }

    // ── Mage restrictions ───────────────────────────────────────────────────

    [Fact]
    public void Wand_OnlyMage_CanEquip()
    {
        Assert.True(Of(5).CanEquip(ArchetypeWeapon.Wand));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    public void Wand_NonMageClasses_CannotEquip(int classId)
    {
        Assert.False(Of(classId).CanEquip(ArchetypeWeapon.Wand));
    }

    [Theory]
    [InlineData(ArchetypeWeapon.Sword)]
    [InlineData(ArchetypeWeapon.ShortSword)]
    [InlineData(ArchetypeWeapon.Axe)]
    [InlineData(ArchetypeWeapon.Mace)]
    [InlineData(ArchetypeWeapon.Bow)]
    public void Mage_MartialWeapons_CannotEquip(ArchetypeWeapon archetype)
    {
        Assert.False(Of(5).CanEquip(archetype));
    }

    // ── Warrior weapons (Barbarian=1, Knight=2, Paladin=3, Fighter=8) ───────

    [Theory]
    [InlineData(ArchetypeWeapon.Sword)]
    [InlineData(ArchetypeWeapon.ShortSword)]
    [InlineData(ArchetypeWeapon.Axe)]
    [InlineData(ArchetypeWeapon.Mace)]
    [InlineData(ArchetypeWeapon.Hammer)]
    [InlineData(ArchetypeWeapon.Lance)]
    [InlineData(ArchetypeWeapon.Spear)]
    [InlineData(ArchetypeWeapon.Bow)]
    [InlineData(ArchetypeWeapon.Crossbow)]
    public void Fighter_AllMartialWeapons_CanEquip(ArchetypeWeapon archetype)
    {
        Assert.True(Of(8).CanEquip(archetype));
    }

    // ── Priest restrictions ─────────────────────────────────────────────────

    [Theory]
    [InlineData(ArchetypeWeapon.Mace)]
    [InlineData(ArchetypeWeapon.Hammer)]
    [InlineData(ArchetypeWeapon.MorningStar)]
    [InlineData(ArchetypeWeapon.Sling)]
    public void Priest_DivineBludgeoningWeapons_CanEquip(ArchetypeWeapon archetype)
    {
        Assert.True(Of(4).CanEquip(archetype));
    }

    [Theory]
    [InlineData(ArchetypeWeapon.Sword)]
    [InlineData(ArchetypeWeapon.Axe)]
    [InlineData(ArchetypeWeapon.Bow)]
    [InlineData(ArchetypeWeapon.Lance)]
    public void Priest_BladeAndWarWeapons_CannotEquip(ArchetypeWeapon archetype)
    {
        Assert.False(Of(4).CanEquip(archetype));
    }

    // ── Druid restrictions ──────────────────────────────────────────────────

    [Theory]
    [InlineData(ArchetypeWeapon.Sword)] // scimitar treated as Sword
    [InlineData(ArchetypeWeapon.Spear)]
    [InlineData(ArchetypeWeapon.Mace)]
    [InlineData(ArchetypeWeapon.Sling)]
    public void Druid_NaturalWeapons_CanEquip(ArchetypeWeapon archetype)
    {
        Assert.True(Of(7).CanEquip(archetype));
    }

    [Theory]
    [InlineData(ArchetypeWeapon.Axe)]
    [InlineData(ArchetypeWeapon.Lance)]
    [InlineData(ArchetypeWeapon.MorningStar)]
    public void Druid_HeavyWarWeapons_CannotEquip(ArchetypeWeapon archetype)
    {
        Assert.False(Of(7).CanEquip(archetype));
    }

    // ── Rogue restrictions ──────────────────────────────────────────────────

    [Theory]
    [InlineData(ArchetypeWeapon.ShortSword)]
    [InlineData(ArchetypeWeapon.Sword)]
    [InlineData(ArchetypeWeapon.Bow)]
    [InlineData(ArchetypeWeapon.Crossbow)]
    public void Rogue_FinesseAndRanged_CanEquip(ArchetypeWeapon archetype)
    {
        Assert.True(Of(9).CanEquip(archetype));
    }

    [Theory]
    [InlineData(ArchetypeWeapon.Axe)]
    [InlineData(ArchetypeWeapon.Mace)]
    [InlineData(ArchetypeWeapon.Lance)]
    public void Rogue_HeavyWeapons_CannotEquip(ArchetypeWeapon archetype)
    {
        Assert.False(Of(9).CanEquip(archetype));
    }

    // ── Two-handed swords (warrior classes only: 1,2,3,8) ────────────────────

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(8)]
    public void TwoHandedSword_WarriorClasses_CanEquip(int classId)
    {
        Assert.True(Of(classId).CanEquip(ArchetypeWeapon.TwoHandedSword));
    }

    [Theory]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(9)] [InlineData(10)]
    public void TwoHandedSword_NonWarriorClasses_CannotEquip(int classId)
    {
        Assert.False(Of(classId).CanEquip(ArchetypeWeapon.TwoHandedSword));
    }

    // ── Two-handed battle-axes (warrior classes only) ────────────────────────

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(8)]
    public void TwoHandedBattleAxe_WarriorClasses_CanEquip(int classId)
    {
        Assert.True(Of(classId).CanEquip(ArchetypeWeapon.TwoHandedBattleAxe));
    }

    [Theory]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(9)] [InlineData(10)]
    public void TwoHandedBattleAxe_NonWarriorClasses_CannotEquip(int classId)
    {
        Assert.False(Of(classId).CanEquip(ArchetypeWeapon.TwoHandedBattleAxe));
    }

    // ── Two-handed warhammers (warriors + Priest) ────────────────────────────

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(8)]
    public void TwoHandedWarhammer_WarriorsAndPriest_CanEquip(int classId)
    {
        Assert.True(Of(classId).CanEquip(ArchetypeWeapon.TwoHandedWarhammer));
    }

    [Theory]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(9)] [InlineData(10)]
    public void TwoHandedWarhammer_NonAllowedClasses_CannotEquip(int classId)
    {
        Assert.False(Of(classId).CanEquip(ArchetypeWeapon.TwoHandedWarhammer));
    }

    // ── Ranger restrictions (class 10) ───────────────────────────────────────

    [Theory]
    [InlineData(ArchetypeWeapon.Bow)]
    [InlineData(ArchetypeWeapon.Crossbow)]
    [InlineData(ArchetypeWeapon.ShortSword)]
    [InlineData(ArchetypeWeapon.Dagger)]
    [InlineData(ArchetypeWeapon.Spear)]
    public void Ranger_RangerWeapons_CanEquip(ArchetypeWeapon archetype)
    {
        Assert.True(Of(10).CanEquip(archetype));
    }

    [Theory]
    [InlineData(ArchetypeWeapon.TwoHandedSword)]
    [InlineData(ArchetypeWeapon.TwoHandedBattleAxe)]
    [InlineData(ArchetypeWeapon.TwoHandedWarhammer)]
    [InlineData(ArchetypeWeapon.Mace)]
    [InlineData(ArchetypeWeapon.MorningStar)]
    [InlineData(ArchetypeWeapon.Lance)]
    public void Ranger_HeavyWeapons_CannotEquip(ArchetypeWeapon archetype)
    {
        Assert.False(Of(10).CanEquip(archetype));
    }

    // ── Unknown class ───────────────────────────────────────────────────────

    [Fact]
    public void UnknownClassId_AnyWeapon_ReturnsFalse()
    {
        Assert.False(Of(99).CanEquip(ArchetypeWeapon.Sword));
    }
}
