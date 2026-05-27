# Full Combat — End-to-End Combat Simulation
#
# This feature validates the complete combat pipeline end-to-end:
#   character creation → gear equipping → turnmeter-driven initiative →
#   attack resolution → HP tracking → combat log generation.
#
# Dice rolls are LIVE (non-deterministic), so we assert structural
# invariants that must hold regardless of which combatant wins:
#
#   ✔ The combat ends before the tick ceiling (both combatants can deal
#     lethal damage, so a winner is guaranteed within reasonable ticks)
#   ✔ Exactly one combatant has HP > 0 (the winner)
#   ✔ Exactly one combatant has HP ≤ 0 (the loser)
#   ✔ The combat log is non-empty and contains every expected event type
#
# Combat math reference for this scenario:
#   Fighter AttackPower ≈ (20-14) + 5 + 4 + 2 = 17
#   Orc     AttackPower ≈ (20-16) + 3 + 3 + 1 = 11
#   Fighter DefensePower ≈ (20-5) + 1 = 16   (Chain Mail AC5, DEX mod +1 capped)
#   Orc     DefensePower ≈ (20-7) + (-1) = 12 (Leather AC7, DEX mod -1)
Feature: Full Combat — End-to-End Combat Simulation
    As a game designer
    I want to run a complete geared combat between two combatants
    So that I can verify the combat loop ends correctly and produces a full combat log

    # ── Combatant profiles ─────────────────────────────────────────────────────
    #
    # FIGHTER — Theron
    #   Level 5, STR 18 (+4), DEX 12 (+1), StrikeRating 14, TurnSpeed 10, 50 HP
    #   Weapon:  Longsword  — 1d8 Slashing, +2 attack bonus
    #   Armor:   Chain Mail — ArmorClass 5 (AD&D raw), mitigation 2
    #
    # ORC — Gruk
    #   Level 3, STR 16 (+3), DEX 8 (-1),  StrikeRating 16, TurnSpeed 6,  35 HP
    #   Weapon:  Battle Axe   — 1d8 Slashing, +1 attack bonus
    #   Armor:   Leather Armor — ArmorClass 7 (AD&D raw), mitigation 1
    #
    # ArmorClass here is the raw AD&D value (lower = better).
    # EffectiveAC (used in DefensePower) = 20 - ArmorClass:
    #   Chain Mail  → EffectiveAC 15   Leather → EffectiveAC 13
    # ──────────────────────────────────────────────────────────────────────────
    Scenario: Geared Fighter enters combat with a Geared Orc to the death
        # Fighter setup
        Given a Fighter named "Theron" with level 5, strength 18, dexterity 12, strike rating 14, turn speed 10, and 50 hit points
        And "Theron" wields a "Longsword" dealing 1d8 Slashing damage with attack bonus 2
        And "Theron" wears "Chain Mail" with armor class 5 and mitigation 2

        # Orc setup
        And an Orc named "Gruk" with level 3, strength 16, dexterity 8, strike rating 16, turn speed 6, and 35 hit points
        And "Gruk" wields a "Battle Axe" dealing 1d8 Slashing damage with attack bonus 1
        And "Gruk" wears "Leather Armor" with armor class 7 and mitigation 1

        # Run the combat with a generous tick ceiling.
        # Given the stats above, average damage per hit is 6–8, so the combat
        # should resolve in far fewer than 500 ticks.
        When the combat is simulated with a maximum of 500 ticks

        # ── Structural assertions ──────────────────────────────────────────────
        # The combat MUST end before the tick ceiling (not a stalemate).
        Then the combat should have ended before the tick limit

        # Exactly one survivor — consistent HP accounting throughout.
        And the winning combatant should have hit points above zero
        And the losing combatant should have zero or fewer hit points

        # Log completeness — every event type must appear at least once.
        And the combat log should not be empty
        And the combat log should contain turnmeter gain events
        And the combat log should contain at least one attack event
        And the combat log should contain at least one damage event
