namespace BattleArena.ReqnrollTests.StepDefinitions;

using Application.Interfaces;
using Application.Services;
using Application.Services.QuestValidators;
using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using NSubstitute;
using Reqnroll;
using Xunit;

[Binding]
public sealed class QuestSystemSteps
{
    private readonly IQuestRepository _questRepo = Substitute.For<IQuestRepository>();
    private readonly IQuestService _questService;
    private readonly Quest _quest = new();
    private readonly CharacterQuest _characterQuest = new();
    private string? _statusMessage;
    private bool _completionResult;

    public QuestSystemSteps()
    {
        var validator = new DefaultQuestValidator();
        _questService = new QuestService(_questRepo, [validator]);

        // Default: quest exists, character has NOT accepted it yet
        _characterQuest.Status = QuestStatus.Active;
        _characterQuest.ProgressJson = "{}";

        _questRepo.GetByIdAsync(Arg.Any<int>()).Returns(_quest);
        _questRepo.GetCharacterQuestAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((CharacterQuest?)null);
    }

    [Given(@"a quest ""([^""]+)"" of type (\w+) at level (\d+) rewarding (\d+) XP")]
    public void GivenAQuest(string name, string type, int level, int xp)
    {
        _quest.Name = name;
        _quest.QuestType = Enum.Parse<QuestType>(type);
        _quest.LevelRequirement = level;
        _quest.RewardXp = xp;
    }

    [Given(@"a character with id (\d+)")]
    public void GivenACharacterWithId(int id)
    {
        _characterQuest.CharacterId = id;
    }

    [Given(@"the character has accepted the quest")]
    public async Task GivenTheCharacterHasAcceptedTheQuest()
    {
        _characterQuest.Status = QuestStatus.Active;
        _characterQuest.ProgressJson = "{}";
        _questRepo.GetCharacterQuestAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(_characterQuest);
        await _questService.AcceptQuestAsync(_characterQuest.CharacterId, _quest.Id);
    }

    [Given("the character reports progress:")]
    public void GivenTheCharacterReportsProgress(string progressJson)
    {
        _characterQuest.ProgressJson = progressJson;
    }

    [Given(@"the quest is already completed")]
    public void GivenTheQuestIsAlreadyCompleted()
    {
        _characterQuest.Status = QuestStatus.Completed;
    }

    [When(@"the character accepts the quest")]
    public async Task WhenTheCharacterAcceptsTheQuest()
    {
        await _questService.AcceptQuestAsync(_characterQuest.CharacterId, _quest.Id);
    }

    [When(@"the character tries to complete the quest")]
    public async Task WhenTheCharacterTriesToCompleteTheQuest()
    {
        (_completionResult, _statusMessage) = await _questService.TryCompleteQuestAsync(
            _characterQuest.CharacterId, _quest.Id);
    }

    [Then(@"the quest should appear in the character's active quests")]
    public async Task ThenTheQuestShouldAppearInActiveQuests()
    {
        _questRepo.GetCharacterQuestsAsync(_characterQuest.CharacterId, QuestStatus.Active)
            .Returns([_characterQuest]);

        var quests = await _questService.GetCharacterQuestsAsync(_characterQuest.CharacterId, QuestStatus.Active);
        Assert.Contains(quests, q => q.QuestId == _quest.Id);
    }

    [Then("the result should be success")]
    public void ThenTheResultShouldBeSuccess()
    {
        Assert.True(_completionResult);
    }

    [Then("the result should be failure")]
    public void ThenTheResultShouldBeFailure()
    {
        Assert.False(_completionResult);
    }

    [Then(@"the message should contain ""([^""]+)""")]
    public void ThenTheMessageShouldContain(string expected)
    {
        Assert.NotNull(_statusMessage);
        Assert.Contains(expected, _statusMessage, StringComparison.OrdinalIgnoreCase);
    }
}
