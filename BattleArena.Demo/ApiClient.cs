using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleArena.Core.Entities;

namespace BattleArena;

/// <summary>
/// Thin HTTP client for the BattleArena REST API.
/// Logs every API call to both console (dimmed) and an optional log file.
/// </summary>
internal sealed class BattleArenaApiClient
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly Action<string>? _consoleLogger;
    private readonly TextWriter? _fileLogger;

    public BattleArenaApiClient(string baseUrl, Action<string>? consoleLogger = null, TextWriter? fileLogger = null)
    {
        _consoleLogger = consoleLogger;
        _fileLogger = fileLogger;
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/')),
            Timeout     = TimeSpan.FromSeconds(10)
        };
    }

    private void LogCall(string method, string path)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        var msg = $"  [API] {ts}  {method} {path}";
        _consoleLogger?.Invoke(msg);
        _fileLogger?.WriteLine(msg);
    }

    private void LogResult(string method, string path, string summary)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        var msg = $"  [API] {ts}  {method} {path}  →  {summary}";
        _consoleLogger?.Invoke(msg);
        _fileLogger?.WriteLine(msg);
        _fileLogger?.Flush();
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        LogCall("GET", "/v1/characters");
        var result = await _http.GetFromJsonAsync<List<Character>>("/v1/characters", _json);
        var list = result ?? [];
        LogResult("GET", "/v1/characters", $"{list.Count} characters");
        return list;
    }

    public async Task<List<Weapon>> GetWeaponsAsync()
    {
        LogCall("GET", "/v1/weapons");
        var result = await _http.GetFromJsonAsync<List<Weapon>>("/v1/weapons", _json);
        var list = result ?? [];
        LogResult("GET", "/v1/weapons", $"{list.Count} weapons");
        return list;
    }
}
