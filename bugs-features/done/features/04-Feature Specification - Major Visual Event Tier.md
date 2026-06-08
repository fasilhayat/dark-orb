# Feature Specification - Major Visual Event Tier

Project: Dark Orb

File: `feature-major-visual-event-tier.md`

---

## Objective

Introduce a **middle visual event tier** between Normal (fire-and-forget) and Incredible (blocking) for **upgraded spells** — spells whose power scales with the caster's level significantly above the spell's base level.

When a spell qualifies as "upgraded," its visual presentation is promoted from Normal to **Major**: larger overlay, brighter color, longer duration, but still non-blocking (the playback engine continues without waiting).

---

## Tier Definitions

| Tier | Bus method | Blocks engine? | Visual treatment | Used for |
|------|-----------|----------------|-----------------|----------|
| **Normal** | `PublishNormal` | No | Standard overlay, ~1–1.2s | Regular attacks, DoT ticks, normal spells |
| **Major** | `PublishMajor` (new) | No | Larger overlay, ~1.8–2s, brighter/golden tint, optional sound emphasis | Upgraded spells (caster level significantly above spell level) |
| **Incredible** | `PublishIncredible` | Yes (waits for animation) | Cinematic, ~2.5s+, blocking | Boss transitions, killing blows (future) |

---

## Upgrade Condition

A spell qualifies as **upgraded** (and emits a Major event instead of Normal) when:

```
caster.Level >= spell.SpellLevel + UpgradeThreshold
```

Where `UpgradeThreshold` is a tunable constant (initially **5**).

Examples with threshold = 5:

| Spell Level | Caster Level | Upgraded? |
|-------------|-------------|-----------|
| 1 | 6+ | Yes |
| 2 | 7+ | Yes |
| 3 | 8+ | Yes |
| 5 | 9 | No (9 < 10) |
| 5 | 10+ | Yes |

---

## Visual Behavior

### Normal spell cast (not upgraded)
- Current behaviour: spell overlay text, standard color, 1200ms duration

### Upgraded spell cast (Major event)
- Overlay text: `"{SPELL} ✦"` (gold star suffix)
- Color: Gold gradient (`#ffdd44`)
- Duration: 1800ms
- Scale: 1.3× normal overlay size
- Optional: distinct sound effect ("SpellUpgrade")

---

## Pipeline Changes Required

### 1. `VisualEventBus` — new `PublishMajor` method
- Non-blocking (like Normal)
- Separate `MajorEventPublished` event
- `AvaloniaCombatPresenter` subscribes with `OnMajorVisualEvent`

### 2. `CombatPlaybackEngine.EmitVisualEvents`
- `Attack` event with `IsSpell == true`: check if the spell qualifies as upgraded
- If upgraded: call `bus.PublishMajor(...)` instead of `bus.PublishNormal(...)`
- Pass spell level / caster level info (via `CombatLogEntry` or parameter)

### 3. `AvaloniaCombatPresenter`
- `OnMajorVisualEvent`: larger overlay scale, longer animation, gold color
- Same `FlashBorder` and `AnimateOverlay` pattern but with elevated parameters

### 4. `CombatLogEntry` (optional)
- Add `IsUpgradedSpell` boolean field if the playback engine needs to know without computing it

### 5. `VisualEvent`
- Add `IsMajor` flag (or the presenter infers it from being on the Major event)

---

## Upgrade Decision Location

The upgrade check must happen in **one place only** — the `CombatSimulator` (source of truth) or `CombatPlaybackEngine` (presentation layer).

**Recommended: `CombatPlaybackEngine`** — keeps game logic separate from visual classification. The engine checks:

```csharp
bool IsUpgraded(CombatLogEntry entry) =>
    entry.IsSpell &&
    entry.SpellLevel.HasValue &&
    entry.CasterLevel.HasValue &&
    entry.CasterLevel.Value >= entry.SpellLevel.Value + UpgradeThreshold;
```

But this requires adding `SpellLevel` and `CasterLevel` fields to `CombatLogEntry`.

