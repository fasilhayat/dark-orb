namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Reqnroll;
using Xunit;

/// <summary>
/// Step definitions for the TurnmeterSystem.feature.
/// Exercises TurnmeterService directly to verify tick gain calculation,
/// the CanTakeTurn threshold, and the dual-action mechanic.
/// </summary>
[Binding]
public class TurnmeterSteps
{
    private readonly TurnmeterService _sut = new();
    private Character _combatant = new();
    private TurnmeterState _state = new();
    private int _gainResult;

    [Given(@"a combatant with turn speed (\d+) and dexterity (\d+)")]
    public void GivenACombatantWithTurnSpeedAndDexterity(int turnSpeed, int dexterity)
    {
        _combatant = new Character { TurnSpeed = turnSpeed, Dexterity = dexterity };
        _state = new TurnmeterState { CharacterId = 1, CharacterName = _combatant.Name };
    }

    [Given(@"the combatant wears chest armor with a turn meter penalty of (\d+)")]
    public void GivenTheCombatantWearsChestArmorWithATurnMeterPenaltyOf(int penalty)
    {
        _combatant.Equipment.Chest = new Armor { TurnMeterPenalty = penalty };
    }

    [Given(@"the combatant wears head armor with a turn meter penalty of (\d+)")]
    public void GivenTheCombatantWearsHeadArmorWithATurnMeterPenaltyOf(int penalty)
    {
        _combatant.Equipment.Head = new Armor { TurnMeterPenalty = penalty };
    }

    [Given(@"the combatant has a speed buff granting \+(\d+) turn meter per tick")]
    public void GivenTheCombatantHasASpeedBuffGrantingTurnMeterPerTick(int modifier)
    {
        _combatant.ActiveStatusEffects.Add(new StatusEffect
        {
            Name = "Haste",
            Type = StatusEffectType.Buff,
            TurnMeterModifier = modifier
        });
    }

    [Given(@"the combatant's turn meter is at (\d+)")]
    public void GivenTheCombatantsTurnMeterIsAt(int value)
    {
        _state = new TurnmeterState
        {
            CharacterId = _combatant.Id,
            CharacterName = _combatant.Name,
            CurrentValue = value
        };
    }

    [When(@"the turn meter gain is computed")]
    public void WhenTheTurnMeterGainIsComputed()
    {
        _gainResult = _sut.ComputeGainPerTick(_combatant);
    }

    [When(@"the turn meter ticks once")]
    public void WhenTheTurnMeterTicksOnce()
    {
        _state = _sut.Tick(_combatant, _state);
    }

    [When(@"the combatant takes their turn")]
    public void WhenTheCombatantTakesTheirTurn()
    {
        _state = _sut.AfterTurn(_state);
    }

    [Then(@"the turn meter gain per tick should be (\d+)")]
    public void ThenTheTurnMeterGainPerTickShouldBe(int expected)
    {
        Assert.Equal(expected, _gainResult);
    }

    [Then(@"the turn meter value should be (\d+)")]
    public void ThenTheTurnMeterValueShouldBe(int expected)
    {
        Assert.Equal(expected, _state.CurrentValue);
    }

    [Then(@"the combatant can take their turn")]
    public void ThenTheCombatantCanTakeTheirTurn()
    {
        Assert.True(_state.CanTakeTurn);
    }

    [Then(@"the combatant has a dual action available")]
    public void ThenTheCombatantHasADualActionAvailable()
    {
        Assert.True(_state.HasDualAction);
    }
}
