namespace BattleArena.Api;

using Application.Interfaces;
using Application.Services;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;

public static class AddServices
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiKeyOptions>(configuration.GetSection("ApiKeys"));

        var connectionString = configuration.GetConnectionString("ArenaDatabase")
            ?? "Host=battle-arena-db;Port=5432;Database=battle-arena_data;Username=postgres;Password=postgres";

        services.AddScoped<IDbContext>(_ => new DbContext(connectionString));
        services.AddScoped<IDiceService, DiceService>();
        services.AddScoped<ICombatStatsService, CombatStatsService>();
        services.AddScoped<ITurnmeterService, TurnmeterService>();
        services.AddScoped<IStatusEffectService, StatusEffectService>();
        services.AddScoped<ICombatService, CombatService>();
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IWeaponRepository, WeaponRepository>();
        services.AddScoped<IArmorRepository, ArmorRepository>();
        services.AddScoped<IRaceRepository, RaceRepository>();
        services.AddScoped<IRingRepository, RingRepository>();
        services.AddScoped<IAmuletRepository, AmuletRepository>();
        services.AddScoped<IGirdleRepository, GirdleRepository>();
        services.AddScoped<INpcRepository, NpcRepository>();
        services.AddScoped<IItemSetRepository, ItemSetRepository>();
    }
}
