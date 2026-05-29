# Demo GUI Data Contract
#
# This feature validates that the BattleArena combat simulation produces data that
# satisfies every field required by the GUI display contract (gui-display-contract.json).
#
# Scope: DATA model contract — we verify that CombatResult, CombatLogEntry, and
#        Character objects carry all the values the GUI needs to render. Layout and
#        exact phrasing are intentionally out of scope here; the JSON contract file
#        is the authoritative reference.
#
# Hit-label logic is extracted into CombatHitLabelService (Application layer) so
# the computation rules can be tested deterministically without touching the console.
#
Feature: Demo GUI Data Contract
    As a GUI developer
    I want the combat simulation to populate every field defined in the display contract
    So that I can build new GUI frontends confidently using a stable data specification

    Background:
        Given the GUI display contract is loaded from "gui-display-contract.json"

    # ── Character card ──────────────────────────────────────────────────────────

    Scenario: Character card fields are all available from the simulation result
        Given two standard fighters are set up for GUI contract testing
        When a GUI contract combat is simulated with 300 ticks
        Then the character card contract fields are satisfied for each combatant

    # ── Attack events ────────────────────────────────────────────────────────────

    Scenario: Every attack event carries all required display fields
        Given two standard fighters are set up for GUI contract testing
        When a GUI contract combat is simulated with 300 ticks
        Then all attack event contract fields are populated in the combat log

    # ── Damage events ────────────────────────────────────────────────────────────

    Scenario: Every damage event carries all required display fields
        Given two standard fighters are set up for GUI contract testing
        When a GUI contract combat is simulated with 300 ticks
        Then all damage event contract fields are populated in the combat log

    # ── Combat summary ───────────────────────────────────────────────────────────

    Scenario: The combat summary carries all required display fields
        Given two standard fighters are set up for GUI contract testing
        When a GUI contract combat is simulated with 300 ticks
        Then the combat summary contract fields are all populated

    # ── Hit severity labels ──────────────────────────────────────────────────────
    #
    # CombatHitLabelService.GetLabel(damage, targetMaxHp) must return the
    # exact label string defined in the contract for each damage / max-HP band.
    # Boundary values are tested explicitly.
    #
    Scenario Outline: Hit severity label matches the damage percentage threshold defined in the contract
        When the hit severity label is computed for <damage> damage against a target with <maxHp> max HP
        Then the hit label should be "<expectedLabel>"

        Examples:
            | damage | maxHp | expectedLabel |
            |      1 |   100 | GRAZE         |
            |      2 |   100 | GRAZE         |
            |      3 |   100 | GLANCING HIT  |
            |      7 |   100 | GLANCING HIT  |
            |      8 |   100 | SOLID HIT     |
            |     14 |   100 | SOLID HIT     |
            |     15 |   100 | HEAVY HIT     |
            |     24 |   100 | HEAVY HIT     |
            |     25 |   100 | CRUSHING HIT  |
            |     50 |   100 | CRUSHING HIT  |
            |      1 |    10 | SOLID HIT     |
            |      3 |    10 | CRUSHING HIT  |
