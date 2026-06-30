# BattleArena — AI Assistant Instructions (GitHub Copilot)

> **Mirrored file.** The canonical source is `AGENTS.md` at the repository root (read by OpenCode).
> This copy exists solely because GitHub Copilot reads `.github/copilot-instructions.md`.
> Edit `AGENTS.md`, then run `make sync-instructions` to update this file.

## Stale Makefile targets

Any target referencing `BattleArena.Demo/BattleArena.Demo.csproj` or
`battle-arena-demo` fails — the Demo project was deleted (empty dir with no `.csproj` remains).
Broken: `up-dev`, `up-test`, `demo-local`, `publish-demo`, `install`, `install-dev`,
`redo-local`, `run-dev`, `dev-up`.
Working: `install-gui`, `gui-local`, `start-gui`, `build-local` (alias for `publish`),
`up-local`, `up-preprod`, `up-prod`.

## Commands

All from `src/`. Solution: `src/BattleArena.sln`. .NET 8, `ImplicitUsings` + `Nullable` enabled project-wide.

| Command | Action |
|---------|--------|
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Coverlet + opencover (MSBuild props, runsettings at `coverlet.runsettings`) |
| `make up-local` | DB + API in Docker (ports 5432, 8585) |
| `make up-preprod` / `make up-prod` | DB + API, no host ports |
| `make down` | Stop all env containers |
| `make clean` | Down + wipe volumes + delete `../publish` |
| `make gui-local` / `make start-gui` | Avalonia GUI standalone, no DB needed |
| `make build-local` | Alias for `make publish` |
| `make publish` | Host-side `dotnet publish Api` to `../publish` (required before any `up-*`) |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |
| `make clean-logs` | Delete generated `combat-logs/` files |

Single test: `dotnet test --filter "FullyQualifiedName~TestMethodName"`
Unit tests only: `dotnet test --project BattleArena.UnitTests/BattleArena.UnitTests.csproj`

## OpenCode skills

`.opencode/skills/` has 5 skill files: `work-intake.md`, `combat-mechanics.md`,
`combat-log-analysis.md`, `combat-order.md`, `makefile-orchestration.md`.

## Project structure

Build order: **Core → {Application, Infrastructure} → everything else**.
Core never references Application/Infrastructure. Application never references Infrastructure.

| Project | Role | Depends on |
|---------|------|------------|
| Core | Domain entities, enums, interfaces | — |
| Application | Services (CombatSimulator, LevelingService, CombatService), models | Core |
| Infrastructure | Repositories, raw Npgsql DbContext (no EF Core) | Core |
| Api | ASP.NET CRUD endpoints, `AddServices.cs` wires DI | Application, Infrastructure |
| Presentation | GUI-agnostic playback engine, `ICombatPresenter` | Core, Application |
| Gui | Avalonia bridge, no combat logic | Application, Core, Presentation |
| UnitTests | xUnit + NSubstitute | Application, Core, Presentation, Gui |
| AcceptanceTests | Reqnroll BDD | Application, Core, Presentation |

## Constraints

- **API**: pure CRUD — no dice rolling, combat, or game logic (CombatEndpoint.cs is a 2-line comment tombstone). Endpoints: Character, Equipment, Accessories, Npc, Lore, Quest, Health. Health check at `/api/healthcheck`. Port 8585. Swagger only in Development/LocalDev. Requires `X-Api-Key` header.
- **GUI** (Avalonia): must never contain combat logic. `ICombatPresenter` in Presentation is the only rendering contract.
- **DiceService**: seed-based deterministic via `Random.Shared.Next()` or explicit constructor.
- **No EF Core** — Npgsql 8.0.5 + custom DbContext wrapper in Infrastructure.
- **No CI**, no `Directory.Build.props`.
- **Makefile targets are Windows-only** — `pwsh`, `cmd`, `powershell`.
- **Docker**: `dotnet publish` on host, COPY pre-built output. No NuGet restore inside containers.
- **PostgreSQL init** in `src/.postgres-init/` runs alphabetically: `01-schema.sql` … `06-quest-seed.sql`.
- **Setup**: copy `src/.env.example` to `src/.env` (defaults to `localdev`).
- **NuGet config**: repo-root `nuget.config` (nuget.org only).

## Combat engine

`Application/Services/CombatSimulator.cs` orchestrates turn-based combat (any NvN, up to 6v6).
Services in `Application/Services/Combat/`: AttackResolver, CombatLogger, VictoryEvaluator,
TurnMeterProcessor, StatusEffectProcessor, SpellProcessor, TurnProcessor, CharacterExtensions,
CombatSimulatorHelpers, ActorSetup (internal).
State: `CombatantState`, `QueuedSpellInfo` (internal), `ActorSetup` (internal).

**Attack resolution** (opposed roll, never THAC0): `d20 + AttackPower >= d20 + DefensePower`.
Priority in `CombatService.ResolveAttack()`:
1. TotalReversal (atk=1, def=20)
2. DevastatingStrike (atk=20, def=1)
3. PerfectParry (both 20)
4. Fumble (atk=1)
5. Critical (atk=20)
6. PerfectParry (def=20)
7. Normal opposed roll

`IsClash` is declared on `AttackResult` but never set to `true` — `AttackResolver.ProcessClashAsync()` is dead code.

**HP**: >0 alive, 0 to −9 KnockedOut, −10 or lower Dead.

**EventType** is a plain `string` on `CombatLogEntry` (not an enum). EventType values flow through `CombatLogWriter.cs` (30+ values: TurnStart, TurnEnd, Attack, Damage, TurnMeterGain, ManaDeduct, ManaRegen, SpellQueued, SpellCharging, SpellLost, SpellDisrupted, ConcentrationPass, InsufficientMana, SummonPet, PetSummoned, PetExpired, SummonFailed, Healed, DoTTick, HoTTick, LeechTick, EffectApplied, EffectResisted, EffectExpired, EffectReflected, FumblePenalty, Death, KnockedOut, SkippedTurn, ExtraAttack, Move, RoundStart, RoundEnd, Clash, DevastatingStrike, PerfectParry, TotalReversal, ApiCall, TurnMeterSnapshot).

**Combat log**: `CombatLogWriter.Write()` produces a `.txt` (compact block-per-turn) + companion `.json` file for replay. The `.json` can be loaded via `CombatReplayer.ReplayFromFile()` for deterministic replay. `CombatLogMerger` inserts ApiCall dice entries per-actor before Attack/SpellQueued events.

## Spell visual effects

`Presentation/SpellSymbolRegistry.cs` maps spell names to animated Unicode symbols (no emojis).
Symbols are font-hinted (Segoe UI Symbol, Times New Roman) and colored per category.

## Testing quirks

- **Always** mock `IDiceService` when testing dice-dependent methods.
- **Never** mock `CombatSimulator` — wire full real stack for diagnostics.
- **Acceptance tests**: `Features/<Name>.feature`, steps in `StepDefinitions/<Name>Steps.cs`.
  Namespace `BattleArena.ReqnrollTests.StepDefinitions` (NOT the csproj root). Never edit `*.feature.cs` (auto-generated by Reqnroll).
- **Dice-based acceptance tests**: conservative bounds (p=0.8 with 100 trials → assert >= 60).
- `coverlet.runsettings` excludes `BattleArena.Api.Program` and `BattleArena.Api.AddServices`.

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
