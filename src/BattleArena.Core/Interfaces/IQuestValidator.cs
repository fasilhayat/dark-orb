namespace BattleArena.Core.Interfaces;

using Core.Entities;
using Core.Entities.Enums;

/// <summary>
/// Strategy for validating quest completion.  Each quest type gets its own
/// validator registered in DI.  Multiple validators can chain.
/// </summary>
public interface IQuestValidator
{
    /// <summary>The quest type this validator handles, or null to handle all.</summary>
    QuestType? Handles { get; }

    /// <summary>
    /// Returns true when the character's progress satisfies the quest's
    /// completion conditions.
    /// </summary>
    bool CanComplete(CharacterQuest characterQuest, Quest quest);
}
