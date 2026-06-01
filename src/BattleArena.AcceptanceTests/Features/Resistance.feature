# Resistance System — Effect Infliction
#
# Characters resist status effects through three layered sources:
#   1. Racial feats  (e.g. Elf "Magic Resistance" = 25)
#   2. Equipped armor pieces
#   3. Active protective status-effect buffs (e.g. Arcane Ward)
#
# Two-phase infliction roll:
#   Phase 1: ApplicationChance roll — if this fails the effect is silently ignored
#   Phase 2: Resistance roll (D100 ≤ resistance → effect is resisted, logged)
#
# Resistance is capped at 95 so there is always at least a 5 % infliction chance.
Feature: Resistance System — Effect Infliction
    As a game designer
    I want magical effects to be resistable based on a character's resistances
    So that racial traits, gear and protective spells have meaningful combat impact

    @resistance
    # ComputeResistance is the single source of truth for character resistances,
    # summing racial feats + equipped armor + active status-effect buffs,
    # then clamping to [0, 95].  A value of 0 means that source is absent.
    Scenario Outline: Magic resistance stacks correctly from all sources
        Given a character with no resistance sources
        And a racial feat granting <racial> magic resistance
        And chest armor with <chest> magic resistance
        And an active buff granting <buff> magic resistance
        Then the character's computed magic resistance should be <expected>

        Examples:
            | racial | chest | buff | expected |
            |      0 |     0 |    0 |        0 |
            |     25 |     0 |    0 |       25 |
            |      0 |     0 |   30 |       30 |
            |     15 |    20 |   10 |       45 |
            |     25 |    20 |   60 |       95 |

    @resistance
    # Fire resistance from multiple armor pieces is summed across slots.
    Scenario Outline: Fire resistance stacks from armor pieces
        Given a character wearing armor with <chest> fire resistance
        And the character also wears boots with <boots> fire resistance
        Then the character's computed fire resistance should be <expected>

        Examples:
            | chest | boots | expected |
            |    30 |     0 |       30 |
            |    15 |    10 |       25 |

    @resistance
    # Having fire resistance does not affect cold resistance computation.
    Scenario: Resistance of the wrong type does not count
        Given a character with a racial feat granting 30 fire resistance
        Then the character's computed cold resistance should be 0

    @resistance
    # With 0 resistance and 100 % application chance every attempt lands.
    Scenario: Zero resistance target receives every effect when application chance is 100%
        Given a target with 0 magic resistance
        When a magical effect with 100% application chance is applied 20 times
        Then all 20 applications should have landed

    @resistance
    # With high resistance most effects are resisted over many trials.
    Scenario: High resistance target resists most effects
        Given a target with 80 magic resistance
        When a magical effect with 100% application chance is applied 100 times
        Then at least 60 of the 100 applications should have been resisted

    @resistance
    # Even at the cap of 95 the effect can still slip through (5% chance).
    Scenario: Maximum resistance still allows some effects through
        Given a target with 95 magic resistance
        When a magical effect with 100% application chance is applied 200 times
        Then at least 1 application should have landed