**Alternative: `CombatSimulator`** stamps `IsUpgradedSpell = true` on the `TurnStart`/`Attack` log entry. The playback engine reads it without computing.

---

## Integration Points

| File | Change |
|------|--------|
| `BattleArena.Presentation/VisualEventBus.cs` | Add `PublishMajor`, `MajorEventPublished` |
| `BattleArena.Presentation/VisualEvent.cs` | No changes needed (color/duration already exist) |
| `BattleArena.Presentation/CombatPlaybackEngine.cs` | Add `IsUpgraded` check in `Attack` case; call `PublishMajor` |
| `BattleArena.Application/Models/CombatLogEntry.cs` | Add `IsUpgradedSpell` (or `SpellLevel` + `CasterLevel`) |
| `BattleArena.Gui/Presenters/AvaloniaCombatPresenter.cs` | Subscribe `OnMajorVisualEvent` with larger scale + gold color |
| `BattleArena.Presentation/CombatSoundRegistry.cs` | Add `SpellUpgrade` sound mapping |
| `BattleArena.Presentation/CombatPlaybackEngine.cs` | Add `SpellUpgrade` sound emission for Major events |
| `.opencode/skills/combat-mechanics.md` | Update visual event pipeline section |

---

## Constraints

- Major events must remain **non-blocking** (do NOT use `ManualResetEventSlim`)
- Must not modify combat logic or damage calculation
- Must be purely presentational
- Must be deterministic in replay mode

---

## Acceptance Criteria

- [x] `VisualEventBus` has `PublishMajor` method + `MajorEventPublished` event
- [x] `CombatPlaybackEngine` emits Major events for upgraded spells instead of Normal
- [x] Upgrade check uses caster level vs spell level with configurable threshold (UpgradeThreshold = 5)
- [x] Major overlay is visually distinct (gold `#ffdd44`, larger scale 0.4→3.0, 1800ms)
- [x] Major events do NOT block the playback pipeline
- [x] Non-upgraded spells remain Normal (unchanged behaviour)
- [x] Zero-cost / non-spell attacks unaffected
- [x] Fully deterministic in replay

---

## Implementation Summary

### VisualEventBus
- Added `PublishMajor(VisualEvent)` — non-blocking (like Normal), fires `MajorEventPublished` event
- `AvaloniaCombatPresenter` subscribes/unsubscribes alongside Normal/Incredible

### CombatLogEntry
- Added `SpellLevel` (int?) and `CasterLevel` (int?) fields — stamped on `TurnStart` and `Attack` events

### CombatSimulator
- `TurnStart` entry now includes `SpellLevel` (when `setup.Source is Spell`) and `CasterLevel` (actor's level)
- `BuildAttackEntry` — extended signature with `spellLevel`/`casterLevel` parameters; call site passes values

### CombatPlaybackEngine
- `IsUpgradedSpell(CombatLogEntry)` — checks `casterLevel >= spellLevel + UpgradeThreshold (5)`
- `EmitVisualEvents` — Attack case: if upgraded, calls `PublishMajor` with gold `#ffdd44`, `✦` star suffix, 1800ms; otherwise continues as Normal
- `EmitCombatSounds` — upgraded spells play `SpellUpgrade` sound instead of generic `SpellCast`

### CombatSoundRegistry
- Added `SpellUpgrade` sound ID → description: "Resonant surge of amplified arcane energy"

### AvaloniaCombatPresenter
- `OnMajorVisualEvent` — flashes borders, triggers overlay with `AnimateMajorOverlay`
- `AnimateMajorOverlay` — 1.8s duration, scale 0.4→3.0, opacity fades after 12% of animation

### Tests
- `Attack_HighLevelCasterLowLevelSpell_StampsSpellAndCasterLevel` — verifies SpellLevel/CasterLevel on TurnStart and Attack entries
- `Attack_WeaponAttack_DoesNotHaveSpellLevel` — verifies null SpellLevel for non-spells
- `VisualEventBus_PublishMajor_FiresMajorEvent` — verifies Major event bus propagation
- All **705 tests pass** (585 unit + 120 acceptance)
