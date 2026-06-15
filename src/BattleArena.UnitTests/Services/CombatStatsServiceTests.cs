namespace BattleArena.UnitTests.Services;

using Application.Services;
using Core.Entities;
using Core.Entities.Enums;

public class CombatStatsServiceTests
{
    private readonly CombatStatsService _sut = new();

    [Fact]
    public void ComputeAttackerStats_IncludesWeaponFeatRaceAndStatusBonuses()
    {
        var attacker = new Character
        {
            Level = 3,
            Strength = 16,
            Dexterity = 12,
            StrikeRating = 17,
            Feats = new List<Feat> { new() { AttackBonus = 2 } },
            Race = new Race
            {
                Feats = new List<Feat> { new() { AttackBonus = 1 } }
            },
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { Name = "Battle Cry", AttackPowerModifier = 3, StackRule = StackRule.Stack },
                new() { Name = "Bless", AttackPowerModifier = 4, StackRule = StackRule.HighestWins },
                new() { Name = "Focus", AttackPowerModifier = 2, StackRule = StackRule.HighestWins },
                new() { Name = "Curse", AttackPowerModifier = -2, StackRule = StackRule.NoStack }
            }
        };
        var weapon = new Weapon
        {
            AttackType = AttackType.Melee,
            AttackBonus = 2
        };

        var result = _sut.ComputeAttackerStats(attacker, weapon);

