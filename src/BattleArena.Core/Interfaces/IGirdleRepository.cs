namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IGirdleRepository
{
    Task<List<Girdle>> GetAllAsync();
}
