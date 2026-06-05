# Character Combat Readiness — Equipment & Data Integrity
#
# These scenarios verify that characters have the equipment and stats needed
# to participate in combat. A character must have valid stats, at least one
# attack source, and armor providing meaningful protection.
#
# This feature covers the data composition layer — ensuring that armor slots,
# weapon hands, and shields are all properly wired for the combat system.
Feature: Character Combat Readiness
    As a game designer
    I want characters to have complete equipment data for combat
    So that every character can fight effectively

    @equipment
    # A character with no equipment still has empty armor slots.
    # Basic combat stats are available but there is no protection or attack source.
    Scenario: Unarmed character has default armor slots
        Given a character with strength 10 and dexterity 10
        Then the character's total armor class should be 0
        And the character's total mitigation should be 0
        And the character should have no attack source

    @equipment
    # A single chest armor piece provides armor class and damage mitigation.
    Scenario: Character with chest armor has armor protection
        Given a character with strength 10 and dexterity 10
        And the character wears "Chain Mail" with armor class 5 and mitigation 2
        Then the character's total armor class should be 5
        And the character's total mitigation should be 2

    @equipment
    # Multiple armor pieces across different slots stack their protection.
    Scenario: Character with multiple armor pieces has cumulative protection
        Given a character with strength 10 and dexterity 10
        And the character wears "Chain Mail" with armor class 5 and mitigation 2
        And the character also wears a "Helm" with armor class 1 and mitigation 1
        And the character also wears "Leather Boots" with armor class 0 and mitigation 0
        Then the character's total armor class should be 6
        And the character's total mitigation should be 3

    @equipment
    # A weapon in the right hand provides the primary attack source.
    Scenario: Character with weapon has attack source from right hand
        Given a character with strength 14 and dexterity 10
        And the character wields a "Longsword" in their right hand dealing 1d8 Slashing damage with attack bonus 2
        Then the character should have an attack source named "Longsword"
        And the attack source should deal 1d8 damage

    @equipment
    # A shield in the left hand provides additional defense.
    Scenario: Character with shield has defense bonus
        Given a character with strength 10 and dexterity 10
        And the character wears a shield with defense bonus 2
        Then the character's shield defense bonus should be 2

    @equipment
    # A character with no weapon or spells defaults to the unarmed strike (Fists).
    Scenario: Unarmed character fights with fists as fallback
        Given a character with strength 14 and dexterity 10
        Then the character's unarmed strike should be named "Fists"
        And the unarmed strike should deal 1d4 Bludgeoning damage
        And the unarmed strike should be a melee attack
        And the unarmed strike should have 0 attack bonus

    @equipment
    # Fully geared character with armor, weapon, and shield across all relevant slots.
    Scenario: Fully equipped character is combat-ready
        Given a character with strength 18 and dexterity 14
        And the character wears "Plate Mail" with armor class 3 and mitigation 8
        And the character also wears a "Helm" with armor class 0 and mitigation 1
        And the character wields a "Longsword" in their right hand dealing 1d8 Slashing damage with attack bonus 3
        And the character wears a shield with defense bonus 2
        Then the character's total armor class should be 3
        And the character's total mitigation should be 9
        And the character should have an attack source named "Longsword"
        And the character's shield defense bonus should be 2
        And the character's right hand weapon should be "Longsword"

    @equipment @restrictions
    # A Fighter (class 8) can equip a two-handed sword with sufficient Strength.
    Scenario: Fighter with STR 16 can equip two-handed sword
        Given a Fighter with strength 16
        And the character wields a "Greatsword" two-handed sword in their right hand
        Then the character should be able to equip the weapon
        And the character's two-handed weapon bonus should be 0

    @equipment @restrictions
    # A Fighter cannot equip a two-handed sword with insufficient Strength.
    Scenario: Fighter with STR 15 cannot equip two-handed sword
        Given a Fighter with strength 15
        And the character wields a "Greatsword" two-handed sword in their right hand
        Then the character should not be able to equip the weapon

    @equipment @restrictions
    # A Mage (class 5) cannot equip a two-handed sword regardless of Strength.
    Scenario: Mage cannot equip two-handed sword due to class restriction
        Given a Mage with strength 18
        And the character wields a "Greatsword" two-handed sword in their right hand
        Then the character should not be able to equip the weapon

    @equipment @restrictions
    # A character with low base Strength but a Strength-boosting belt can equip a two-handed weapon.
    Scenario: Strength-boosting gear allows two-handed weapon use
        Given a Fighter with strength 14
        And the character wears a "Belt of Giant Strength" that grants +4 Strength
        And the character wields a "Greatsword" two-handed sword in their right hand
        Then the character should be able to equip the weapon

    @equipment @restrictions
    # A Fighter with STR 15 can dual-wield two short swords.
    Scenario: Fighter with STR 15 can dual-wield
        Given a Fighter with strength 15
        And the character wields a "Shortsword" in their right hand dealing 1d6 Piercing damage with attack bonus 0
        And the character wields a "Shortsword" in their left hand dealing 1d6 Piercing damage with attack bonus 0
        Then the character should be able to dual-wield

    @equipment @restrictions
    # A Fighter with STR 14 cannot dual-wield.
    Scenario: Fighter with STR 14 cannot dual-wield
        Given a Fighter with strength 14
        And the character wields a "Shortsword" in their right hand dealing 1d6 Piercing damage with attack bonus 0
        And the character wields a "Shortsword" in their left hand dealing 1d6 Piercing damage with attack bonus 0
        Then the character should not be able to dual-wield

    @equipment @restrictions
    # A Rogue can dual-wield short sword and dagger.
    Scenario: Rogue with STR 15 can dual-wield short sword and dagger
        Given a Rogue with strength 15
        And the character wields a "Shortsword" in their right hand dealing 1d6 Piercing damage with attack bonus 0
        And the character wields a "Dagger" in their left hand dealing 1d4 Piercing damage with attack bonus 0
        Then the character should be able to dual-wield

    @equipment @restrictions
    # A Rogue cannot dual-wield two long swords (class restriction).
    Scenario: Rogue with STR 15 cannot dual-wield two long swords
        Given a Rogue with strength 15
        And the character wields a "Longsword" in their right hand dealing 1d8 Slashing damage with attack bonus 0
        And the character wields a "Longsword" in their left hand dealing 1d8 Slashing damage with attack bonus 0
        Then the character should not be able to dual-wield

    @equipment @restrictions
    # A Knight cannot dual-wield at all.
    Scenario: Knight with STR 16 cannot dual-wield
        Given a Knight with strength 16
        And the character wields a "Longsword" in their right hand dealing 1d8 Slashing damage with attack bonus 0
        And the character wields a "Longsword" in their left hand dealing 1d8 Slashing damage with attack bonus 0
        Then the character should not be able to dual-wield

    @equipment @restrictions
    # A Barbarian wearing heavy armor violates class restrictions.
    Scenario: Barbarian with heavy armor has armor violation
        Given a Barbarian with strength 16
        And the character wears "Chain Mail" with armor class 5 and mitigation 2
        Then the character should have an armor violation

    @equipment @restrictions
    # A Barbarian wearing light armor has no violation.
    Scenario: Barbarian with light armor has no armor violation
        Given a Barbarian with strength 16
        And the character wears "Leather Armor" with armor class 2 and mitigation 1
        Then the character should not have an armor violation
