using BattleArena.Core.Entities;

namespace BattleArena.Core.Interfaces;

public interface IGirdleRepository
{
    Task<List<Girdle>> GetAllAsync();
}
