# Feature Design - Turn-Based Action Selection System

Project: Dark Orb

File: `feature-turn-action-selection.md`

Status: Post-Implementation Retrospective

Priority: Core Combat UX System

---

## Objective

Introduce a structured **turn-based action selection system** that defines what a character is allowed to do when their turn is ready.

At minimum, the system must support:

- Basic attack action
- Movement action (placeholder for future movement system)
- Spell casting actions (dynamic based on character loadout)

This system becomes the foundation for all future combat interaction expansion.

---

## Current Problem

Combat currently assumes implicit actions during a turn (attack/spell execution is triggered without a formal decision layer).

This creates issues:

- No explicit player decision structure
- No extensible action system
- No clear UI action state
- Hard to integrate future systems (movement, abilities, items, reactions)

---

## Design Goal

Every turn must explicitly present the player (or AI) with a **set of valid actions** derived from the current game state.

The system must answer:

> "What can this character do right now?"

---

## Core Concept

Introduce an **Action Menu Layer** triggered when a character reaches:

```text
Turn Ready State
```

At this point, the system must generate a list of available actions.

---

## Action Types (Initial Version)

### 1. Attack Action

Always available unless explicitly disabled.

Behavior:

- Uses equipped weapon
- Executes standard combat resolution
- Consumes turn action

Example UI label:

```text
Attack
```

---

### 2. Move Action (Placeholder)

Always available, but currently not implemented in gameplay logic.

Behavior:

- Reserved slot for future movement system integration
- Should appear in UI but be disabled or marked "Not Implemented"

Example UI label:

```text
Move (Not Implemented)
```

OR

```text
Move (Disabled)
```

---

### 3. Spell Actions (Dynamic)

Only visible if character has available spells.

Each spell becomes an individual selectable action.

Example:

```text
Smite
Heal
Fireball
Ice Storm
```

Each spell action must include:

- Mana cost
- Range requirement (future extension)
- Target requirement
- Status effects preview (future extension)

---

## Action Availability Rules

### Attack

- Always available
- Unless character is incapacitated (stun/root/fear future extension)

---

### Move

- Always visible
- Execution blocked until movement system exists

---

### Spells

Only visible if:

- Spell exists in character loadout
- Character has sufficient mana (optional UI hinting)
- Spell is not on cooldown (future system)

---

## Turn Flow Integration

When a character reaches turn readiness:

```text
Turn Meter Full → Action Selection Phase → Action Execution → Turn Ends
```

---

### Phase Breakdown

#### 1. Turn Ready

Character becomes eligible to act.

---

#### 2. Action Selection Phase

UI displays:

- Attack button
- Move button (disabled or placeholder)
- Spell list (if any)

No automatic execution allowed.

---

#### 3. Action Execution

Selected action is executed:

- Attack → Combat resolution
- Spell → Spell system resolution
- Move → future movement system hook

---

#### 4. Turn End

Turn Meter resets or updates according to existing system rules.

---

## UI Requirements

### Action Panel

Must dynamically render:

- Attack action
- Move action placeholder
- Spell list (scrollable if needed)

---

### Spell Entries

Each spell must display:

- Name
- Mana cost
- Optional icon
- Optional damage/effect preview (future enhancement)

---

### Disabled States

Move action must clearly indicate:

```text
Not implemented
```

or

```text
Coming soon
```

No hidden UI elements allowed.

---

## AI Behavior Integration

AI must use same action selection system:

- Evaluate valid actions
- Choose optimal action
- Respect mana constraints
- Respect status effects (future)

No separate AI-only logic paths allowed.

---

## Extensibility Requirements

This system must support future additions without redesign:

### Future Action Types

- Movement actions (full system)
- Item usage
- Defensive stance
- Reaction abilities
- Charge / Dash / Flee
- Environmental interactions

---

## Combat System Constraints

This feature must NOT:

- Change combat math
- Modify turn meter logic
- Alter damage calculation
- Alter spell effects
- Introduce randomness

It is purely a **decision layer on top of existing combat engine**.

---

## Logging Requirements

Action selection must be logged:

```text
ACTION SELECTED: High Priestess Luna chooses Smite
ACTION SELECTED: Sister Elira Vane chooses Heal
```

Future extension:

- Track unused actions
- Track AI decision confidence (optional)

---

## Replay System Impact

Action selection must be recorded in replay data:

- Chosen action type
- Selected spell (if applicable)
- Target selection (future extension)

This ensures full determinism in replay.

---

## Edge Cases

