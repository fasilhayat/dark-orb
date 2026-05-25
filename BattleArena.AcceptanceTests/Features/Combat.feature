Feature: Combat System
    As a game designer
    I want to resolve attacks and damage
    So that combat follows the AD&D-inspired rules

    Background:
        Given a character with strength 14 and strike rating 19
        And a weapon named "Longsword" with D8 damage die and +0 attack bonus

    @ability
    Scenario: Calculate ability modifier for various ability scores
        When the ability modifier is calculated for score 8
        Then the modifier should be -1
        When the ability modifier is calculated for score 10
        Then the modifier should be 0
        When the ability modifier is calculated for score 14
        Then the modifier should be 2
        When the ability modifier is calculated for score 18
        Then the modifier should be 4
        When the ability modifier is calculated for score 20
        Then the modifier should be 5

    @attack
    Scenario: Successful melee attack hits and deals damage with strength bonus
        Given the D20 roll is 12
        And the damage die roll is 5
        When the character attacks a target with armor class 5
        Then the attack should hit
        And the hit roll should be 12
        And the damage should be 7
        And the weapon used should be "Longsword"

    @attack
    Scenario: Attack misses when roll is too low to beat the target armor class
        Given the D20 roll is 5
        And the damage die roll is 3
        When the character attacks a target with armor class 10
        Then the attack should miss
        And the damage should be 0

    @attack
    Scenario: Damage cannot go below zero even with a negative strength modifier
        Given a character with strength 6 and strike rating 19
        And the D20 roll is 20
        And the damage die roll is 1
        When the character attacks a target with armor class 5
        Then the attack should hit
        And the damage should be 0

    @attack
    Scenario: Lower strike rating makes attacks more likely to hit
        Given a character with strength 16 and strike rating 15
        And the D20 roll is 8
        And the damage die roll is 6
        When the character attacks a target with armor class 5
        Then the attack should hit
        And the damage should be 9

    @attack
    Scenario: Weapon attack bonus improves hit chance
        Given a character with strength 10 and strike rating 19
        And a weapon named "Soul Reaver" with D12 damage die and +3 attack bonus
        And the D20 roll is 10
        And the damage die roll is 7
        When the character attacks a target with armor class 5
        Then the attack should hit
        And the damage should be 7

    @attack
    Scenario: High armor class makes attacks miss
        Given the D20 roll is 10
        And the damage die roll is 4
        When the character attacks a target with armor class 15
        Then the attack should miss
        And the damage should be 0

    @damage
    Scenario: Roll damage for a weapon
        Given the damage die roll is 7
        When the character rolls damage for the weapon
        Then the damage result should be 7
        And the damage die type should be D8

    @damage
    Scenario: Roll damage for a weapon with different die
        Given a weapon named "Dagger" with D4 damage die and +0 attack bonus
        And the damage die roll is 3
        When the character rolls damage for the weapon
        Then the damage result should be 3
        And the damage die type should be D4
