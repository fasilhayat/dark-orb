using BattleArena.Core.Entities;

namespace BattleArena.Core.Interfaces;

public interface IArmorRepository
{
    Task<List<Armor>> GetAllAsync();
    Task<Armor?> GetByIdAsync(int id);
}