### No Spells Available

Only show:

- Attack
- Move (disabled)

---

### Stunned / Rooted (future status system)

Action selection may be restricted:

- Attack disabled
- Move disabled
- Only pass turn allowed (future feature)

---

### Dead Character

No actions available.

---

## Acceptance Criteria

- [ ] Turn-based action selection phase exists
- [ ] Attack action always available
- [ ] Move action displayed as placeholder
- [ ] Spell list dynamically generated per character
- [ ] Spells show correct mana costs
- [ ] Action selection is required before execution
- [ ] AI uses same action system as player
- [ ] No changes to combat mechanics
- [ ] Logging captures selected actions
- [ ] Replay system stores action decisions
- [ ] System supports future extensibility

---

## Retrospective — Implementation Attempt (Jun 2026)

### What Was Implemented

The following changes were committed and retained:

- **`IActionDecisionSource` interface** — `ChooseActionAsync` signature added with `availableActions`, `allies`, `engagementRange` parameters
- **`AutoActionDecisionSource`** — flattens spell category children via `SelectMany` for AI selection
- **`TurnProcessor.BuildAvailableActions`** — groups character spells under a single `Spells` category entry with sub-children, logs `ActionSelected` after choice
- **`ConsoleActionDecisionSource`** — updated to new interface
- **`MainWindowViewModel`** — `ActiveActorName` property added for double-border highlight
- **`MainWindow.axaml` / `.axaml.cs`** — action panel layout updated for spell category drill-down

### What Was Reverted / Not Kept

The following files were created during the session but removed (not committed). Future implementation must recreate them:

- **`ActionLabels.cs`** — All display label constants (`AttackFormat`, `MoveFormat`, `SpellsCategoryLabel`, `BackLabel`, `ActionSelectedFormat`, `ManaSuffixFormat`, etc.)
- **`AvailableAction.cs`** — Model with `IsSpellCategory`, `Children`, `IsBack` properties for hierarchical action menus
- **`GuiActionDecisionSource.cs`** — Single-wait pattern (`_actionWaitHandle` only), posts available actions to VM, sets `ActiveActorName`
- **`LiveCombatObserver.cs`** — Was intended for live turn-by-turn pipeline (reverted)
- **`NotBoolConverter.cs`** — Helper converter for UI bindings

### Key Lessons Learned

1. **Live observer pipeline was over-engineered.** The initial approach split `Simulate` into two phases (decision + execution) using `ILiveCombatObserver`. This introduced bugs and was reverted. Future implementations should stick with the existing `Simulate` + `PlayTurnBased` flow and inject decisions via `IActionDecisionSource.ChooseActionAsync` inside the existing `Simulate` Task.Run loop.

2. **Action panel orchestrator pattern works.** The `GuiActionDecisionSource` single-wait approach (signal `_actionWaitHandle` on UI click) is correct. Avoid adding a second wait handle (`_actionAdvanceHandle`) or `IsAutoMode` flag — keep it simple.

3. **Spell category drill-down needs `AvailableAction` model.** The hierarchical model (`IsSpellCategory` with `Children` + `IsBack` navigation) is required for spell sub-menus. Both player and AI paths flatten or navigate this tree.

4. **Action selection must be required.** No automatic action bypass — the panel must block execution until the player clicks a valid leaf action.

5. **Enemy team uses `AutoActionDecisionSource` directly** — no panel needed. Only the hero team triggers the GUI panel.

6. **Do not build a two-phase pipeline** (decision phase separate from execution phase). The architecture decision is: decisions happen inside `Simulate`, synchronously, via the `IActionDecisionSource` callback. Any attempt to split `Simulate` into separate decision + run calls will break the existing event/applier/processor chain.

7. **UI active-turn highlighting:** `ActiveActorName` should be set at the start of `ChooseActionAsync` and cleared when the action is chosen or the turn completes. The double-border ring is a visual-only concern — no game state changes needed.

### Requirements for Future Implementation

- Implement `AvailableAction` model (from scratch — not in commit) with `Name`, `Source` (spell), `IsSpellCategory`, `Children`, `IsBack`
- Implement `ActionLabels` constants class for all display strings
- Rebuild `GuiActionDecisionSource` with single-wait pattern only
- Wire action panel to spell sub-menu using `_actionMenuStack` (push/pop) with `Back` button
- Ensure all display text uses `ActionLabels` constants — no inline strings
- Keep `Simulate` intact — do not split into two-phase pipeline
- Add acceptance tests for spell sub-menu navigation