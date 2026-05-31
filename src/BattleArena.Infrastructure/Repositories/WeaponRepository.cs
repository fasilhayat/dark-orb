namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class WeaponRepository : IWeaponRepository
{
    private readonly IDbContext _context;

    public WeaponRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Weapon>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_weapons", MapWeapon);
    }

    public async Task<List<Weapon>> GetByArchetypeAsync(ArchetypeWeapon archetype)
    {
        return await _context.ExecuteQueryAsync(
            "fn_get_weapons(p_type := @p_type)",
            MapWeapon,
            new NpgsqlParameter("p_type", archetype.ToString()));
    }

    public async Task<Weapon?> GetByIdAsync(int id)
    {
        var results = await _context.ExecuteQueryAsync(
            "fn_get_weapons(p_id := @p_id)",
            MapWeapon,
            new NpgsqlParameter("p_id", id));
        return results.FirstOrDefault();
    }

    private static Weapon MapWeapon(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        Archetype = Enum.Parse<ArchetypeWeapon>((string)reader["weapon_type"]),
        DamageDie = Enum.Parse<DieType>((string)reader["damage_die"]),
        DamageType = Enum.Parse<DamageType>((string)reader["damage_type"]),
        AttackType = Enum.Parse<AttackType>((string)reader["attack_type"]),
        DamageCount = reader["damage_count"] as int? ?? 1,
        Hands = reader["hands"] as int? ?? 1,
        Quality = Enum.Parse<GearQuality>((string)reader["quality"]),
        AttackBonus = reader["attack_bonus"] as int? ?? 0
    };
}
