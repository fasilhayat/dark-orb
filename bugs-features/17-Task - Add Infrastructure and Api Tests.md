# Task — Add Infrastructure and Api Tests

Project: Dark Orb

Priority: Medium-High

Type: Test Coverage

Status: Draft

---

## Objective

Add test coverage for the `BattleArena.Infrastructure` and `BattleArena.Api` projects, which currently have **zero tests**.

---

## Current State

| Project | .cs files | Test files | Test count | Coverage |
|---------|-----------|------------|------------|----------|
| Core | 69 | 0 (tests via Application) | ~45% |
| Application | 62 | 33 unit test files | 593 unit | ~95% |
| Presentation | 20 | 1 test file | ✓ |
| Infrastructure | 19 | **0** | **0** | **0%** |
| Api | 14 | **0** | **0** | **0%** |

Neither `BattleArena.UnitTests` nor `BattleArena.AcceptanceTests` reference the `Infrastructure` or `Api` projects.

---

## What's Missing

### Infrastructure (19 files, 0% coverage)

Key classes untested:
- `DbContext` — the Npgsql wrapper. All CRUD operations go through it.
- All 14 repository implementations: `CharacterRepository`, `SpellRepository`, `RaceRepository`, `WeaponRepository`, `ArmorRepository`, `RingRepository`, `AmuletRepository`, `GirdleRepository`, `NpcRepository`, `DeityRepository`, `ClassRepository`, `PetRepository`, `BestiaryRepository`, `ItemSetRepository`
- `IDbContext` interface

### Api (14 files, 0% coverage)

Key components untested:
- All 6 endpoint groups: `CharacterEndpoint`, `EquipmentEndpoint`, `AccessoriesEndpoint`, `NpcEndpoint`, `LoreEndpoint`, `HealthEndpoint`
- `Program.cs` — service registration
- `AddServices.cs` — DI container setup
- `ApiKeyOptions` — auth configuration

---

## Proposed Approach

### Phase 1: Repository contract verification (unit tests, no DB)

Add unit tests that verify repository interface contracts compile and can be mocked. These don't require a database — they test that repository methods return correct types and that the `DbContext` query logic is structurally sound.

Since Infrastructure uses raw Npgsql (no EF Core), true unit testing requires mocking `NpgsqlCommand`/`NpgsqlDataReader`, which is impractical. Instead, write **contract verification tests** that:

1. Verify `IDbContext` returns expected types
2. Verify repository methods use correct SQL query patterns
3. Test error handling paths (connection failure, null results)

Alternatively, use a test double for `IDbContext` with in-memory data.

### Phase 2: API endpoint contract tests (unit + integration)

1. **Unit tests**: Use `Microsoft.AspNetCore.TestHost` or similar to test endpoint routing, request validation, and response serialization without a real database
2. **Contract tests**: Verify each endpoint returns correct HTTP status codes, content types, and response shapes
3. **Auth tests**: Verify `X-Api-Key` header validation

### Phase 3: Integration tests (optional, requires PostgreSQL)

Add an integration test project that:
1. Starts a test PostgreSQL container (Testcontainers)
2. Runs schema migrations
3. Inserts seed data
4. Tests repository CRUD operations end-to-end

This is higher effort but provides the most value.

---

## Files to Modify

| File | Change |
|------|--------|
| `BattleArena.UnitTests/BattleArena.UnitTests.csproj` | Add project reference to `BattleArena.Infrastructure` and `BattleArena.Api` |
| `BattleArena.UnitTests/Infrastructure/` | **New directory** — repository contract tests |
| `BattleArena.UnitTests/Api/` | **New directory** — endpoint contract tests |

### New test files (suggested)

```
BattleArena.UnitTests/
├── Infrastructure/
│   ├── DbContextTests.cs
│   ├── CharacterRepositoryTests.cs
│   ├── SpellRepositoryTests.cs
│   ├── RaceRepositoryTests.cs
│   └── ...
└── Api/
    ├── CharacterEndpointTests.cs
    ├── EquipmentEndpointTests.cs
    ├── AccessoriesEndpointTests.cs
    ├── NpcEndpointTests.cs
    ├── LoreEndpointTests.cs
    ├── HealthEndpointTests.cs
    ├── ApiKeyAuthTests.cs
    └── ProgramRegistrationTests.cs
```

---

## Acceptance Criteria

- [ ] `BattleArena.UnitTests` references `Infrastructure` and `Api`
- [ ] At least one test exists per repository interface (contract verification)
- [ ] At least one test per API endpoint group (GET returns 200, DELETE returns 204, etc.)
- [ ] `X-Api-Key` auth is tested (missing key → 401, wrong key → 403, valid key → 200)
- [ ] Health endpoint test (GET `/api/healthcheck` returns 200)
- [ ] No changes to production code
- [ ] All 719 existing tests still pass
