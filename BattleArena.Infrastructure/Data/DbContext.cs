using Npgsql;

namespace BattleArena.Infrastructure.Data;

public class DbContext : IDbContext
{
    private readonly string _connectionString;

    public DbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    private static string WithParens(string name) =>
        name.Contains('(') ? name : $"{name}()";

    public async Task<T?> ExecuteScalarAsync<T>(string functionName, params NpgsqlParameter[] parameters)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"SELECT * FROM arena_data.{WithParens(functionName)}", connection)
        {
            CommandType = System.Data.CommandType.Text
        };

        if (parameters.Length != 0)
            command.Parameters.AddRange(parameters);

        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? default : (T?)result;
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(string functionName, Func<NpgsqlDataReader, T> map, params NpgsqlParameter[] parameters)
    {
        var results = new List<T>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"SELECT * FROM arena_data.{WithParens(functionName)}", connection)
        {
            CommandType = System.Data.CommandType.Text
        };

        if (parameters.Length != 0)
            command.Parameters.AddRange(parameters);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(map(reader));

        return results;
    }

    public async Task ExecuteProcedureAsync(string procedureName, params NpgsqlParameter[] parameters)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"CALL arena_data.{WithParens(procedureName)}", connection)
        {
            CommandType = System.Data.CommandType.Text
        };

        if (parameters.Length != 0)
            command.Parameters.AddRange(parameters);

        await command.ExecuteNonQueryAsync();
    }
}
