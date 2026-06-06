namespace BattleArena.Presentation;

public static class CombatSoundRegistry
{
    private static readonly Dictionary<string, string> _effectSounds = new()
    {
        ["Burning"]  = "BurnTick",
        ["Ignite"]   = "BurnTick",
        ["Poisoned"] = "PoisonTick",
        ["Bleeding"] = "BleedTick",
        ["Frozen"]   = "FrostTick",
        ["Freeze"]   = "FrostTick",
        ["Shocked"]  = "ShockTick",
    };

    private static readonly Dictionary<string, string> _eventSounds = new()
    {
        ["PerfectParry"]      = "PerfectParry",
        ["PerfectDodge"]      = "PerfectDodge",
        ["CounterAttack"]     = "CounterAttack",
        ["DevastatingStrike"] = "CriticalHit",
        ["TotalReversal"]     = "Fumble",
        ["FumblePenalty"]     = "Fumble",
        ["KillingBlow"]       = "KillingBlow",
        ["Death"]             = "KillingBlow",
        ["Resurrection"]      = "Resurrection",
        ["SpellCast"]         = "SpellCast",
        ["HealCast"]          = "HealCast",
    };

    public static string GetSpellCastSoundId() => "SpellCast";

    public static string GetHealCastSoundId() => "HealCast";

    public static string GetEffectSoundId(string effectName) =>
        _effectSounds.GetValueOrDefault(effectName) ?? string.Empty;

    public static string GetEventSoundId(string eventType) =>
        _eventSounds.GetValueOrDefault(eventType) ?? string.Empty;

    public static string GetCriticalHitSoundId() => "CriticalHit";

    public static IEnumerable<string> AllKnownSoundIds
    {
        get
        {
            var ids = new HashSet<string>();
            foreach (var s in _effectSounds.Values) ids.Add(s);
            foreach (var s in _eventSounds.Values) ids.Add(s);
            ids.Add(GetCriticalHitSoundId());
            return ids;
        }
    }
}
