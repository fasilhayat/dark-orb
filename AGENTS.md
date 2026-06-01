# BattleArena — AI Assistant Instructions

> **Canonical source.** This file (`AGENTS.md`) is the single source of truth for project rules.
> It is read by **OpenCode** automatically.
> The copy at `.github/copilot-instructions.md` is read by **GitHub Copilot**.
> Edit only this file, then run `make sync-instructions` to propagate changes.

These rules apply to every coding task in this repository.
Follow them without being asked.

---

## 1. The absolute rule — never commit without approval

**Do not create git commits or push branches unless the user explicitly says so.**
Always stop after making file changes and let the user review the diff before committing.

---

## 2. Project vocabulary

| Term | Meaning | Example |
|------|---------|---------|
| **Combat** | A single simulated encounter between two parties | `CombatSimulator`, `CombatResult`, `CombatLogEntry` |
| **Battle** | A higher-level campaign concept (multiple combats, a war) | Reserved — not yet implemented |
| **BattleArena** | The overarching project / namespace name | Never rename this |

**Rules:**
- Use **Combat** for anything inside the simulation engine (classes, variables, comments, log messages, test names).
- Never rename `BattleArena.*` namespace or project names.
- When a feature refers to "a fight", call it a **combat**, not a battle.

---

## 3. Solution structure

```
BattleArena.Core            — Domain entities, enums, interfaces (no dependencies)
BattleArena.Application     — Services, interfaces, models (depends on Core only)
BattleArena.Infrastructure  — Repositories, DbContext (depends on Core + Application)
BattleArena.Api             — ASP.NET endpoints (depends on Application + Infrastructure)
BattleArena.Demo            — Console demo (depends on Application + Core)
BattleArena.UnitTests       — xUnit unit tests (NSubstitute for mocks)
BattleArena.AcceptanceTests — Reqnroll BDD acceptance tests
```

**Do not add project references that violate these dependency arrows.**
Core must not reference Application; Application must not reference Infrastructure.

---

## 4. Testing rules

### 4.1 When tests are required

| Work done | Required test(s) |
|-----------|-----------------|
| New service method or class | Unit test covering happy path + key edge cases |
| New status-effect behaviour | Unit test for `TryApply` / `TickAll` + acceptance scenario |
| New resistance source | Unit test for `ComputeResistance` with that source |
| New combat mechanic | Diagnostic test in `CombatDiagnosticTests` running a live sim |
| New API endpoint | At minimum one acceptance test or integration smoke test |
| Bug fix | A regression test that fails before the fix and passes after |

All tests must pass (`dotnet test BattleArena.sln`) before the task is considered done.
If a test count drops, investigate before proceeding.

### 4.2 Unit tests — `BattleArena.UnitTests`

Location rules:
- Service logic → `Services/<ServiceName>Tests.cs`
- Simulation diagnostics (live dice, full sim) → `Diagnostics/CombatDiagnosticTests.cs`
- No other top-level folders.

Mocking rules:
- **Always** mock `IDiceService` when testing methods that roll dice (use `NSubstitute`).
- **Never** mock `CombatSimulator`; wire up the full real service stack for diagnostic tests.
- Use `Substitute.For<IDiceService>()` and `.Returns(value1, value2, ...)` to control roll sequences.

Naming convention: `MethodName_Condition_ExpectedOutcome`
Examples:
- `TryApply_ZeroResistance_AlwaysAppliesWhenChancePasses`
- `ComputeResistance_WrongResistanceType_ReturnsZero`
- `TickAll_EffectAtDurationOne_RemovesEffectAndReturnsName`

### 4.3 Acceptance tests — `BattleArena.AcceptanceTests`

Framework: **Reqnroll** (BDD / Gherkin).

Location rules:
- Feature files → `Features/<FeatureName>.feature`
- Step definitions → `StepDefinitions/<FeatureName>Steps.cs`
- Auto-generated `*.feature.cs` files are **never edited manually** (regenerated on build).

Scenario rules:
- Each feature file covers one cohesive concern (e.g., `StatusEffects.feature`, `Resistance.feature`).
- Scenario names must be plain English, readable by a non-developer.
- Probabilistic scenarios (dice-based) must use conservative bounds:
  - With p=0.8 resistance and 100 trials → assert **≥ 60** resisted (not ≥ 80).
  - With p=0 resistance and 20 trials → assert **all 20** landed.

