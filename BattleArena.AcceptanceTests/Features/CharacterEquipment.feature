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
