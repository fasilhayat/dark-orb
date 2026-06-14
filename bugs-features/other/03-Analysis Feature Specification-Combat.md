# Analysis Feature Specification - Combat Benchmark System

Project: Dark Orb

File: `analysis-feature.md`

---

## Objective

Build a deterministic combat analysis system that batch-runs many simulations and detects balance issues by measuring win rates, damage output, and survivability.

---

## Use Cases

### 1. Luna (lvl 14 Priest) vs Vaelith Moonveil (lvl 9 Fighter)

- Luna should win ~95% of fights (higher level, better gear, healing)
- Currently Vaelith wins too often — detect actual rate and tune

### 2. Target Golem (lvl 14) vs Practice Dummy (lvl 10)

- Reference match for calibration
- Golem uses spells + melee, Dummy is passive target
- Measure hit rates, damage distribution, spell effectiveness

---

## Approach

Use the existing `CombatSimulator` with real service stack (no mocks). Batch-run N simulations (e.g. 1000) per matchup, collect aggregate stats.

### Stats per matchup

| Metric | Description |
|--------|-------------|
| Win rate | % of fights won by each side |
| Avg ticks | Average combat duration |
| Avg damage dealt | Per combatant |
| Hit rate | % of attacks/spells that hit |
| Crit rate | % of attacks that crit |
| Fumble rate | % of attacks that fumble |
| Spell cast count | How often each spell was used |
| Healing done | Total HP restored |

---

## Implementation

Create `CombatBenchmarkTests.cs` in `UnitTests/Diagnostics/` — a diagnostic test that:
1. Loads characters from the same data/builders as existing diagnostic tests
2. Runs N simulations per matchup
3. Aggregates stats
4. Prints results via `ITestOutputHelper`
5. Fails if win rate is outside expected bounds (e.g. Luna win rate < 80%)

---

## Tuning

After identifying balance issues, tune:
- Damage formulas
- Hit/crit rates
- Spell mana costs
- Character stats (HP, StrikeRating, etc.)
- AI decision weights

Re-run benchmark to verify fix.

---

## Acceptance Criteria

- [ ] Luna vs Vaelith 1000x benchmark runs and reports win rate
- [ ] Golem vs Dummy 1000x benchmark runs and reports stats
- [ ] Results are printed to test output and logged to file
- [ ] Deviations from expected balance are flagged
- [ ] Tool can be re-run after tuning changes
