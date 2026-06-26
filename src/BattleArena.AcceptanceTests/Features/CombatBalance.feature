# Combat Balance — Regression Tests
#
# Verifies balance fixes are active:
#   1. Level provides defensive scaling (LevelDefenseBonus in DefensePower)
#   2. Priest spells use Wisdom, Mage spells use Intelligence
#   3. Armor mitigation scales with defender level
#   4. Higher-level characters win consistently against lower-level opponents

Feature: Combat Balance — Regression Guards
    As a game developer
    I want automated verification of combat balance formulas
    So that balance tuning is not accidentally reverted

    @balance
    # Level should contribute to defense power at half the rate of offense.
    # A level 10 character with AC 10 and DEX 10 has DefensePower = 10 + 0 + 5 = 15.
    Scenario: Level defense bonus contributes to defense power
        Given a defender at level 10 with armor class 10 and dexterity 10
        When the defender's combat stats are computed
        Then the defense power should be 15

    @balance
    # Higher level characters gain more defense from level scaling.
    Scenario: Higher level gives more defense power
        Given a defender at level 5 with armor class 10 and dexterity 10
        When the defender's combat stats are computed
        Then the defense power should be 12

    @balance
    # Priest (Deity school) spells use Wisdom, not Intelligence.
    Scenario: Priest spells use wisdom for attack modifier
        Given a priest with intelligence 8 and wisdom 20
        And a deity school spell
        When attack stats are computed for the spell
        Then the attribute modifier should be 5

    @balance
    # Mage (Stormcraft school) spells use Intelligence.
    Scenario: Mage spells use intelligence for attack modifier
        Given a mage with intelligence 20 and wisdom 8
        And a stormcraft school spell
        When attack stats are computed for the spell
        Then the attribute modifier should be 5

    @balance
    # Armor mitigation increases with defender level (reduced scaling).
    # Plate Armor (mitigation 4) at level 14 gives 4 * (1.0 + 14/20.0) = 6.
    Scenario: Armor mitigation scales with defender level at reduced rate
        Given a defender with plate armor at level 14
        When mitigation is computed
        Then the scaled mitigation should be 6

    @balance
    # Luna (level 14 Priest) should consistently beat Vaelith (level 9 Fighter).
    Scenario: Higher level priest consistently beats lower level fighter
        Given High Priestess Luna at level 14
        And Vaelith Moonveil at level 9
        When they fight 100 duels
        Then Luna should win at least 70 duels
