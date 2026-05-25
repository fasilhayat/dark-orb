using BattleArena.Core.Entities;

namespace BattleArena.Core.Interfaces;

public interface IRingRepository
{
    Task<List<Ring>> GetAllAsync();
}
