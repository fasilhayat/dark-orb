using BattleArena.Core.Entities;
using BattleArena.Core.Entities.Enums;

namespace BattleArena.Core.Interfaces;

public interface IWeaponRepository
{
    Task<List<Weapon>> GetAllAsync();
    Task<List<Weapon>> GetByArchetypeAsync(ArchetypeWeapon archetype);
    Task<Weapon?> GetByIdAsync(int id);
}
