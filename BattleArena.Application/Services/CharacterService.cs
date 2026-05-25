using BattleArena.Application.Interfaces;
using BattleArena.Core.Entities;
using BattleArena.Core.Interfaces;

namespace BattleArena.Application.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IRaceRepository _raceRepository;

    public CharacterService(ICharacterRepository characterRepository, IRaceRepository raceRepository)
    {
        _characterRepository = characterRepository;
        _raceRepository = raceRepository;
    }

    public async Task<Character?> GetCharacterAsync(int id)
    {
        return await _characterRepository.GetByIdAsync(id);
    }

    public async Task<List<Character>> GetAllCharactersAsync()
    {
        return await _characterRepository.GetAllAsync();
    }

    public async Task<int> CreateCharacterAsync(Character character)
    {
        return await _characterRepository.CreateAsync(character);
    }

    public async Task UpdateCharacterAsync(Character character)
    {
        await _characterRepository.UpdateAsync(character);
    }

    public async Task DeleteCharacterAsync(int id)
    {
        await _characterRepository.DeleteAsync(id);
    }
}
