namespace BattleArena.Infrastructure.Data;

using Npgsql;

/// <summary>
/// A simple implementation of IDbContext that uses Npgsql to interact with a PostgreSQL database. It provides methods to execute scalar functions, query functions, and stored procedures. The connection string is provided through the constructor, and connections are created as needed for each operation. 
/// The WithParens method ensures that function and procedure names are correctly formatted when constructing SQL commands.
/// </summary>
public class DbContext : IDbContext
{
    /// <summary>
    /// The connection string used to connect to the PostgreSQL database. It is provided through the constructor and stored as a private readonly field.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// The constructor for the DbContext class, which takes a connection string as a parameter and assigns it to the private field.
    /// </summary>
    /// <param name="connectionString">The connection string for the PostgreSQL database.</param>
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
