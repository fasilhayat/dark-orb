# Combat System — Elemental Afterburn
#
# When a spell has an elemental type (Fire, Ice, Lightning, etc.) and hits a
# target, the afterburn system automatically applies a matching damage-over-time
# status effect.  These scenarios validate that the effect is applied, ticks
# damage, and expires normally.
#
# Dice are mocked so assertions are deterministic.
Feature: Combat — Elemental Afterburn
    As a game designer
    I want elemental spells to leave afterburn DoT effects on their targets
    So that fire burns, ice chills, and lightning shocks

    @afterburn
    Scenario: Fire spell applies Burning afterburn and deals tick damage
        Given a spellcaster named "Lyra" with intelligence 18
        And a spell "Fireball" dealing 3d6 fire damage with fire elemental type
        And a target "Dummy" with 100 hit points and turn speed 50
        And the D20 roll is 15
        And the D100 roll is 50
        And the damage die roll is 4
        When the combat is simulated for 50 ticks
        Then the combat log should contain an EffectApplied event for "Burning" on "Dummy"
        And the combat log should contain DoTTick events for "Burning" on "Dummy" with damage dealt
        And the DoTTick messages should read like "Dummy suffers (\d+) Burning damage."

    @afterburn
    Scenario: Non-elemental spell does not apply afterburn
        Given a spellcaster named "Lyra" with intelligence 18
        And a spell "Magic Dart" dealing 2d4 Psychic damage with no elemental type
        And a target "Dummy" with 100 hit points and turn speed 50
        And the D20 roll is 15
        And the D100 roll is 50
        And the damage die roll is 4
        When the combat is simulated for 50 ticks
        Then the combat log should contain no afterburn EffectApplied events
