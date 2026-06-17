# BattleArena — AI instructions (OpenCode)

> Canonical source. After editing, run `make sync-instructions` from `src/`
> to mirror to `.github/copilot-instructions.md`.

**Stale Makefile targets** (Demo project was removed in commit `d63b1ef`):
`up-dev`, `up-test`, `demo-local`, `publish-demo`, `install`, `install-dev`,
`redo-local`, `run-dev` — all reference the deleted `BattleArena.Demo` project.

## Commands

All `make` commands run from `src/`. Solution: `src/BattleArena.sln`.
Dockerfile: `src/Dockerfile` (not root). NuGet config: repo-root `nuget.config`.

| Command | Action |
|---------|--------|
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Coverlet + opencover |
| `make up-local` | DB + API in Docker (ports exposed) |
| `make up-preprod` / `make up-prod` | DB + API only |
| `make down` | Stop all environment containers |
| `make clean` | Down + wipe volumes + delete publish output |
| `make gui-local` | Avalonia GUI standalone (no DB needed) |
| `make publish` | Host-side publish for Docker |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |

Single test: `dotnet test --filter "FullyQualifiedName~TestMethodName"`
Unit tests only: `dotnet test --project BattleArena.UnitTests/BattleArena.UnitTests.csproj`

## OpenCode skills

`.opencode/skills/` contains 5 skill files the agent should use when relevant:
`work-intake.md` (loads pending work from `bugs-features/`), `combat-mechanics.md`,
`combat-log-analysis.md`, `combat-order.md`, `makefile-orchestration.md`.

## Project structure

Build order: Core → {Application, Infrastructure} → everything else.
Core never references Application/Infrastructure. Application never references Infrastructure.

| Project | Role | Depends |
|---------|------|---------|
| Core | Domain entities, enums, interfaces | none |
| Application | Services, interfaces, models; includes `LevelingService` (XP/leveling) and `CombatSimulatorFactory` | Core only |
| Infrastructure | Repositories, DbContext (raw Npgsql, no EF Core) | Core only |
| Api | ASP.NET CRUD endpoints | Application + Infrastructure |
| Presentation | GUI-agnostic playback, `ICombatPresenter` | Core + Application |
| Gui | Avalonia bridge, no combat logic | Application + Core + Presentation |
| UnitTests | xUnit + NSubstitute | — |
| AcceptanceTests | Reqnroll BDD | — |
| Demo | Source removed (commit `d63b1ef`); `bin/obj` artifacts remain; stale Makefile targets still reference it | — |

## Combat engine

`CombatSimulator` (`Application/Services/`) orchestrates via services in
`Application/Services/Combat/`: `CombatLogger`, `VictoryEvaluator`,
`TurnMeterProcessor`, `StatusEffectProcessor`, `SpellProcessor`,
`AttackResolver`, `TurnProcessor`, `CharacterExtensions`,
`CombatSimulatorHelpers`.

State models: `CombatantState`, `QueuedSpellInfo` (`Application/Models/Combat/`); `ActorSetup` (`Application/Services/Combat/`, `internal`).

**Attack resolution** (opposed roll, never THAC0):
`d20 + AttackPower >= d20 + DefensePower`. Priority in `CombatService.ResolveAttack()`: TotalReversal → DevastatingStrike → Fumble → Critical → PerfectParry → normal. Clash is a separate code path in `AttackResolver.ProcessClashAsync()` (triggered when both rolls are equal). `StrikeRating` higher = better. `ArmorClass` higher = more defensive.

**HP**: >0 alive, 0 to -9 = KnockedOut, -10 or lower = Dead.

**EventType** is a plain `string` on `CombatLogEntry` — not an enum.

## Constraints

- **API**: pure CRUD — no dice rolling, combat resolution, or game logic. Endpoint groups: Character, Equipment, Accessories, Npc, Lore. `CombatEndpoint.cs` exists as a tombstone (logic moved to engine — do not add game logic there). Health check at `/api/healthcheck`. Port 8585. Swagger only in Development/LocalDev. Requires `X-Api-Key` header.
- **GUI** (Avalonia): must never contain combat logic. `ICombatPresenter` in `Presentation` is the only rendering contract.
- **DiceService**: seed-based, deterministic. Seed via `Random.Shared.Next()` or explicit constructor.
- **No EF Core** — raw Npgsql + custom `DbContext` wrapper in Infrastructure.
- **No CI**, no `Directory.Build.props`.
- **Makefile targets are Windows-only** — uses `pwsh`, `cmd`, `powershell`.
- **Docker builds**: `dotnet publish` on host, then `COPY` pre-built output. No NuGet restore inside containers.
- **PostgreSQL init** in `src/.postgres-init/` runs alphabetically (`01-schema.sql`, `02-seed-data.sql`, `03-characters.sql`, `04-bestiary.sql`).
- **Setup**: copy `src/.env.example` to `src/.env` to choose environment (defaults to `localdev`).

## Testing quirks

- **Always** mock `IDiceService` when testing dice-dependent methods.
- **Never** mock `CombatSimulator` — wire full real stack for diagnostics.
- **Acceptance tests**: `Features/<Name>.feature`, steps in `StepDefinitions/<Name>Steps.cs`.
  Namespace `BattleArena.ReqnrollTests.StepDefinitions`. Never edit `*.feature.cs` (auto-generated).
- **Dice-based acceptance tests**: conservative bounds (p=0.8 with 100 trials → assert >= 60).
- `coverlet.runsettings` excludes `BattleArena.Api.Program` and `BattleArena.Api.AddServices` from coverage.

## Code style

- `namespace` before `using` in hand-written code.
- No reflection (no `System.Reflection`, `GetType().GetProperty()`, `SetValue()`).

## Bugs-features workflow

`bugs-features/` — numbered files. Process in priority order: read → implement → test → mark `[x]` with summary → move to `bugs-features/done/<category>/` (category = `bugs`, `features`, or `task`).

## Doc sync obligations

- **README.md**: update for new project, API endpoint, DB table, Makefile target, Docker service, or test framework change.
- **design/docs/**: update when SQL seed data changes. Must match DB exactly.
- **design/docs/systems/spell-icon-design.md**: stay in sync with master spellbook and SQL seed.
- **release-notes.md**: do NOT touch unless asked.
