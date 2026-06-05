namespace BattleArena.UnitTests.Services;

using Core.Entities.Enums;
using Core.Models;

/// <summary>
/// Tests for <see cref="ClassCombatData"/> static lookup tables.
/// Must stay in sync with PlayerClass DB seed data.
/// Class IDs: 1=Barbarian, 2=Knight, 3=Paladin, 4=Priest, 5=Mage,
///            6=Bard, 7=Druid, 8=Fighter, 9=Rogue, 10=Ranger
/// </summary>
public class ClassCombatDataTests
{
    // ── AttacksPerTurn ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 3)]  // Barbarian
    [InlineData(2, 2)]  // Knight
    [InlineData(3, 2)]  // Paladin
    [InlineData(4, 1)]  // Priest
    [InlineData(5, 1)]  // Mage
    [InlineData(6, 1)]  // Bard
    [InlineData(7, 1)]  // Druid
    [InlineData(8, 2)]  // Fighter
    [InlineData(9, 1)]  // Rogue
    [InlineData(10, 2)] // Ranger
    public void AttacksPerTurn_AllClasses_ReturnsExpected(int classId, int expected)
    {
        Assert.Equal(expected, ClassCombatData.AttacksPerTurn(classId));
    }

    // ── BowAttacksPerTurn ────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(10, 3)] // Ranger: 3 attacks with bow
    public void BowAttacksPerTurn_ReturnsExpected(int classId, int expected)
    {
        Assert.Equal(expected, ClassCombatData.BowAttacksPerTurn(classId));
    }

    // ── ArmorRestriction ─────────────────────────────────────────────────────

    [Fact]
    public void ArmorRestriction_Barbarian_ReturnsLight()
    {
        Assert.Equal("Light", ClassCombatData.ArmorRestriction(1));
    }

    [Theory]
    [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)]
    [InlineData(8)] [InlineData(9)] [InlineData(10)]
    public void ArmorRestriction_NonBarbarianClasses_ReturnsNull(int classId)
    {
        Assert.Null(ClassCombatData.ArmorRestriction(classId));
    }

    // ── CanDualWield ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(8)]  // Fighter
    [InlineData(9)]  // Rogue
    [InlineData(10)] // Ranger
    public void CanDualWield_DualWieldClasses_ReturnsTrue(int classId)
    {
        Assert.True(ClassCombatData.CanDualWield(classId));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void CanDualWield_SingleWieldClasses_ReturnsFalse(int classId)
    {
        Assert.False(ClassCombatData.CanDualWield(classId));
    }

    // ── WeaponSwitchCostMultiplier ───────────────────────────────────────────

    [Theory]
    [InlineData(1, 0.0)]  // Barbarian: free switch (melee only)
    [InlineData(2, 0.5)]  // Knight: half cost
    [InlineData(3, 0.5)]  // Paladin: half cost
    [InlineData(4, 1.0)]  // Priest: full cost
    [InlineData(5, 1.0)]  // Mage: full cost
    [InlineData(8, 0.5)]  // Fighter: half cost
    [InlineData(9, 1.0)]  // Rogue: full cost
    [InlineData(10, 0.0)] // Ranger: free switch
    public void WeaponSwitchCostMultiplier_ReturnsExpected(int classId, double expected)
    {
        Assert.Equal(expected, ClassCombatData.WeaponSwitchCostMultiplier(classId));
    }

    // ── TwoHandedWeaponBonus ─────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 2)]  // Barbarian: +2
    [InlineData(3, 2)]  // Paladin: +2
    [InlineData(2, 0)]  // Knight: none
    [InlineData(8, 0)]  // Fighter: none
    [InlineData(10, 0)] // Ranger: none
    public void TwoHandedWeaponBonus_ReturnsExpected(int classId, int expected)
    {
        Assert.Equal(expected, ClassCombatData.TwoHandedWeaponBonus(classId));
    }

    // ── ShieldBonusDamage ────────────────────────────────────────────────────

    [Theory]
    [InlineData(2, 2)]  // Knight: +2
    [InlineData(1, 0)]  // Barbarian: none
    [InlineData(3, 0)]  // Paladin: none
    [InlineData(8, 0)]  // Fighter: none
    [InlineData(10, 0)] // Ranger: none
    public void ShieldBonusDamage_ReturnsExpected(int classId, int expected)
    {
        Assert.Equal(expected, ClassCombatData.ShieldBonusDamage(classId));
    }

    // ── RangedAttackBonus ────────────────────────────────────────────────────

    [Theory]
    [InlineData(10, 1)]  // Ranger: +1
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(8, 0)]
    public void RangedAttackBonus_ReturnsExpected(int classId, int expected)
    {
        Assert.Equal(expected, ClassCombatData.RangedAttackBonus(classId));
    }

    // ── IsTwoHandedArchetype ─────────────────────────────────────────────────

    [Theory]
    [InlineData(ArchetypeWeapon.TwoHandedSword, true)]
    [InlineData(ArchetypeWeapon.TwoHandedBattleAxe, true)]
    [InlineData(ArchetypeWeapon.TwoHandedWarhammer, true)]
    [InlineData(ArchetypeWeapon.Sword, false)]
    [InlineData(ArchetypeWeapon.Axe, false)]
    [InlineData(ArchetypeWeapon.Bow, false)]
    public void IsTwoHandedArchetype_CorrectlyClassifies(ArchetypeWeapon archetype, bool expected)
    {
        Assert.Equal(expected, ClassCombatData.IsTwoHandedArchetype(archetype));
    }

    [Fact]
    public void IsTwoHandedArchetype_Bow_ReturnsFalse()
    {
        Assert.False(ClassCombatData.IsTwoHandedArchetype(ArchetypeWeapon.Bow));
    }

    // ── IsBowArchetype ───────────────────────────────────────────────────────

    [Fact]
    public void IsBowArchetype_Bow_ReturnsTrue()
    {
        Assert.True(ClassCombatData.IsBowArchetype(ArchetypeWeapon.Bow));
    }

    [Fact]
    public void IsBowArchetype_Sword_ReturnsFalse()
    {
        Assert.False(ClassCombatData.IsBowArchetype(ArchetypeWeapon.Sword));
    }

    // ── Unknown class safety ─────────────────────────────────────────────────

    [Fact]
    public void AttacksPerTurn_UnknownClass_ReturnsOne()
    {
        Assert.Equal(1, ClassCombatData.AttacksPerTurn(999));
    }

    [Fact]
    public void TwoHandedWeaponBonus_UnknownClass_ReturnsZero()
    {
        Assert.Equal(0, ClassCombatData.TwoHandedWeaponBonus(999));
    }
}
