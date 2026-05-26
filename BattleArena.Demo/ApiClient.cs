using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleArena.Core.Entities;

namespace BattleArena;

/// <summary>
/// Thin HTTP client for the BattleArena REST API.
/// Activated only when BATTLE_ARENA_API_URL is set; the demo falls back gracefully.
/// </summary>
internal sealed class BattleArenaApiClient
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public BattleArenaApiClient(string baseUrl) =>
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/')),
            Timeout     = TimeSpan.FromSeconds(10)
        };

    public async Task<List<Character>> GetCharactersAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Character>>("/v1/characters", _json);
        return result ?? [];
    }

    public async Task<List<Weapon>> GetWeaponsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<Weapon>>("/v1/weapons", _json);
        return result ?? [];
    }
}
