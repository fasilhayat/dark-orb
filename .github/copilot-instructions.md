# BattleArena — AI Assistant Instructions (GitHub Copilot)

> **Mirrored file.** The canonical source is `AGENTS.md` at the repository root (read by OpenCode).
> This copy exists solely because GitHub Copilot reads `.github/copilot-instructions.md`.
> Edit `AGENTS.md`, then run `make sync-instructions` to update this file.

---

## 1. Never commit without approval

**Do not create commits or push branches unless the user explicitly says so.**

---

## 2. Test-failure analysis

**When a test fails, do NOT blindly modify it to make it pass.** First understand what contract it asserts, then re-read your implementation. Fix the implementation if it violated the contract. Only update the test if it is genuinely stale (testing wrong behaviour, referencing removed feature).

---

## 3. Project vocabulary

| Term | Meaning | Example |
|------|---------|---------|
| **Combat** | A single simulated encounter | `CombatSimulator`, `CombatResult`, `CombatLogEntry` |
| **Battle** | Higher-level campaign concept (multiple combats) | Reserved — not yet implemented |
| **BattleArena** | Overarching namespace | Never rename this |

Use **Combat** for simulation engine code. Never call a fight a "battle" in code.

---

## 4. Solution structure

```
Core               — Domain entities, enums, interfaces (no dependencies)
Application        — Services, interfaces, models (depends on Core only)
Infrastructure     — Repositories, DbContext (depends on Core only)
Api                — ASP.NET endpoints (depends on Application + Infrastructure)
Demo               — Console demo (depends on Application + Core + Presentation)
Presentation       — GUI-agnostic playback engine, ICombatPresenter (depends on Core + Application)
Gui                — Avalonia bridge GUI (depends on Application + Core + Presentation)
UnitTests          — xUnit + NSubstitute
AcceptanceTests    — Reqnroll BDD
```

**Do not violate dependency arrows.** Core must not reference Application; Application must not reference Infrastructure.

---

## 5. Testing

### Required tests per change

| Change | Test |
|--------|------|
| New service method | Unit test (happy path + edge cases) |
| New status-effect behaviour | Unit test for `TryApply` / `TickAll` + acceptance scenario |
| New resistance source | Unit test for `ComputeResistance` with that source |
| New combat mechanic | Diagnostic test in `CombatDiagnosticTests` (live sim) |
| New API endpoint | Acceptance or integration smoke test |
| Bug fix | Regression test that fails before fix, passes after |

All tests must pass (`dotnet test BattleArena.sln` from `src/`). If test count drops, investigate.

### Unit test rules

- Service tests → `Services/<Name>Tests.cs`. Diagnostics → `Diagnostics/CombatDiagnosticTests.cs`.
- **Always** mock `IDiceService` when testing methods that roll dice (`NSubstitute`).
- **Never** mock `CombatSimulator` — wire full real stack for diagnostics.
- Naming: `MethodName_Condition_ExpectedOutcome`.

### Acceptance test rules (Reqnroll)

- Features → `Features/<Name>.feature`. Steps → `StepDefinitions/<Name>Steps.cs`.
- `*.feature.cs` is auto-generated — never edit manually.
- Namespace: `BattleArena.ReqnrollTests.StepDefinitions`.
- Use real services unless scenario tests isolation.
- Dice-based scenarios use conservative bounds (p=0.8 with 100 trials → assert ≥ 60).

### Coverage

```bash
make test-coverage    # from src/ — inline coverlet properties (OpenCover format). Pass --settings coverlet.runsettings to get exclusions.
```

Alternative: `dotnet test BattleArena.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`

Single test project: `dotnet test BattleArena.sln --project BattleArena.UnitTests/BattleArena.UnitTests.csproj`
Single test: `dotnet test BattleArena.sln --filter "FullyQualifiedName~TestMethodName"`

Targets: Application/Services ≥ 80 %, Core/Entities key methods ≥ 1 dedicated test per source, every interface tested.

---

## 6. Combat system — modern D&D opposed-roll, NOT THAC0

**This is the single most important domain rule.** BattleArena uses modern opposed-roll D&D:

| Concept | Correct | Wrong (THAC0 — never use) |
|---------|---------|---------------------------|
| Formula | `d20 + AttackPower ≥ d20 + DefensePower` | single roll ≥ `THAC0 - AC` |
| StrikeRating | Higher = better attacker | lower was better |
| ArmorClass | Higher = more defensive | lower was better |

Hard rules:
- `ClassAccuracyBase = attacker.StrikeRating` — never `20 - StrikeRating`.
- `EffectiveAC = equipment.TotalArmorClass` — never `20 - AC`.
- `LevelingService.EffectiveStrikeRating` returns `StrikeRating + levelGain`.
- Any code/document saying "lower SR is better" or using `20 - X` is a THAC0 remnant — flag and fix.

---

## 7. Combat engine rules

### Event types

`EventType` is a plain string on `CombatLogEntry` — no enum. Common types:

`RoundStart`, `RoundEnd`, `TurnMeterGain`, `TurnStart`, `TurnEnd`, `Attack`, `Damage`, `SkippedTurn`, `EffectApplied`, `EffectResisted`, `EffectExpired`, `EffectReflected`, `DoTTick`, `HoTTick`, `LeechTick`, `Healed`, `FumblePenalty`, `Death` (HP ≤ -10), `KnockedOut` (HP -9–0), `PerfectParry`, `DevastatingStrike`, `TotalReversal`, `Clash`, `ManaDeduct`, `ManaRegen`, `SpellQueued`, `SpellCharging`, `PetSummoned`, `PetExpired`, `Resurrection`.

Check `Application/Models/CombatLogEntry.cs` for all fields.

### Key constraints

- **Never add game logic to `Demo`.** It may read state and render; it must not compute outcomes.
- **IAttackSource** needs `[JsonDerivedType]` discriminators (`weapon`, `spell`, `unarmed`) for the `POST /v1/combat/simulate` endpoint.
- **Combat modifier pipeline**: implement `ICombatModifier` and register at DI. Priority bands: 10 = positional/range, 20 = environmental, 30 = item/set. No changes to `CombatService` needed.
- **Diagnostic test armor** must come from `UnitTests.TestData.ArmorCatalog` (mirrors `02-seed-data.sql`). Update both in sync.

### Resistance system

- `Character.ComputeResistance(ResistanceType)` is the single source of truth (race feats + armor + buffs).
- Capped at 95 (always ≥ 5 % infliction chance).
- `StatusEffectService.TryApply` is the only two-phase infliction roll: (1) D100 > ApplicationChance → quiet miss, (2) D100 ≤ resistance → `EffectResisted`, else `EffectApplied`.
- New effects must declare `ResistanceType` explicitly. Buffs granting resistance use `ResistanceBonuses` on the effect, not hard-coded values in the simulator.

---

## 8. GUI is a pure renderer

The GUI (`BattleArena.Gui` — Avalonia bridge) must never contain combat logic. All simulation, dice rolling, damage, status effects, and turn management stay in `Application` and `Core`. The production target is Unity; Avalonia is bridgework.

`ICombatPresenter` (in `Presentation`) is the only rendering contract. `BattleArena.Presentation` contains the playback engine, display state, and JSON-driven field visibility — these survive the Avalonia→Unity migration unchanged.

---

## 9. Code style (non-obvious conventions)

- **Namespace before usings** — in hand-written code only:
  ```csharp
  namespace BattleArena.Foo;

  using System.Globalization;
  ```
- **Cyclomatic complexity** ≤ 10 per method (modified McCabe — `&&`/`||` counts as +1). 11–12 acceptable only where splitting would add parameters without reducing real complexity.
- **One public type per file** (partial classes like `Demo.*` are the exception).
- **No magic numbers** — named constants or enums.
- Simulator itself is synchronous; `async`/`await` is for display pipeline only.
- `IReadOnlyList<T>` for returned collections.

---

## 10. Skills

Project-scoped skills live in `.opencode/skills/` and are auto-discovered:

