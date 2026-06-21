namespace BattleArena.UnitTests.Services;

using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using NSubstitute;

public class QuestServiceTests
{
    private readonly IQuestRepository _questRepo = Substitute.For<IQuestRepository>();
    private readonly IQuestValidator _defaultValidator = Substitute.For<IQuestValidator>();
    private readonly QuestService _sut;

    public QuestServiceTests()
    {
        _defaultValidator.Handles.Returns((QuestType?)null);
        _defaultValidator.CanComplete(Arg.Any<CharacterQuest>(), Arg.Any<Quest>()).Returns(true);
        _sut = new QuestService(_questRepo, [_defaultValidator]);
    }

    [Fact]
    public async Task GetQuestAsync_ReturnsQuestFromRepo()
    {
        var expected = new Quest { Id = 1, Name = "Test Quest" };
        _questRepo.GetByIdAsync(1).Returns(expected);

        var result = await _sut.GetQuestAsync(1);

        Assert.Same(expected, result);
        await _questRepo.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task GetQuestAsync_WhenNotFound_ReturnsNull()
    {
        _questRepo.GetByIdAsync(99).Returns((Quest?)null);

        var result = await _sut.GetQuestAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllQuestsAsync_ReturnsAllFromRepo()
    {
        var expected = new List<Quest> { new() { Id = 1 }, new() { Id = 2 } };
        _questRepo.GetAllAsync(null).Returns(expected);

        var result = await _sut.GetAllQuestsAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateQuestAsync_DelegatesToRepo()
    {
        var quest = new Quest { Name = "New Quest" };
        _questRepo.CreateAsync(quest).Returns(42);

        var id = await _sut.CreateQuestAsync(quest);

        Assert.Equal(42, id);
        await _questRepo.Received(1).CreateAsync(quest);
    }

    [Fact]
    public async Task UpdateQuestAsync_DelegatesToRepo()
    {
        var quest = new Quest { Id = 1, Name = "Updated" };

        await _sut.UpdateQuestAsync(quest);

        await _questRepo.Received(1).UpdateAsync(quest);
    }

    [Fact]
    public async Task DeleteQuestAsync_DelegatesToRepo()
    {
        await _sut.DeleteQuestAsync(5);

        await _questRepo.Received(1).DeleteAsync(5);
    }

    [Fact]
    public async Task AcceptQuestAsync_DelegatesToRepo()
    {
        await _sut.AcceptQuestAsync(1, 10);

        await _questRepo.Received(1).AcceptQuestAsync(1, 10);
    }

    [Fact]
    public async Task GetCharacterQuestsAsync_ReturnsFromRepo()
    {
        var expected = new List<CharacterQuest>
        {
            new() { CharacterId = 1, QuestId = 10, Status = QuestStatus.Active },
        };
        _questRepo.GetCharacterQuestsAsync(1, null).Returns(expected);

        var result = await _sut.GetCharacterQuestsAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task UpdateProgressAsync_DelegatesToRepo()
    {
        await _sut.UpdateProgressAsync(1, 10, "{\"kills\": 3}");

        await _questRepo.Received(1).UpdateQuestProgressAsync(1, 10, "{\"kills\": 3}");
    }

    [Fact]
    public async Task TryCompleteQuestAsync_WhenQuestNotFound_ReturnsFalse()
    {
        _questRepo.GetByIdAsync(99).Returns((Quest?)null);

        var (completed, message) = await _sut.TryCompleteQuestAsync(1, 99);

        Assert.False(completed);
        Assert.Equal("Quest not found.", message);
    }

    [Fact]
    public async Task TryCompleteQuestAsync_WhenNotAccepted_ReturnsFalse()
    {
        _questRepo.GetByIdAsync(10).Returns(new Quest { Id = 10 });
        _questRepo.GetCharacterQuestAsync(1, 10).Returns((CharacterQuest?)null);

        var (completed, _) = await _sut.TryCompleteQuestAsync(1, 10);

        Assert.False(completed);
    }

    [Fact]
    public async Task TryCompleteQuestAsync_WhenAlreadyCompleted_ReturnsFalse()
    {
        _questRepo.GetByIdAsync(10).Returns(new Quest { Id = 10 });
        _questRepo.GetCharacterQuestAsync(1, 10).Returns(new CharacterQuest
        {
            CharacterId = 1,
            QuestId = 10,
            Status = QuestStatus.Completed,
        });

        var (completed, _) = await _sut.TryCompleteQuestAsync(1, 10);

        Assert.False(completed);
    }

    [Fact]
    public async Task TryCompleteQuestAsync_WhenConditionsMet_Completes()
    {
        _questRepo.GetByIdAsync(10).Returns(new Quest { Id = 10, QuestType = QuestType.Side });
        _questRepo.GetCharacterQuestAsync(1, 10).Returns(new CharacterQuest
        {
            CharacterId = 1,
            QuestId = 10,
            Status = QuestStatus.Active,
        });

        var (completed, _) = await _sut.TryCompleteQuestAsync(1, 10);

        Assert.True(completed);
        await _questRepo.Received(1).CompleteQuestAsync(1, 10);
    }

    [Fact]
    public async Task TryCompleteQuestAsync_WhenValidatorRejects_ReturnsFalse()
    {
        var strictValidator = Substitute.For<IQuestValidator>();
        strictValidator.Handles.Returns((QuestType?)null);
        strictValidator.CanComplete(Arg.Any<CharacterQuest>(), Arg.Any<Quest>()).Returns(false);

        var sut = new QuestService(_questRepo, [strictValidator]);
        _questRepo.GetByIdAsync(10).Returns(new Quest { Id = 10 });
        _questRepo.GetCharacterQuestAsync(1, 10).Returns(new CharacterQuest
        {
            CharacterId = 1,
            QuestId = 10,
            Status = QuestStatus.Active,
        });

        var (completed, _) = await sut.TryCompleteQuestAsync(1, 10);

        Assert.False(completed);
        await _questRepo.DidNotReceive().CompleteQuestAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task TryCompleteQuestAsync_SelectsCorrectValidatorByType()
    {
        var mainValidator = Substitute.For<IQuestValidator>();
        mainValidator.Handles.Returns(QuestType.Main);
        mainValidator.CanComplete(Arg.Any<CharacterQuest>(), Arg.Any<Quest>()).Returns(true);

        var sut = new QuestService(_questRepo, [mainValidator, _defaultValidator]);
        _questRepo.GetByIdAsync(10).Returns(new Quest { Id = 10, QuestType = QuestType.Main });
        _questRepo.GetCharacterQuestAsync(1, 10).Returns(new CharacterQuest
        {
            CharacterId = 1,
            QuestId = 10,
            Status = QuestStatus.Active,
        });

        var (completed, _) = await sut.TryCompleteQuestAsync(1, 10);

        Assert.True(completed);
        mainValidator.Received(1).CanComplete(Arg.Any<CharacterQuest>(), Arg.Any<Quest>());
    }
}
