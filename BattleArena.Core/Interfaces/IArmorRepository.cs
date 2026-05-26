namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface IArmorRepository
{
    Task<List<Armor>> GetAllAsync();
    Task<Armor?> GetByIdAsync(int id);
}
