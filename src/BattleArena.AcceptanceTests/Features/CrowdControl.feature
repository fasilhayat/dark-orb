# Combat System — Crowd Control Effects
#
# CC effects (Stun, Fear, Root) prevent a character from acting on their turn.
# When a CC'd character's turn meter reaches 100, they generate a SkippedTurn
# event instead of attacking.
#
# Stun: magical paralysis (ResistanceType.Magic)
# Fear: fright effect (ResistanceType.Fear)
# Root: movement lock (ResistanceType.Physical)
#
# All three produce the same gameplay outcome: the character loses their turn.

Feature: Combat — Crowd Control
    As a game designer
    I want crowd-control effects to cause skipped turns
    So that status effects can shape the flow of combat

    @crowdcontrol
    Scenario: Stunned character skips their turn
        Given a Fighter named "Theron" with level 5, strength 18, dexterity 12, strike rating 14, turn speed 10, and 50 hit points
        And "Theron" wields a "Longsword" dealing 1d8 Slashing damage with attack bonus 2
        And "Theron" wears "Chain Mail" with armor class 16 and mitigation 2
        And a Fighter named "Gruk" with level 3, strength 16, dexterity 8, strike rating 16, turn speed 6, and 35 hit points
        And "Gruk" wields a "Battle Axe" dealing 1d8 Slashing damage with attack bonus 1
        And "Gruk" wears "Leather Armor" with armor class 11 and mitigation 1
        And "Gruk" is afflicted with a Stun status effect lasting 3 turns
        When the combat is simulated with a maximum of 300 ticks
        Then the combat log should contain at least one "SkippedTurn" event

    @crowdcontrol
    Scenario: Fear effect causes skipped turn
        Given a Fighter named "Theron" with level 5, strength 18, dexterity 12, strike rating 14, turn speed 10, and 50 hit points
        And "Theron" wields a "Longsword" dealing 1d8 Slashing damage with attack bonus 2
        And "Theron" wears "Chain Mail" with armor class 16 and mitigation 2
        And a Fighter named "Gruk" with level 3, strength 16, dexterity 8, strike rating 16, turn speed 6, and 35 hit points
        And "Gruk" wields a "Battle Axe" dealing 1d8 Slashing damage with attack bonus 1
        And "Gruk" wears "Leather Armor" with armor class 11 and mitigation 1
        And "Gruk" is afflicted with a Fear status effect lasting 2 turns
        When the combat is simulated with a maximum of 300 ticks
        Then the combat log should contain at least one "SkippedTurn" event

    @crowdcontrol
    Scenario: CC effect expires and character acts normally
        Given a Fighter named "Theron" with level 3, strength 14, dexterity 12, strike rating 12, turn speed 10, and 40 hit points
        And "Theron" wields a "Shortsword" dealing 1d6 Piercing damage with attack bonus 0
        And "Theron" wears "Leather Armor" with armor class 11 and mitigation 1
        And a Fighter named "Gruk" with level 5, strength 16, dexterity 8, strike rating 16, turn speed 6, and 50 hit points
        And "Gruk" wields a "Battle Axe" dealing 1d8 Slashing damage with attack bonus 1
        And "Gruk" wears "Chain Mail" with armor class 16 and mitigation 2
        And "Gruk" is afflicted with a Stun status effect lasting 1 turn
        When the combat is simulated with a maximum of 300 ticks
        Then the combat log should contain an "EffectExpired" event for the Stun effect
        And the combat log should contain at least one "Attack" event from "Gruk"
