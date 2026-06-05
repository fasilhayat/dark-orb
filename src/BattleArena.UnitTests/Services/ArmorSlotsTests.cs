namespace BattleArena.UnitTests.Services;

using Core.Entities;
using Core.Entities.Enums;

public class ArmorSlotsTests
{
    private static Armor Piece(
        int ac = 0,
        int mitigation = 0,
        int tmPenalty = 0,
        int tmCostReduction = 0,
        int manaRegen = 0,
        int maxMana = 0,
        int spellSlots = 0) => new()
    {
        ArmorClass          = ac,
        Mitigation          = mitigation,
        TurnMeterPenalty    = tmPenalty,
        TurnMeterCostReduction = tmCostReduction,
        ManaRegenBonus      = manaRegen,
        MaxManaBonus        = maxMana,
        SpellSlotsBonus     = spellSlots,
    };

    // ── TotalArmorClass ─────────────────────────────────────────────────────

    [Fact]
    public void TotalArmorClass_MultipleSlots_SumsAllPieces()
    {
        var slots = new ArmorSlots
        {
            Chest = Piece(ac: 16),
            Head  = Piece(ac: 2),
            Boots = Piece(ac: 1),
        };

        Assert.Equal(19, slots.TotalArmorClass);
    }

    [Fact]
    public void TotalArmorClass_EmptySlots_ReturnsZero()
    {
        Assert.Equal(0, new ArmorSlots().TotalArmorClass);
    }

    // ── TotalMitigation ─────────────────────────────────────────────────────

    [Fact]
    public void TotalMitigation_MultipleSlots_SumsCorrectly()
    {
        var slots = new ArmorSlots
        {
            Chest = Piece(mitigation: 3),
            Hands = Piece(mitigation: 1),
        };

        Assert.Equal(4, slots.TotalMitigation);
    }

    // ── TotalTurnMeterPenalty ───────────────────────────────────────────────

    [Fact]
    public void TotalTurnMeterPenalty_MultipleHeavyPieces_SumsCorrectly()
    {
        var slots = new ArmorSlots
        {
            Chest = Piece(tmPenalty: 5),
            Head  = Piece(tmPenalty: 2),
            Boots = Piece(tmPenalty: 1),
        };

        Assert.Equal(8, slots.TotalTurnMeterPenalty);
    }

    // ── TotalTurnMeterCostReduction ─────────────────────────────────────────

    [Fact]
    public void TotalTurnMeterCostReduction_MagicRobesAndNeck_SumsBothSlots()
    {
        var slots = new ArmorSlots
        {
            Chest = Piece(tmCostReduction: 10),
            Neck  = Piece(tmCostReduction: 5),
        };

        Assert.Equal(15, slots.TotalTurnMeterCostReduction);
    }

    [Fact]
    public void TotalTurnMeterCostReduction_NoMagicPieces_ReturnsZero()
    {
        var slots = new ArmorSlots { Chest = Piece(ac: 16) };

        Assert.Equal(0, slots.TotalTurnMeterCostReduction);
    }

    // ── TotalManaRegenBonus ─────────────────────────────────────────────────

    [Fact]
    public void TotalManaRegenBonus_MultipleSlots_SumsCorrectly()
    {
        var slots = new ArmorSlots
        {
            Chest = Piece(manaRegen: 3),
            Back  = Piece(manaRegen: 2),
        };

        Assert.Equal(5, slots.TotalManaRegenBonus);
    }

    // ── TotalMaxManaBonus ───────────────────────────────────────────────────

    [Fact]
    public void TotalMaxManaBonus_ArcaneRobesAndAmulet_SumsBothSlots()
    {
        var slots = new ArmorSlots
        {
            Chest = Piece(maxMana: 20),
            Neck  = Piece(maxMana: 10),
        };

        Assert.Equal(30, slots.TotalMaxManaBonus);
    }

    // ── TotalSpellSlotsBonus ────────────────────────────────────────────────

    [Fact]
    public void TotalSpellSlotsBonus_HeadAndNeckSlots_SumsCorrectly()
    {
        // SpellSlotsBonus only counted from: Head, Neck, Back, Hands
        var slots = new ArmorSlots
        {
            Head  = Piece(spellSlots: 2),
            Neck  = Piece(spellSlots: 1),
            Chest = Piece(spellSlots: 99), // chest NOT included for spell slots
        };

        Assert.Equal(3, slots.TotalSpellSlotsBonus);
    }

    [Fact]
    public void TotalSpellSlotsBonus_ChestSlotExcluded_DoesNotContribute()
    {
        var slots = new ArmorSlots { Chest = Piece(spellSlots: 10) };

        Assert.Equal(0, slots.TotalSpellSlotsBonus);
    }

    // ── TotalResistance ─────────────────────────────────────────────────────

    [Fact]
    public void TotalResistance_MultipleSlots_SumsMatchingType()
    {
        var fireResist = new Armor
        {
            ArmorClass  = 14,
            Resistances = [new ResistanceBonus(ResistanceType.Fire, 25)]
        };
        var moreFireResist = new Armor
        {
            Resistances = [new ResistanceBonus(ResistanceType.Fire, 10)]
        };
        var slots = new ArmorSlots { Chest = fireResist, Neck = moreFireResist };

        Assert.Equal(35, slots.TotalResistance(ResistanceType.Fire));
    }

    [Fact]
    public void TotalResistance_WrongType_ReturnsZero()
    {
        var slots = new ArmorSlots
        {
            Chest = new Armor
            {
                Resistances = [new ResistanceBonus(ResistanceType.Fire, 25)]
            }
        };

        Assert.Equal(0, slots.TotalResistance(ResistanceType.Magic));
    }

    // ── TotalStrengthBonus ──────────────────────────────────────────────────

    [Fact]
    public void TotalStrengthBonus_EmptySlots_ReturnsZero()
    {
        Assert.Equal(0, new ArmorSlots().TotalStrengthBonus);
    }

    [Fact]
    public void TotalStrengthBonus_ArmorSlot_ContributesBonus()
    {
        var slots = new ArmorSlots
        {
            Chest = new Armor { StrengthBonus = 2 }
        };

        Assert.Equal(2, slots.TotalStrengthBonus);
    }

    [Fact]
    public void TotalStrengthBonus_MultipleSlots_SumsCorrectly()
    {
        var slots = new ArmorSlots
        {
            Chest = new Armor { StrengthBonus = 2 },
            Waist = new Armor { StrengthBonus = 4 },
            Neck = new Armor { StrengthBonus = 1 }
        };

        Assert.Equal(7, slots.TotalStrengthBonus);
    }

    [Fact]
    public void TotalStrengthBonus_AccessorySlots_ContributeBonus()
    {
        var slots = new ArmorSlots
        {
            LeftRing = new Armor { StrengthBonus = 2 },
            RightRing = new Armor { StrengthBonus = 3 },
            Ornament = new Armor { StrengthBonus = 1 }
        };

        Assert.Equal(6, slots.TotalStrengthBonus);
    }

    [Fact]
    public void TotalStrengthBonus_GearAndAccessorySlots_AllSummed()
    {
        var slots = new ArmorSlots
        {
            Chest = new Armor { StrengthBonus = 2 },
            Waist = new Armor { StrengthBonus = 4 },
            LeftRing = new Armor { StrengthBonus = 1 },
            RightRing = new Armor { StrengthBonus = 1 }
        };

        Assert.Equal(8, slots.TotalStrengthBonus);
    }
}
