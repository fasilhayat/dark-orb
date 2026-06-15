# Task — Sound ID Strings to Static Constants

Project: Dark Orb

Priority: Medium

Type: Refactoring

Status: Draft

---

## Objective

Replace all 37+ hardcoded sound ID strings (`"BurnTick"`, `"CriticalHit"`, `"Fireball"`, etc.) with a `static class SoundIds` to eliminate duplication and provide a single source of truth for sound effect identifiers.

---

## Current State

Sound IDs are hardcoded in 3 locations:

**`CombatSoundRegistry.cs`** — Canonical registry with 37 string literal sound IDs:
- Lines 7-13: 7 effect→sound mappings (e.g., `["Burning"] = "BurnTick"`)
- Lines 18-28: 11 event→sound mappings (e.g., `["PerfectParry"] = "PerfectParry"`)
- Lines 33-39: 7 spell→sound mappings (e.g., `["Fireball"] = "Fireball"`)
- Lines 61-67: 4 fallback sound IDs (`"SpellCast"`, `"SpellUpgrade"`, `"HealCast"`)
- Line 69: `GetCriticalHitSoundId()` returns `"CriticalHit"`
- Lines 71-96: 22 additional sound ID strings in description switch

**`CombatPlaybackEngine.cs:393-431`** — 6 string arguments passed to `GetEventSoundId()`:
- `"SpellUpgrade"`, `"PerfectParry"`, `"FumblePenalty"`, `"Death"`, `"Resurrection"`, `"Leech"`

**`CombatSoundRegistryTests.cs`** — 27 hardcoded strings in test assertions

---

## Proposed Solution

Add a `static class SoundIds` in `BattleArena.Presentation`:

```csharp
namespace BattleArena.Presentation;

public static class SoundIds
{
    // Effect sounds
    public const string BurnTick = "BurnTick";
    public const string PoisonTick = "PoisonTick";
    public const string BleedTick = "BleedTick";
    public const string FrostTick = "FrostTick";
    public const string ShockTick = "ShockTick";

    // Event sounds
    public const string PerfectParry = "PerfectParry";
    public const string PerfectDodge = "PerfectDodge";
    public const string CounterAttack = "CounterAttack";
    public const string CriticalHit = "CriticalHit";
    public const string Fumble = "Fumble";
    public const string KillingBlow = "KillingBlow";
    public const string Resurrection = "Resurrection";
    public const string LeechTick = "LeechTick";
    public const string SpellUpgrade = "SpellUpgrade";

    // Spell sounds
    public const string Fireball = "Fireball";
    public const string IceBolt = "IceBolt";
    public const string ShockSpell = "ShockSpell";
    public const string StaticShock = "StaticShock";
    public const string SmiteCast = "SmiteCast";
    public const string HealCast = "HealCast";
    public const string MassHeal = "MassHeal";
    public const string SpellCast = "SpellCast";
}
```

Then update all references to use `SoundIds.X`.

---

## Files to Modify

| File | Changes |
|------|---------|
| `BattleArena.Presentation/SoundIds.cs` | **New file** — static constants class |
| `BattleArena.Presentation/CombatSoundRegistry.cs` | Replace all string literals with `SoundIds.X` |
| `BattleArena.Presentation/CombatPlaybackEngine.cs` | Replace 6 string arguments with `SoundIds.X` |
| `BattleArena.UnitTests/Services/CombatSoundRegistryTests.cs` | Replace all string literals with `SoundIds.X` |

---

## Acceptance Criteria

- [ ] `SoundIds` static class exists with all sound ID constants
- [ ] All `CombatSoundRegistry` dictionary keys and values use `SoundIds.X`
- [ ] All `GetEventSoundId`/`GetEffectSoundId` calls use `SoundIds.X`
- [ ] Test assertions use `SoundIds.X`
- [ ] No change to runtime behavior (string values remain identical)
- [ ] All 719 tests pass
