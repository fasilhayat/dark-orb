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

        app.MapPost("/v1/combat/simulate", async (CombatSimulateByMembersRequest req, HttpContext ctx) =>
        {
            var combatService = ctx.RequestServices.GetRequiredService<ICombatService>();
            var turnmeterService = ctx.RequestServices.GetRequiredService<ITurnmeterService>();
            var statusEffectService = ctx.RequestServices.GetRequiredService<IStatusEffectService>();
            var diceService = ctx.RequestServices.GetRequiredService<IDiceService>();
            var characterService = ctx.RequestServices.GetRequiredService<ICharacterService>();

            var heroMembers = await BuildPartyMembers(req.HeroPartyMemberIds, characterService);
            var enemyMembers = await BuildPartyMembers(req.EnemyPartyMemberIds, characterService);

            var heroParty = Party.HeroParty(req.HeroPartyName, heroMembers);
            var enemyParty = new Party { Name = req.EnemyPartyName, Members = enemyMembers.ToList() };

            var heroSelector = CreateSelector(req.HeroTargetStrategy);
            var enemySelector = CreateSelector(req.EnemyTargetStrategy);

            var simulator = new CombatSimulator(
                combatService, turnmeterService, statusEffectService, diceService,
                heroSelector, enemySelector);

            var result = await simulator.SimulateAsync(heroParty, enemyParty, req.MaxTicks);
            return Results.Ok(result);
        });
    }

    private static async Task<IEnumerable<PartyMember>> BuildPartyMembers(
        List<int> characterIds, ICharacterService characterService)
    {
        var members = new List<PartyMember>();
        foreach (var id in characterIds)
        {
            var character = await characterService.GetCharacterAsync(id);
            if (character is null) continue;
            members.Add(new PartyMember
            {
                Character = character,
                AttackSource = ResolveAttackSource(character)
            });
        }
        return members;
    }

    private static IAttackSource ResolveAttackSource(Character character) =>
        CharacterAttackResolver.Resolve(character);

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

public record CombatSimulateByMembersRequest(
    string HeroPartyName,
    List<int> HeroPartyMemberIds,
    string EnemyPartyName,
    List<int> EnemyPartyMemberIds,
    int MaxTicks = 500,
    string HeroTargetStrategy = "lowestHp",
    string EnemyTargetStrategy = "lowestHp"
);
