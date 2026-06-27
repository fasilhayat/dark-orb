# BattleArena — AI instructions (OpenCode)

> Canonical source. Edit this file, then run `make sync-instructions` from `src/`
> to mirror to `.github/copilot-instructions.md`.

## Stale Makefile targets

Any target referencing `BattleArena.Demo/BattleArena.Demo.csproj` or
`battle-arena-demo` fails — the Demo project was deleted. Affected:
`up-dev`, `up-test`, `demo-local`, `publish-demo`, `install`, `install-dev`,
`redo-local`, `run-dev`, `dev-up`. `install-gui`, `gui-local`, `start-gui`,
`build-local` (alias for `publish`), `up-local`, `up-preprod`, `up-prod` still work.

## Commands

All from `src/`. Solution: `src/BattleArena.sln`. .NET 8, `ImplicitUsings` + `Nullable` enabled project-wide.

| Command | Action |
|---------|--------|
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Coverlet + opencover (uses `coverlet.runsettings`) |
| `make up-local` | DB + API in Docker (ports 5432, 8585 exposed) |
| `make up-preprod` / `make up-prod` | DB + API only, no host ports |
| `make down` | Stop all environment containers |
| `make clean` | Down + wipe volumes + delete `../publish` |
| `make gui-local` / `make start-gui` | Avalonia GUI standalone (no DB needed) |
| `make build-local` | Alias for `make publish` |
| `make publish` | Host-side `dotnet publish Api` to `../publish` (required before any `up-*`) |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |
| `make clean-logs` | Delete generated `combat-logs/` files |

Single test: `dotnet test --filter "FullyQualifiedName~TestMethodName"`
Unit tests only: `dotnet test --project BattleArena.UnitTests/BattleArena.UnitTests.csproj`

## OpenCode skills

`.opencode/skills/` has 5 skill files: `work-intake.md` (loads pending work from `bugs-features/`), `combat-mechanics.md`, `combat-log-analysis.md`, `combat-order.md`, `makefile-orchestration.md`.

## Project structure

Build order: Core → {Application, Infrastructure} → everything else.
Core never references Application/Infrastructure. Application never references Infrastructure.

| Project | Role | Depends |
|---------|------|---------|
| Core | Domain entities, enums, interfaces | none |
| Application | Services, interfaces, models (LevelingService, CombatService, CombatSimulatorFactory) | Core |
| Infrastructure | Repositories, raw Npgsql DbContext (no EF Core) | Core |
| Api | ASP.NET CRUD endpoints, AddServices.cs wires DI | Application + Infrastructure |
| Presentation | GUI-agnostic playback engine, ICombatPresenter | Core + Application |
| Gui | Avalonia bridge, no combat logic | Application + Core + Presentation |
| UnitTests | xUnit + NSubstitute | Application + Core + Presentation + Gui |
| AcceptanceTests | Reqnroll BDD | Application + Core + Presentation |

## Constraints

- **API**: pure CRUD — no dice rolling, combat, or game logic. Endpoint groups: Character, Equipment, Accessories, Npc, Lore, Quest, Health. `CombatEndpoint.cs` is a 2-line comment tombstone. Health check at `/api/healthcheck`. Port 8585. Swagger only in Development/LocalDev. Requires `X-Api-Key` header.
- **GUI** (Avalonia): must never contain combat logic. `ICombatPresenter` in Presentation is the only rendering contract.
- **DiceService**: seed-based, deterministic via `Random.Shared.Next()` or explicit constructor.
- **No EF Core** — Npgsql + custom DbContext wrapper in Infrastructure.
- **No CI**, no `Directory.Build.props`.
- **Makefile targets are Windows-only** — uses `pwsh`, `cmd`, `powershell`.
- **Docker**: `dotnet publish` on host, COPY pre-built output. No NuGet restore inside containers.
- **PostgreSQL init** in `src/.postgres-init/` runs alphabetically: `01-schema.sql`, `02-seed-data.sql`, `03-characters.sql`, `04-bestiary.sql`.
- **Setup**: copy `src/.env.example` to `src/.env` (defaults to `localdev`).
- **NuGet config**: repo-root `nuget.config` (nuget.org only).

## Combat engine

`Application/Services/CombatSimulator.cs` orchestrates turn-based combat.
Combat services live in `Application/Services/Combat/`: AttackResolver, CombatLogger, VictoryEvaluator, TurnMeterProcessor, StatusEffectProcessor, SpellProcessor, TurnProcessor, CharacterExtensions, CombatSimulatorHelpers.
State models: `CombatantState`, `QueuedSpellInfo` (Application/Models/Combat/); `ActorSetup` (internal).

**Attack resolution** (opposed roll, never THAC0): `d20 + AttackPower >= d20 + DefensePower`. Priority in `CombatService.ResolveAttack()`: TotalReversal → DevastatingStrike → PerfectParry(both 20) → Fumble → Critical → PerfectParry(def 20) → normal. `IsClash` is declared on `AttackResult` but never set to `true` in current code — the `AttackResolver.ProcessClashAsync()` code path is dead. `StrikeRating` higher = better. `ArmorClass` higher = more defensive.

**HP**: >0 alive, 0 to −9 KnockedOut, −10 or lower Dead.

**EventType** is a plain `string` on `CombatLogEntry` — not an enum.

## Testing quirks

- **Always** mock `IDiceService` when testing dice-dependent methods.
- **Never** mock `CombatSimulator` — wire full real stack for diagnostics.
- **Acceptance tests**: `Features/<Name>.feature`, steps in `StepDefinitions/<Name>Steps.cs`. Namespace `BattleArena.ReqnrollTests.StepDefinitions`. Never edit `*.feature.cs` (auto-generated).
- **Dice-based acceptance tests**: conservative bounds (p=0.8 with 100 trials → assert >= 60).
- `coverlet.runsettings` excludes `BattleArena.Api.Program` and `BattleArena.Api.AddServices` from coverage.

## Code style

- `namespace` before `using` in hand-written code.
- No reflection (`System.Reflection`, `GetType().GetProperty()`, `SetValue()`).

## Bugs-features workflow

`bugs-features/` — process in priority order: read → implement → test → mark `[x]` with summary → move to `bugs-features/done/<category>/` (category = `bugs`, `features`, or `task`).

## Doc sync obligations

- **README.md**: update for new project, API endpoint, DB table, Makefile target, Docker service, or test framework change.
- **design/docs/**: update when SQL seed data changes. Must match DB exactly.
- **design/docs/systems/spell-icon-design.md**: stay in sync with master spellbook and SQL seed.
- **release-notes.md**: do NOT touch unless asked.
