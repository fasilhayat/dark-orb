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
    # A character with no resistance sources has 0 computed resistance.
    Scenario: Character with no resistance sources has zero computed resistance
        Given a character with no resistance sources
        Then the character's computed magic resistance should be 0

    @resistance
    # A racial feat granting magic resistance is picked up by ComputeResistance.
    Scenario: Racial feat contributes to magic resistance
        Given a character with a racial feat granting 25 magic resistance
        Then the character's computed magic resistance should be 25

    @resistance
    # A single armor piece with fire resistance is reflected correctly.
    Scenario: Armor piece contributes to elemental resistance
        Given a character wearing armor with 30 fire resistance
        Then the character's computed fire resistance should be 30

    @resistance
    # Two armor pieces each contributing fire resistance are summed.
    Scenario: Multiple armor pieces with the same resistance type are summed
        Given a character wearing armor with 15 fire resistance
        And the character also wears boots with 10 fire resistance
        Then the character's computed fire resistance should be 25

    @resistance
    # An active protective buff grants additional resistance.
    Scenario: Active protective buff contributes to magic resistance
        Given a character with no resistance sources
        When an Arcane Ward buff granting 30 magic resistance is applied to the character
        Then the character's computed magic resistance should be 30

    @resistance
    # Resistance from all three sources stacks before being capped.
    Scenario: All three resistance sources stack correctly
        Given a character with a racial feat granting 15 magic resistance
        And the character wears chest armor with 20 magic resistance
        And the character has an active buff granting 10 magic resistance
        Then the character's computed magic resistance should be 45

    @resistance
    # The cap at 95 prevents a character from becoming fully immune.
    Scenario: Combined resistance is capped at 95
        Given a character with a racial feat granting 25 magic resistance
        And the character wears chest armor with 20 magic resistance
        And the character has an active buff granting 60 magic resistance
        Then the character's computed magic resistance should be 95

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
