# Modern D&D System — THAC0 Regression Guard
#
# BattleArena uses the modern D&D 5e combat system, NOT the old AD&D THAC0 system.
#
# THAC0 (AD&D — WRONG for BattleArena):
#   - Lower THAC0 score = better attacker   (Mage THAC0=20 = worst, Fighter THAC0=18)
#   - Lower ArmorClass  = harder to hit     (negative AC = near-invulnerable)
#   - Level progression DECREASES THAC0     (subtraction = improvement)
#
# Modern D&D 5e (CORRECT for BattleArena):
#   - Higher StrikeRating = better attacker (Mage SR ~4–7, Fighter SR ~12–17)
#   - Higher ArmorClass   = harder to hit   (Plate AC=18, Unarmored AC=10)
#   - Level progression INCREASES SR        (addition = improvement)
#
# If any scenario in this file fails, a THAC0 regression has been introduced.

Feature: Modern D&D System — THAC0 Regression Guard
    As a game developer
    I want permanent automated verification of modern D&D 5e attack semantics
    So that any THAC0-style regression is caught immediately

    @modern-dnd
    # Hit rate must move in the correct modern D&D direction across the SR and AC scale.
    # Rows 1–2 verify SR direction: Fighter-tier SR=17 hits far more often than Mage-tier SR=4.
    # Rows 3–4 verify AC direction: a low-AC target is hit far more often than plate-armored.
    # Under THAC0, rows 1–2 would be inverted; rows 3–4 would also be inverted.
    # Bounds are conservative (>3-sigma margin). Theoretical hit rates in parentheses.
    Scenario Outline: Hit rate reflects modern D&D 5e semantics across the SR and AC scale
        Given a distribution attacker at level 1 with strength 10 and strike rating <sr>
        And the distribution attacker wields an unarmed strike
        And a distribution defender with armor class <ac> and dexterity 10
        When 1000 attacks are resolved
        Then the total hit count should be between <min_hits> and <max_hits>

        Examples:
            | sr | ac | min_hits | max_hits |
            | 17 | 12 |      600 |      850 |
            |  4 | 12 |      130 |      320 |
            | 10 |  6 |      400 |      600 |
            | 10 | 18 |       30 |      320 |

    @modern-dnd
    # SR gain is strictly additive (positive, growing with level) — not subtractive like THAC0.
    # Each archetype gains SR faster or slower based on martial role.
    # The gain value at each level must exactly match the modern D&D progression formula.
    Scenario Outline: Strike rating level gain increases additively with level for every archetype
        When the SR gain for a <archetype> character at level <level> is computed
        Then the SR gain should be <expected_gain>

        Examples:
            | archetype | level | expected_gain |
            | martial   |     1 |             0 |
            | martial   |     3 |             1 |
            | martial   |    11 |             5 |
            | martial   |    20 |             9 |
            | caster    |     1 |             0 |
            | caster    |     5 |             1 |
            | caster    |    20 |             4 |
            | hybrid    |     1 |             0 |
            | hybrid    |     4 |             1 |
            | hybrid    |    20 |             6 |

    @modern-dnd
    # Martial classes always gain SR faster than caster classes at every level.
    # This directly guards against THAC0 class ordering where the roles were reversed.
    Scenario Outline: Martial SR gain always exceeds caster SR gain at the same level
        When the SR gain for a martial character at level <level> is computed
        And the SR gain for a caster character at level <level> is computed
        Then the martial SR gain should exceed the caster SR gain

        Examples:
            | level |
            |     5 |
            |    10 |
            |    20 |
