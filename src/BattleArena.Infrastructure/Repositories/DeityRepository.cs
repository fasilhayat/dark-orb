namespace BattleArena.Infrastructure.Repositories;

using Core.Entities;
using Core.Entities.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Npgsql;

public class DeityRepository : IDeityRepository
{
    private readonly IDbContext _context;

    public DeityRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<Deity>> GetAllAsync()
    {
        return await _context.ExecuteQueryAsync("fn_get_deities", MapDeity);
    }

    public async Task<List<Deity>> GetByAlignmentAsync(string? alignment)
    {
        if (string.IsNullOrWhiteSpace(alignment))
            return await GetAllAsync();

        return await _context.ExecuteQueryAsync(
            "fn_get_deities(p_alignment := @p_alignment)",
            MapDeity,
            new NpgsqlParameter("p_alignment", alignment));
    }

    private static Deity MapDeity(NpgsqlDataReader reader) => new()
    {
        Id = (int)reader["id"],
        Name = (string)reader["name"],
        Alignment = Enum.TryParse<DeityAlignment>((string)reader["alignment"], true, out var align)
            ? align : DeityAlignment.Light,
        Description = reader["description"] as string ?? string.Empty,
        Domain = reader["domain"] as string ?? string.Empty
    };
}
