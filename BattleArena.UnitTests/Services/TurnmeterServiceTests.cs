namespace BattleArena.UnitTests.Services;

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;

public class TurnmeterServiceTests
{
    private readonly TurnmeterService _sut = new();

    [Fact]
    public void ComputeGainPerTick_IncludesDexBuffsAndArmorPenalty()
    {
        var character = new Character
        {
            Dexterity = 14,
            TurnSpeed = 10,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { TurnMeterPenalty = 2 },
                Head = new Armor { TurnMeterPenalty = 1 }
            },
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { Name = "Haste", Type = StatusEffectType.Buff, TurnMeterModifier = 4 }
            }
        };

        var result = _sut.ComputeGainPerTick(character);

        Assert.Equal(13, result);
    }

    [Fact]
    public void ComputeGainPerTick_HasMinimumOfOne()
    {
        var character = new Character
        {
            Dexterity = 6,
            TurnSpeed = 1,
            Equipment = new ArmorSlots
            {
                Chest = new Armor { TurnMeterPenalty = 10 }
            }
        };

        var result = _sut.ComputeGainPerTick(character);

        Assert.Equal(1, result);
    }

    [Fact]
    public void Tick_AddsGainAndEnablesDualActionWhenOverTwoHundred()
    {
        var character = new Character
        {
            Id = 7,
            Name = "Scout",
            Dexterity = 14,
            TurnSpeed = 10,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new() { TurnMeterModifier = 5 }
            }
        };
        var state = new TurnmeterState
        {
            CharacterId = 7,
            CharacterName = "Scout",
            CurrentValue = 190
        };

        var result = _sut.Tick(character, state);

        Assert.Equal(207, result.CurrentValue);
        Assert.True(result.CanTakeTurn);
        Assert.True(result.HasDualAction);
    }

    [Fact]
    public void AfterTurn_SubtractsOneHundred()
    {
        var state = new TurnmeterState
        {
            CharacterId = 1,
            CharacterName = "Rogue",
            CurrentValue = 205
        };

        var result = _sut.AfterTurn(state);

        Assert.Equal(105, result.CurrentValue);
        Assert.True(result.CanTakeTurn);
    }
}
