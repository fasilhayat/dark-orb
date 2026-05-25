using BattleArena.Core.Interfaces;

namespace BattleArena.Api.Endpoints;

public static class EquipmentEndpoint
{
    public static void MapEquipmentEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/weapons", async (IWeaponRepository weaponRepository) =>
        {
            var weapons = await weaponRepository.GetAllAsync();
            return Results.Ok(weapons);
        });

        app.MapGet("/v1/weapons/{archetype}", async (string archetype, IWeaponRepository weaponRepository) =>
        {
            if (!Enum.TryParse<Core.Entities.Enums.ArchetypeWeapon>(archetype, true, out var parsed))
                return Results.BadRequest($"Invalid weapon archetype: {archetype}");

            var weapons = await weaponRepository.GetByArchetypeAsync(parsed);
            return Results.Ok(weapons);
        });

        app.MapGet("/v1/armor", async (IArmorRepository armorRepository) =>
        {
            var armor = await armorRepository.GetAllAsync();
            return Results.Ok(armor);
        });

        app.MapGet("/v1/races", async (IRaceRepository raceRepository) =>
        {
            var races = await raceRepository.GetAllAsync();
            return Results.Ok(races);
        });

        app.MapGet("/v1/race/{id:int}", async (int id, IRaceRepository raceRepository) =>
        {
            var race = await raceRepository.GetByIdAsync(id);
            return race is not null ? Results.Ok(race) : Results.NotFound();
        });

        app.MapGet("/v1/race/{id:int}/feats", async (int id, IRaceRepository raceRepository) =>
        {
            var feats = await raceRepository.GetFeatsByRaceIdAsync(id);
            return Results.Ok(feats);
        });
    }
}
