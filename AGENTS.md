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

## 1b. Critical test-failure analysis

**When a unit test fails, do NOT blindly modify it to make it pass.** Always ask: *"Is the test telling me the implementation introduced a bug?"*

- First, read the test and understand what contract it asserts.
- Then re-read your implementation to see if it violates that contract.
- If the implementation is wrong, fix the implementation.
- Only if the test is genuinely stale (referencing a removed feature, testing wrong behaviour) should you update the test — and document why.

**This is especially important with tests that pre-date changes:** a failing test is often the first signal that your change broke an existing invariant. Treat it as a valuable guard, not an inconvenience.

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
BattleArena.Gui             — Avalonia bridge GUI (depends on Application + Core + Presentation)
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

## 6. Combat system model — modern D&D opposed-roll, NOT THAC0

> **This is the authoritative rule. It overrides any older references to THAC0 or AD&D-style subtraction.**

BattleArena uses the **modern opposed-roll D&D model**. The old THAC0 system has been fully retired.

### What this means in practice

| Concept | Modern (current) | THAC0 (retired — do NOT use) |
|---------|-----------------|-------------------------------|
| Formula | `d20 + AttackPower ≥ d20 + DefensePower` (both sides roll) | single roll ≥ `THAC0 - AC` |
| StrikeRating | **Higher = better attacker** (`ClassAccuracyBase = StrikeRating`) | lower was better |
| ArmorClass | **Higher = more defensive** (`EffectiveAC = TotalArmorClass`) | lower was better |
| Level scaling | `LevelScaling = Level / 2` (attacker), `LevelDefenseBonus = Level` (defender) | single flat modifier |

### Hard rules — violations must be flagged and corrected

1. `ClassAccuracyBase` is always `attacker.StrikeRating` — **never** `20 - StrikeRating`.
2. `EffectiveAC` is always `equipment.TotalArmorClass` — **never** `20 - ArmorClass`.
3. "Higher StrikeRating = better attacker." Test names, comments, and design docs must use this framing.
4. "Higher ArmorClass value = more defensive." Plate Armor (AC 18) gives `EffectiveAC 18`, which is good.
5. `LevelingService.EffectiveStrikeRating` returns `StrikeRating + levelGain` — SR increases with level. "SR improved" means the value went **up**, not down.
6. Any code or document that says "lower SR is better", "20 - StrikeRating", or "20 - AC" is a THAC0 remnant and must be corrected immediately.

---

## 6b. Combat simulator rules

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
- **Namespace before usings**: in hand-written code only, declare the file-scoped namespace first, then `using` statements beneath it:
  ```csharp
  namespace BattleArena.Gui.Views;

  using Avalonia.Data.Converters;
  using System.Globalization;
  ```
  Never put `using` statements before the namespace declaration. Do not rearrange auto-generated files (`.feature.cs`, designer files, etc.).

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

## 12. GUI layer — Avalonia bridge (future: Unity target)

**CRITICAL: The GUI layer must be pluggable and unpluggable.** Combat rules and logic must NEVER live in the GUI. The GUI is a pure renderer — it reads state and presents it. All combat simulation, dice rolling, status effect resolution, damage calculation, and turn management stay in `BattleArena.Application` and `BattleArena.Core`. Swapping the GUI (Avalonia → Unity → web) must require zero changes to the simulation engine.

`BattleArena.Gui` is an **Avalonia** project that acts as a bridge GUI — it validates the `ICombatPresenter` contract today and proves the display pipeline end-to-end. The final production GUI target is **Unity** (or another open-source game engine). Avalonia is temporary bridgework, not the destination.

### 12.1 What survives the Avalonia → Unity migration

| Layer | Project | Survives? |
|-------|---------|-----------|
| `ICombatPresenter` | `BattleArena.Presentation` | ✅ Reimplemented in Unity as `UnityCombatPresenter` |
| `CombatPlaybackEngine` | `BattleArena.Presentation` | ✅ Unchanged — references `ICombatPresenter`, not Avalonia |
| `CombatDisplayState` / `CharDisplayState` | `BattleArena.Presentation` | ✅ Unchanged — pure data objects |
| `GuiDisplayConfig` + `gui-display-contract.json` | `BattleArena.Presentation` | ✅ Unchanged — JSON-driven field visibility |
| `CombatLayout` | `BattleArena.Presentation` | ✅ Unchanged |
| `CombatLogMerger` | `BattleArena.Presentation` | ✅ Unchanged |
| **Avalonia XAML views (`.axaml` files)** | `BattleArena.Gui` | ❌ Replaced by Unity UI canvas / prefabs |
| **Avalonia ViewModels** | `BattleArena.Gui` | ❌ Replaced by Unity `MonoBehaviour` components |
| **`AvaloniaCombatPresenter`** | `BattleArena.Gui` | ❌ Replaced by `UnityCombatPresenter` |

### 12.2 Hard rules — Avalonia bridge must remain thin

