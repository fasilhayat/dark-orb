using BattleArena.Core.Entities;

namespace BattleArena.Core.Interfaces;

public interface IRaceRepository
{
    Task<List<Race>> GetAllAsync();
    Task<Race?> GetByIdAsync(int id);
    Task<List<Feat>> GetFeatsByRaceIdAsync(int raceId);
}
