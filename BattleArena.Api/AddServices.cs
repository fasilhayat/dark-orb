using BattleArena.Application.Interfaces;
using BattleArena.Application.Services;
using BattleArena.Core.Interfaces;
using BattleArena.Infrastructure.Data;
using BattleArena.Infrastructure.Repositories;

namespace BattleArena.Api;

public static class AddServices
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ArenaDatabase")
            ?? "Host=battle-arena-db;Port=5432;Database=battle-arena_data;Username=postgres;Password=postgres";

        services.AddScoped<IDbContext>(_ => new DbContext(connectionString));
        services.AddScoped<IDiceService, DiceService>();
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
