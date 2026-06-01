# Combat System — Damage Formula
#
# Damage resolution is a separate step from the hit/miss check.
# It models the full §9 damage pipeline:
#
#   BaseDamage  = WeaponDiceRoll + AttributeModifier + FlatDamageBonus + Level/2
#   FinalDamage = (int)(BaseDamage × TypeMultiplier) - ArmorMitigation + ElementalDamage
#   FinalDamage is clamped to a minimum of 0.
#
# TypeMultiplier is 1.5 when the defender has a vulnerability to the weapon's
# damage type; otherwise 1.0.  Mitigation is the total flat damage reduction
# from all equipped armor pieces.  Elemental damage is added after mitigation.
Feature: Combat — Damage Formula
    As a game designer
    I want damage to reflect weapon dice, attributes, vulnerabilities, mitigation, and elemental bonuses
    So that item choices and character builds have meaningful impact

    # Baseline attacker: STR 14 (+2 modifier), D8 slashing weapon.
    Background:
        Given a damage formula attacker with strength 14
        And a Slashing damage weapon with D8 die

    @damage-formula
    # The simplest case: no bonuses, no mitigation, no vulnerability.
    # BaseDamage = d8(5) + 2 + Level/2(0) = 7. TypeMultiplier = 1.0. FinalDamage = 7.
    Scenario: Standard damage applies weapon dice and attribute modifier
        And a damage formula target with no modifiers
        And the weapon damage die rolls 5
        When damage is resolved against the target
        Then the base damage should be 7
        And the final damage should be 7

    @damage-formula
    # A weapon's flat damage bonus is added to BaseDamage before any multipliers.
    # BaseDamage = d8(5) + 2 + 2 + Level/2(0) = 9. FinalDamage = 9.
    Scenario: Flat weapon damage bonus increases base damage
        And a damage formula target with no modifiers
        And the weapon has a flat damage bonus of 2
        And the weapon damage die rolls 5
        When damage is resolved against the target
        Then the base damage should be 9
        And the final damage should be 9

    @damage-formula
    # Elemental damage is added after the mitigation step, so armor cannot reduce it.
    # BaseDamage = d8(5) + 2 + Level/2(0) = 7. Mitigation = 0. Elemental = +3. FinalDamage = 10.
    Scenario: Elemental damage bonus is added after armor mitigation
        And a damage formula target with no modifiers
        And the weapon deals 3 elemental bonus damage
        And the weapon damage die rolls 5
        When damage is resolved against the target
        Then the final damage should be 10

    @damage-formula
    # Armor mitigation reduces final damage as a flat subtraction.
    # BaseDamage = d8(5) + 2 + Level/2(0) = 7. Mitigation = 3. FinalDamage = 7 - 3 = 4.
    Scenario: Armor mitigation reduces final damage
        And a damage formula target with armor mitigation of 3
        And the weapon damage die rolls 5
        When damage is resolved against the target
        Then the final damage should be 4

    @damage-formula
    # Mitigation can exceed raw damage, but final damage is clamped to zero.
    # BaseDamage = d4(1) + 0 + Level/2(0) = 1 (STR 10, no modifier). Mitigation = 5. 1 - 5 = -4 → 0.
    Scenario: Final damage cannot fall below zero when mitigation exceeds base damage
        Given a damage formula attacker with strength 10
        And a Slashing damage weapon with D4 die
        And a damage formula target with armor mitigation of 5
        And the weapon damage die rolls 1
        When damage is resolved against the target
        Then the final damage should be 0

    @damage-formula
    # Vulnerability multiplies BaseDamage by 1.5 before applying mitigation.
    # BaseDamage = d8(6) + 2 + Level/2(0) = 8. 8 * 1.5 = 12.0 → int = 12. FinalDamage = 12.
    Scenario: Defender vulnerability multiplies base damage by one point five
        And a damage formula target with no modifiers
        And the target is vulnerable to Slashing damage
        And the weapon damage die rolls 6
        When damage is resolved against the target
        Then the base damage should be 8
        And the final damage should be 12

    @damage-formula
    # Full pipeline: flat bonus + vulnerability + mitigation + elemental all combine.
    # BaseDamage = d8(5) + 2 + 1 + Level/2(0) = 8
    # After vulnerability: (int)(8 * 1.5) = 12
    # After mitigation: 12 - 2 = 10
    # After elemental: 10 + 3 = 13
    Scenario: All damage modifiers combine correctly in the full pipeline
        And the weapon has a flat damage bonus of 1
        And the weapon deals 3 elemental bonus damage
        And a damage formula target with armor mitigation of 2
        And the target is vulnerable to Slashing damage
        And the weapon damage die rolls 5
        When damage is resolved against the target
        Then the base damage should be 8
        And the final damage should be 13