| Skill | File | When to load |
|-------|------|-------------|
| `makefile-orchestration` | `.opencode/skills/makefile-orchestration.md` | Docker, demo runs, tests, container mgmt |
| `combat-mechanics` | `.opencode/skills/combat-mechanics.md` | Changes to attack, damage, TM, effects, resistance, logging |
| `combat-order` | `.opencode/skills/combat-order.md` | Turn meter timeline or acting-order questions |
| `combat-log-analysis` | `.opencode/skills/combat-log-analysis.md` | User reports unexpected combat behaviour |
| `work-intake` | `.opencode/skills/work-intake.md` | Processing numbered files in `bugs-features/` |

Load via OpenCode's skill tool. `combat-mechanics` has `self-update: true` — it auto-refreshes when combat code changes.

---

## 11. Docker quick reference (from `src/`)

| Command | What starts |
|---------|-------------|
| `make up-local` | DB + API in Docker (ports 5432, 8585). Demo on host via `make demo-local` |
| `make up-dev` | Build demo in Release, start DB + API + demo (interactive `run --rm`) |
| `make dev-up` | Alias for `up-dev` (build, start, run demo) |
| `make run-dev` | Re-run demo container only (DB + API must already be up) |
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Tests with OpenCover format |
| `make gui-local` | Run Avalonia GUI standalone (no DB required) |
| `make install` | Full cycle: clean Docker → test → up-local → demo |
| `make install-dev` | Full dev setup: clean + dotnet clean + test + dev-up |
| `make clean-logs` | Delete generated `combat-logs/` files |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |

`battle-arena-demo` uses `profiles: [demo]` — not started by plain `docker compose up`.

**Docker build strategy**: `dotnet publish` runs on the host, then Docker `COPY`s the pre-built output. No NuGet restore inside containers. Do NOT add `dotnet restore`/`dotnet build` steps to the Dockerfile.

**Dev demo** (`docker-compose.dev.yml`) mounts `combat-logs/` from the repo root into the demo container — logs survive container teardown.

---

## 12. Tooling non-obvious facts

- **No EF Core** — Data access uses raw Npgsql + a custom `DbContext` wrapper. Do not write Entity Framework code.
- **No CI configured** — `.github/workflows/` is empty. The agent must not rely on CI to catch issues; run `dotnet test` locally.
- **No `opencode.json`** — This repo has no OpenCode config file. Instructions come solely from `AGENTS.md`.
- **API requires `X-Api-Key` header** — All endpoints except `/swagger` and `/api/healthcheck` require an API key. Default: `BA-DEV-2024-SECRET`. 500 errors return JSON `{"error":"..."}`.
- **Swagger only in Development/LocalDev** — `app.UseSwagger()` is gated on `IsDevelopment() || IsEnvironment("LocalDev")`.
- **SQL init files** — `.postgres-init/` contains: `01-schema.sql`, `02-seed-data.sql`, `03-characters.sql`, `04-bestiary.sql`. Keep seed data in sync with design docs.
- **`design/docs/`** — Contains game design docs organized into `world/`, `reference/`, and `systems/`. Keep in sync with SQL seed data.
- **`bugs-features/`** — Numbered files represent pending work. Process them in ascending numeric order, moving to `bugs-features/done/` when complete. Load the `work-intake` skill for the full workflow.
- **`scripts/generate-sounds.ps1`** — Generates placeholder WAV files for GUI combat sound effects (installed in `BattleArena.Gui/Assets/Sounds/`).
- **No `Directory.Build.props`** — Each `.csproj` sets its own SDK version, nullable, ImplicitUsings. No central package management.

---

## 13. Doc update obligations

- **README.md**: update when new project, API endpoint, DB table, Makefile target, Docker service, or test framework change. Keep Mermaid ER diagram in sync with `01-schema.sql`.
- **design/docs/**: update `docs/world/lore.md`, `docs/reference/deities.md`, `docs/reference/equipment.md`, `docs/reference/pets.md`, `docs/reference/npcs.md`, `docs/reference/spells.md`, `docs/reference/bestiary.md`, or `docs/systems/leveling-plan.md` when SQL seed adds races, classes, deities, pets, weapons, armor, accessories, item sets, NPCs, spells, subraces, or XP formula changes. Entries must match the DB exactly.
- **release-notes.md**: do NOT touch unless the user explicitly asks. The file is managed manually and has its own maturity-assessment format.