Step definition rules:
- Namespace: `BattleArena.ReqnrollTests.StepDefinitions`
- Use real services (`DiceService`, `StatusEffectService`, etc.) unless the scenario specifically tests isolation.
- Shared character-setup steps must match the same Gherkin pattern across all feature files.

### 4.4 Coverage expectations

| Project | Target |
|---------|--------|
| `BattleArena.Application/Services` | ≥ 80 % line coverage |
| `BattleArena.Core/Entities` | Key methods (e.g., `ComputeResistance`) must have ≥ 1 dedicated test per source |
| `BattleArena.Application/Interfaces` | Every interface must have ≥ 1 test exercising its contract |

Run coverage locally with:
```bash
dotnet test BattleArena.sln --collect:"XPlat Code Coverage" --results-directory coverage
```

---

## 5. Resistance system rules

When adding or changing the resistance system:

1. `Character.ComputeResistance(ResistanceType)` is the **single source of truth**.  
   It sums: race feats + equipped armor + active status-effect buffs. Do not duplicate this logic elsewhere.

2. Resistance is **capped at 95** (always at least 5 % infliction chance).

3. `StatusEffectService.TryApply` is the **only** place where the two-phase infliction roll happens:  
   - Phase 1: `D100 > ApplicationChance` → quiet miss (no log event)  
   - Phase 2: `D100 ≤ resistance` → `EffectResisted` log event  
   - Otherwise: `Apply()` → `EffectApplied` log event

4. New status effects must declare `ResistanceType` explicitly (do not rely on the Magic default for elemental effects).

5. Protective spell buffs that grant resistance must set `ResistanceBonuses` on the `StatusEffect`, not hard-code values in the simulator.

---

## 6. Combat simulator rules

- `CombatSimulator` depends on `ICombatService`, `ITurnmeterService`, `IStatusEffectService`, `IDiceService`.
- All combat-log events use `CombatLogEntry` with an `EventType` string field. Stick to the established event types:

| EventType | Meaning |
|-----------|---------|
| `TurnMeterGain` | TM increased this tick |
| `TurnStart` | Actor begins their turn |
| `Attack` | Hit or miss resolved |
| `Damage` | HP reduced |
| `SkippedTurn` | CC'd actor cannot act |
| `EffectApplied` | Status effect landed |
| `EffectResisted` | Resistance roll blocked the effect |
| `EffectExpired` | Duration reached zero |
| `DoTDamage` | Damage-over-time tick |
| `FumblePenalty` | Fumble side-effect applied |
| `Death` | HP ≤ -10 |
| `KnockedOut` | HP in range -9 to 0 |
| `PerfectParry` | Defender deflects attack (also on both-20), gains TM bonus |
| `DevastatingStrike` | Triple-damage hit (atk=20 vs def=1) |
| `TotalReversal` | Fumble flipped; defender gains TM, attacker penalised harder (atk=1 vs def=20) |

- **Never add game logic to `BattleArena.Demo`**. The demo may read game state and render it; it must not compute combat outcomes.
- **API combat endpoint**: `POST /v1/combat/simulate` accepts `{ heroParty, enemyParty, maxTicks, heroTargetStrategy, enemyTargetStrategy }` and returns a `CombatResult`. The demo calls this endpoint when `UseApiRoster && ApiClient is not null` — the entire simulation runs server-side.
- **IAttackSource** must use `[JsonDerivedType]` for polymorphic serialization (`weapon`, `spell`, `unarmed` discriminators) — required by the combat simulate endpoint.
- **Combat modifier pipeline**: `ICombatModifier` implementations are registered at DI startup and applied by `CombatService.ResolveAttack`. Priority bands: 10 = positional/range, 20 = environmental, 30 = item/set bonuses. Context carries `AttackPowerDelta` and `DefensePowerDelta`. Add new modifiers by implementing `ICombatModifier` — no changes to `CombatService` needed.
- **Diagnostic test armor**: armor values used in `CombatDiagnosticTests` must come from `BattleArena.UnitTests.TestData.ArmorCatalog`, which mirrors `02-seed-data.sql`. Update `ArmorCatalog.cs` when SQL seed values change.

