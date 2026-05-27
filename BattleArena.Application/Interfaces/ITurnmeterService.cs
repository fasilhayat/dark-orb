namespace BattleArena.Application.Interfaces;

using Models;
using Core.Entities;

public interface ITurnmeterService
{
    TurnmeterState Tick(Character character, TurnmeterState currentState);
    TurnmeterState AfterTurn(TurnmeterState state, int cost = 100);
    int ComputeGainPerTick(Character character);
}
