---
name: combat-log-analysis
description: Use when the user shares a combat log file and asks why something happened — sustained misses, zero damage, never casts spells, NOMANA messages, or unexpected outcomes. Load this skill before analysing any combat log.
---

# Combat Log Analysis

Standardised procedure for analysing a combat log when the user reports unexpected behaviour.

---

## 1. Read the log header

Check the following fields in order:

| Field | What to check |
|-------|---------------|
| `Mode` | Turn-based (manual decisions) or Auto (AI decisions). This determines whether to blame the decision source or the user. |
| `Seed` | Note the seed for reproducibility. |
| **Party composition** | Levels, HP, STR/DEX/INT, armor (AC + mitigation), weapon/spell attack details. |

---

## 2. Identify the symptom

Ask: *what did the user expect vs what happened?*

Common symptoms:
- **Sustained misses** — check `Attack Power (AP)` vs `Defense Power (DP)` on each miss
- **Zero damage on hit** — check `mitigation` and `elemental_damage` vs resistance
- **NOMANA log entries** — check `CurrentMana` vs `ManaCost` of each memorised spell
- **Never casts spells** — in **Turn-based** mode, the user chose the attack; in **Auto** mode, check `AutoActionDecisionSource` logic
- **Never acts / skipped turns** — check CC effects (Stun, Fear, Root) and TM accumulation
- **Dies too fast** — check incoming damage vs max HP, mitigation, and any defensive buffs

---

## 3. Classify the root cause

| Category | What to look for |
|----------|-----------------|
| **Stats / gear mismatch** | AP too low to beat DP; damage below mitigation threshold; wrong damage type vs resistance |
| **Mana constraint** | Spell cost exceeds current mana (check regen rate, max mana, spell costs in seed data) |
| **Decision logic** | Turn-based: user chose the action. Auto: check `AutoActionDecisionSource` (defaultAttack bypass, spell selection, mana check) |
| **TM starvation** | TM gain too slow to act; TM penalty from armor; opponent TM far ahead |
| **CC lock** | Stun, Fear, or Root preventing action; check `SkippedTurn` events and resistance rolls |
| **Bug / regression** | Compare against prior working logs; look for recent code changes |

---

## 4. Trace a single turn

To understand why an attack missed or hit:

```
Attack   d20_atk=11  d20_def=15  +AP  15  vs DP  17  total=26  -> MISS
```

Formula: `total = d20_atk + AP` vs `d20_def + DP`
- Attack total = 11 + 15 = **26**
- Defense total = 15 + 17 = **32**
- 26 < 32 → **MISS**

If the result is unexpected, check where the values come from:

| Value | Source |
|-------|--------|
| `d20_atk` / `d20_def` | Raw d20 roll (displayed) |
| `AP` (Attack Power) | `CombatStats.ComputeAttackerStats()` — uses STR/DEX, level, weapon bonuses, buffs |
| `DP` (Defense Power) | `CombatStats.ComputeDefenderStats()` — uses AC, DEX, level, armor bonuses, buffs |

---

## 5. Trace damage on hit

```
Damage   roll(3) + attr(-2) + flat(0) + lvl(2) = 3 x1.0 - mit(4) + elem(0) = 0
```

Formula: `(roll + attrBonus + flatBonus + levelBonus) × dmgPower - mitigation + elementalDamage`

| Term | Source |
|------|--------|
| `roll` | Raw damage die roll |
| `attr` | Attacking stat modifier (STR for melee, INT for spells) |
| `flat` | `flat_damage_bonus` from weapon/spell |
| `lvl` | `Level / 2` (int division) |
| `dmgPower` | Damage power multiplier (1.0 normal, 3.0 devastating) |
| `mit` | Target's armor mitigation |
| `elem` | Elemental damage (bypasses physical mitigation) |

If `mit` ≥ total before subtraction → **0 damage**. This is common when physical weapons hit heavily armoured targets.

---

## 6. Check spellcasting in Auto mode

When `Mode: Auto` and a caster never casts:

1. Read `AutoActionDecisionSource.ChooseAttackAsync()`
2. Does `defaultAttack` bypass spells? (line 22: `if (defaultAttack is not null) return defaultAttack;`)
3. Does the character have an equipped weapon that becomes their `AttackSource`?
4. Does the random spell pick pass the mana check (`actor.CurrentMana < spell.ManaCost`)?

For API characters, equipped weapons set `AttackSource` to non-null. The Demo local path (`GetAttackSource`) returns `null` for spellcasters, but DB-loaded characters keep their weapon.

---

## 7. Compare with prior logs

When investigating a regression:

1. Find a **working** combat log (same or similar matchup) and a **broken** one
2. Compare:
   - `Mode`, participants, levels, gear
   - AP/DP values for the same characters
   - Damage formula breakdowns
   - Hit/miss ratios
3. Look for recent commits that touched:
   - `CombatService.cs` (damage formula, AP/DP computation)
   - `AutoActionDecisionSource.cs` (decision logic)
   - `Character.cs` (stat computation)
   - `*.sql` seed files (gear, spells, character stats)
   - `Demo.Data.cs` (local demo character/spell data)

---

## 8. Cross-reference seed data

When stats seem wrong, check the source:

| Data | File |
|------|------|
| Spells (mana cost, damage dice, attack bonus) | `02-seed-data.sql` (lines 720–760) or `Demo.Data.cs` |
| Characters (stats, HP, mana) | `03-characters.sql` or `Demo.Data.cs` |
| Armor (AC, mitigation) | `02-seed-data.sql` or `Demo.Data.cs` |
| Weapons (damage, attack bonus) | `02-seed-data.sql` or `Demo.Data.cs` |

---

## 9. Common pitfalls

- **NOMANA in Turn-based mode**: The user chose a non-spell attack. The message is informational — the system detected a spellcaster using a weapon. Not a bug.
- **0 damage on hit**: Physical damage vs mitigation. Check if the attacker has spells with elemental damage that bypasses mitigation.
- **Caster never uses spells in Auto mode**: The `defaultAttack` (equipped weapon) is non-null and short-circuits spell selection. Fixed by checking spells before defaultAttack.
- **Probabilistic test failure**: Distribution tests (fumble rate, hit rate) can flake 1–2 % above/below bounds on a single run. Re-run before investigating.
