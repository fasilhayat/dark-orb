namespace BattleArena.UnitTests.Services;

using Application.Modifiers;
using Core.Entities;
using Core.Entities.Enums;
using Core.Models;

public class DamageModifierTests
{
    private readonly DamageModifier _sut = new();

    private static CombatModifierContext MakeCtx(List<StatusEffect> defenderEffects, ElementalType elemType = ElementalType.None)
    {
        var source = new Weapon
        {
            Name = "Test", AttackType = AttackType.Melee, DamageDie = DieType.D6,
            ElementalType = elemType, ElementalDamage = elemType != ElementalType.None ? 5 : 0
        };
        var defender = new Character { Name = "Defender", Strength = 10 };
        defender.ActiveStatusEffects.AddRange(defenderEffects);
        return new CombatModifierContext
        {
            Attacker         = new Character { Name = "Attacker", Strength = 10 },
            Defender         = defender,
            Source           = source,
            Range            = EngagementRange.Melee,
            BaseAttackPower  = 10,
            BaseDefensePower = 8
        };
    }

    [Fact]
    public void NoBuffs_NoModifiers()
    {
        var ctx = MakeCtx(new List<StatusEffect>());
        _sut.Apply(ctx);
        Assert.Equal(0, ctx.DamageDelta);
        Assert.Equal(1.0, ctx.DamageMultiplier);
    }

    [Fact]
    public void ProtectiveBuff_ReducesDamage()
    {
        var effects = new List<StatusEffect>
        {
            new()
            {
                Name = "Arcane Ward",
                Type = StatusEffectType.Buff,
                DefensePowerModifier = 6,
                Duration = 3,
                StackRule = StackRule.NoStack,
                Source = "Arcane Ward"
            }
        };
        var ctx = MakeCtx(effects);
        _sut.Apply(ctx);

        // DefensePowerModifier 6 / 2 = 3 damage reduction
        Assert.Equal(-3, ctx.DamageDelta);
        // 5% damage reduction multiplier
        Assert.Equal(0.95, ctx.DamageMultiplier);
    }

    [Fact]
    public void MultipleProtectiveBuffs_StackDamageReduction()
    {
        var effects = new List<StatusEffect>
        {
            new()
            {
                Name = "Arcane Ward", Type = StatusEffectType.Buff,
                DefensePowerModifier = 6, Duration = 3, StackRule = StackRule.NoStack, Source = "Arcane Ward"
            },
            new()
            {
                Name = "Stone Skin", Type = StatusEffectType.Buff,
                DefensePowerModifier = 4, Duration = 3, StackRule = StackRule.NoStack, Source = "Stone Skin"
            }
        };
        var ctx = MakeCtx(effects);
        _sut.Apply(ctx);

        // (6 + 4) / 2 = 5 damage reduction
        Assert.Equal(-5, ctx.DamageDelta);
        // 0.95 * 0.95 = 0.9025
        Assert.Equal(0.9025, ctx.DamageMultiplier, 4);
    }

    [Fact]
    public void Debuffs_DoNotTriggerDamageReduction()
    {
        var effects = new List<StatusEffect>
        {
            new()
            {
                Name = "Weakened", Type = StatusEffectType.Debuff,
                DefensePowerModifier = -4, Duration = 3, StackRule = StackRule.NoStack, Source = "Curse"
            }
        };
        var ctx = MakeCtx(effects);
        _sut.Apply(ctx);

        // Debuffs should not trigger protective damage reduction
        Assert.Equal(0, ctx.DamageDelta);
        Assert.Equal(1.0, ctx.DamageMultiplier);
    }

    [Fact]
    public void ResistanceBonus_MatchingElementalType_ReducesDamage()
    {
        var effects = new List<StatusEffect>
        {
            new()
            {
                Name = "Fire Ward", Type = StatusEffectType.Buff,
                ResistanceBonuses = new() { new(ResistanceType.Fire, 20) },
                Duration = 3, StackRule = StackRule.NoStack, Source = "Fire Ward"
            }
        };
        var ctx = MakeCtx(effects, ElementalType.Fire);
        _sut.Apply(ctx);

        // Resistance 20 / 2 = 10 damage reduction from elemental
        Assert.Equal(-10, ctx.DamageDelta);
    }

    [Fact]
    public void ResistanceBonus_WrongElementalType_NoReduction()
    {
        var effects = new List<StatusEffect>
        {
            new()
            {
                Name = "Fire Ward", Type = StatusEffectType.Buff,
                ResistanceBonuses = new() { new(ResistanceType.Fire, 20) },
                Duration = 3, StackRule = StackRule.NoStack, Source = "Fire Ward"
            }
        };
        var ctx = MakeCtx(effects, ElementalType.Ice);
        _sut.Apply(ctx);

        // Fire resistance doesn't protect against ice
        Assert.Equal(0, ctx.DamageDelta);
    }

    [Fact]
    public void Priority_IsItemSetBand()
    {
        Assert.Equal(30, _sut.Priority);
    }
}
