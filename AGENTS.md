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

`CombatSimulator` (in `Application/Services/`) is a thin orchestrator (~325 lines). Game logic lives in extracted processors under `Application/Services/Combat/`:

| Processor | Responsibility |
|-----------|---------------|
| `CombatLogger` | Builds all `CombatLogEntry` instances |
| `VictoryEvaluator` | Checks defeat conditions, builds `CombatResult` |
| `TurnMeterProcessor` | TM gain, mana regen, mana leech, defender TM boost |
| `StatusEffectProcessor` | Leech, DoT, HoT, self-buffs, on-hit effects, resist rolls, fumble penalty, pet expiry, effect expiry |
| `SpellProcessor` | Healing spells, mana deduction, spell queuing, pet summoning, spell disruption, concentration checks |
| `AttackResolver` | Attack outcome dispatch, clash handling, hit processing |
| `TurnProcessor` | Attack setup (queued spell / new attack), crowd control, target selection |
| `CharacterExtensions` | `TryGetCrowdControlLabel`, status effect helpers |
| `CombatSimulatorHelpers` | `BuildCombatantStates`, `GetActingOrder` |

State models: `CombatantState`, `QueuedSpellInfo`, `ActorSetup` — all `internal` in `Models/Combat/` and `Services/Combat/`.

## 6. Combat system — modern opposed-roll D&D

Formula: `d20 + AttackPower >= d20 + DefensePower`. **Never THAC0.**

- `StrikeRating` = higher is better. `ClassAccuracyBase = attacker.StrikeRating`.
- `ArmorClass` = higher is more defensive. `EffectiveAC = equipment.TotalArmorClass`.
- Any "lower SR is better" or `20 - X` is a THAC0 remnant — flag and fix.

## 7. Combat event types

`EventType` is a plain string on `CombatLogEntry` — no enum. Common types: `RoundStart`, `TurnMeterGain`, `TurnStart`, `Attack`, `Damage`, `DoTTick`, `HoTTick`, `LeechTick`, `Healed`, `EffectApplied`, `EffectResisted`, `EffectExpired`, `FumblePenalty`, `SkippedTurn`, `Move`, `Death`, `KnockedOut`, `PerfectParry`, `DevastatingStrike`, `TotalReversal`, `Clash`, `ManaDeduct`, `ManaRegen`, `SpellQueued`, `SpellCharging`, `SpellDisrupted`, `SpellLost`, `ConcentrationPass`, `InsufficientMana`, `PetSummoned`, `PetExpired`, `Resurrection`. See `.opencode/skills/combat-mechanics.md` for full detail.

## 8. API — CRUD only, no game logic

The API (`BattleArena.Api`) is a pure CRUD layer over the database. It must NOT contain:
- Dice rolling or randomness generation
- Combat resolution or rule evaluation
- Any game logic

All dice rolls originate from `DiceService` in `Application` (seed-based, deterministic). The `LoggingDiceService` (used in GUI) logs every roll into `DiceLog` for the combat log.

The `/v1/combat/simulate` endpoint was removed. Combat runs locally via `CombatSimulator`.

## 9. GUI — pure renderer

`BattleArena.Gui` (Avalonia) must never contain combat logic. `ICombatPresenter` (in `Presentation`) is the only rendering contract. The playback engine and display state survive the Avalonia→Unity migration unchanged.

## 10. Testing

```
dotnet test BattleArena.sln                       # full suite
dotnet test --project UnitTests/BattleArena.UnitTests.csproj   # unit tests only
dotnet test --filter "FullyQualifiedName~TestMethodName"        # single test
make test-coverage                                # coverlet, OpenCover format
```

- Service tests → `Services/<Name>Tests.cs`. Diagnostics → `Diagnostics/CombatDiagnosticTests.cs`.
- **Always** mock `IDiceService` when testing dice-dependent methods.
- **Never** mock `CombatSimulator` — wire full real stack for diagnostics.
- Acceptance tests (Reqnroll): Features → `Features/<Name>.feature`, steps → `StepDefinitions/<Name>Steps.cs`. Namespace: `BattleArena.ReqnrollTests.StepDefinitions`. Never edit `*.feature.cs` manually.
- Dice-based acceptance tests use conservative bounds (p=0.8 with 100 trials → assert >= 60).
- **Flaky test**: `PriestHealsThemselfAfterTakingDamage` — Sera starts at 42.8% HP, just above the 40% heal threshold. The AI may or may not cast Heal depending on damage taken. Re-run if it fails.

## 11. Code style

- `namespace` before `using` in hand-written code.
- Cyclomatic complexity <= 10 per method (`&&`/`||` counts as +1). 11–12 acceptable only where splitting would add params without reducing real complexity.
- One public type per file (partial classes like `Demo.*` are the exception).
- No magic numbers — named constants or enums.
- **No reflection** — never use `System.Reflection`, `GetType().GetProperty()`, `SetValue()`, or any runtime type inspection to modify objects. If an `init`-only property blocks modification, create a new instance with the desired value instead.

## 12. Makefile commands (from `src/`)

| Command | What it does |
|---------|-------------|
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Coverlet, opencover format |
| `make build-local` | Publish API to `../publish` |
| `make gui-local` | Run Avalonia GUI standalone (no DB needed) |
| `make up-local` | DB + API in Docker (requires build-local first). Demo via `make demo-local` |
| `make demo-local` | Run demo on host (sets `DOTNET_ENVIRONMENT=LocalDev`) |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |
| `make install` | Clean Docker → test → up-local → demo |
| `make clean-logs` | Delete `combat-logs/` |

Docker builds: `dotnet publish` runs on host, then `COPY` pre-built output. No NuGet restore inside containers.

## 13. Tooling quirks

- **No EF Core** — raw Npgsql + custom `DbContext` wrapper.
- **No CI** — `.github/workflows/` is empty. Run `dotnet test` locally.
- **API requires `X-Api-Key` header** — default `BA-DEV-2024-SECRET`.
- **Swagger only in Development/LocalDev**.
- **No `Directory.Build.props`** — each `.csproj` sets its own SDK, nullable, ImplicitUsings.
- **`bugs-features/`** — numbered files representing pending work. Move to `done/<category>/` when complete where category is `bugs`, `features`, or `task`.
- **`design/docs/`** — Game design docs. Keep in sync with SQL seed data (`.postgres-init/`).
- **`.opencode/skills/`** — contains auxiliary technical references (combat mechanics, turn order, makefile orchestration, work intake, log analysis). These are loaded by OpenCode when tasks match their descriptions. AGENTS.md remains the canonical behavioural instruction file.

## 14. Doc update obligations

- **README.md**: update when new project, API endpoint, DB table, Makefile target, Docker service, or test framework change.
- **design/docs/**: update when SQL seed adds races, classes, deities, pets, weapons, armor, spells, etc. Entries must match the DB exactly.
- **release-notes.md**: do NOT touch unless asked.
