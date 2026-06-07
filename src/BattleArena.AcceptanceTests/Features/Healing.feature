# Combat System — Healing Spells
#
# Healing spells restore hit points to injured characters during combat.
# The combat simulator auto-casts healing spells on the lowest-HP ally when
# the healer's turn comes up and a healing spell is randomly picked.
#
# Heal: single-target, restores 2d8 + 4 HP.
# Mass Heal: group heal, restores 3d6 + 6 HP to all injured allies.

Feature: Combat — Healing Spells
    As a game designer
    I want healing spells to restore HP to injured characters
    So that support characters can keep the party alive

    @healing
    Scenario: Priest heals themself after taking damage
        Given a Priest "Sera" with 35 HP and 60 mana
        And an Orc "Gruk" with 35 HP and 0 mana who wields a Battle Axe
        And "Sera" has 15 hit points remaining
        And "Sera" has memorized spells: "Heal"
        When the healing combat is simulated with a maximum of 300 ticks
        Then the heal event appears in the log

    @healing
    Scenario: Mass Heal is cast during party combat
        Given a Priest "Sera" with 35 HP and 60 mana
        And a Fighter "Theron" with 50 HP and 0 mana who wields a Longsword
        And an Orc "Gruk" with 35 HP and 0 mana who wields a Battle Axe
        And "Theron" has 19 hit points remaining
        And "Sera" has memorized spells: "Mass Heal"
        When the healing party combat is simulated with a maximum of 300 ticks
        Then the heal event appears in the log
