# Combat System — Turnmeter System
#
# The turnmeter determines when each combatant acts.  It fills on every tick
# and drains after each turn, creating natural action frequency variation.
#
# Gain per tick formula:
#   max(1, TurnSpeed + DexModifier + BuffModifiers - ArmorPenalty)
#
# Turn rules:
#   TurnMeter >= 100 → character can take their turn (subtract 100 after)
#   TurnMeter >= 200 → character has a dual action available this turn
#
# The minimum gain per tick is always 1, so even the most heavily armored
# character with low dexterity will eventually get a turn.
Feature: Combat — Turnmeter System
    As a game designer
    I want each combatant's action frequency driven by speed, dexterity, armor, and buffs
    So that fast lightly-armored characters act more frequently than slow heavy ones

    @turnmeter
    # Gain per tick = max(1, TurnSpeed + DexModifier + BuffModifiers - ArmorPenalties)
    # Values of 0 for head armor or buff mean those sources are absent.
    Scenario Outline: Turn meter gain per tick is computed correctly
        Given a combatant with turn speed <speed> and dexterity <dex>
        And the combatant wears chest armor with a turn meter penalty of <chest>
        And the combatant wears head armor with a turn meter penalty of <head>
        And the combatant has a speed buff granting +<buff> turn meter per tick
        When the turn meter gain is computed
        Then the turn meter gain per tick should be <expected>

        Examples:
            | speed | dex | chest | head | buff | expected |
            |    10 |  14 |     2 |    1 |    4 |       13 |
            |     1 |   6 |    10 |    0 |    0 |        1 |

    @turnmeter
    # Starting at 190 with a gain of 17 brings the meter to 207.
    # 207 >= 200 means the character has a dual action available this turn.
    # TurnSpeed 10, DEX 14 (+2), haste buff +5. Gain = 10 + 2 + 5 = 17.
    Scenario: Reaching 200 or above grants a dual action on that turn
        Given a combatant with turn speed 10 and dexterity 14
        And the combatant has a speed buff granting +5 turn meter per tick
        And the combatant's turn meter is at 190
        When the turn meter ticks once
        # 190 + 17 = 207
        Then the turn meter value should be 207
        And the combatant can take their turn
        And the combatant has a dual action available

    @turnmeter
    # After acting, 100 is subtracted from the meter.
    # Starting at 205: 205 - 100 = 105. Still >= 100, so a second action is possible
    # (the dual action was already granted when the meter hit 200).
    Scenario: Turn meter is reduced by one hundred after taking a turn
        Given the combatant's turn meter is at 205
        When the combatant takes their turn
        # 205 - 100 = 105, which is still above the 100 threshold
        Then the turn meter value should be 105
        And the combatant can take their turn
