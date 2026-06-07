namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class SpellRepository : ISpellRepository
{
    private readonly IDbContext _context;

    public SpellRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Spell>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_spells", MapSpell);
    }

    public async Task<List<Spell>> GetBySchoolAsync(string? school)
    {
        if (string.IsNullOrWhiteSpace(school))
            return await GetAllAsync();

        return await _context.ExecuteQueryAsync(
            "fn_get_spells(p_school := @p_school)",
            MapSpell,
            new NpgsqlParameter("p_school", school));
    }

    private static Spell MapSpell(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? string.Empty,
        School = Enum.TryParse<SpellSchool>((string)reader["school"], true, out var school)
            ? school : SpellSchool.Stormcraft,
        ManaCost = reader["mana_cost"] as int? ?? 0,
        TurnMeterCost = reader["turn_meter_cost"] as int? ?? 100,
        SpellLevel = reader["spell_level"] as int? ?? 1,
        DamageCount = reader["damage_count"] as int? ?? 1,
        AttackBonus = reader["attack_bonus"] as int? ?? 0,
        FlatDamageBonus = reader["flat_damage_bonus"] as int? ?? 0,
        ElementalType = Enum.TryParse<ElementalType>(reader["elemental_type"] as string, true, out var elem)
            ? elem : ElementalType.None,
        ElementalDamage = reader["elemental_damage"] as int? ?? 0,
        DamageDie = ParseDieType(reader["damage_die"] as string),
        DamageType = Enum.TryParse<DamageType>(reader["damage_type"] as string, true, out var dmg)
            ? dmg : DamageType.Bludgeoning,
        AttackType = Enum.TryParse<AttackType>(reader["attack_type"] as string, true, out var atk)
            ? atk : AttackType.Ranged
    };

    private static DieType ParseDieType(string? value) => value switch
    {
        "D4" => DieType.D4,
        "D6" => DieType.D6,
        "D8" => DieType.D8,
        "D10" => DieType.D10,
        "D12" => DieType.D12,
        "D20" => DieType.D20,
        "D100" => DieType.D100,
        _ => DieType.D4
    };

    public async Task<List<SpellSchoolInfo>> GetAllSchoolsAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_spell_schools", MapSchool);
    }

    private static SpellSchoolInfo MapSchool(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Description = reader["description"] as string ?? ""
    };
}
