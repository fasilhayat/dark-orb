namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Interfaces;
using NSubstitute;

public class CharacterServiceTests
{
    private readonly ICharacterRepository _characterRepo = Substitute.For<ICharacterRepository>();
    private readonly IRaceRepository _raceRepo = Substitute.For<IRaceRepository>();
    private readonly CharacterService _sut;

    public CharacterServiceTests()
    {
        // Default returns for new enrichment methods (empty lists to avoid NRE)
        _characterRepo.GetCharacterArmorAsync(Arg.Any<int>()).Returns([]);
        _characterRepo.GetCharacterWeaponsAsync(Arg.Any<int>()).Returns([]);
        _characterRepo.GetCharacterSpellsAsync(Arg.Any<int>()).Returns([]);
        _raceRepo.GetByIdAsync(Arg.Any<int>()).Returns((Race?)null);
        _raceRepo.GetSubracesByRaceIdAsync(Arg.Any<int>()).Returns([]);

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
    public async Task GetCharacterAsync_WhenHasSubrace_EnrichesSubraceWithFeats()
    {
        var subraces = new List<Subrace>
        {
            new()
            {
                Id = 10, RaceId = 1, Name = "Cave Kobold",
                Feats = [new Feat { Name = "Dark-Dweller", AttackBonus = 2 }]
            }
        };
        _raceRepo.GetSubracesByRaceIdAsync(1).Returns(subraces);

        var character = new Character { Id = 1, Name = "Kriin", RaceId = 1, SubraceId = 10 };
        _characterRepo.GetByIdAsync(1).Returns(character);

        var result = await _sut.GetCharacterAsync(1);

        Assert.NotNull(result);
        Assert.NotNull(result.Subrace);
        Assert.Equal("Cave Kobold", result.Subrace.Name);
        Assert.Single(result.Subrace.Feats);
        Assert.Equal("Dark-Dweller", result.Subrace.Feats[0].Name);
        Assert.Equal(2, result.Subrace.Feats[0].AttackBonus);
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
