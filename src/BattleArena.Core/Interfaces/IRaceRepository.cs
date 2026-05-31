namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IRaceRepository
{
    Task<List<Race>> GetAllAsync();
    Task<Race?> GetByIdAsync(int id);
    Task<List<Feat>> GetFeatsByRaceIdAsync(int raceId);
    Task<List<ResistanceBonus>> GetFeatResistancesAsync(int featId);
}
