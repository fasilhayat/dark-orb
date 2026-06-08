# Balance and AI Tuning — Shock, Luna Armor, AI Tactics

Project: Dark Orb

Priority: Medium

Type: Balance / AI Improvement

---

## Problem

Analysis of Vaelith Moonveil (Lv9) vs High Priestess Luna (Lv12) combat revealed several balance issues:

1. **Shock status effect was too punishing** — 100% application chance halved turn meter gain, creating an unwinnable death spiral once applied
2. **Luna had wrong equipment** — combat used Chain Mail (AC 16), design doc said Padded Armor (AC 11), player wanted Plate Armor
3. **AI made poor tactical choices** — prioritized healing at 70% HP threshold, picked weak spells like Bless (0d4), never used melee weapon

---

## Changes

### 1. Shock TM Penalty Reduction

**File:** `src/BattleArena.Application/Services/TurnmeterService.cs`

Changed from: `gain = Math.Max(1, gain / 2)` (halved TM gain)
Changed to: `gain = Math.Max(1, gain - gain / 3)` (33% reduction)

The death spiral from halved TM gain + repeated Shock application made it impossible for affected characters to recover. A 33% reduction still penalizes without completely shutting down the target.

### 2. Luna's Equipment Upgrade

**Files:**
- `src/BattleArena.Gui/Data/roster.json` — changed chest from `"Chain Mail"` to `"Plate Armor"`
- `design/NPC/npc-characters.md` — updated AC from 11 to 18, equipment entry

Plate Armor: AC 18, Mitigation 5, no DEX bonus (Luna has DEX 12, so minimal loss).

### 3. AI Tactics Improvements

**File:** `src/BattleArena.Application/Services/AutoActionDecisionSource.cs`

Changes:
- **Heal threshold lowered** from 70% to 40% — AI no longer panic-heals when slightly scratched
- **Zero-damage spells filtered** — spells with `DamageCount <= 0` and no on-hit effects are skipped (prevents Bless spam)
- **Single-target heal priority** — prefers single-target heals over group heals when only one ally needs healing
- **Weapon priority increased** — equipped weapon is checked before `defaultAttack`, ensuring spellcasters with melee weapons use them when out of mana

---

## Acceptance Criteria

- [x] Shock TM penalty changed from `/2` to `- gain/3` (33% reduction)
- [x] Luna's chest armor changed to Plate Armor (AC 18, Mit 5)
- [x] NPC design doc updated to reflect plate mail
- [x] AI heal threshold lowered to 40%
- [x] Zero-damage spells with no on-hit effects are filtered out
- [x] AI prefers single-target heals when only one ally injured
- [x] AI uses equipped weapon when no affordable spells remain
- [x] All 583 non-pre-existing tests pass
- [x] Build succeeds with 0 errors

## Pre-existing Failures

2 summon tests were failing before this change and remain failing:
- `Simulate_SummonSpell_EmitsPetEventsAndExpiresAtRoundEnd`
- `Simulate_SummonedPet_PrefersLastAttackerOfMaster`
