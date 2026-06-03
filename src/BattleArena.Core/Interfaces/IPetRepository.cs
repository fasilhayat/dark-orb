namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IPetRepository
{
    Task<List<Pet>> GetAllAsync();
    Task<List<Pet>> GetByClassAndRaceAsync(int? classId, int? raceId);
}