        Assert.Equal(17, result.ClassAccuracyBase);
        Assert.Equal(1, result.LevelScaling);  // Level 3 / 2 = 1
        Assert.Equal(3, result.AttributeModifier);
        Assert.Equal(2, result.WeaponAttackBonus);
        Assert.Equal(2, result.SkillModifiers);
        Assert.Equal(5, result.BuffModifiers);
        Assert.Equal(1, result.RacialModifiers);
        Assert.Equal(31, result.AttackPower);
    }

    [Fact]
    public void ComputeAttackerStats_BarbarianWithTwoHandedSword_IncludesTwoHandedBonus()
    {
        var attacker = new Character
        {
            ClassId = 1,
            Level = 5,
            Strength = 18,
            StrikeRating = 14,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon
                {
                    Archetype = ArchetypeWeapon.TwoHandedSword,
                    Hands = 2,
                    AttackType = AttackType.Melee,
                    AttackBonus = 3
                }
            }
        };
        var weapon = attacker.Equipment.RightHand!;

        var result = _sut.ComputeAttackerStats(attacker, weapon);

        // WeaponAttackBonus = AttackBonus(3) + classBonus(0) + twoHandedBonus(2) + shieldBonus(0) + elvenRangerBonus(0)
        Assert.Equal(5, result.WeaponAttackBonus);
    }

    [Fact]
    public void ComputeAttackerStats_KnightWithShield_IncludesShieldBonus()
    {
        var attacker = new Character
        {
            ClassId = 2,
            Level = 5,
            Strength = 16,
            StrikeRating = 14,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon
                {
                    Archetype = ArchetypeWeapon.Sword,
                    Hands = 1,
                    AttackType = AttackType.Melee,
                    AttackBonus = 2
                },
                Shield = new Shield { DefenseBonus = 2 }
            }
        };
        var weapon = attacker.Equipment.RightHand!;

        var result = _sut.ComputeAttackerStats(attacker, weapon);

        // WeaponAttackBonus = AttackBonus(2) + classBonus(0) + twoHandedBonus(0) + shieldBonus(2) + elvenRangerBonus(0)
        Assert.Equal(4, result.WeaponAttackBonus);
    }

    [Fact]
    public void ComputeAttackerStats_RangerWithBow_IncludesRangedBonus()
    {
        var attacker = new Character
        {
            ClassId = 10,
            Level = 5,
            Dexterity = 18,
            StrikeRating = 14,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon
                {
                    Archetype = ArchetypeWeapon.Bow,
                    Hands = 2,
                    AttackType = AttackType.Ranged,
                    AttackBonus = 2
                }
            }
        };
        var weapon = attacker.Equipment.RightHand!;

        var result = _sut.ComputeAttackerStats(attacker, weapon);

        // WeaponAttackBonus = AttackBonus(2) + classBonus(+1 Ranger) + twoHandedBonus(0) + shieldBonus(0) + elvenRangerBonus(0)
        Assert.Equal(3, result.WeaponAttackBonus);
    }

    [Fact]
    public void ComputeAttackerStats_ElfRangerWithBow_IncludesElvenDexBonus()
    {
        var attacker = new Character
        {
            ClassId = 10,
            Level = 5,
            Dexterity = 18,
            StrikeRating = 14,
            Race = new Race { Name = "Elf" },
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon
                {
                    Archetype = ArchetypeWeapon.Bow,
                    Hands = 2,
                    AttackType = AttackType.Ranged,
                    AttackBonus = 2
                }
            }
        };
        var weapon = attacker.Equipment.RightHand!;

        var result = _sut.ComputeAttackerStats(attacker, weapon);

        // WeaponAttackBonus = AttackBonus(2) + classBonus(+1 Ranger) + twoHandedBonus(0) + shieldBonus(0) + elvenRangerDexBonus(4)
        Assert.Equal(7, result.WeaponAttackBonus);
    }

    [Fact]
    public void ComputeDefenderStats_AppliesArmorDexCapShieldBuffsAndDefenseBonuses()
    {
        var defender = new Character
        {
            Dexterity = 18,
            Feats = new List<Feat> { new() { DefenseBonus = 1 } },
            Race = new Race
            {
                Feats = new List<Feat> { new() { DefenseBonus = 2 } }
            },
            Equipment = new ArmorSlots
            {
                Chest = new Armor { ArmorClass = 12, MaxDexterityBonus = 2 },
                Head = new Armor { ArmorClass = 4, MaxDexterityBonus = 1 },
                Shield = new Shield { DefenseBonus = 3 }
            },
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { Name = "Ward I", Type = StatusEffectType.Buff, DefensePowerModifier = 4, StackRule = StackRule.HighestWins, Source = "spell-a" },
                new() { Name = "Ward II", Type = StatusEffectType.Buff, DefensePowerModifier = 2, StackRule = StackRule.HighestWins, Source = "spell-a" },
                new() { Name = "Barrier", Type = StatusEffectType.Buff, DefensePowerModifier = 1, StackRule = StackRule.HighestWins, Source = "spell-b" },
                new() { Name = "Expose", Type = StatusEffectType.Debuff, DefensePowerModifier = -2, StackRule = StackRule.Stack, Source = "enemy" }
            }
        };

        var result = _sut.ComputeDefenderStats(defender);

        Assert.Equal(16, result.EffectiveAC);
        Assert.Equal(3, result.DexterityModifier);
        Assert.Equal(3, result.ShieldBonus);
        Assert.Equal(3, result.DefensiveBuffs);
        Assert.Equal(3, result.DefenseRacialModifiers);
        Assert.Equal(28, result.DefensePower);
    }

    // ── Regression: LevelDefenseBonus is part of DefensePower ──────────────

    [Fact]
    public void ComputeDefenderStats_LevelDefenseBonusIncludedInDefensePower()
    {
        var defender = new Character
        {
            Level = 10,
            Dexterity = 10,
            Equipment = new ArmorSlots { Chest = new Armor { ArmorClass = 10 } },
        };

        var result = _sut.ComputeDefenderStats(defender);

        Assert.Equal(10, result.EffectiveAC);
        Assert.Equal(0, result.DexterityModifier);
        Assert.Equal(5, result.LevelDefenseBonus);   // Level 10 / 2 = 5
        Assert.Equal(15, result.DefensePower);        // AC 10 + DEX 0 + Level 5 = 15
    }

    [Fact]
    public void ComputeDefenderStats_HigherLevelGivesMoreDefense()
    {
        var low = MakeDefender(5);
        var high = MakeDefender(15);

        var lowDp = _sut.ComputeDefenderStats(low).DefensePower;
        var highDp = _sut.ComputeDefenderStats(high).DefensePower;

        Assert.True(highDp > lowDp, $"Higher level should give more defense ({highDp} <= {lowDp})");
    }

    private static Character MakeDefender(int level) => new()
    {
        Level = level,
        Dexterity = 10,
        Equipment = new ArmorSlots { Chest = new Armor { ArmorClass = 10 } },
    };

    // ── Regression: Priest spells use Wisdom, Mage spells use Intelligence ──

    [Fact]
    public void PriestSpell_UsesWisdomNotIntelligence()
    {
        var priest = new Character { Intelligence = 8, Wisdom = 20 };
        var spell = new Spell { School = SpellSchool.Deity, DamageType = DamageType.Holy }; // Deity = Priest school

        var stats = _sut.ComputeAttackerStats(priest, spell);

        Assert.False(spell.UsesIntelligence, "Deity spells should not use Intelligence");
        Assert.Equal(5, stats.AttributeModifier); // WIS 20 → (20-10)/2 = 5
    }

    [Fact]
    public void MageSpell_UsesIntelligence()
    {
        var mage = new Character { Intelligence = 20, Wisdom = 8 };
        var spell = new Spell { School = SpellSchool.Stormcraft, DamageType = DamageType.Fire };

        var stats = _sut.ComputeAttackerStats(mage, spell);

        Assert.True(spell.UsesIntelligence, "Stormcraft spells should use Intelligence");
        Assert.Equal(5, stats.AttributeModifier); // INT 20 → (20-10)/2 = 5
    }

    // ── MAGIC RESISTANCE DEMONSTRATION ─────────────────────────

    [Fact]
    public void MagicResistance_ElfGetsPlus5BonusVsSpells()
    {
        var elf = new Character
        {
            Level = 10, Dexterity = 10, Wisdom = 10,
            Race = new Race
            {
                Name = "Elf",
                Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Magic, 25)] }]
            },
            Equipment = new ArmorSlots { Chest = new Armor { ArmorClass = 10 } },
        };
        var human = new Character
        {
            Level = 10, Dexterity = 10, Wisdom = 10,
            Equipment = new ArmorSlots { Chest = new Armor { ArmorClass = 10 } },
        };

        var spell = new Weapon { Name = "Fireball", AttackType = AttackType.Spell }; // triggers spell path

        var elfDef = _sut.ComputeDefenderStats(elf, spell);
        var humanDef = _sut.ComputeDefenderStats(human, spell);

        // Both start with same base: Wisdom(10) mod=0 + Level 10/2=5 = 5 DP
        // Elf adds MagicResistance: 25/5 = +5 → total DP = 10
        // Human adds 0 → total DP = 5
        Assert.Equal(5, elfDef.LevelDefenseBonus);
        Assert.Equal(5, elfDef.MagicResistanceBonus);   // 25/5 = +5
        Assert.Equal(10, elfDef.DefensePower);           // 0 + 0 + 5 + 5 = 10
        Assert.Equal(5, humanDef.DefensePower);          // 0 + 0 + 5 + 0 = 5
    }

    [Fact]
    public void MagicResistance_PhysicalAttacksUnaffected()
    {
        var elf = new Character
        {
            Level = 10, Dexterity = 10,
            Race = new Race
            {
                Name = "Elf",
                Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Magic, 25)] }]
            },
            Equipment = new ArmorSlots { Chest = new Armor { ArmorClass = 14 } },
        };

        var melee = new Weapon { Name = "Sword", AttackType = AttackType.Melee };

        var def = _sut.ComputeDefenderStats(elf, melee);

        // Magic resistance does NOT apply vs physical attacks
        Assert.Equal(0, def.MagicResistanceBonus);
        Assert.Equal(14, def.EffectiveAC);
    }

    [Fact]
    public void MagicResistance_EquipmentStackingGivesHigherBonus()
    {
        var elf = new Character
        {
            Level = 10, Wisdom = 10,
            Race = new Race
            {
                Name = "Elf",
                Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Magic, 25)] }]
            },
            Equipment = new ArmorSlots
            {
                Chest = new Armor
                {
                    Name = "Mithril Chain",
                    ArmorClass = 14, Mitigation = 2,
                    Resistances = [new ResistanceBonus(ResistanceType.Magic, 5)]
                },
            },
        };

        var spell = new Weapon { Name = "Fireball", AttackType = AttackType.Spell };
        var def = _sut.ComputeDefenderStats(elf, spell);

        // Total magic resistance: 25 (racial) + 5 (armor) = 30
        // MagicResistanceBonus: 30 / 5 = 6
        Assert.Equal(6, def.MagicResistanceBonus);
        Assert.Equal(11, def.DefensePower); // 0 + 0 + 5 + 6 = 11
    }
}
