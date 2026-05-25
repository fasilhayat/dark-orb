using BattleArena.Application.Interfaces;
using BattleArena.Application.Services;
using BattleArena.Core.Entities;
using BattleArena.Core.Interfaces;
using NSubstitute;

namespace BattleArena.UnitTests.Services;

public class CharacterServiceTests
{
    private readonly ICharacterRepository _characterRepo = Substitute.For<ICharacterRepository>();
    private readonly IRaceRepository _raceRepo = Substitute.For<IRaceRepository>();
    private readonly CharacterService _sut;

    public CharacterServiceTests()
    {
        _sut = new CharacterService(_characterRepo, _raceRepo);
    }

    [Fact]
    public async Task GetCharacterAsync_ReturnsCharacterFromRepo()
    {
        var expected = new Character { Id = 1, Name = "Test" };
        _characterRepo.GetByIdAsync(1).Returns(expected);

        var result = await _sut.GetCharacterAsync(1);

        Assert.Same(expected, result);
        await _characterRepo.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task GetCharacterAsync_WhenNotFound_ReturnsNull()
    {
        _characterRepo.GetByIdAsync(99).Returns((Character?)null);

        var result = await _sut.GetCharacterAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllCharactersAsync_ReturnsAllFromRepo()
    {
        var expected = new List<Character>
        {
            new() { Id = 1, Name = "Hero" },
            new() { Id = 2, Name = "Villain" }
        };
        _characterRepo.GetAllAsync().Returns(expected);

        var result = await _sut.GetAllCharactersAsync();

        Assert.Equal(2, result.Count);
        await _characterRepo.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task GetAllCharactersAsync_WhenEmpty_ReturnsEmptyList()
    {
        _characterRepo.GetAllAsync().Returns(new List<Character>());

        var result = await _sut.GetAllCharactersAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateCharacterAsync_ReturnsNewId()
    {
        var character = new Character { Name = "New Hero", RaceId = 1, ClassId = 2 };
        _characterRepo.CreateAsync(character).Returns(42);

        var result = await _sut.CreateCharacterAsync(character);

        Assert.Equal(42, result);
        await _characterRepo.Received(1).CreateAsync(character);
    }

    [Fact]
    public async Task UpdateCharacterAsync_CallsRepoWithCharacter()
    {
        var character = new Character { Id = 1, Name = "Updated" };

        await _sut.UpdateCharacterAsync(character);

        await _characterRepo.Received(1).UpdateAsync(character);
    }

    [Fact]
    public async Task DeleteCharacterAsync_CallsRepoWithId()
    {
        await _sut.DeleteCharacterAsync(5);

        await _characterRepo.Received(1).DeleteAsync(5);
    }
}
