namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;

public class TurnmeterService : ITurnmeterService
{
    public int ComputeGainPerTick(Character character)
    {
        var dexMod = (character.Dexterity - 10) / 2;
        var armorPenalty = character.Equipment.TotalTurnMeterPenalty;
        var buffMod = character.ActiveStatusEffects.Sum(e => e.TurnMeterModifier);
        return Math.Max(1, character.TurnSpeed + dexMod + buffMod - armorPenalty);
    }

    public TurnmeterState Tick(Character character, TurnmeterState currentState)
    {
        var gain = ComputeGainPerTick(character);
        return new TurnmeterState
        {
            CharacterId = currentState.CharacterId,
            CharacterName = currentState.CharacterName,
            CurrentValue = currentState.CurrentValue + gain
        };
    }

    public TurnmeterState AfterTurn(TurnmeterState state)
    {
        return new TurnmeterState
        {
            CharacterId = state.CharacterId,
            CharacterName = state.CharacterName,
            CurrentValue = state.CurrentValue - 100
        };
    }
}
