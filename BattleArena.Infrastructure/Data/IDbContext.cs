namespace BattleArena.Infrastructure.Data;

using Npgsql;

public interface IDbContext
{
    NpgsqlConnection CreateConnection();
    Task<T?> ExecuteScalarAsync<T>(string functionName, params NpgsqlParameter[] parameters);
    Task<List<T>> ExecuteQueryAsync<T>(string functionName, Func<NpgsqlDataReader, T> map, params NpgsqlParameter[] parameters);
    Task ExecuteProcedureAsync(string procedureName, params NpgsqlParameter[] parameters);
}
