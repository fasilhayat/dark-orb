namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IClassRepository
{
    Task<List<PlayerClass>> GetAllAsync();
    Task<PlayerClass?> GetByIdAsync(int id);
}
