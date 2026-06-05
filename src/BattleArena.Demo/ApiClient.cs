namespace BattleArena;

using Application.Models;
using Application.Services;
using Core.Entities;
using Core.Entities.Enums;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public BattleArenaApiClient(string baseUrl, string? apiKey = null, Action<string>? consoleLogger = null, TextWriter? fileLogger = null)
    {
        _consoleLogger = consoleLogger;
        _fileLogger = fileLogger;
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/')),
            Timeout     = TimeSpan.FromSeconds(10)
        };
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    public BattleArenaApiClient(string baseUrl, TextWriter? fileLogger)
        : this(baseUrl, apiKey: null, fileLogger: fileLogger)
    {
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

    // ── Dice API ───────────────────────────────────────────────────────────

    public async Task<int> RollDieAsync(DieType dieType)
    {
        var path = $"/v1/roll/{dieType}";
        LogCall("GET", path);
        var dto = await _http.GetFromJsonAsync<DieRollResponse>(path, _json);
        LogResult("GET", path, $"{dto?.Result}");
        return dto?.Result ?? 0;
    }

    public async Task<int> RollDiceAsync(int count, int sides)
    {
        var path = $"/v1/roll/{count}d{sides}";
        LogCall("GET", path);
        var dto = await _http.GetFromJsonAsync<DiceRollResponse>(path, _json);
        LogResult("GET", path, $"{dto?.Result}");
        return dto?.Result ?? 0;
    }

    public async Task<int> RollWithAdvantageAsync(DieType dieType)
    {
        var path = $"/v1/roll/advantage/{dieType}";
        LogCall("GET", path);
        var dto = await _http.GetFromJsonAsync<DieRollResponse>(path, _json);
        LogResult("GET", path, $"{dto?.Result}");
        return dto?.Result ?? 0;
    }

    public async Task<int> RollWithDisadvantageAsync(DieType dieType)
    {
        var path = $"/v1/roll/disadvantage/{dieType}";
        LogCall("GET", path);
        var dto = await _http.GetFromJsonAsync<DieRollResponse>(path, _json);
        LogResult("GET", path, $"{dto?.Result}");
        return dto?.Result ?? 0;
    }

    internal record DieRollResponse(string Die, int Result);
    internal record DiceRollResponse(string Dice, int Result);

    public async Task<CombatResult> SimulateCombatAsync(
        string heroPartyName, List<int> heroMemberIds,
        string enemyPartyName, List<int> enemyMemberIds,
        int maxTicks = CombatSimulator.DefaultMaxTicks,
        string heroTargetStrategy = "lowestHp",
        string enemyTargetStrategy = "lowestHp")
    {
        LogCall("POST", "/v1/combat/simulate");
        var req = new CombatSimulateByMembersRequest(
            heroPartyName, heroMemberIds,
            enemyPartyName, enemyMemberIds,
            maxTicks, heroTargetStrategy, enemyTargetStrategy);
        var resp = await _http.PostAsJsonAsync("/v1/combat/simulate", req, _json);
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<CombatResult>(_json);
        LogResult("POST", "/v1/combat/simulate", $"tick {result?.TotalTicks ?? 0}, log {result?.Log.Count ?? 0} entries");
        return result ?? new CombatResult();
    }
}

public record CombatSimulateByMembersRequest(
    string HeroPartyName,
    List<int> HeroPartyMemberIds,
    string EnemyPartyName,
    List<int> EnemyPartyMemberIds,
    int MaxTicks = CombatSimulator.DefaultMaxTicks,
    string HeroTargetStrategy = "lowestHp",
    string EnemyTargetStrategy = "lowestHp"
);
