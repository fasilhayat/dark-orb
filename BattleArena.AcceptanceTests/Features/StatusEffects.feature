# Combat System — Status Effects
#
# Status effects are applied to characters during combat and modify their stats
# for a number of turns.  Three stacking rules control how duplicate effects
# from the same or different sources interact:
#
#   NoStack     → a second application of the same effect name is ignored
#   HighestWins → the weaker instance is replaced by the stronger one
#   Stack       → multiple instances coexist, but only from different sources
#
# Duration ticking:
#   Each tick decrements Duration by 1.  When Duration reaches 0 the effect
#   is removed automatically.  Effects with Duration = 0 are permanent and
#   are never ticked down.
Feature: Combat — Status Effects
    As a game designer
    I want status effects to stack, replace, or block based on defined rules
    So that buff and debuff interactions are predictable and exploitable

    @status-effects
    # NoStack means the second application is silently discarded.
    # Applying "Bless" twice from the same source should result in exactly one instance.
    Scenario: NoStack prevents a second application of the same effect
        Given a fresh character with no status effects
        # First application — accepted because no existing "Bless"
        When the effect "Bless" with NoStack rule and magnitude 3 from "priest" is applied
        # Second application — rejected because "Bless" already exists
        And the effect "Bless" with NoStack rule and magnitude 3 from "priest" is applied
        Then the character should have 1 active status effect

    @status-effects
    # HighestWins keeps only the strongest instance of an effect by name.
    # Magnitude 2 is applied first, then magnitude 5 — the weaker instance is evicted.
    Scenario: HighestWins replaces a weaker instance with a stronger one
        Given a fresh character with no status effects
        # Weaker "Shield" applied first
        When the effect "Shield" with HighestWins rule and magnitude 2 from "spell-a" is applied
        # Stronger "Shield" replaces it
        And the effect "Shield" with HighestWins rule and magnitude 5 from "spell-b" is applied
        Then the character should have 1 active status effect
        And the active effect "Shield" should have magnitude 5

    @status-effects
    # HighestWins does NOT replace an existing effect if the new one is weaker.
    # Applying magnitude 5 first and then magnitude 2 should keep the original.
    Scenario: HighestWins keeps the existing effect when the new one is weaker
        Given a fresh character with no status effects
        When the effect "Shield" with HighestWins rule and magnitude 5 from "spell-a" is applied
        And the effect "Shield" with HighestWins rule and magnitude 2 from "spell-b" is applied
        Then the character should have 1 active status effect
        And the active effect "Shield" should have magnitude 5

    @status-effects
    # Stack allows the same effect name from different sources to coexist.
    # Same source is blocked (enemy-a applied twice → 1 instance),
    # then a second source adds a new instance (enemy-b → 2 total).
    Scenario: Stack allows the same effect from different sources but blocks the same source
        Given a fresh character with no status effects
        # First from enemy-a — accepted
        When the effect "Bleed" with Stack rule and magnitude 1 from "enemy-a" is applied
        # Second from enemy-a — rejected (same source already present)
        And the effect "Bleed" with Stack rule and magnitude 1 from "enemy-a" is applied
        # From enemy-b — accepted (different source)
        And the effect "Bleed" with Stack rule and magnitude 1 from "enemy-b" is applied
        Then the character should have 2 active status effects

    @status-effects
    # Duration ticking decrements each effect by 1 per tick.
    # "Haste" started at 2 → becomes 1 (remains active).
    # "Burn" started at 1 → reaches 0 → removed automatically.
    Scenario: Effects expire when their duration reaches zero after ticking
        Given a fresh character with active effect "Haste" lasting 2 turns
        And the character also has active effect "Burn" lasting 1 turn
        # One tick passes
        When status effects tick once
        Then the character should have 1 active status effect
        And the active effect "Haste" should have 1 turn remaining
        And the active effect "Burn" should have expired

    @status-effects
    # Duration 0 marks an effect as permanent — the tick loop skips it entirely.
    # No matter how many ticks pass, the effect stays on the character.
    Scenario: Permanent effects with duration zero are never removed by ticking
        Given a fresh character with active effect "Aura" lasting 0 turns
        # Three ticks should not affect a permanent effect
        When status effects tick once
        And status effects tick once
        And status effects tick once
        Then the character should have 1 active status effect
        And the active effect "Aura" should have 0 turns remaining
