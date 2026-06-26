# Task — Combat Balance Fixes

## Summary

Implemented six balance changes based on combat log analysis of Ser_Garrick_Dawnshield_vs_Lord_Aethor_Valeborn_party. The fight revealed that Plate Armor (Mit5) with level-scaling (`1.0 + Lv/10`) was so punishing that physical weapons dealt 0–1 damage, and dagger users literally could not damage plate wearers.

## Changes

### 1. Logger fix — display scaled mitigation
**File**: `src/BattleArena.Application/Services/CombatService.cs` (line 259)
- `DamageContext.ArmorMitigation` now stores the **scaled** mitigation value (with elemental half-bypass) instead of the raw equipment value
- The log line now shows `mit(N)` where N is the actual value used in the formula

### 2. Reduce mitigation scaling factor
**File**: `src/BattleArena.Application/Services/CombatService.cs` (line 247)
- Changed from `1.0 + defender.Level / 10.0` to `1.0 + defender.Level / 20.0`
- At Lv10: mitigation is now 1.5x base (was 2.0x)
- At Lv20: mitigation is now 2.0x base (was 3.0x)

### 3. Reduce Plate Armor mitigation
**File**: `src/.postgres-init/02-seed-data.sql` (line 907)
- Plate Armor mitigation reduced from 5 to 4

### 4. Elemental damage types half-bypass physical mitigation
**File**: `src/BattleArena.Application/Services/CombatService.cs` (lines 248–249)
- Damage types `Bludgeoning`, `Piercing`, `Slashing` (physical) face full mitigation
- All other types (`Fire`, `Ice`, `Lightning`, `Holy`, `Shadow`, `Poison`, `Acid`, `Psychic`) face only 50% mitigation
- This makes spells naturally effective against heavy armor, without needing a separate armor penetration stat

### 5. Finesse weapons use best of STR/DEX
**Files**: 
- `src/BattleArena.Core/Entities/IAttackSource.cs` — added `bool IsFinesse { get; }`
- `src/BattleArena.Core/Entities/Weapon.cs` — added `IsFinesse` auto-property
- `src/BattleArena.Core/Entities/Spell.cs` — added `IsFinesse => false`
- `src/BattleArena.Core/Entities/UnarmedStrike.cs` — added `IsFinesse => false`
- `src/BattleArena.Application/Services/MoveIntent.cs` — added `IsFinesse => false`
- `src/BattleArena.Application/Services/CombatService.cs` — finesse weapons use `Math.Max(STR, DEX)` for damage modifier

Dagger/ShortSword users (Finnick, Vex with STR 8) now get their damage from DEX instead of STR.

### 6. Minimum damage floor of 1
**File**: `src/BattleArena.Application/Services/CombatService.cs` (lines 103, 248)
- Changed `Math.Max(0, ...)` to `Math.Max(1, ...)` in both normal and DevastatingStrike paths
- Healing already had `Math.Max(1, ...)`

## Updated tests

### Unit tests
- **CombatServiceTests.cs**: `ResolveAttack_DamageCannotGoBelowZero` now expects 1 instead of 0
- **CombatBenchmarkTests.cs**: Updated Plate Armor references (5→4), added `ExpectHigherWins` flag for the Infernal Commander vs Golem matchup (spellcaster beating pure melee is expected after elemental bypass)
- All 609 unit tests pass

### Acceptance tests (Reqnroll)
- **Combat.feature**: "Damage cannot go below zero" scenario now expects 1 (floor) instead of 0
- **DamageFormula.feature**: Updated scenario description and expected value (0→1)
- **CombatBalance.feature**: Updated scaled mitigation expectation from 12 to 6
- **CombatBalanceSteps.cs**: Updated scaling formula from `/10.0` to `/20.0`, Plate from 5 to 4
- All 131 acceptance tests pass
