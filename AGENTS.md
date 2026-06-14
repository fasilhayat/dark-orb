# BattleArena — AI Assistant Instructions

> Canonical source. Edit only this file, then run `make sync-instructions` from `src/` to copy to `.github/copilot-instructions.md`.

## 1. Never commit without approval

**Do not create commits or push branches unless the user explicitly says so.**

## 2. Test-failure analysis

When a test fails, understand what contract it asserts before modifying it. Fix the implementation if it violated the contract. Only update the test if it is genuinely stale.

## 3. Project vocabulary

| Term | Meaning |
|------|---------|
| **Combat** | A single simulated encounter |
| **Battle** | Higher-level campaign concept — reserved, not implemented |

Use **Combat** for simulation engine code. Never call a fight a "battle" in code.

## 4. Solution structure

```
Core               — Domain entities, enums, interfaces (zero dependencies)
Application        — Services, interfaces, models (depends on Core only)
Infrastructure     — Repositories, DbContext (depends on Core only)
Api                — ASP.NET CRUD endpoints (depends on Application + Infrastructure)
Demo               — Console app (depends on Application + Core + Presentation)
Presentation       — GUI-agnostic playback engine, ICombatPresenter (depends on Core + Application)
Gui                — Avalonia bridge (depends on Application + Core + Presentation)
UnitTests          — xUnit + NSubstitute
AcceptanceTests    — Reqnroll BDD
```

Core must not reference Application. Application must not reference Infrastructure.

## 5. CombatSimulator architecture

`CombatSimulator` (`Application/Services/`) is a thin orchestrator (~325 lines). Game logic lives in `Application/Services/Combat/`:

| Processor | Responsibility |
|-----------|---------------|
| `CombatLogger` | Builds all `CombatLogEntry` instances |
| `VictoryEvaluator` | Checks defeat conditions, builds `CombatResult` |
| `TurnMeterProcessor` | TM gain, mana regen/leech, defender TM boost |
| `StatusEffectProcessor` | Leech, DoT, HoT, self-buffs, on-hit effects, resist rolls, fumble, pet/effect expiry |
| `SpellProcessor` | Healing, mana deduction, spell queuing, pet summoning, disruption, concentration |
| `AttackResolver` | Attack outcome dispatch, clash, hit processing |
| `TurnProcessor` | Attack setup (queued spell / new attack), crowd control, target selection |
| `CharacterExtensions` | `TryGetCrowdControlLabel`, status effect helpers |
| `CombatSimulatorHelpers` | `BuildCombatantStates`, `GetActingOrder` |

State models: `CombatantState`, `QueuedSpellInfo`, `ActorSetup` — all `internal` in `Models/Combat/` and `Services/Combat/`.

## 6. Combat system — modern opposed-roll D&D

Formula: `d20 + AttackPower >= d20 + DefensePower`. **Never THAC0.**

- `StrikeRating` = higher is better. `ClassAccuracyBase = attacker.StrikeRating`.
- `ArmorClass` = higher is more defensive. `EffectiveAC = equipment.TotalArmorClass`.
- Any "lower SR is better" or `20 - X` is a THAC0 remnant — flag and fix.
- Attack resolution: 7-case priority matrix — TotalReversal → DevastatingStrike → Clash → Fumble → Critical → PerfectParry → normal opposed roll. See `.opencode/skills/combat-mechanics.md`.

## 7. Combat event types

`EventType` is a plain string on `CombatLogEntry` — not an enum. See `.opencode/skills/combat-mechanics.md` for the full list (~30 types: Attack, Damage, DoTTick, Healed, EffectApplied, PerfectParry, FumblePenalty, SpellDisrupted, etc.). Never introduce a new string without checking whether an existing one fits.

## 8. API — CRUD only, no game logic

The API (`BattleArena.Api`) is a pure CRUD layer. Must NOT contain dice rolling, combat resolution, or any game logic. Dice rolls originate from `DiceService` in `Application` (seed-based, deterministic). `CombatEndpoint.cs` is intentionally empty — combat runs locally via `CombatSimulator`.

Registered endpoint groups in `Program.cs`: `CombatEndpoints` (removed), `CharacterEndpoints`, `EquipmentEndpoints`, `AccessoriesEndpoints`, `NpcEndpoints`, `LoreEndpoints`. Lore serves `/v1/classes`, `/v1/subraces`, `/v1/deities`, `/v1/pets`, `/v1/spells`, `/v1/schools`, `/v1/bestiary`.

Port 8585 in Docker. Health check at `/api/healthcheck` (exempt from API key). Swagger only in Development/LocalDev.

## 9. GUI — pure renderer

`BattleArena.Gui` (Avalonia) must never contain combat logic. `ICombatPresenter` (in `Presentation`) is the only rendering contract. The playback engine and display state survive the Avalonia→Unity migration unchanged.

