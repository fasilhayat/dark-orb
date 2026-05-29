namespace BattleArena.Api.Endpoints;

using Core.Interfaces;

public static class NpcEndpoint
{
    public static void MapNpcEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/npcs", async (INpcRepository npcRepository) =>
        {
            var npcs = await npcRepository.GetAllAsync();
            return Results.Ok(npcs);
        });
    }
}
