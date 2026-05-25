using BattleArena.Core.Entities;

namespace BattleArena.Core.Interfaces;

public interface IAmuletRepository
{
    Task<List<Amulet>> GetAllAsync();
}
