# Combat Outcome Distributions — Statistical Verification
#
# This feature runs many attack resolutions with live (seeded) dice and
# measures outcome distributions to verify that the AP/DP formulas and
# special-rule triggers produce results within expected theoretical ranges.
#
# AP = StrikeRating + Level/2 + STRmod + WeaponBonus + Feats + Buffs
# DP = EffectiveAC + DEXmod (capped) + Shield + Buffs
#
# Every scenario uses 2000 attack resolutions.
# All assertions use conservative ≥3-sigma statistical bounds.
Feature: Combat — Outcome Distributions
    As a game designer
    I want combat outcomes to fall within expected statistical bounds
    So that I can verify the AP/DP formulas and special rules are correct

    @combat-distribution
    # Balanced: AP ≈ DP
    #   Attacker: Level 2, STR 14 (+2), SR 8   → AP = 8 + 1 + 2 = 11
    #   Defender: AC 8, DEX 10 (+0)            → DP = max(10, 8) + 0 + 1 = 11
    #   P(hit) ≈ 52 % (opposed d20 with equal modifiers)
    #   N=2000 → expected ~1040
    Scenario: Balanced combat hit rate is approximately 52%
        Given a distribution attacker at level 2 with strength 14 and strike rating 8
        And the distribution attacker wields an unarmed strike
        And a distribution defender with armor class 8 and dexterity 10
        When 2000 attacks are resolved
        Then the total hit count should be between 900 and 1180
        And the critical hit rate should be between 3% and 6%
        And the fumble rate should be between 3% and 7%
        And the perfect parry rate should be between 2% and 7%

    @combat-distribution
    # Defensive advantage: DP > AP
    #   Attacker: Level 1, STR 10 (+0), SR 8   → AP = 8 + 0 + 0 = 8
    #   Defender: AC 14, DEX 10 (+0)           → DP = 14 + 0 = 14
    #   P(hit) = (1 + 18 + 91) / 400 = 27.50 %
    #   N=2000 → expected 550, 3σ [490, 610]
    Scenario: Defensive advantage suppresses hit rate to approximately 28%
        Given a distribution attacker at level 1 with strength 10 and strike rating 8
        And the distribution attacker wields an unarmed strike
        And a distribution defender with armor class 14 and dexterity 10
        When 2000 attacks are resolved
        Then the total hit count should be between 480 and 620

    @combat-distribution
    # Attacker advantage: AP > DP
    #   Attacker: Level 1, STR 18 (+4), SR 14  → AP = 14 + 0 + 4 = 18
    #   Defender: AC 5, DEX 10 (+0)            → DP = max(10, 5) + 0 + 1 = 11
    #   P(hit) ≈ 80 %
    #   N=2000 → expected ~1600
    Scenario: Attacker advantage elevates hit rate to approximately 80%
        Given a distribution attacker at level 1 with strength 18 and strike rating 14
        And the distribution attacker wields an unarmed strike
        And a distribution defender with armor class 5 and dexterity 10
        When 2000 attacks are resolved
        Then the total hit count should be between 1450 and 1750

    @combat-distribution
    # High level: tests LevelScaling = Level/2 at level 5
    #   Attacker: Level 5, STR 18 (+4), SR 17  → AP = 17 + 2 + 4 = 23
    #   Defender: AC 10, DEX 10 (+0)           → DP = 10 + 0 = 10
    #   P(hit) = (1 + 18 + 332) / 400 = 87.75 %
    #   N=2000 → expected 1755, 3σ [1711, 1799]
    #   Old formula (LevelScaling = Level): AP = 26, P(hit) = 98.50 %
    Scenario: Level scaling change keeps high-level hit rate at approximately 88%
        Given a distribution attacker at level 5 with strength 18 and strike rating 17
        And the distribution attacker wields an unarmed strike
        And a distribution defender with armor class 10 and dexterity 10
        When 2000 attacks are resolved
        Then the total hit count should be between 1700 and 1810
