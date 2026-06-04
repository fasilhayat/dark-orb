namespace BattleArena.Presentation;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class GuiDisplayConfig
{
    private static readonly JsonSerializerOptions _opts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Dictionary<string, bool> _screenEnabled = new();
    private readonly Dictionary<string, HashSet<string>> _enabledFields = new();

    private GuiDisplayConfig()
    {
    }

    private GuiDisplayConfig(GuiContractModel contract)
    {
        void Register(string key, GuiScreenModel? screen)
        {
            if (screen is null)
            {
                return;
            }

            _screenEnabled[key] = screen.Enabled ?? true;
            _enabledFields[key] = (screen.RequiredFields ?? [])
                .Where(f => f.Enabled != false)
                .Select(f => f.Field)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        Register("characterCard",  contract.Screens?.CharacterCard);
        Register("startupSheets", contract.Screens?.StartupSheets);
        Register("roundBar",      contract.Screens?.RoundBar);
        Register("attackEvent",   contract.Screens?.AttackEvent);
        Register("manaEvent",     contract.Screens?.ManaEvent);
        Register("damageEvent",   contract.Screens?.DamageEvent);
        Register("hotTick",       contract.Screens?.HotTick);
        Register("healedEvent",   contract.Screens?.HealedEvent);
        Register("combatSummary", contract.Screens?.CombatSummary);
    }

    public bool IsScreenEnabled(string screen) =>
        !_screenEnabled.TryGetValue(screen, out var enabled) || enabled;

    public bool IsFieldEnabled(string screen, string field) =>
        IsScreenEnabled(screen) &&
        (!_enabledFields.TryGetValue(screen, out var fields) || fields.Contains(field));

    public static GuiDisplayConfig Default { get; } = new();

    public static GuiDisplayConfig Load(string? path = null, Action<string>? logger = null)
    {
        path ??= Path.Combine(AppContext.BaseDirectory, "gui-display-contract.json");
        if (!File.Exists(path))
        {
            logger?.Invoke($"[GuiDisplayConfig] Not found: {path} — using defaults.");
            return Default;
        }

        try
        {
            var contract = JsonSerializer.Deserialize<GuiContractModel>(
                File.ReadAllText(path), _opts);
            return contract is null ? Default : new GuiDisplayConfig(contract);
        }
        catch (Exception ex)
        {
            logger?.Invoke($"[GuiDisplayConfig] Parse error: {ex.Message} — using defaults.");
            return Default;
        }
    }

    private sealed record GuiContractModel(
        [property: JsonPropertyName("version")] string? Version,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("screens")] GuiScreensModel? Screens);

    private sealed record GuiScreensModel(
        [property: JsonPropertyName("characterCard")]  GuiScreenModel? CharacterCard,
        [property: JsonPropertyName("startupSheets")]  GuiScreenModel? StartupSheets,
        [property: JsonPropertyName("roundBar")]       GuiScreenModel? RoundBar,
        [property: JsonPropertyName("attackEvent")]    GuiScreenModel? AttackEvent,
        [property: JsonPropertyName("manaEvent")]      GuiScreenModel? ManaEvent,
        [property: JsonPropertyName("damageEvent")]    GuiScreenModel? DamageEvent,
        [property: JsonPropertyName("hotTick")]        GuiScreenModel? HotTick,
        [property: JsonPropertyName("healedEvent")]    GuiScreenModel? HealedEvent,
        [property: JsonPropertyName("combatSummary")]  GuiScreenModel? CombatSummary);

    private sealed record GuiScreenModel(
        [property: JsonPropertyName("enabled")] bool? Enabled,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("requiredFields")] List<GuiFieldModel>? RequiredFields);

    private sealed record GuiFieldModel(
        [property: JsonPropertyName("enabled")] bool? Enabled,
        [property: JsonPropertyName("field")] string Field,
        [property: JsonPropertyName("source")] string? Source,
        [property: JsonPropertyName("description")] string? Description);
}
