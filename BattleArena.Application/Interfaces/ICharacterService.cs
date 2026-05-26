namespace BattleArena.Application.Interfaces;

using Core.Entities;

public interface ICharacterService
{
    Task<Character?> GetCharacterAsync(int id);
    Task<List<Character>> GetAllCharactersAsync();
    Task<int> CreateCharacterAsync(Character character);
    Task UpdateCharacterAsync(Character character);
    Task DeleteCharacterAsync(int id);
}
