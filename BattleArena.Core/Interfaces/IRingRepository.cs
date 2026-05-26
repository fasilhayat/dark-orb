namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IRingRepository
{
    Task<List<Ring>> GetAllAsync();
}
