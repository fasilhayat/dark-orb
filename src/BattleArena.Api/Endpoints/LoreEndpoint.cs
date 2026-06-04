namespace BattleArena.Api.Endpoints;

using Core.Interfaces;

public static class LoreEndpoint
{
    public static void MapLoreEndpoints(this WebApplication app)
    {
        // ── Classes ─────────────────────────────────────────────
        app.MapGet("/v1/classes", async (IClassRepository classRepository) =>
        {
            var classes = await classRepository.GetAllAsync();
            return Results.Ok(classes);
        });

        app.MapGet("/v1/class/{id:int}", async (int id, IClassRepository classRepository) =>
        {
            var cls = await classRepository.GetByIdAsync(id);
            return cls is not null ? Results.Ok(cls) : Results.NotFound();
        });

        // ── Subraces ────────────────────────────────────────────
        app.MapGet("/v1/subraces", async (IRaceRepository raceRepository) =>
        {
            var subraces = await raceRepository.GetAllSubracesAsync();
            return Results.Ok(subraces);
        });

        app.MapGet("/v1/subrace/{id:int}", async (int id, IRaceRepository raceRepository) =>
        {
            var subrace = await raceRepository.GetSubraceByIdAsync(id);
            return subrace is not null ? Results.Ok(subrace) : Results.NotFound();
        });

        app.MapGet("/v1/race/{id:int}/subraces", async (int id, IRaceRepository raceRepository) =>
        {
            var subraces = await raceRepository.GetSubracesByRaceIdAsync(id);
            return Results.Ok(subraces);
        });

        // ── Deities ─────────────────────────────────────────────
        app.MapGet("/v1/deities", async (string? alignment, IDeityRepository deityRepository) =>
        {
            var deities = await deityRepository.GetByAlignmentAsync(alignment);
            return Results.Ok(deities);
        });

        // ── Pets ────────────────────────────────────────────────
        app.MapGet("/v1/pets", async (int? classId, int? raceId, IPetRepository petRepository) =>
        {
            var pets = await petRepository.GetByClassAndRaceAsync(classId, raceId);
            return Results.Ok(pets);
        });

        // ── Spells ──────────────────────────────────────────────
        app.MapGet("/v1/spells", async (string? school, ISpellRepository spellRepository) =>
        {
            var spells = await spellRepository.GetBySchoolAsync(school);
            return Results.Ok(spells);
        });

        // ── Schools ─────────────────────────────────────────────
        app.MapGet("/v1/schools", async (ISpellRepository spellRepository) =>
        {
            var schools = await spellRepository.GetAllSchoolsAsync();
            return Results.Ok(schools);
        });

        // ── Bestiary ────────────────────────────────────────────
        app.MapGet("/v1/bestiary", async (string? category, int? level, IBestiaryRepository bestiaryRepository) =>
        {
            var entries = await bestiaryRepository.GetByCategoryAndLevelAsync(category, level);
            return Results.Ok(entries);
        });
    }
}
