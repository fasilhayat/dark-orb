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
    // All values are percentages of a full turn (100 = 100% = one full turn).

    [Fact]
    public void ComputeSpellTurnMeterCost_AverageStats_ReturnsSpellBaseCost()
    {
        // INT 10 → intMod 0; Level 1 → 1% reduction; 50% - 1% = 49%
        var character = new Character { Intelligence = 10, Level = 1 };
        var spell = new Spell { TurnMeterCost = 50 };

        Assert.Equal(49, character.ComputeSpellTurnMeterCost(spell));
    }

    [Fact]
    public void ComputeSpellTurnMeterCost_HighIntelligenceAndLevel_ReducesCost()
    {
        // INT 18 → intMod 4 → 12% reduction; Level 9 → 9% reduction; 60% - 21% = 39%
        var character = new Character { Intelligence = 18, Level = 9 };
        var spell = new Spell { TurnMeterCost = 60 };

        Assert.Equal(39, character.ComputeSpellTurnMeterCost(spell));
    }

    [Fact]
    public void ComputeSpellTurnMeterCost_LargeReduction_ClampsToMinimumTenPercent()
    {
        // INT 20 → intMod 5 → 15% reduction; Level 10 → 10% reduction; 30% - 25% = 5% → clamped to 10% minimum
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

    // ── HasSufficientStrength ──────────────────────────────────────────────────

    [Fact]
    public void HasSufficientStrength_TwoHandedSwordWithStr16_ReturnsTrue()
    {
        var fighter = new Character { ClassId = 8, Strength = 16 };
        var weapon = new Weapon { Archetype = ArchetypeWeapon.TwoHandedSword, Hands = 2 };

        Assert.True(fighter.HasSufficientStrength(weapon));
    }

    [Fact]
    public void HasSufficientStrength_TwoHandedSwordWithStr15_ReturnsFalse()
    {
        var fighter = new Character { ClassId = 8, Strength = 15 };
        var weapon = new Weapon { Archetype = ArchetypeWeapon.TwoHandedSword, Hands = 2 };

        Assert.False(fighter.HasSufficientStrength(weapon));
    }

    [Fact]
    public void HasSufficientStrength_TwoHandedWeaponWithGearBonus_ReturnsTrue()
    {
        // STR 14 base + Girdle of Giant Strength (+18) = Effective 32 >= 16
        var fighter = new Character
        {
            ClassId = 8,
            Strength = 14,
            Equipment = new ArmorSlots
            {
                Waist = new Armor { StrengthBonus = 18 }
            }
        };
        var weapon = new Weapon { Archetype = ArchetypeWeapon.TwoHandedSword, Hands = 2 };

        Assert.True(fighter.HasSufficientStrength(weapon));
    }

    [Fact]
    public void HasSufficientStrength_OneHandedWeaponWithStr8_ReturnsTrue()
    {
        // One-handed weapons have no STR requirement
        var fighter = new Character { ClassId = 8, Strength = 8 };
        var weapon = new Weapon { Archetype = ArchetypeWeapon.Sword, Hands = 1 };

        Assert.True(fighter.HasSufficientStrength(weapon));
    }

    // ── CanEquip with STR check ─────────────────────────────────────────────────

    [Fact]
    public void CanEquip_FighterWithTwoHandedSwordAndStr16_ReturnsTrue()
    {
        var fighter = new Character { ClassId = 8, Strength = 16 };
        var weapon = new Weapon { Archetype = ArchetypeWeapon.TwoHandedSword, Hands = 2 };

        Assert.True(fighter.CanEquip(weapon));
    }

    [Fact]
    public void CanEquip_FighterWithTwoHandedSwordAndStr15_ReturnsFalse()
    {
        // Fighter can equip archetype, but STR 15 < 16 requirement
        var fighter = new Character { ClassId = 8, Strength = 15 };
        var weapon = new Weapon { Archetype = ArchetypeWeapon.TwoHandedSword, Hands = 2 };

        Assert.False(fighter.CanEquip(weapon));
    }

    [Fact]
    public void CanEquip_RogueWithDaggerAndStr10_ReturnsTrue()
    {
        // Dagger has no STR requirement
        var rogue = new Character { ClassId = 9, Strength = 10 };
        var dagger = new Weapon { Archetype = ArchetypeWeapon.Dagger, Hands = 1 };

        Assert.True(rogue.CanEquip(dagger));
    }

    // ── CanDualWield ───────────────────────────────────────────────────────────

    [Fact]
    public void CanDualWield_FighterStr15_TwoShortSwords_ReturnsTrue()
    {
        var fighter = new Character
        {
            ClassId = 8,
            Strength = 15,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 },
                LeftHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 }
            }
        };

        Assert.True(fighter.CanDualWield);
    }

    [Fact]
    public void CanDualWield_FighterStr14_TwoShortSwords_ReturnsFalse()
    {
        var fighter = new Character
        {
            ClassId = 8,
            Strength = 14,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 },
                LeftHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 }
            }
        };

        Assert.False(fighter.CanDualWield);
    }

    [Fact]
    public void CanDualWield_FighterStr13WithGearBonus_ReturnsTrue()
    {
        // STR 13 base + 2 from belt = Effective 15 >= 15
        var fighter = new Character
        {
            ClassId = 8,
            Strength = 13,
            Equipment = new ArmorSlots
            {
                Waist = new Armor { StrengthBonus = 2 },
                RightHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 },
                LeftHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 }
            }
        };

        Assert.True(fighter.CanDualWield);
    }

    [Fact]
    public void CanDualWield_RogueStr15_DaggerAndShortSword_ReturnsTrue()
    {
        var rogue = new Character
        {
            ClassId = 9,
            Strength = 15,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 },
                LeftHand = new Weapon { Archetype = ArchetypeWeapon.Dagger, Hands = 1 }
            }
        };

        Assert.True(rogue.CanDualWield);
    }

    [Fact]
    public void CanDualWield_RogueStr15_TwoLongSwords_ReturnsFalse()
    {
        // Rogues can only dual-wield shortsword+dagger or 2 daggers
        var rogue = new Character
        {
            ClassId = 9,
            Strength = 15,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Sword, Hands = 1 },
                LeftHand = new Weapon { Archetype = ArchetypeWeapon.Sword, Hands = 1 }
            }
        };

        Assert.False(rogue.CanDualWield);
    }

    [Fact]
    public void CanDualWield_RangerStr15_TwoShortSwords_ReturnsTrue()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Strength = 15,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 },
                LeftHand = new Weapon { Archetype = ArchetypeWeapon.ShortSword, Hands = 1 }
            }
        };

        Assert.True(ranger.CanDualWield);
    }

    [Fact]
    public void CanDualWield_KnightWithShield_ReturnsFalse()
    {
        // Knights cannot dual-wield
        var knight = new Character
        {
            ClassId = 2,
            Strength = 16,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Sword, Hands = 1 },
                LeftHand = new Weapon { Archetype = ArchetypeWeapon.Sword, Hands = 1 }
            }
        };

        Assert.False(knight.CanDualWield);
    }

    // ── AttacksPerTurn ─────────────────────────────────────────────────────────

    [Fact]
    public void AttacksPerTurn_Barbarian_ReturnsThree()
    {
        var barbarian = new Character { ClassId = 1 };

        Assert.Equal(3, barbarian.AttacksPerTurn);
    }

    [Fact]
    public void AttacksPerTurn_RangerWithBow_ReturnsThree()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Bow, AttackType = AttackType.Ranged }
            }
        };

        Assert.Equal(3, ranger.AttacksPerTurn);
    }

    [Fact]
    public void AttacksPerTurn_RangerWithoutBow_ReturnsTwo()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Sword, AttackType = AttackType.Melee }
            }
        };

        Assert.Equal(2, ranger.AttacksPerTurn);
    }

    // ── WeaponSwitchTurnMeterCost ──────────────────────────────────────────────

    [Fact]
    public void WeaponSwitchTurnMeterCost_Barbarian_ReturnsZero()
    {
        var barbarian = new Character { ClassId = 1 };

        Assert.Equal(0, barbarian.WeaponSwitchTurnMeterCost);
    }

    [Fact]
    public void WeaponSwitchTurnMeterCost_Fighter_ReturnsFifty()
    {
        var fighter = new Character { ClassId = 8 };

        Assert.Equal(50, fighter.WeaponSwitchTurnMeterCost);
    }

    [Fact]
    public void WeaponSwitchTurnMeterCost_Mage_ReturnsOneHundred()
    {
        var mage = new Character { ClassId = 5 };

        Assert.Equal(100, mage.WeaponSwitchTurnMeterCost);
    }

    // ── TwoHandedWeaponBonus ────────────────────────────────────────────────────

    [Fact]
    public void TwoHandedWeaponBonus_BarbarianWithTwoHandedSword_ReturnsBonus()
    {
        var barbarian = new Character
        {
            ClassId = 1,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.TwoHandedSword, Hands = 2 }
            }
        };

        Assert.Equal(2, barbarian.TwoHandedWeaponBonus);
    }

    [Fact]
    public void TwoHandedWeaponBonus_BarbarianWithOneHandedWeapon_ReturnsZero()
    {
        var barbarian = new Character
        {
            ClassId = 1,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Sword, Hands = 1 }
            }
        };

        Assert.Equal(0, barbarian.TwoHandedWeaponBonus);
    }

    [Fact]
    public void TwoHandedWeaponBonus_KnightWithTwoHandedSword_ReturnsZero()
    {
        var knight = new Character
        {
            ClassId = 2,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.TwoHandedSword, Hands = 2 }
            }
        };

        Assert.Equal(0, knight.TwoHandedWeaponBonus);
    }

    // ── ShieldBonusDamage ───────────────────────────────────────────────────────

    [Fact]
    public void ShieldBonusDamage_KnightWithShield_ReturnsBonus()
    {
        var knight = new Character
        {
            ClassId = 2,
            Equipment = new ArmorSlots
            {
                Shield = new Shield { DefenseBonus = 2 }
            }
        };

        Assert.Equal(2, knight.ShieldBonusDamage);
    }

    [Fact]
    public void ShieldBonusDamage_KnightWithoutShield_ReturnsZero()
    {
        var knight = new Character { ClassId = 2 };

        Assert.Equal(0, knight.ShieldBonusDamage);
    }

    [Fact]
    public void ShieldBonusDamage_BarbarianWithShield_ReturnsZero()
    {
        var barbarian = new Character
        {
            ClassId = 1,
            Equipment = new ArmorSlots
            {
                Shield = new Shield { DefenseBonus = 2 }
            }
        };

        Assert.Equal(0, barbarian.ShieldBonusDamage);
    }

    // ── RangedAttackBonus ──────────────────────────────────────────────────────

    [Fact]
    public void RangedAttackBonus_RangerWithBow_ReturnsBonus()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Bow, AttackType = AttackType.Ranged }
            }
        };

        Assert.Equal(1, ranger.RangedAttackBonus);
    }

    [Fact]
    public void RangedAttackBonus_RangerWithSword_ReturnsZero()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Sword, AttackType = AttackType.Melee }
            }
        };

        Assert.Equal(0, ranger.RangedAttackBonus);
    }

    [Fact]
    public void RangedAttackBonus_FighterWithBow_ReturnsZero()
    {
        var fighter = new Character
        {
            ClassId = 8,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Bow, AttackType = AttackType.Ranged }
            }
        };

        Assert.Equal(0, fighter.RangedAttackBonus);
    }

    // ── ElvenRangerDexBonus ────────────────────────────────────────────────────

    [Fact]
    public void ElvenRangerDexBonus_ElfRangerWithBow_ReturnsDexMod()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Dexterity = 18,
            Race = new Race { Name = "Elf" },
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Bow, AttackType = AttackType.Ranged }
            }
        };

        Assert.Equal(4, ranger.ElvenRangerDexBonus);
    }

    [Fact]
    public void ElvenRangerDexBonus_HumanRangerWithBow_ReturnsZero()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Dexterity = 18,
            Race = new Race { Name = "Human" },
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Bow, AttackType = AttackType.Ranged }
            }
        };

        Assert.Equal(0, ranger.ElvenRangerDexBonus);
    }

    [Fact]
    public void ElvenRangerDexBonus_ElfRangerWithSword_ReturnsZero()
    {
        var ranger = new Character
        {
            ClassId = 10,
            Dexterity = 18,
            Race = new Race { Name = "Elf" },
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Archetype = ArchetypeWeapon.Sword, AttackType = AttackType.Melee }
            }
        };

        Assert.Equal(0, ranger.ElvenRangerDexBonus);
    }

    // ── HasArmorViolation ──────────────────────────────────────────────────────

    [Fact]
    public void HasArmorViolation_BarbarianWithLeatherArmor_ReturnsFalse()
    {
        var barbarian = new Character
        {
            ClassId = 1,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Category = "Light", ArmorClass = 2 }
            }
        };

        Assert.False(barbarian.HasArmorViolation);
    }

    [Fact]
    public void HasArmorViolation_BarbarianWithChainMail_ReturnsTrue()
    {
        var barbarian = new Character
        {
            ClassId = 1,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Category = "Medium", ArmorClass = 5 }
            }
        };

        Assert.True(barbarian.HasArmorViolation);
    }

    [Fact]
    public void HasArmorViolation_KnightWithPlateMail_ReturnsFalse()
    {
        var knight = new Character
        {
            ClassId = 2,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Category = "Heavy", ArmorClass = 8 }
            }
        };

        Assert.False(knight.HasArmorViolation);
    }

    // ── EffectiveStrength ──────────────────────────────────────────────────────

    [Fact]
    public void EffectiveStrength_NoEquipment_EqualsBaseStrength()
    {
        var character = new Character { Strength = 14 };

        Assert.Equal(14, character.EffectiveStrength);
    }

    [Fact]
    public void EffectiveStrength_WithGearBonuses_SumsCorrectly()
    {
        var character = new Character
        {
            Strength = 14,
            Equipment = new ArmorSlots
            {
                Waist = new Armor { StrengthBonus = 4 },
                LeftRing = new Armor { StrengthBonus = 2 }
            }
        };

        Assert.Equal(20, character.EffectiveStrength);
    }
}
