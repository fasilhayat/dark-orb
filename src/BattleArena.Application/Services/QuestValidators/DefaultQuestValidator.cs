namespace BattleArena.Application.Services.QuestValidators;

using System.Text.Json;
using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;

/// <summary>
/// Default validator: a quest completes when its progress JSON contains
/// a "completed" key set to true, or when all tracked counters reach
/// their target values.
/// </summary>
public class DefaultQuestValidator : IQuestValidator
{
    public QuestType? Handles => null;

    public bool CanComplete(CharacterQuest characterQuest, Quest quest)
    {
        if (string.IsNullOrWhiteSpace(characterQuest.ProgressJson))
            return false;

        using var doc = JsonDocument.Parse(characterQuest.ProgressJson);
        var root = doc.RootElement;

        // Explicit "completed": true flag
        if (root.TryGetProperty("completed", out var flag) && flag.ValueKind == JsonValueKind.True)
            return true;

        // All numeric counters meet or exceed their targets
        // e.g. {"kills": 5, "target": 5} → completes when kills >= target
        if (root.TryGetProperty("target", out var target) && target.ValueKind == JsonValueKind.Number)
        {
            // Sum all tracked numeric values (excluding target itself)
            var total = 0;
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name != "target" && prop.Value.ValueKind == JsonValueKind.Number)
                    total += prop.Value.GetInt32();
            }
            return total >= target.GetInt32();
        }

        return false;
    }
}
