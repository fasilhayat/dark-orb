# Combat System — Critical Hits and Fumbles
#
# A natural roll (before modifiers) of 20 is a Critical Hit.
# A natural roll of 1 is a Fumble.
# These outcomes are determined BEFORE the normal hit/miss check, so they
# override armor class completely — a 20 always hits, a 1 always misses.
#
# Critical Hit effect:  base damage is doubled (before mitigation).
# Fumble effect:        automatic miss + -2 AttackPower penalty on attacker next turn.
Feature: Combat — Critical Hits and Fumbles
    As a game designer
    I want natural 20s and natural 1s to have special outcomes
    So that combat has exciting high and low points

    @critical
    # A natural 20 auto-hits regardless of how high the defender's armor class is.
    # Base damage = d8(6) + STR mod(0) = 6. Doubled on crit = 12.
    Scenario: Natural 20 always hits and doubles base damage
        Given a character with strength 10 and strike rating 19
        And a weapon named "Longsword" with D8 damage die and +0 attack bonus
        And the D20 roll is 20
        And the damage die roll is 6
        # Armor class 99 would be impossible to hit normally — the crit ignores it
        When the character attacks a target with armor class 99
        Then the attack should hit
        And the attack is a critical hit
        # (6 + 0) * 2 = 12
        And the damage should be 12

    @critical
    # Strength modifier is included in base damage before doubling.
    # STR 14 gives +2. Base = d6(4) + 2 = 6. Doubled = 12.
    Scenario: Critical hit doubles damage including the strength modifier
        Given a character with strength 14 and strike rating 19
        And a weapon named "Greatsword" with D6 damage die and +0 attack bonus
        And the D20 roll is 20
        And the damage die roll is 4
        When the character attacks a target with armor class 99
        Then the attack is a critical hit
        # (4 + 2) * 2 = 12
        And the damage should be 12

    @fumble
    # A natural 1 is always a fumble regardless of attacker strength or target armor.
    # Even a powerful warrior (STR 18, StrikeRating 10) fumbles on a 1.
    Scenario: Natural 1 is always a fumble regardless of attacker power
        Given a character with strength 18 and strike rating 10
        And a weapon named "Battleaxe" with D8 damage die and +0 attack bonus
        And the D20 roll is 1
        # Armor class 1 would normally be hit with ease — the fumble overrides this
        When the character attacks a target with armor class 1
        Then the attack should miss
        And the attack is a fumble
        And the damage should be 0

    @fumble
    # A fumble applies a -2 AttackPower penalty to the attacker's next turn,
    # representing the loss of footing or overextension from the wild swing.
    Scenario: Fumble applies a minus two attack power penalty for next turn
        Given a character with strength 10 and strike rating 19
        And a weapon named "Dagger" with D4 damage die and +0 attack bonus
        And the D20 roll is 1
        When the character attacks a target with armor class 1
        Then the attack is a fumble
        And the attacker receives an attack power penalty of -2
