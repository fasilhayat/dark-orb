# Combat System — AttackPower and DefensePower Derivation
#
# Rather than using raw numbers, the combat system computes composite stats
# from all character properties before each attack resolution.
#
# AttackPower breakdown:
#   ClassAccuracyBase  = 20 - StrikeRating     (lower rating = better attacker)
#   LevelScaling       = Character.Level
#   AttributeModifier  = STR mod (melee) or DEX mod (ranged)
#   WeaponAttackBonus  = weapon enchantment / quality bonus
#   SkillModifiers     = sum of feat attack bonuses
#   BuffModifiers      = active status effects (with stacking rules applied)
#   RacialModifiers    = sum of racial feat attack bonuses
#
# DefensePower breakdown:
#   EffectiveAC        = 20 - TotalArmorClass   (converts AD&D-style AC to a power value)
#   DexterityModifier  = DEX mod, capped by lowest MaxDexterityBonus across all armor
#   ShieldBonus        = equipped shield's DefenseBonus
#   DefensiveBuffs     = active status effects (HighestWins per source, debuffs always stack)
#   RacialModifiers    = racial feat + character feat defense bonuses
Feature: Combat — AttackPower and DefensePower Derivation
    As a game designer
    I want attack and defense power to aggregate all character sources
    So that every stat investment contributes meaningfully to combat

    @combat-stats
    # Level 3 fighter, STR 16, StrikeRating 17, melee weapon +2.
    # ClassAccuracyBase = 20 - 17 = 3
    # LevelScaling = 3
    # AttributeModifier = (16-10)/2 = 3
    # WeaponAttackBonus = 2
    # SkillModifiers = 2 (feat)
    # RacialModifiers = 1 (racial feat)
    # BuffModifiers = 0
    # Total AttackPower = 3+3+3+2+2+1 = 14
    Scenario: Attack power aggregates class accuracy, level, strength, weapon, feats, and race
        Given an attacker at level 3 with strength 16 and strike rating 17
        And the attacker has a combat feat granting +2 attack bonus
        And the attacker's race has a combat feat granting +1 attack bonus
        And the attacker uses a melee weapon with attack bonus 2
        When attack power is computed
        Then the total attack power should be 14

    @combat-stats
    # Buff stacking rules are enforced before summing:
    #   Stack rule     → all instances from different sources are summed
    #   HighestWins    → only the strongest instance is counted
    #   NoStack        → ignored if the effect name already exists
    #   Debuffs        → always sum regardless of rule
    # Level=1, STR=10, StrikeRating=20 → base = (20-20)+1+0 = 1
    # Stack buff +3, HighestWins buffs [+4, +2] → max = 4, debuff -2
    # BuffModifiers = 3 + 4 + (-2) = 5. AttackPower = 1 + 5 = 6.
    Scenario: Buff stacking rules are applied correctly to attack power
        Given an attacker at level 1 with strength 10 and strike rating 20
        And the attacker uses a melee weapon with attack bonus 0
        And the attacker has a stacking attack buff with +3 modifier
        And the attacker has a highest-wins attack buff with +4 modifier
        And the attacker has a highest-wins attack buff with +2 modifier
        And the attacker has an attack debuff with -2 modifier
        When attack power is computed
        # Base 1 + buffs 5 = 6
        Then the total attack power should be 6

    @combat-stats
    # EffectiveAC = 20 - 5 = 15.  No shield, no dex modifier, no buffs.
    # DefensePower = 15.
    Scenario: Defense power converts raw armor class to effective armor class
        Given a stats defender with dexterity 10
        And the stats defender wears chest armor with class 5 and max dex bonus 10
        When defense power is computed
        # EffectiveAC = 20 - 5 = 15, LevelDefenseBonus = Level = 1, total = 16
        Then the total defense power should be 16

    @combat-stats
    # Heavy armor caps the dexterity bonus even when DEX is very high.
    # TotalArmorClass = 6 + 2 = 8 → EffectiveAC = 12.
    # DEX 18 → raw +4, capped to min(2 + 1) = 3 by armor.
        # Shield +3. LevelDefenseBonus = Level = 1.
        # DefensePower = 12 + 3 (dex capped) + 3 (shield) + 1 (level) = 19.
    Scenario: Dexterity bonus is capped by the most restrictive armor piece
        Given a stats defender with dexterity 18
        And the stats defender wears chest armor with class 6 and max dex bonus 2
        And the stats defender wears head armor with class 2 and max dex bonus 1
        And the stats defender carries a shield with +3 defense bonus
        When defense power is computed
        Then the computed effective armor class should be 12
        And the computed dexterity modifier should be 3
        And the total defense power should be 19
