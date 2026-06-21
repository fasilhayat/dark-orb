namespace BattleArena.Api.Endpoints;

using Application.Interfaces;
using Core.Entities;
using Core.Entities.Enums;

public static class QuestEndpoint
{
    public static void MapQuestEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/quests", async (int? level, IQuestService questService) =>
        {
            var quests = await questService.GetAllQuestsAsync(level);
            return Results.Ok(quests);
        });

        app.MapGet("/v1/quests/{id:int}", async (int id, IQuestService questService) =>
        {
            var quest = await questService.GetQuestAsync(id);
            return quest is not null ? Results.Ok(quest) : Results.NotFound();
        });

        app.MapPost("/v1/quests", async (Quest quest, IQuestService questService) =>
        {
            var id = await questService.CreateQuestAsync(quest);
            quest.Id = id;
            return Results.Created($"/v1/quests/{id}", quest);
        });

        app.MapPut("/v1/quests/{id:int}", async (int id, Quest quest, IQuestService questService) =>
        {
            quest.Id = id;
            await questService.UpdateQuestAsync(quest);
            return Results.NoContent();
        });

        app.MapDelete("/v1/quests/{id:int}", async (int id, IQuestService questService) =>
        {
            await questService.DeleteQuestAsync(id);
            return Results.NoContent();
        });

        // Character quest journal
        app.MapGet("/v1/character/{characterId:int}/quests", async (int characterId, string? status, IQuestService questService) =>
        {
            QuestStatus? parsed = status is not null ? Enum.Parse<QuestStatus>(status, ignoreCase: true) : null;
            var quests = await questService.GetCharacterQuestsAsync(characterId, parsed);
            return Results.Ok(quests);
        });

        app.MapPost("/v1/character/{characterId:int}/quests/{questId:int}/accept", async (int characterId, int questId, IQuestService questService) =>
        {
            await questService.AcceptQuestAsync(characterId, questId);
            return Results.Ok();
        });

        app.MapPut("/v1/character/{characterId:int}/quests/{questId:int}/progress", async (int characterId, int questId, ProgressRequest request, IQuestService questService) =>
        {
            await questService.UpdateProgressAsync(characterId, questId, request.ProgressJson);
            return Results.Ok();
        });

        app.MapPost("/v1/character/{characterId:int}/quests/{questId:int}/complete", async (int characterId, int questId, IQuestService questService) =>
        {
            var (completed, message) = await questService.TryCompleteQuestAsync(characterId, questId);
            return completed ? Results.Ok() : Results.BadRequest(new { error = message });
        });
    }
}

public sealed record ProgressRequest(string ProgressJson);
