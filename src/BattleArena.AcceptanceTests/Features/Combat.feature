# Combat System — Core Attack Resolution
#
# These scenarios verify the fundamental attack flow: rolling a d20, adding
# AttackPower (derived from strength, strike rating, level, and weapon bonus),
# and comparing against the defender's DefensePower (derived from armor class).
#
# Formula reference:
#   AttackPower = StrikeRating + Level/2 + StrengthModifier + WeaponAttackBonus
#   DefensePower = EffectiveAC = TotalArmorClass + LevelDefenseBonus + other modifiers
#   Hit if: d20 + AttackPower >= d20 + DefensePower   (modern opposed-roll, NOT THAC0)
Feature: Combat — Core Attack Resolution
    As a game designer
    I want to resolve attacks and damage
    So that combat follows the AD&D-inspired rules

    # Every scenario starts with a baseline character and weapon.
    # AttackPower for this background: 2 (SR) + 1 (level) + 2 (STR mod) + 0 = 5
    Background:
        Given a character with strength 14 and strike rating 2
        And a weapon named "Longsword" with D8 damage die and +0 attack bonus

    @ability
    # Ability modifiers use the standard (score - 10) / 2 formula.
    # These cover the full range from weak (8) to exceptional (20).
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
    # AttackPower = 4 (SR 2 + STR 14). Roll 16 + 4 = 20 >= DefensePower 10 (AC floor 10). Hit.
    # Damage = d8(5) + STR mod(2) + Level/2(0) = 7.
    Scenario: Successful melee attack hits and deals damage with strength bonus
        Given the D20 roll is 16
        And the damage die roll is 5
        When the character attacks a target with armor class 10
        Then the attack should hit
        And the hit roll should be 16
        And the damage should be 7
        And the weapon used should be "Longsword"

    @attack
    # Roll 5 + AttackPower 5 = 10 < DefensePower 11 (AC=10 + Level 1). Miss.
    # A miss always deals zero damage.
    Scenario: Attack misses when roll is too low to beat the target armor class
        Given the D20 roll is 5
        And the damage die roll is 3
        When the character attacks a target with armor class 10
        Then the attack should miss
        And the damage should be 0

    @attack
    # A natural 20 is a critical hit — it auto-hits and doubles base damage.
    # STR 6 gives -2 mod. d4(1) + (-2) + Level/2(0) = -1. Crit: -1 * 2 = -2 → clamped to 0.
    # Damage floor is at 0 regardless of negative STR mod.
    Scenario: Damage cannot go below zero even with a negative strength modifier
        Given a character with strength 6 and strike rating 19
        And the D20 roll is 20
        And the damage die roll is 1
        When the character attacks a target with armor class 5
        Then the attack should hit
        And the damage should be 0

    @attack
    # Higher StrikeRating means higher ClassAccuracyBase (SR is used directly; higher = better attacker).
    # StrikeRating 15 → ClassAccuracyBase 15. AttackPower = 15 + 1 + 3 + 0 = 19.
    # Roll 8 + 19 = 27 >= DefensePower 6 (AC=5 + Level 1). Hit. Damage = d6(6) + 3 + Level/2(0) = 9.
    Scenario: Higher strike rating makes attacks more likely to hit
        Given a character with strength 16 and strike rating 15
        And the D20 roll is 8
        And the damage die roll is 6
        When the character attacks a target with armor class 5
        Then the attack should hit
        And the damage should be 9

    @attack
    # Weapon attack bonus is added directly to AttackPower.
    # AttackPower = 19 + 1 + 0 + 3 = 23. Roll 10 + 23 = 33 >= 6 (AC=5 + Level 1). Hit.
    # Damage = d12(7) + STR mod(0) + Level/2(0) = 7.
    Scenario: Weapon attack bonus improves hit chance
        Given a character with strength 10 and strike rating 19
        And a weapon named "Soul Reaver" with D12 damage die and +3 attack bonus
        And the D20 roll is 10
        And the damage die roll is 7
        When the character attacks a target with armor class 5
        Then the attack should hit
        And the damage should be 7

    @attack
    # High armor class produces a high DefensePower (EffectiveAC = 15 + Level 1 = 16).
    # AttackPower = 5 (background). Roll 10 + 5 = 15 < 16. Miss.
    Scenario: High armor class makes attacks miss
        Given the D20 roll is 10
        And the damage die roll is 4
        When the character attacks a target with armor class 15
        Then the attack should miss
        And the damage should be 0

    @damage
    # RollDamage is a standalone helper that only rolls the weapon's die.
    # It does not apply strength or any other modifiers.
    Scenario: Roll damage for a weapon
        Given the damage die roll is 7
        When the character rolls damage for the weapon
        Then the damage result should be 7
        And the damage die type should be D8

    @damage
    # A different weapon die type is used to confirm the die type is reported correctly.
    Scenario: Roll damage for a weapon with different die
        Given a weapon named "Dagger" with D4 damage die and +0 attack bonus
        And the damage die roll is 3
        When the character rolls damage for the weapon
        Then the damage result should be 3
        And the damage die type should be D4
