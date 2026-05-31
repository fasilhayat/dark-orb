namespace BattleArena.Api.Endpoints;

using Core.Interfaces;

public static class AccessoriesEndpoint
{
    public static void MapAccessoriesEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/rings", async (IRingRepository ringRepository) =>
        {
            var rings = await ringRepository.GetAllAsync();
            return Results.Ok(rings);
        });

        app.MapGet("/v1/amulets", async (IAmuletRepository amuletRepository) =>
        {
            var amulets = await amuletRepository.GetAllAsync();
            return Results.Ok(amulets);
        });

        app.MapGet("/v1/girdles", async (IGirdleRepository girdleRepository) =>
        {
            var girdles = await girdleRepository.GetAllAsync();
            return Results.Ok(girdles);
        });

        app.MapGet("/v1/sets", async (IItemSetRepository itemSetRepository) =>
        {
            var sets = await itemSetRepository.GetAllSetsAsync();
            return Results.Ok(sets);
        });

        app.MapGet("/v1/sets/{id:int}/bonuses", async (int id, IItemSetRepository itemSetRepository) =>
        {
            var bonuses = await itemSetRepository.GetSetBonusesAsync(id);
            return Results.Ok(bonuses);
        });
    }
}
