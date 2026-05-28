namespace BattleArena.Api.Endpoints;

using Application.Interfaces;
using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;

public static class CombatEndpoint
{
    public static void MapCombatEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/roll/{dieType}", (DieType dieType, IDiceService dice) =>
        {
            var result = dice.Roll(dieType);
            return Results.Ok(new { die = dieType.ToString(), result });
        });

        app.MapGet("/v1/roll/{count}d{sides}", (int count, int sides, IDiceService dice) =>
        {
            var result = dice.Roll(count, sides);
            return Results.Ok(new { dice = $"{count}d{sides}", result });
        });

        app.MapGet("/v1/roll/advantage/{dieType}", (DieType dieType, IDiceService dice) =>
        {
            var result = dice.RollWithAdvantage(dieType);
            return Results.Ok(new { die = dieType.ToString(), result, type = "advantage" });
        });

        app.MapGet("/v1/roll/disadvantage/{dieType}", (DieType dieType, IDiceService dice) =>
        {
            var result = dice.RollWithDisadvantage(dieType);
            return Results.Ok(new { die = dieType.ToString(), result, type = "disadvantage" });
        });

        app.MapGet("/v1/attack/{targetAc:int}", async (int targetAc, ICombatService combat, IWeaponRepository weaponRepo) =>
        {
            var weapons = await weaponRepo.GetAllAsync();
            var weapon = weapons.FirstOrDefault();
            if (weapon is null)
                return Results.NotFound("No weapons found");

            var attacker = new Character
            {
                Name = "Attacker",
                Strength = 10,
                StrikeRating = 20
            };
            var defender = CreateDefender(targetAc);

            var result = combat.ResolveAttack(attacker, defender, weapon);
            return Results.Ok(result);
        });

        app.MapPost("/v1/combat/simulate", async (CombatSimulateRequest req, HttpContext ctx) =>
        {
            var combatService = ctx.RequestServices.GetRequiredService<ICombatService>();
            var turnmeterService = ctx.RequestServices.GetRequiredService<ITurnmeterService>();
            var statusEffectService = ctx.RequestServices.GetRequiredService<IStatusEffectService>();
            var diceService = ctx.RequestServices.GetRequiredService<IDiceService>();

            var heroSelector = CreateSelector(req.HeroTargetStrategy);
            var enemySelector = CreateSelector(req.EnemyTargetStrategy);

            var simulator = new CombatSimulator(
                combatService, turnmeterService, statusEffectService, diceService,
                heroSelector, enemySelector);

            var result = await simulator.SimulateAsync(req.HeroParty, req.EnemyParty, req.MaxTicks);
            return Results.Ok(result);
        });
    }

    private static ITargetSelector CreateSelector(string strategy) => strategy.ToLowerInvariant() switch
    {
        "random" => new RandomTargetSelector(),
        "lowesthp" => new LowestHpTargetSelector(),
        _ => new LowestHpTargetSelector()
    };

    private static Character CreateDefender(int targetAc)
    {
        return new Character
        {
            Name = "Defender",
            Equipment = new ArmorSlots
            {
                Chest = new Armor
                {
                    ArmorClass = Math.Max(0, 20 - targetAc)
                }
            }
        };
    }
}

public record CombatSimulateRequest(
    Party HeroParty,
    Party EnemyParty,
    int MaxTicks = 500,
    string HeroTargetStrategy = "random",
    string EnemyTargetStrategy = "lowestHp"
);
