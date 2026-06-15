# BattleArena — AI instructions (OpenCode)

> Canonical source. After editing, run `make sync-instructions` from `src/`
> to mirror to `.github/copilot-instructions.md` (GitHub Copilot).

## 1. Never commit without approval

Do not create commits or push branches unless the user explicitly says so.

## 2. Working directory

All `make` commands run from `src/`. Dockerfile is at `src/Dockerfile`, not root.
The solution file is `src/BattleArena.sln`.

## 3. Test-failure analysis

Understand what contract a test asserts before modifying it. Fix the implementation
if it violated the contract. Only update the test if it is genuinely stale.

## 4. Project vocabulary

| Term | Meaning |
|------|---------|
| **Combat** | A single simulated encounter |
| **Battle** | Higher-level campaign concept — reserved, not implemented |

Use **Combat** for simulation engine code. Never call a fight a "battle" in code.

## 5. Solution structure

| Project | Role | Dependencies |
|---------|------|-------------|
| `Core` | Domain entities, enums, interfaces | none |
| `Application` | Services, interfaces, models | Core only |
| `Infrastructure` | Repositories, DbContext | Core only |
| `Api` | ASP.NET CRUD endpoints | Application + Infrastructure |
| `Demo` | Console app | Application + Core + Presentation |
| `Presentation` | GUI-agnostic playback engine, `ICombatPresenter` | Core + Application |
| `Gui` | Avalonia bridge | Application + Core + Presentation |
| `UnitTests` | xUnit + NSubstitute | — |
| `AcceptanceTests` | Reqnroll BDD | — |

Build order: Core → {Application, Infrastructure} → everything else.
Core must never reference Application or Infrastructure. Application must never
reference Infrastructure.

## 6. Combat engine architecture

`CombatSimulator` (`Application/Services/`) is a thin orchestrator. Game logic
lives in `Application/Services/Combat/`:

| Processor | Responsibility |
|-----------|---------------|
| `CombatLogger` | Builds all `CombatLogEntry` instances |
| `VictoryEvaluator` | Defeat conditions, `CombatResult` |
| `TurnMeterProcessor` | TM gain, mana regen/leech, defender TM boost |
| `StatusEffectProcessor` | Leech, DoT, HoT, self-buffs, on-hit, resist, fumble, pet/effect expiry |
| `SpellProcessor` | Healing, mana, spell queuing, pet summoning, disruption, concentration |
| `AttackResolver` | Attack outcome dispatch, clash, hit processing |
| `TurnProcessor` | Attack setup, crowd control, target selection |
| `CharacterExtensions` | Status effect helpers |
| `CombatSimulatorHelpers` | `BuildCombatantStates`, `GetActingOrder` |

State models (`internal`): `CombatantState`, `QueuedSpellInfo`, `ActorSetup`.

Full formulas (attack resolution, damage, turn meter, status effects, healing,
terrain, visual/sound pipeline) at `.opencode/skills/combat-mechanics.md`.

## 7. Combat system — modern opposed-roll D&D

Formula: `d20 + AttackPower >= d20 + DefensePower`. **Never THAC0.**

- `StrikeRating` = higher is better. `ArmorClass` = higher is more defensive.
- Any "lower SR is better" or `20 - X` code is a THAC0 remnant — flag and fix.
- Attack resolution: 7-case priority matrix (TotalReversal → DevastatingStrike →
  Clash → Fumble → Critical → PerfectParry → normal opposed roll).

## 8. Event types

`EventType` is a plain `string` on `CombatLogEntry` — not an enum. ~30 types
(Attack, Damage, DoTTick, Healed, EffectApplied, PerfectParry, etc.). Never
introduce a new string without checking existing ones in the skill file above.

## 9. API — CRUD only, no game logic

`BattleArena.Api` is a pure CRUD layer. No dice rolling, combat resolution, or
game logic. `DiceService` (seed-based, deterministic) lives in `Application`.
`/v1/combat/simulate` was removed — combat runs locally via `CombatSimulator`.

Port 8585 in Docker. Health check at `/api/healthcheck` (exempt from API key).
Swagger only in Development/LocalDev.

## 10. GUI — pure renderer

