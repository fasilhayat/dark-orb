using BattleArena.Application.Interfaces;
using BattleArena.Core.Entities;

namespace BattleArena.Api.Endpoints;

public static class CharacterEndpoint
{
    public static void MapCharacterEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/characters", async (ICharacterService characterService) =>
        {
            var characters = await characterService.GetAllCharactersAsync();
            return Results.Ok(characters);
        });

        app.MapGet("/v1/character/{id:int}", async (int id, ICharacterService characterService) =>
        {
            var character = await characterService.GetCharacterAsync(id);
            return character is not null ? Results.Ok(character) : Results.NotFound();
        });

        app.MapPost("/v1/character", async (Character character, ICharacterService characterService) =>
        {
            var id = await characterService.CreateCharacterAsync(character);
            return Results.Created($"/v1/character/{id}", character);
        });

        app.MapPut("/v1/character/{id:int}", async (int id, Character character, ICharacterService characterService) =>
        {
            character.Id = id;
            await characterService.UpdateCharacterAsync(character);
            return Results.NoContent();
        });

        app.MapDelete("/v1/character/{id:int}", async (int id, ICharacterService characterService) =>
        {
            await characterService.DeleteCharacterAsync(id);
            return Results.NoContent();
        });
    }
}
