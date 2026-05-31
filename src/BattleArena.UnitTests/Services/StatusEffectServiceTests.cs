namespace BattleArena.UnitTests.Services;

using Application.Services;
using Core.Entities;
using Core.Entities.Enums;

public class StatusEffectServiceTests
{
    private readonly StatusEffectService _sut = new();

    [Fact]
    public void Apply_NoStack_DoesNotDuplicateExistingEffect()
    {
        var target = new Character();
        var effect = new StatusEffect
        {
            Name = "Bless",
            StackRule = StackRule.NoStack,
            Source = "spell-a"
        };

        _sut.Apply(target, effect);
        _sut.Apply(target, new StatusEffect
        {
            Name = "Bless",
            StackRule = StackRule.NoStack,
            Source = "spell-a"
        });

        Assert.Single(target.ActiveStatusEffects);
    }

    [Fact]
    public void Apply_HighestWins_ReplacesWeakerEffect()
    {
        var target = new Character();

        _sut.Apply(target, new StatusEffect
        {
            Name = "Shield",
            StackRule = StackRule.HighestWins,
            Magnitude = 2,
            Source = "spell-a"
        });
        _sut.Apply(target, new StatusEffect
        {
            Name = "Shield",
            StackRule = StackRule.HighestWins,
            Magnitude = 5,
            Source = "spell-b"
        });

        Assert.Single(target.ActiveStatusEffects);
        Assert.Equal(5, target.ActiveStatusEffects[0].Magnitude);
    }

    [Fact]
    public void Apply_Stack_AddsOnlyDifferentSources()
    {
        var target = new Character();

        _sut.Apply(target, new StatusEffect
        {
            Name = "Bleed",
            StackRule = StackRule.Stack,
            Source = "enemy-a"
        });
        _sut.Apply(target, new StatusEffect
        {
            Name = "Bleed",
            StackRule = StackRule.Stack,
            Source = "enemy-a"
        });
        _sut.Apply(target, new StatusEffect
        {
            Name = "Bleed",
            StackRule = StackRule.Stack,
            Source = "enemy-b"
        });

        Assert.Equal(2, target.ActiveStatusEffects.Count);
    }

    [Fact]
    public void TickAll_DecrementsDurationAndRemovesExpiredEffects()
    {
        var target = new Character
        {
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { Name = "Haste", Duration = 2 },
                new() { Name = "Burn", Duration = 1 }
            }
        };

        _sut.TickAll(target);

        Assert.Single(target.ActiveStatusEffects);
        Assert.Equal("Haste", target.ActiveStatusEffects[0].Name);
        Assert.Equal(1, target.ActiveStatusEffects[0].Duration);
    }

    [Fact]
    public void SumModifierMethods_ReturnCombinedValues()
    {
        var target = new Character
        {
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { AttackPowerModifier = 2, DefensePowerModifier = 1, TurnMeterModifier = 3 },
                new() { AttackPowerModifier = -1, DefensePowerModifier = -2, TurnMeterModifier = 4 }
            }
        };

        Assert.Equal(1, _sut.SumAttackModifiers(target));
        Assert.Equal(-1, _sut.SumDefenseModifiers(target));
        Assert.Equal(7, _sut.SumTurnMeterModifiers(target));
    }

    [Fact]
    public void TickDoT_SumsDamageFromAllDoTEffects()
    {
        var target = new Character { CurrentHitPoints = 100 };
        _sut.Apply(target, new StatusEffect
        {
            Name = "Burning", Type = StatusEffectType.DamageOverTime,
            DamagePerTurn = 5, Duration = 3, StackRule = StackRule.Stack, Source = "fire"
        });
        _sut.Apply(target, new StatusEffect
        {
            Name = "Poison", Type = StatusEffectType.DamageOverTime,
            DamagePerTurn = 3, Duration = 3, StackRule = StackRule.Stack, Source = "poison"
        });

        var total = _sut.TickDoT(target);

        Assert.Equal(8, total);
        Assert.Equal(92, target.CurrentHitPoints);
    }

    [Fact]
    public void TickDoT_NoDoTEffects_ReturnsZero()
    {
        var target = new Character { CurrentHitPoints = 50 };
        _sut.Apply(target, new StatusEffect
        {
            Name = "Bless", Type = StatusEffectType.Buff,
            Duration = 3, StackRule = StackRule.NoStack, Source = "priest"
        });

        var total = _sut.TickDoT(target);

        Assert.Equal(0, total);
        Assert.Equal(50, target.CurrentHitPoints);
    }

    [Fact]
    public void HasEffectType_ReturnsTrueWhenMatchingEffectPresent()
    {
        var target = new Character();
        _sut.Apply(target, new StatusEffect
        {
            Name = "Paralysis", Type = StatusEffectType.Stun,
            Duration = 2, StackRule = StackRule.NoStack, Source = "spell"
        });

        Assert.True(_sut.HasEffectType(target, StatusEffectType.Stun));
        Assert.False(_sut.HasEffectType(target, StatusEffectType.DamageOverTime));
    }

    [Fact]
    public void Remove_RemovesEffectsByName()
    {
        var target = new Character();
        _sut.Apply(target, new StatusEffect
        {
            Name = "Bless", Duration = 5, StackRule = StackRule.NoStack, Source = "priest"
        });
        _sut.Apply(target, new StatusEffect
        {
            Name = "Shield", Duration = 5, StackRule = StackRule.NoStack, Source = "mage"
        });

        _sut.Remove(target, "Bless");

        Assert.Single(target.ActiveStatusEffects);
        Assert.Equal("Shield", target.ActiveStatusEffects[0].Name);
    }

    [Fact]
    public void GetActive_ReturnsReadOnlyViewOfEffects()
    {
        var target = new Character();
        _sut.Apply(target, new StatusEffect
        {
            Name = "Haste", Duration = 3, StackRule = StackRule.NoStack, Source = "spell"
        });

        var active = _sut.GetActive(target);

        Assert.Single(active);
        Assert.Equal("Haste", active[0].Name);
    }
}
