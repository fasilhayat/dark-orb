namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IRaceRepository
{
    Task<List<Race>> GetAllAsync();
    Task<List<Race>> GetPlayableAsync();
    Task<Race?> GetByIdAsync(int id);
    Task<List<Feat>> GetFeatsByRaceIdAsync(int raceId);
    Task<List<ResistanceBonus>> GetFeatResistancesAsync(int featId);
    Task<List<Subrace>> GetSubracesByRaceIdAsync(int raceId);
    Task<List<Subrace>> GetAllSubracesAsync();
    Task<Subrace?> GetSubraceByIdAsync(int subraceId);
    Task<List<Feat>> GetSubraceAbilitiesAsync(int subraceId);
    Task<List<ResistanceBonus>> GetSubraceFeatResistancesAsync(int featId);
}
