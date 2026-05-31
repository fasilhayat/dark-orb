namespace BattleArena.Core.Interfaces;

using Core.Entities;

public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(int id);
    Task<List<Character>> GetAllAsync();
    Task<int> CreateAsync(Character character);
    Task UpdateAsync(Character character);
    Task DeleteAsync(int id);
    Task<List<Weapon>> GetCharacterWeaponsAsync(int characterId);
    Task<List<(Armor Armor, string SlotName)>> GetCharacterArmorAsync(int characterId);
    Task<List<Spell>> GetCharacterSpellsAsync(int characterId);
}