1. **Zero combat logic in Avalonia ViewModels.** ViewModels are thin wrappers that read from `CharDisplayState` — they must not compute combat outcomes, apply status effects, or make dice rolls. All simulation logic stays in `BattleArena.Application`.
2. **Zero business logic in `.axaml` code-behind.** No `if/else` that interprets combat state. That belongs in `AvaloniaCombatPresenter` or in `BattleArena.Presentation`.
3. **`ICombatPresenter` is the only rendering contract.** The `CombatPlaybackEngine` calls it; Avalonia implements it. When Unity arrives, you delete `AvaloniaCombatPresenter` and write `UnityCombatPresenter`. The engine and the presenter interface never change.
4. **Do not extend `CharDisplayState` speculatively.** Add fields only when the Avalonia presenter actually needs them to render. Unused fields in `CharDisplayState` are dead code that survives migration for no reason.
5. **`gui-display-contract.json` is authoritative.** Both Avalonia and future Unity presenter must respect `GuiDisplayConfig.IsFieldEnabled()`. If a field is disabled in the JSON, no renderer should show it.
6. **Demo is not the GUI.** The demo (`BattleArena.Demo`) remains the independent console showcase. The GUI project must not depend on or duplicate Demo code. Both consume `BattleArena.Presentation` and `ICombatPresenter`.

### 12.3 UI chrome state — badges, mode indicators, overlays

Non-character-display UI state (e.g. "API MODE" badge, combat mode label, speed indicator) must also survive the migration. The rules:

1. **`CombatDisplayState` carries UI chrome flags.** Add properties like `IsApiMode` directly to `CombatDisplayState` in `BattleArena.Presentation`. The presenter reads them from the state it already receives — no new interface methods needed.

2. **Avalonia bridge pattern:** The ViewModel mirrors the chrome flags so XAML can bind to them. The `AvaloniaCombatPresenter` copies `state.IsApiMode → _vm.IsApiMode` inside `ShowInitialScreen`. This keeps the XAML binding clean and the escape hatch simple.

3. **Unity migration:** `UnityCombatPresenter` reads `state.IsApiMode` the same way and shows/hides a Unity UI element. No changes to `CombatDisplayState` or `ICombatPresenter` — just delete `_vm.IsApiMode` along with the rest of `BattleArena.Gui`.

4. **Setup/pre-combat chrome is framework-specific.** Badges shown before `CombatDisplayState` exists (e.g. API mode badge on the character-selection screen) are driven by each framework's own code (Avalonia code-behind, Unity scene scripts). They do not need a shared contract — they are replaced during migration along with the rest of the screen.

5. **`gui-display-contract.json` is for character-display fields only.** Do **not** add UI chrome flags there. The contract controls which character stats are rendered (HP, Mana, DieRoll, etc.), not which badges are shown. Chrome flags belong on `CombatDisplayState`.

### 12.4 Migration workflow

```
Today:     AvaloniaCombatPresenter → ICombatPresenter → CombatPlaybackEngine
             (XAML views + ViewModels for validation only)

Migration: UnityCombatPresenter → ICombatPresenter → CombatPlaybackEngine
             (Canvas UI + MonoBehaviour components)
             │
             └── Delete: BattleArena.Gui entirely (all .axaml, ViewModels, AvaloniaCombatPresenter)
```

Steps when Unity arrives:
1. Create `BattleArena.Unity` project (or integrate into existing Unity solution)
2. Write `UnityCombatPresenter : ICombatPresenter` — maps `CharDisplayState` to Unity UI elements
3. Reference `BattleArena.Application` + `BattleArena.Presentation` from Unity
4. Call `CombatPlaybackEngine.PlayTurnBased(result, state, unityPresenter)` — same call Avalonia makes today
5. Delete `BattleArena.Gui` from solution

---

## 13. Project skills (opencode.jsonc)

Two project-scoped skills are registered in `opencode.jsonc`:

| Skill | File | When to load |
|-------|------|-------------|
| `makefile-orchestration` | `.opencode/skills/makefile-orchestration.md` | Docker builds, demo runs, test execution, container management |
| `combat-mechanics` | `.opencode/skills/combat-mechanics.md` | Combat system changes (attack, damage, TM, effects, resistance, logging) |

Load them explicitly via the skill tool. The combat-mechanics skill contains a `self-update-trigger` for automatic refresh when combat code changes.

---

## 14. Release notes (`release-notes.md`) update protocol

**Do not touch `release-notes.md` unless the user explicitly says "update the release-notes.md".**

When instructed to update it:

1. **Review first.** Read the current `release-notes.md` and assess the project state across all dimensions (combat mechanics, API+DB, tests, maintainability, Big-O, architecture).
2. **Update the maturity matrix** — re-score each dimension based on current state.
3. **Update the Next Features table** — re-prioritize based on what's been completed and what gaps remain. Remove done items, add new ones.
4. **Keep the format** — Markdown table with the same columns. The matrix and Next Features sections are the two required sections.

Format:

```markdown
# BattleArena — Maturity Assessment

| Dimension | Score | Key Strengths | Key Weaknesses |
|-----------|-------|---------------|----------------|
| ... | ... | ... | ... |
| **Overall** | **X/10** | ... | ... |

## Next Features

| Priority | Feature | Notes |
|----------|---------|-------|
| 1 | ... | ... |
```
