# Combat System — Sound Event Contract
#
# The combat simulation produces events that the sound system consumes.
# These scenarios verify that the simulation generates the event types and
# data fields required for sound mapping.
Feature: Combat — Sound Event Contract

    @sound
    # Def=20, Atk=2-19 triggers PerfectParry (auto-miss, defender TM bonus).
    # PerfectParry maps to the "PerfectParry" sound ID.
    Scenario: PerfectParry event maps to PerfectParry sound
        Given a sound fighter with strength 10 and strike rating 19
        And sound fighter wields "Longsword" dealing D8 damage with +0 bonus
        And the sound D20 rolls are 10 and 20
        And the sound damage die roll is 4
        When the sound fighter attacks a target with armor class 5 and strike rating 19
        Then the sound attack should miss
        And the attack result has PerfectParry

    @sound
    # Atk=20, Def=1 triggers DevastatingStrike (triple damage auto-hit).
    # DevastatingStrike maps to "CriticalHit" sound ID via CombatSoundRegistry.
    # Note: DevastatingStrike sets IsHit=true + IsDevastatingStrike=true but
    # NOT IsCriticalHit (the damage formula is different from a normal crit).
    Scenario: DevastatingStrike maps to CriticalHit sound
        Given a sound fighter with strength 14 and strike rating 19
        And sound fighter wields "Greatsword" dealing D8 damage with +0 bonus
        And the sound D20 rolls are 20 and 1
        And the sound damage die roll is 8
        When the sound fighter attacks a target with armor class 10 and strike rating 8
        Then the sound attack should hit
        And the attack result has DevastatingStrike

    @sound
    # Atk=1, Def=20 triggers TotalReversal (auto-miss, -4 AP penalty, defender TM).
    # TotalReversal maps to "Fumble" sound ID via CombatSoundRegistry.
    Scenario: TotalReversal maps to Fumble sound
        Given a sound fighter with strength 10 and strike rating 8
        And sound fighter wields "Dagger" dealing D8 damage with +0 bonus
        And the sound D20 rolls are 1 and 20
        And the sound damage die roll is 2
        When the sound fighter attacks a target with armor class 14 and strike rating 19
        Then the sound attack should be a fumble
        And the attack result has TotalReversal

    @sound
    # A full combat between a strong warrior and a weak goblin must end
    # with a Death or KnockedOut event, which maps to the "KillingBlow" sound ID.
    # Both outcomes are valid since 1d12+5 damage can KO (6-11 dmg → -5 to -10 HP)
    # or kill (12-17 dmg → ≤ -11 HP) the 1-HP goblin.
    Scenario: Death event in full combat maps to KillingBlow sound
        Given a sound combatant named "Hero" with level 10, strength 18, dexterity 14, strike rating 19, turn speed 12, and 100 hit points
        And sound combatant "Hero" wields a "Greatsword" dealing 1d12 Slashing damage with attack bonus 5
        And a sound combatant named "Goblin" with level 1, strength 8, dexterity 8, strike rating 3, turn speed 5, and 1 hit points
        And sound combatant "Goblin" wields a "Dagger" dealing 1d4 Piercing damage with attack bonus 0
        When the sound combat is simulated with a maximum of 500 ticks
        Then the sound combat should have ended before the tick limit
        And the sound combat log contains "Death" or "KnockedOut"

    @sound
    # CombatSoundRegistry sound ID mappings must be correct for all known
    # effects and events consumed by the sound system.
    Scenario: CombatSoundRegistry has correct effect-to-sound mappings
        Given the combat sound registry is loaded
        When effect sound mappings are verified
        Then "Burning" should map to sound "BurnTick"
        And "Ignite" should map to sound "BurnTick"
        And "Poisoned" should map to sound "PoisonTick"
        And "Bleeding" should map to sound "BleedTick"
        And "Frozen" should map to sound "FrostTick"
        And "Freeze" should map to sound "FrostTick"
        And "Shocked" should map to sound "ShockTick"

    @sound
    Scenario: CombatSoundRegistry has correct event-to-sound mappings
        Given the combat sound registry is loaded
        When event sound mappings are verified
        Then "PerfectParry" should map to sound "PerfectParry"
        And "PerfectDodge" should map to sound "PerfectDodge"
        And "CounterAttack" should map to sound "CounterAttack"
        And "DevastatingStrike" should map to sound "CriticalHit"
        And "TotalReversal" should map to sound "Fumble"
        And "FumblePenalty" should map to sound "Fumble"
        And "KillingBlow" should map to sound "KillingBlow"
        And "Death" should map to sound "KillingBlow"
        And "Resurrection" should map to sound "Resurrection"
