namespace BattleArena.Application.Services;

using Application.Interfaces;
using Application.Models;
using Core.Entities;
using Core.Entities.Enums;

public class TurnmeterService : ITurnmeterService
{
    public int ComputeGainPerTick(Character character)
    {
        var dexMod       = (character.Dexterity - 10) / 2;
        var armorPenalty = character.Equipment.TotalTurnMeterPenalty;
        var archetype    = LevelProgression.Archetype(character.ClassId);
        var levelBonus   = LevelProgression.TurnMeterLevelBonus(character.Level, archetype);
        var buffMod      = 0;
        foreach (var e in character.ActiveStatusEffects)
            buffMod += e.TurnMeterModifier;
        var gain = Math.Max(1, character.TurnSpeed + dexMod + levelBonus + buffMod - armorPenalty);

        if (character.ActiveStatusEffects.Any(e => e.Type == StatusEffectType.Shock))
            gain = Math.Max(1, gain - gain / 3);

        return gain;
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

    public TurnmeterState AfterTurn(TurnmeterState state, int cost = TurnmeterState.TurnThreshold)
    {
        return new TurnmeterState
        {
            CharacterId = state.CharacterId,
            CharacterName = state.CharacterName,
            CurrentValue = Math.Max(0, state.CurrentValue - cost)
        };
    }
}
