namespace BattleArena.Application.Services;

using Application.Interfaces;
using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;

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
        var character = await _characterRepository.GetByIdAsync(id);
        if (character is null) return null;
        await EnrichCharacterAsync(character);
        return character;
    }

    public async Task<List<Character>> GetAllCharactersAsync()
    {
        var characters = await _characterRepository.GetAllAsync();
        foreach (var character in characters)
            await EnrichCharacterAsync(character);
        return characters;
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

    private async Task EnrichCharacterAsync(Character character)
    {
        // Load race with feats and resistances
        if (character.RaceId > 0)
            character.Race = await _raceRepository.GetByIdAsync(character.RaceId);

        // Load subrace with feats and resistances
        if (character.SubraceId > 0)
            character.Subrace = (await _raceRepository.GetSubracesByRaceIdAsync(character.RaceId))
                .FirstOrDefault(s => s.Id == character.SubraceId);

        // Load equipment
        await LoadEquipmentAsync(character);

        // Load memorized spells
        character.MemorizedSpells = await _characterRepository.GetCharacterSpellsAsync(character.Id);
    }

    private async Task LoadEquipmentAsync(Character character)
    {
        var armorList = await _characterRepository.GetCharacterArmorAsync(character.Id);
        var weapons = await _characterRepository.GetCharacterWeaponsAsync(character.Id);

        var slots = character.Equipment = new ArmorSlots();

        foreach (var (armor, slotName) in armorList)
        {
            switch (slotName)
            {
                case "Head":  slots.Head = armor; break;
                case "Chest": slots.Chest = armor; break;
                case "Hands": slots.Hands = armor; break;
                case "Waist": slots.Waist = armor; break;
                case "Foot":  slots.Boots = armor; break;
                case "Neck":  slots.Neck = armor; break;
                case "Back":  slots.Back = armor; break;
                case "LeftHand" when armor.Category == "Shield":
                    slots.Shield = new Shield
                    {
                        Id = armor.Id,
                        Name = armor.Name,
                        Description = armor.Description,
                        DefenseBonus = armor.ArmorClass,
                        Quality = armor.Quality
                    };
                    break;
            }
        }

        foreach (var weapon in weapons)
        {
            // Maps slot_name from DB to the appropriate ArmorSlots property
            // Weapon slot_name is determined by fn_get_character_weapons
            // RightHand and LeftHand are the only weapon slots currently
            slots.RightHand ??= weapon;
            if (slots.RightHand != null && slots.RightHand.Name != weapon.Name)
                slots.LeftHand = weapon;
        }
    }
}
