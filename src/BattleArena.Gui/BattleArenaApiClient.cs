using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleArena.Core.Entities;

namespace BattleArena.Gui;

internal sealed class BattleArenaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public BattleArenaApiClient(string baseUrl, string? apiKey = null)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/')),
            Timeout = TimeSpan.FromSeconds(10)
        };
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    internal BattleArenaApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Character>>("/v1/characters", JsonOptions);
        return result ?? [];
    }

    public async Task<List<Race>> GetRacesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Race>>("/v1/races", JsonOptions);
        return result ?? [];
    }

    public async Task<List<Subrace>> GetSubracesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Subrace>>("/v1/subraces", JsonOptions);
        return result ?? [];
    }

    public async Task<List<PlayerClass>> GetClassesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<PlayerClass>>("/v1/classes", JsonOptions);
        return result ?? [];
    }

    public async Task<List<Deity>> GetDeitiesAsync(string? alignment = null)
    {
        var url = alignment is not null ? $"/v1/deities?alignment={alignment}" : "/v1/deities";
        var result = await _http.GetFromJsonAsync<List<Deity>>(url, JsonOptions);
        return result ?? [];
    }

    public async Task<List<SpellSchoolInfo>> GetSchoolsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<SpellSchoolInfo>>("/v1/schools", JsonOptions);
        return result ?? [];
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var result = await _http.GetAsync("/api/healthcheck", cts.Token);
            return result.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

}
