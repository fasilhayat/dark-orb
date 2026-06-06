# Bug Fix - Mana Overgeneration When Resource is Full

Project: Dark Orb

---

## Problem

Mana continues to generate even when the mana meter has reached its maximum capacity.

This results in:

* Mana values exceeding intended maximum limits (overflow or hidden overcap state)
* Inconsistent combat balance (units effectively gaining resources when they should not)
* Misleading UI behavior if overflow is not visually represented

---

## Expected Behavior

Mana generation must only occur when current mana is strictly below the maximum mana capacity.

When mana is at maximum:

* No additional mana is generated
* No regeneration ticks should be applied
* No incremental updates should occur

When mana is below maximum:

* Mana regeneration proceeds normally until cap is reached

---

## Required Fix

Introduce a strict guard condition in the mana regeneration logic:

* Before applying any mana gain:

  * Validate `currentMana < maxMana`
* If false:

  * Skip regeneration entirely for that tick
* Ensure no cumulative overflow occurs due to delayed or batched updates

Additionally:

* Clamp mana value to max as a safety fallback
* Prevent asynchronous or tick-based systems from applying stale regen events after cap is reached

---

## Acceptance Criteria

### Scenario 1

**Given** a character has full mana

**When** a mana regeneration tick occurs

**Then** no mana is added

**And** mana remains at maximum

---

### Scenario 2

**Given** a character is below maximum mana

**When** regeneration ticks occur

**Then** mana increases normally

**And** stops exactly at maximum without exceeding it

---

### Scenario 3

**Given** mana reaches maximum during a regeneration cycle

**When** subsequent pending regen ticks are processed

**Then** they do not apply additional mana

**And** no overflow or delayed accumulation occurs

---

## Validation Checklist

* [ ] Mana does not increase at full capacity
* [ ] Mana increases correctly when below cap
* [ ] No overflow beyond max mana occurs
* [ ] No delayed tick-based mana accumulation exists
* [ ] UI consistently reflects actual mana value

---

## Notes

Pay special attention to:

* Tick-based regeneration loops
* Event-driven mana updates
* Buff/debuff modifiers affecting regeneration rate

Ensure all mana gain entry points respect the same cap guard condition.
