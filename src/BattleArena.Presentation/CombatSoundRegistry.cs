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
        ["LeechTick"]         = "LeechTick",
        ["SpellUpgrade"]      = "SpellUpgrade",
        ["TotalReversal"]     = "Fumble",
        ["FumblePenalty"]     = "Fumble",
        ["KillingBlow"]       = "KillingBlow",
        ["Death"]             = "KillingBlow",
        ["Resurrection"]      = "Resurrection",
    };

    private static readonly Dictionary<string, string> _spellSounds = new()
    {
        ["Fireball"]     = "Fireball",
        ["Ice Bolt"]     = "IceBolt",
        ["Shock"]        = "ShockSpell",
        ["Static Shock"] = "StaticShock",
        ["Smite"]        = "SmiteCast",
        ["Heal"]         = "HealCast",
        ["Mass Heal"]    = "MassHeal",
    };

    public static string GetEffectSoundId(string effectName) =>
        _effectSounds.GetValueOrDefault(effectName) ?? string.Empty;

    public static string GetEventSoundId(string eventType) =>
        _eventSounds.GetValueOrDefault(eventType) ?? string.Empty;

    public static string GetSpellCastSoundId(string? spellName = null) =>
        spellName is not null && _spellSounds.TryGetValue(spellName, out var id) ? id : "SpellCast";

    public static string GetSpellUpgradeSoundId(string? spellName = null) =>
        spellName is not null && _spellSounds.TryGetValue(spellName, out var id) ? id : "SpellUpgrade";

    public static string GetHealCastSoundId(string? spellName = null) =>
        spellName is not null && _spellSounds.TryGetValue(spellName, out var id) ? id : "HealCast";

    public static string GetCriticalHitSoundId() => "CriticalHit";

    public static string GetSoundDescription(string soundId) => soundId switch
    {
        "BurnTick"      => "Crackling sound of searing flames",
        "PoisonTick"    => "Sizzling hiss of bubbling poison",
        "FrostTick"     => "Crystalline crackle of freezing ice",
        "ShockTick"     => "Sharp crack of arcing lightning",
        "BleedTick"     => "Dripping sound of fresh blood",
        "CriticalHit"   => "Resounding impact of a devastating critical hit",
        "Fumble"        => "Clumsy clatter of a fumbled attack",
        "PerfectParry"  => "Clear ringing tone of a perfectly timed parry",
        "PerfectDodge"  => "Whooshing sound of a near miss",
        "CounterAttack" => "Swift whistling sound of a counter-attack",
        "KillingBlow"   => "Echoing finality of a killing blow",
        "Resurrection"  => "Gentle flowing melody of renewal",
        "LeechTick"     => "Eerie whisper of draining energy",
        "SpellUpgrade"  => "Resonant surge of amplified arcane energy",
        "SpellCast"     => "Rumbling roar of arcane power",
        "HealCast"      => "Soft warm chime of healing energy",
        "Fireball"      => "Explosive roar of a fireball",
        "IceBolt"       => "Sharp crack of shattering ice",
        "ShockSpell"    => "Crackling surge of electrical energy",
        "StaticShock"   => "Snapping discharge of static electricity",
        "SmiteCast"     => "Resonant ring of holy power",
        "MassHeal"      => "Warm wave of soothing restoration",
        _               => string.Empty,
    };

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
