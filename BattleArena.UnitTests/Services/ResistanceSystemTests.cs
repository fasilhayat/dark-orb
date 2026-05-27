namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using NSubstitute;

public class ResistanceSystemTests
{
    // ── ComputeResistance ─────────────────────────────────────────────────────

    [Fact]
    public void ComputeResistance_NoSources_ReturnsZero()
    {
        var character = new Character();
        Assert.Equal(0, character.ComputeResistance(ResistanceType.Magic));
    }

    [Fact]
    public void ComputeResistance_RaceFeat_ReturnsCorrectValue()
    {
        var character = new Character
        {
            Race = new Race
            {
                Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Magic, 25)] }]
            }
        };
        Assert.Equal(25, character.ComputeResistance(ResistanceType.Magic));
    }

    [Fact]
    public void ComputeResistance_SingleArmorPiece_ReturnsCorrectValue()
    {
        var character = new Character
        {
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Resistances = [new ResistanceBonus(ResistanceType.Fire, 30)] }
            }
        };
        Assert.Equal(30, character.ComputeResistance(ResistanceType.Fire));
    }

    [Fact]
    public void ComputeResistance_MultipleArmorSlots_SumsAllSlots()
    {
        var character = new Character
        {
            Equipment = new ArmorSlots
            {
                Chest = new Armor { Resistances = [new ResistanceBonus(ResistanceType.Fire, 15)] },
                Boots = new Armor { Resistances = [new ResistanceBonus(ResistanceType.Fire, 10)] }
            }
        };
        Assert.Equal(25, character.ComputeResistance(ResistanceType.Fire));
    }

    [Fact]
    public void ComputeResistance_ActiveBuff_ReturnsCorrectValue()
    {
        var character = new Character
        {
            ActiveStatusEffects =
            [
                new StatusEffect { ResistanceBonuses = [new ResistanceBonus(ResistanceType.Magic, 20)] }
            ]
        };
        Assert.Equal(20, character.ComputeResistance(ResistanceType.Magic));
    }

    [Fact]
    public void ComputeResistance_WrongResistanceType_ReturnsZero()
    {
        var character = new Character
        {
            Race = new Race
            {
                Feats = [new Feat { Resistances = [new ResistanceBonus(ResistanceType.Fire, 30)] }]
            }
        };
        // Character has Fire resistance, but we're checking Cold — should be 0
        Assert.Equal(0, character.ComputeResistance(ResistanceType.Cold));
    }

    [Fact]
    public void ComputeResistance_SumsAllSources_AndCapsAt95()
    {
        var character = new Character
        {
            Race = new Race
            {
                Feats =
                [
                    new Feat
                    {
                        Resistances = [new ResistanceBonus(ResistanceType.Magic, 25)]
                    }
                ]
            },
            Equipment = new ArmorSlots
            {
                Chest = new Armor
                {
                    Resistances = [new ResistanceBonus(ResistanceType.Magic, 20)]
                }
            },
            ActiveStatusEffects =
            [
                new StatusEffect
                {
                    ResistanceBonuses = [new ResistanceBonus(ResistanceType.Magic, 60)]
                }
            ]
        };

        var result = character.ComputeResistance(ResistanceType.Magic);

        Assert.Equal(95, result);
    }

    [Fact]
    public void TryApply_WhenResistanceRollSucceeds_ReturnsResistedAndDoesNotApply()
    {
        var sut = new StatusEffectService();
        var dice = Substitute.For<IDiceService>();
        var target = new Character();
        var effect = new StatusEffect { Name = "Rooted", ApplicationChance = 100, StackRule = StackRule.NoStack };

        dice.Roll(DieType.D100).Returns(40, 20);

        var result = sut.TryApply(target, effect, 25, dice);

        Assert.False(result.Applied);
        Assert.True(result.WasResisted);
        Assert.Equal(20, result.Roll);
        Assert.Equal(25, result.TotalResistance);
        Assert.Empty(target.ActiveStatusEffects);
    }

    [Fact]
    public void TryApply_ApplicationChanceFails_NotAppliedAndNotResisted()
    {
        var sut = new StatusEffectService();
        var dice = Substitute.For<IDiceService>();
        var target = new Character();
        var effect = new StatusEffect { Name = "Burn", ApplicationChance = 50, StackRule = StackRule.NoStack };

        dice.Roll(DieType.D100).Returns(51); // 51 > 50 → ApplicationChance check fails

        var result = sut.TryApply(target, effect, 0, dice);

        Assert.False(result.Applied);
        Assert.False(result.WasResisted);
        Assert.Empty(target.ActiveStatusEffects);
        dice.Received(1).Roll(DieType.D100); // Exactly one roll — no resist roll made
    }

    [Fact]
    public void TryApply_ResistRollAboveThreshold_EffectApplied()
    {
        var sut = new StatusEffectService();
        var dice = Substitute.For<IDiceService>();
        var target = new Character();
        var effect = new StatusEffect { Name = "Shocked", ApplicationChance = 100, StackRule = StackRule.NoStack };

        // Roll 1: 100 ≤ ApplicationChance(100) → passes chance check
        // Roll 2: 50 > resistance(25) → not resisted → applied
        dice.Roll(DieType.D100).Returns(100, 50);

        var result = sut.TryApply(target, effect, 25, dice);

        Assert.True(result.Applied);
        Assert.False(result.WasResisted);
        Assert.Single(target.ActiveStatusEffects);
    }

    [Fact]
    public void TryApply_ZeroResistance_SkipsResistRollAndAppliesEffect()
    {
        var sut = new StatusEffectService();
        var dice = Substitute.For<IDiceService>();
        var target = new Character();
        var effect = new StatusEffect { Name = "Poison", ApplicationChance = 100, StackRule = StackRule.NoStack };

        dice.Roll(DieType.D100).Returns(50); // Chance check passes; no resist roll (resistance=0)

        var result = sut.TryApply(target, effect, 0, dice);

        Assert.True(result.Applied);
        Assert.False(result.WasResisted);
        Assert.Single(target.ActiveStatusEffects);
        dice.Received(1).Roll(DieType.D100); // Only one roll total
    }

    [Fact]
    public void TryApply_MaxResistance95_StillAppliesWhenRollExceeds95()
    {
        var sut = new StatusEffectService();
        var dice = Substitute.For<IDiceService>();
        var target = new Character();
        var effect = new StatusEffect { Name = "Frozen", ApplicationChance = 100, StackRule = StackRule.NoStack };

        // Roll 1: ApplicationChance passes (100 ≤ 100)
        // Roll 2: 96 > 95 resistance → not resisted → applied
        dice.Roll(DieType.D100).Returns(100, 96);

        var result = sut.TryApply(target, effect, 95, dice);

        Assert.True(result.Applied);
        Assert.False(result.WasResisted);
        Assert.Single(target.ActiveStatusEffects);
    }

    [Fact]
    public void TryApply_AppliedEffect_IsAddedToTargetWithCorrectProperties()
    {
        var sut = new StatusEffectService();
        var dice = Substitute.For<IDiceService>();
        var target = new Character();
        var effect = new StatusEffect
        {
            Name = "Blinded",
            Type = StatusEffectType.Debuff,
            Duration = 3,
            ApplicationChance = 100,
            StackRule = StackRule.NoStack
        };

        dice.Roll(DieType.D100).Returns(100); // Chance passes, resistance=0 no second roll

        sut.TryApply(target, effect, 0, dice);

        Assert.Single(target.ActiveStatusEffects);
        var applied = target.ActiveStatusEffects[0];
        Assert.Equal("Blinded", applied.Name);
        Assert.Equal(3, applied.Duration);
        Assert.Equal(StatusEffectType.Debuff, applied.Type);
    }

    [Fact]
    public void TryApply_RootWithDuration2_ExpiresAfterTwoTicksAndCharacterActsAgain()
    {
        var dice = new DiceService();
        var simulator = new CombatSimulator(
            new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice);

        var hero = new Character
        {
            Name = "Rooted Hero",
            Strength = 14, Dexterity = 10, StrikeRating = 10, TurnSpeed = 100,
            MaxHitPoints = 100, CurrentHitPoints = 100,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Name = "Sword", DamageDie = DieType.D4, DamageCount = 1, DamageType = DamageType.Slashing, AttackType = AttackType.Melee }
            },
            ActiveStatusEffects =
            [
                new StatusEffect { Name = "Rooted", Type = StatusEffectType.Root, Duration = 2, StackRule = StackRule.NoStack }
            ]
        };

        var enemy = new Character
        {
            Name = "Enemy",
            Strength = 10, Dexterity = 10, StrikeRating = 10, TurnSpeed = 100,
            MaxHitPoints = 100, CurrentHitPoints = 100,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Name = "Club", DamageDie = DieType.D4, DamageCount = 1, DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee }
            }
        };

        var result = simulator.Simulate(
            Party.Solo(hero, hero.Equipment.RightHand),
            Party.Solo(enemy, enemy.Equipment.RightHand),
            maxTicks: 5);

        var skippedTurns = result.Log.Where(e => e.EventType == "SkippedTurn" && e.ActorName == "Rooted Hero").ToList();
        Assert.Equal(2, skippedTurns.Count);
        Assert.Contains(result.Log, e => e.EventType == "EffectExpired" && e.ActorName == "Rooted Hero" && e.StatusEffectName == "Rooted");
        Assert.Contains(result.Log, e => e.EventType == "TurnStart" && e.ActorName == "Rooted Hero");
    }

    [Fact]
    public void CombatSimulator_SkippedCrowdControlledTurn_TicksAndExpiresEffects()
    {
        var dice = new DiceService();
        var simulator = new CombatSimulator(
            new CombatService(dice, new CombatStatsService()),
            new TurnmeterService(),
            new StatusEffectService(),
            dice);

        var hero = new Character
        {
            Name = "Hero",
            Strength = 14,
            Dexterity = 10,
            StrikeRating = 10,
            TurnSpeed = 100,
            MaxHitPoints = 100,
            CurrentHitPoints = 100,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Name = "Sword", DamageDie = DieType.D4, DamageCount = 1, DamageType = DamageType.Slashing, AttackType = AttackType.Melee }
            },
            ActiveStatusEffects =
            [
                new StatusEffect { Name = "Rooted", Type = StatusEffectType.Root, Duration = 1, StackRule = StackRule.NoStack }
            ]
        };

        var enemy = new Character
        {
            Name = "Enemy",
            Strength = 10,
            Dexterity = 10,
            StrikeRating = 10,
            TurnSpeed = 100,
            MaxHitPoints = 100,
            CurrentHitPoints = 100,
            Equipment = new ArmorSlots
            {
                RightHand = new Weapon { Name = "Club", DamageDie = DieType.D4, DamageCount = 1, DamageType = DamageType.Bludgeoning, AttackType = AttackType.Melee }
            }
        };

        var result = simulator.Simulate(
            Party.Solo(hero, hero.Equipment.RightHand),
            Party.Solo(enemy, enemy.Equipment.RightHand),
            maxTicks: 2);

        Assert.Contains(result.Log, e => e.EventType == "SkippedTurn" && e.ActorName == "Hero" && e.Tick == 1);
        Assert.Contains(result.Log, e => e.EventType == "EffectExpired" && e.ActorName == "Hero" && e.StatusEffectName == "Rooted" && e.Tick == 1);
        Assert.Contains(result.Log, e => e.EventType == "TurnStart" && e.ActorName == "Hero" && e.Tick == 2);
        Assert.DoesNotContain(hero.ActiveStatusEffects, e => e.Name == "Rooted");
    }
}
