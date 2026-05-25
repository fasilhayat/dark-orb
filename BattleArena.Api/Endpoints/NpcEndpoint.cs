using BattleArena.Core.Interfaces;

namespace BattleArena.Api.Endpoints;

public static class NpcEndpoint
{
    public static void MapNpcEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/npcs", async (bool? merchant, bool? hostile, INpcRepository npcRepository) =>
        {
            var npcs = await npcRepository.GetAllAsync(merchant, hostile);
            return Results.Ok(npcs);
        });
    }
}
