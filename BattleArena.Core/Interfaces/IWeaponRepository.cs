namespace BattleArena.Core.Interfaces;

using Core.Entities;
using Core.Entities.Enums;

public interface IWeaponRepository
{
    Task<List<Weapon>> GetAllAsync();
    Task<List<Weapon>> GetByArchetypeAsync(ArchetypeWeapon archetype);
    Task<Weapon?> GetByIdAsync(int id);
}
