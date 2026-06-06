# Bug Fix - Character Selection Hover Cursor Does Not Indicate Clickability

Project: Dark Orb

---

## Problem

When hovering over selectable character cards in the UI, the mouse cursor remains unchanged.

This creates an inconsistent user experience because buttons throughout the application correctly change the cursor to indicate an interactive element, while character cards do not.

As a result, users receive no visual indication that character cards can be clicked or selected.

---

## Expected Behavior

Any character card that can be selected by the user should use the same cursor style as interactive buttons.

When the mouse enters a selectable character card:

* The cursor changes to the application's standard interactive cursor.
* The behavior matches existing button hover behavior.

When the mouse leaves the character card:

* The cursor returns to its normal state.

---

## Required Fix

Apply the same hover cursor styling currently used by buttons to all selectable character cards.

The implementation should:

* Reuse the existing button cursor style.
* Avoid introducing a separate cursor implementation.
* Ensure consistent behavior across all character selection screens.

Examples include:

* Combat attacker selection
* Combat defender selection
* Team selection screens
* Character roster screens
* Any future screen containing selectable character cards

---

## Acceptance Criteria

### Scenario 1

**Given** a selectable character card

**When** the mouse hovers over the card

**Then** the cursor changes to the same interactive cursor used by buttons

---

### Scenario 2

**Given** the mouse is hovering over a selectable character card

**When** the mouse leaves the card

**Then** the cursor returns to its default appearance

---

### Scenario 3

**Given** multiple selectable characters are displayed

**When** the user moves the cursor between them

**Then** the interactive cursor is shown consistently for all selectable characters

---

## Validation Checklist

* [ ] Character cards use the same cursor style as buttons
* [ ] Cursor changes immediately on hover
* [ ] Cursor resets correctly on mouse leave
* [ ] Behavior is consistent across all character selection screens
* [ ] No custom cursor implementations are introduced
* [ ] Existing button behavior remains unchanged

---

## Notes

This is a UI consistency issue rather than a functional defect. The goal is to improve discoverability and make character cards feel visually interactive in the same way buttons already do.
