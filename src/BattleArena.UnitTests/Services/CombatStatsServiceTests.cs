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
}
