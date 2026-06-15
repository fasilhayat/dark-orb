# BattleArena — AI Assistant Instructions (GitHub Copilot)

> **Mirrored file.** The canonical source is `AGENTS.md` at the repository root (read by OpenCode).
> This copy exists solely because GitHub Copilot reads `.github/copilot-instructions.md`.
> Edit `AGENTS.md`, then run `make sync-instructions` to update this file.

## Commands

All `make` commands run from `src/`. Solution: `src/BattleArena.sln`.
Dockerfile: `src/Dockerfile` (not root).

| Command | Action |
|---------|--------|
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Coverlet + opencover |
| `make up-local` | DB + API in Docker (ports exposed) + `make demo-local` |
| `make up-dev` | DB + API + demo in Docker (interactive) |
| `make up-test` | DB + API + demo in Docker (no host ports) |
| `make gui-local` | Avalonia GUI standalone (no DB needed) |
| `make demo-local` | Run demo on host (`DOTNET_ENVIRONMENT=LocalDev`) |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |

Run single test: `dotnet test --filter "FullyQualifiedName~TestMethodName"`
Run unit tests only: `dotnet test --project UnitTests/BattleArena.UnitTests.csproj`

## Project structure

Build order: Core → {Application, Infrastructure} → everything else.
Core must never reference Application or Infrastructure. Application must never reference Infrastructure.

| Project | Role | Depends |
|---------|------|---------|
| Core | Domain entities, enums, interfaces | none |
| Application | Services, interfaces, models | Core only |
| Infrastructure | Repositories, DbContext | Core only |
| Api | ASP.NET CRUD endpoints | Application + Infrastructure |
| Demo | Console app | Application + Core + Presentation |
| Presentation | GUI-agnostic playback, `ICombatPresenter` | Core + Application |
| Gui | Avalonia bridge, no combat logic | Application + Core + Presentation |
| UnitTests | xUnit + NSubstitute | — |
| AcceptanceTests | Reqnroll BDD | — |

## Combat engine

`CombatSimulator` (`Application/Services/`) orchestrates. Game logic in `Application/Services/Combat/`:

`CombatLogger`, `VictoryEvaluator`, `TurnMeterProcessor`, `StatusEffectProcessor`,
`SpellProcessor`, `AttackResolver`, `TurnProcessor`, `CharacterExtensions`, `CombatSimulatorHelpers`

State models (internal): `CombatantState`, `QueuedSpellInfo`, `ActorSetup`.

**Attack resolution** (opposed roll, never THAC0):
`d20 + AttackPower >= d20 + DefensePower`. Priority: TotalReversal → DevastatingStrike → Clash → Fumble → Critical → PerfectParry → normal opposed roll. `StrikeRating` higher = better. `ArmorClass` higher = more defensive.

**HP**: >0 alive, 0 to -9 = KnockedOut, -10 or lower = Dead.

**EventType** is a plain `string` on `CombatLogEntry` — not an enum.

## Constraints

- **API**: pure CRUD — no dice rolling, combat resolution, or game logic. Endpoint groups: Character, Equipment, Accessories, Npc, Lore. Health check at `/api/healthcheck`. Port 8585. Swagger only in Development/LocalDev. Requires `X-Api-Key` header.
- **GUI** (Avalonia): must never contain combat logic. `ICombatPresenter` in `Presentation` is the only rendering contract.
- **DiceService**: seed-based, deterministic. Seed via `Random.Shared.Next()` or explicit constructor.
- **No EF Core** — raw Npgsql + custom `DbContext` wrapper.
- **No CI**, no `Directory.Build.props`.
- **Makefile targets are Windows-only** — uses `pwsh`, `cmd`, `powershell`.
- **Docker builds**: `dotnet publish` on host, then `COPY` pre-built output. No NuGet restore inside containers.
- **PostgreSQL init** in `.postgres-init/` runs alphabetically (`01-schema.sql`, `02-seed-data.sql`, etc.).
- **Setup**: copy `src/.env.example` to `src/.env` to choose environment (defaults to `localdev`).
- **Modifier pipeline** (`ICombatModifier`): priority bands 10=base/range, 20=environmental, 30=item/set/spell-buff.

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

`bugs-features/` — numbered files. Process in priority order: read → implement → test → mark `[x]` with summary → move to `done/<category>/` (category = `bugs`, `features`, or `task`).

## Doc sync obligations

- **README.md**: update for new project, API endpoint, DB table, Makefile target, Docker service, or test framework change.
- **design/docs/**: update when SQL seed data changes. Must match DB exactly.
- **design/docs/systems/spell-icon-design.md**: stay in sync with master spellbook and SQL seed.
- **release-notes.md**: do NOT touch unless asked.