`BattleArena.Gui` (Avalonia) must never contain combat logic. `ICombatPresenter`
(in `Presentation`) is the only rendering contract.

## 11. Testing

```makefile
make test                           # all tests
make test-coverage                  # Coverlet, opencover format
dotnet test --project UnitTests/BattleArena.UnitTests.csproj   # unit tests only
dotnet test --filter "FullyQualifiedName~TestMethodName"  # single test
```

- **Always** mock `IDiceService` when testing dice-dependent methods.
- **Never** mock `CombatSimulator` — wire full real stack for diagnostics.
- Acceptance tests: `Features/<Name>.feature`, steps in `StepDefinitions/<Name>Steps.cs`.
  Namespace `BattleArena.ReqnrollTests.StepDefinitions`. **Never edit `*.feature.cs`** (auto-generated).
- Dice-based acceptance tests use conservative bounds (p=0.8 with 100 trials → assert >= 60).

## 12. Code style

- `namespace` before `using` in hand-written code.
- Cyclomatic complexity ≤ 10 per method (`&&`/`||` counts as +1).
- One public type per file (partial classes like `Demo.*` are the exception).
- No magic numbers — named constants or enums.
- **No reflection** — no `System.Reflection`, `GetType().GetProperty()`, `SetValue()`,
  or runtime type inspection. If an `init`-only property blocks modification, create
  a new instance.

## 13. Makefile commands

| Command | Action |
|---------|--------|
| `make test` | `dotnet test BattleArena.sln` |
| `make test-coverage` | Coverlet + opencover |
| `make build-local` | Publish API to `../publish` |
| `make up-local` | DB + API in Docker (ports exposed) + `make demo-local` |
| `make up-dev` | DB + API + demo in Docker (interactive) |
| `make up-test` | DB + API + demo in Docker (no host ports) |
| `make up-preprod` / `make up-prod` | DB + API only (no host ports) |
| `make demo-local` | Run demo on host (`DOTNET_ENVIRONMENT=LocalDev`) |
| `make gui-local` | Avalonia GUI standalone (no DB needed) |
| `make run-dev` | Re-run demo container (DB+API must be up) |
| `make install` | Clean Docker → test → up-local → demo |
| `make install-gui` | Clean Docker → build → up-local → GUI |
| `make install-dev` | Clean + `dotnet clean` + test + dev-up |
| `make redo-local` | Clean + build + up-local + demo-local |
| `make sync-instructions` | Copy AGENTS.md → `.github/copilot-instructions.md` |
| `make clean-logs` | Delete `combat-logs/` |
| `make down` | Stop all Docker containers |
| `make clean` | Stop + wipe volumes + delete publish output |

## 14. Tooling quirks

- **Makefile targets are Windows-only** — uses `pwsh`, `cmd /C`, `powershell`.
- **Docker builds**: `dotnet publish` on host, then `COPY` pre-built output
  (`src/Dockerfile`). No NuGet restore inside containers.
- **No EF Core** — raw Npgsql + custom `DbContext` wrapper.
- **No CI** — `.github/workflows/` is empty.
- **API requires `X-Api-Key` header** — default `BA-DEV-2024-SECRET`.
- **No `Directory.Build.props`** — each `.csproj` sets its own SDK, nullable, ImplicitUsings.
- **`bugs-features/`** — numbered files. Process: read → implement → test → mark `[x]`
  with summary → move to `done/<category>/`.
- **`design/docs/`** — game design docs. Must stay in sync with SQL seed
  (`src/.postgres-init/`).
- **`.opencode/skills/`** — loaded by OpenCode on matching tasks. Includes combat
  mechanics, turn order, makefile orchestration, work intake, log analysis.
- **PostgreSQL init** at `src/.postgres-init/` runs alphabetically (`01-`, `02-`, `03-`, `04-`).
- **HP**: 0 to -9 = KnockedOut, -10 or lower = Dead.
- **Modifier pipeline** (`ICombatModifier`): priority bands 10=base/range,
  20=environmental, 30=item/set/spell-buff.
- **`.env.example`** at `src/.env.example`. Copy to `src/.env` and set `ENV=localdev`.
