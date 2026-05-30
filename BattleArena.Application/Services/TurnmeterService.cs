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
        var archetype = LevelProgression.Archetype(character.ClassId);
        var levelBonus = LevelProgression.TurnMeterLevelBonus(character.Level, archetype);
        return Math.Max(1, character.TurnSpeed + dexMod + levelBonus + buffMod - armorPenalty);
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

    public TurnmeterState AfterTurn(TurnmeterState state, int cost = 100)
    {
        return new TurnmeterState
        {
            CharacterId = state.CharacterId,
            CharacterName = state.CharacterName,
            CurrentValue = Math.Max(0, state.CurrentValue - cost)
        };
    }
}