---

## 7. README.md update obligations

Update **`README.md`** when any of the following changes:

| Change | Section to update |
|--------|-------------------|
| New project added to solution | `## Solution overview → Projects` |
| New API endpoint group | `## API surface` |
| New DB table or stored function | `## Database model` and the Mermaid ER diagram |
| New run command or Makefile target | `## Running the solution locally` |
| New Docker service | `## Running the solution locally → Option 1` |
| Test count or framework changes | `## Testing` |

The Mermaid ER diagram must stay in sync with `src/.postgres-init/01-schema.sql`.
If you add a table to the SQL file you must add the entity and its relationships to the diagram.

---

## 8. Lore update obligations (`design/battle-arena-lore.md`)

Update **`design/battle-arena-lore.md`** when any of the following are added to the SQL seed or game data:

| Added content | Lore section |
|---------------|-------------|
| New race or subrace | `## 1. Races` (add stat block, flavour text, available classes, special abilities) |
| New class | `## 2. Classes` |
| New deity | `## 3. Deities` |
| New pet | `## 4. Pets` |
| New weapon (any quality) | Matching quality section (§5–9) |
| New armor piece | Matching quality section (§10–14) |
| New ring / amulet / girdle | `## 15–17` |
| New item set | `## 18. Item Sets` |
| New NPC | `## 19. NPCs` |
| New spell | `## 20. Spells` |
| New subrace | `## 21. Subraces & Special Abilities` |
| XP formula or levelling change | `## 22. Leveling & Experience` |

Lore entries must match what is seeded in the database:
- Stat bonuses in the table must match `race.*_bonus` columns.
- Special abilities must match `race_special_ability` rows.
- Spell descriptions must match `spell.description` in the DB.

---

## 9. Code style

- **Comments**: only when the code is non-obvious. Do not restate what the code already says.
- **Naming**: PascalCase for types/methods, camelCase for locals/fields, `_camelCase` for private fields.
- **File-per-type**: one public type per file. Partial classes (like `Demo.*`) are the exception.
- **No magic numbers**: extract named constants or use enum values.
- **Async**: use `async`/`await` throughout the demo display pipeline; the simulator itself is synchronous.
- **Collections**: use `List<T>` for mutable, `IReadOnlyList<T>` for returned collections.
- **Cyclomatic complexity**: ≤ 10 per method (modified McCabe — counts each `&&`/`||` as +1). Extract private helpers rather than letting any method exceed this limit. Values of 11–12 are acceptable only where splitting would add parameters without reducing real complexity.

---

## 10. Demo partial class structure

The demo (`BattleArena.Demo`) is split into:

| File | Responsibility |
|------|---------------|
| `Demo.Main.cs` | Entry point, top-level menu |
| `Demo.Menus.cs` | Mode selection, option prompts |
| `Demo.Combat.cs` | Wiring up simulator runs, event subscriptions |
| `Demo.Display.cs` | Rendering helpers (colours, banners, character cards) |
| `Demo.Data.cs` | Hardcoded character/weapon/spell data |

**Do not add rendering logic to `Demo.Combat.cs`.**
**Do not add combat wiring to `Demo.Display.cs`.**

---

## 11. Docker rules

- The `battle-arena-demo` service uses `profiles: [demo]` — it does not start with a plain `docker compose up`.
- Use `make up-dev` to build and run everything (DB + API + demo) in Docker.
- Use `make up-local` to start only DB + API (demo runs on host via `make demo-local`).
- Do not run docker commands without the user's explicit instruction.

---

## 12. Project skills (opencode.jsonc)

Two project-scoped skills are registered in `opencode.jsonc`:

| Skill | File | When to load |
|-------|------|-------------|
| `makefile-orchestration` | `.opencode/skills/makefile-orchestration.md` | Docker builds, demo runs, test execution, container management |
| `combat-mechanics` | `.opencode/skills/combat-mechanics.md` | Combat system changes (attack, damage, TM, effects, resistance, logging) |

Load them explicitly via the skill tool. The combat-mechanics skill contains a `self-update-trigger` for automatic refresh when combat code changes.
