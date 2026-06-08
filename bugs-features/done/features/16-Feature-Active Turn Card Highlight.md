# Active Turn Card Highlight

Project: Dark Orb
File: `features/active-turn-card-highlight.md`

---

## Objective

When a combatant begins their turn — just before they perform any action (Attack, spell, heal, etc.) — their character card should be visually highlighted so the player can immediately identify who is acting.

Works in both **auto-play** and **turn-based** modes.

---

## Design

- Highlight = **gold border (#FFD700)**, border thickness increased from `2px` to `4px`
- Portrait border also thickens from `1px` to `2px`
- Highlight appears at the moment the turn flushes (right before the first action event) and persists through all events in that turn
- Cleared automatically when the next combatant's turn begins

---

## Implementation

| File | Change |
|------|--------|
| `BattleArena.Gui/ViewModels/MainWindowViewModel.cs` | Add `IsActiveTurn`, `BorderThickness`, `PortraitBorderThickness` to `CharCardViewModel`; update `BorderColor` cascade and `NotifyDerived` |
| `BattleArena.Gui/Views/CharacterCard.axaml` | Bind root `BorderThickness` and portrait `BorderThickness` to the new VM properties |
| `BattleArena.Gui/Presenters/AvaloniaCombatPresenter.cs` | In `RefreshScreen`, clear `IsActiveTurn` on all cards, then set on the active actor |

No changes to `ICombatPresenter` — `activeActorName` is already passed via `RefreshScreen`.

---

## Acceptance Criteria

- [x] White outer glow border appears on the active combatant's card before their first action
- [x] There is a visible gap (3px padding) between the outer glow border and the inner card border
- [x] Outer glow border uses `BoxShadow` for a glowing effect
- [x] Highlight clears when the next combatant's turn starts
- [x] Works in **turn-based** mode
- [x] Works in **auto-play** mode
- [x] No highlight on initial combat screen (tick 0)
- [x] No highlight on dead combatants
- [x] Does not flicker or flash incorrectly between turns
- [x] Does not interfere with existing border flash / persistent effect systems
- [x] Inner card border (team colors, persistent effects) is unaffected by the active turn state