## 10. Testing

```
make test                           # dotnet test BattleArena.sln
make test-coverage                  # Coverlet, opencover format
dotnet test --project UnitTests/BattleArena.UnitTests.csproj   # unit tests only
dotnet test --filter "FullyQualifiedName~TestMethodName"  # single test
```

- Service tests → `Services/<Name>Tests.cs`. Diagnostics → `Diagnostics/CombatDiagnosticTests.cs`.
- **Always** mock `IDiceService` when testing dice-dependent methods.
- **Never** mock `CombatSimulator` — wire full real stack for diagnostics.
- Acceptance tests (Reqnroll): Features → `Features/<Name>.feature`, steps → `StepDefinitions/<Name>Steps.cs`. Namespace: `BattleArena.ReqnrollTests.StepDefinitions`. **Never edit `*.feature.cs`** (auto-generated).
- Dice-based acceptance tests use conservative bounds (p=0.8 with 100 trials → assert >= 60).

## 11. Code style

- `namespace` before `using` in hand-written code.
- Cyclomatic complexity <= 10 per method (`&&`/`||` counts as +1). 11–12 acceptable only where splitting would add params without reducing real complexity.
- One public type per file (partial classes like `Demo.*` are the exception).
- No magic numbers — named constants or enums.
- **No reflection** — no `System.Reflection`, `GetType().GetProperty()`, `SetValue()`, or runtime type inspection. If an `init`-only property blocks modification, create a new instance.

## 12. Makefile commands (from `src/`)

| Command | What it does |
|---------|-------------|
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Coverlet, opencover format |
| `make build-local` | Publish API to `../publish` |
| `make gui-local` | Run Avalonia GUI standalone (no DB needed) |
| `make up-local` | DB + API in Docker (ports exposed). Demo: `make demo-local` |
| `make up-dev` | DB + API + demo in Docker (interactive) |
| `make up-test` | DB + API + demo in Docker (no host ports) |
| `make up-preprod` / `make up-prod` | DB + API only (no host ports) |
| `make demo-local` | Run demo on host (sets `DOTNET_ENVIRONMENT=LocalDev`) |
| `make run-dev` | Re-run demo container (DB+API must already be up) |
| `make down` | Stop all Docker containers |
| `make clean` | Stop + wipe volumes + delete publish output |
| `make install` | Clean Docker → test → up-local → demo |
| `make install-gui` | Clean Docker → build → up-local → GUI |
| `make install-dev` | Clean + dotnet clean + test + dev-up |
| `make redo-local` | Clean + build + up-local + demo |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |
| `make clean-logs` | Delete `combat-logs/` |

Docker builds: `dotnet publish` runs on host, then `COPY` pre-built output. No NuGet restore inside containers.

## 13. Tooling quirks

- **No EF Core** — raw Npgsql + custom `DbContext` wrapper.
- **No CI** — `.github/workflows/` is empty. Run tests locally.
- **API requires `X-Api-Key` header** — default `BA-DEV-2024-SECRET`.
- **No `Directory.Build.props`** — each `.csproj` sets its own SDK, nullable, ImplicitUsings.
- **Setup**: copy `src/.env.example` to `src/.env` to choose an environment (defaults to `localdev`).
- **`bugs-features/`** — numbered files for pending work. Process in priority order: read → implement → test → mark `[x]` with summary → move to `done/<category>/` (category = `bugs`, `features`, or `task`).
- **`design/docs/`** — game design docs. Must stay in sync with SQL seed data (`.postgres-init/`).
- **`.opencode/skills/`** — auxiliary technical references (combat mechanics, turn order, makefile orchestration, work intake, log analysis). Loaded by OpenCode when tasks match.
- **Coverage**: `coverlet.runsettings` excludes `BattleArena.Api.Program` and `BattleArena.Api.AddServices` from coverage.
- **PostgreSQL init scripts** in `.postgres-init/` run alphabetically — naming (`01-`, `02-`, `03-`, `04-`) determines execution order.
- **HP range**: 0 to -9 = KnockedOut, -10 or lower = Dead.
- **Modifier pipeline** (`ICombatModifier`): priority bands 10=base/range, 20=environmental, 30=item/set/spell-buff.

## 14. Doc update obligations

- **README.md**: update when new project, API endpoint, DB table, Makefile target, Docker service, or test framework change.
- **design/docs/**: update when SQL seed adds races, classes, deities, pets, weapons, armor, spells. Entries must match the DB exactly.
- **design/docs/systems/spell-icon-design.md**: must stay in sync with the master spellbook and SQL seed data — every spell needs an icon spec entry.
- **release-notes.md**: do NOT touch unless asked.
