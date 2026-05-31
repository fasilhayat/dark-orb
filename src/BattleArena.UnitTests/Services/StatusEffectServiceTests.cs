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
}
