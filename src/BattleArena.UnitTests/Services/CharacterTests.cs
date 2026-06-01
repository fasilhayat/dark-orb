namespace BattleArena.UnitTests.Services;

using Core.Entities;
using Core.Entities.Enums;

public class CharacterTests
{
    // ── ManaRegenPerTick ────────────────────────────────────────────────────

    [Fact]
    public void ManaRegenPerTick_BaselineIntelligence_ReturnsZero()
    {
        // INT 10 → mod 0; level factor removed; total = Max(0, 0) = 0
        var character = new Character { Intelligence = 10, Level = 1 };

        Assert.Equal(0, character.ManaRegenPerTick);
    }

    [Fact]
    public void ManaRegenPerTick_HighIntelligence_IncludesIntModOnly()
    {
        // INT 18 → mod (18-10)/2 = 4; level factor removed; total = 4
        var character = new Character { Intelligence = 18, Level = 4 };

        Assert.Equal(4, character.ManaRegenPerTick);
    }

    [Fact]
    public void ManaRegenPerTick_WithStatusEffectBonus_IncludesBonus()
    {
        // INT 10 → mod 0; effect bonus 3 → total = Max(0, 3) = 3
        var character = new Character { Intelligence = 10, Level = 1 };
        character.ActiveStatusEffects.Add(new StatusEffect { ManaRegenModifier = 3 });

        Assert.Equal(3, character.ManaRegenPerTick);
    }

    // ── EffectiveMaxMana ────────────────────────────────────────────────────

    [Fact]
    public void EffectiveMaxMana_NoEquipmentBonus_EqualToMaxMana()
    {
        var character = new Character { MaxMana = 50 };

        Assert.Equal(50, character.EffectiveMaxMana);
    }

    [Fact]
    public void EffectiveMaxMana_WithEquipmentBonus_SumsCorrectly()
    {
        var character = new Character
        {
            MaxMana = 50,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { MaxManaBonus = 20 }
            }
        };

        Assert.Equal(70, character.EffectiveMaxMana);
    }

    // ── SpellMemorizationSlots ──────────────────────────────────────────────

    [Fact]
    public void SpellMemorizationSlots_AverageIntelligence_ReturnsTwo()
    {
        // INT 10 → mod 0; 2 + 0 = 2
        var character = new Character { Intelligence = 10 };

        Assert.Equal(2, character.SpellMemorizationSlots);
    }

    [Fact]
    public void SpellMemorizationSlots_HighIntelligence_ScalesUp()
    {
        // INT 16 → mod 3; 2 + 3 = 5
        var character = new Character { Intelligence = 16 };

        Assert.Equal(5, character.SpellMemorizationSlots);
    }

    [Fact]
    public void SpellMemorizationSlots_MinimumIsOne_EvenWithLowIntelligence()
    {
        // INT 4 → mod -3; 2 + (-3) = -1 → clamped to 1
        var character = new Character { Intelligence = 4 };

        Assert.Equal(1, character.SpellMemorizationSlots);
    }

    // ── ComputeSpellTurnMeterCost ───────────────────────────────────────────

    [Fact]
    public void ComputeSpellTurnMeterCost_AverageStats_ReturnsSpellBaseCost()
    {
        // INT 10 → intMod 0; Level 1 → level factor 1; reduction = 0*3 + 1*1 = 1; 50 - 1 = 49
        var character = new Character { Intelligence = 10, Level = 1 };
        var spell = new Spell { TurnMeterCost = 50 };

        Assert.Equal(49, character.ComputeSpellTurnMeterCost(spell));
    }

    [Fact]
    public void ComputeSpellTurnMeterCost_HighIntelligenceAndLevel_ReducesCost()
    {
        // INT 18 → intMod 4; Level 9 → level factor 9; reduction = 4*3 + 9 = 21; 60 - 21 = 39
        var character = new Character { Intelligence = 18, Level = 9 };
        var spell = new Spell { TurnMeterCost = 60 };

        Assert.Equal(39, character.ComputeSpellTurnMeterCost(spell));
    }

    [Fact]
    public void ComputeSpellTurnMeterCost_LargeReduction_ClampsToMinimumTen()
    {
        // INT 20 → intMod 5; Level 10 → level factor 10; reduction = 15 + 10 = 25; 30 - 25 = 5 → clamped to 10
        var character = new Character { Intelligence = 20, Level = 10 };
        var spell = new Spell { TurnMeterCost = 30 };

        Assert.Equal(10, character.ComputeSpellTurnMeterCost(spell));
    }

    // ── CanEquip ────────────────────────────────────────────────────────────

    [Fact]
    public void CanEquip_FighterWithSword_ReturnsTrue()
    {
        var fighter = new Character { ClassId = 1 };
        var sword = new Weapon { Archetype = ArchetypeWeapon.Sword };

        Assert.True(fighter.CanEquip(sword));
    }

    [Fact]
    public void CanEquip_MageWithSword_ReturnsFalse()
    {
        // Class 4 (Mage) cannot use swords
        var mage = new Character { ClassId = 4 };
        var sword = new Weapon { Archetype = ArchetypeWeapon.Sword };

        Assert.False(mage.CanEquip(sword));
    }

    [Fact]
    public void CanEquip_AnyClassWithDagger_ReturnsTrue()
    {
        // Daggers are the universal side-arm — every class can use one
        var mage = new Character { ClassId = 4 };
        var dagger = new Weapon { Archetype = ArchetypeWeapon.Dagger };

        Assert.True(mage.CanEquip(dagger));
    }

    [Fact]
    public void CanEquip_ArchetypeOverload_MatchesWeaponOverload()
    {
        var fighter = new Character { ClassId = 1 };

        Assert.Equal(fighter.CanEquip(ArchetypeWeapon.Axe), fighter.CanEquip(new Weapon { Archetype = ArchetypeWeapon.Axe }));
    }
}
