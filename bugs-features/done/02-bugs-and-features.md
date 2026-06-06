# Feature Request - Combat Sound Effects System

Project: Dark Orb

## Objective

Analyze the existing combat event system, spell effect processing pipeline, status effect execution flow, and combat event notifications.

Implement a centralized combat sound system that plays appropriate audio effects for spell effects, damage-over-time effects, and combat special events.

The system must support future combat sounds without requiring event-specific audio logic to be duplicated throughout the codebase.

---

# Feature - Minor Effect Sound System

## Goal

Provide audio feedback for combat effects and status effect processing.

Players should immediately hear when a spell effect, damage effect, or status effect triggers.

## Requirement

Every minor combat effect should trigger a short sound effect.

Examples include:

* Burn damage tick
* Poison damage tick
* Frost damage tick
* Arcane damage effect
* Shadow damage effect
* Holy damage effect
* Bleed damage tick
* Shock damage effect
* Future damage-over-time effects

These sounds should be:

* Short
* Non-intrusive
* Distinct
* Appropriate for the effect type

The sound should play at the moment the effect is applied or triggers.

### Example Sound Themes

* Burn → Small fire burst or ember crackle
* Poison → Toxic hiss or bubbling effect
* Frost → Ice crack or freezing sound
* Shock → Electric zap
* Arcane → Magical pulse
* Holy → Soft radiant chime
* Shadow → Dark whisper or magical whoosh
* Bleed → Light slash impact

---

# Feature - Major Combat Event Sound System

## Goal

Provide stronger audio feedback for significant combat events.

Major events should feel impactful and immediately recognizable.

## Requirement

The following combat events must use longer and more dramatic sound effects than normal spell effects.

### Events

* Critical Hit
* Fumble
* Perfect Parry
* Perfect Dodge
* Counter Attack
* Killing Blow
* Resurrection
* Level Up (future use)
* Boss Event Triggers (future use)

### Example Sound Themes

#### Critical Hit

* Heavy weapon impact
* Deep strike effect
* Powerful combat hit

#### Fumble

* Failed swing
* Weapon slip
* Humorous failure tone

#### Perfect Parry

* Two swords clashing forcefully
* Metallic impact
* Defensive success sound

#### Perfect Dodge

* Fast whoosh
* Air displacement
* Evasion cue

#### Counter Attack

* Immediate retaliatory strike
* Quick aggressive impact

#### Killing Blow

* Strong finishing strike
* Dramatic impact effect

#### Resurrection

* Magical restoration
* Rising energy effect
* Holy or mystical tone

---

# Sound Asset Requirement

## Sound Folder Structure

Create a dedicated audio folder structure.

Example:

```text
Assets/
└── Sounds/
    ├── Effects/
    ├── StatusEffects/
    ├── CombatEvents/
    └── UI/
```

The implementation should not hardcode individual file locations throughout the codebase.

All sound references should originate from a centralized configuration source.

---

# Sound Asset Acquisition

## Requirement

Locate suitable WAV files for all required sounds.

Use royalty-free or permissively licensed sound assets.

Preferred sources:

* OpenGameArt
* Kenney
* Freesound
* Pixabay Sound Effects
* Similar royalty-free sources

## Selection Guidelines

Minor Effect Sounds:

* Short duration
* Approximately 0.1 to 0.5 seconds
* Low auditory fatigue
* Frequently repeatable

Major Event Sounds:

* Approximately 0.5 to 2 seconds
* More impactful
* Clearly distinguishable

If exact matches cannot be found, select the closest available sound that communicates the intended event.

---

# Centralized Sound Configuration

## Architectural Requirement

Implement a centralized sound registry.

All combat audio must be resolved through a single source of truth.

Examples:

* Sound identifiers
* Asset paths
* Playback volume
* Playback category
* Future audio settings

Combat systems should request sounds through sound identifiers rather than direct file paths.

### Example Concept

```text
BurnTick
PoisonTick
FrostTick
ShockTick
ArcaneEffect
HolyEffect
ShadowEffect
BleedTick

CriticalHit
Fumble
PerfectParry
PerfectDodge
CounterAttack
KillingBlow
Resurrection
```

Future combat systems should be able to register additional sounds without modifying existing combat logic.

---

# Playback Behaviour

## Minor Effects

When a status effect or spell effect triggers:

* Play the associated effect sound.
* Multiple simultaneous effects may queue.
* Sound playback must not block combat execution.

## Major Events

When a major combat event occurs:

* Play the associated event sound immediately.
* Event sound should take playback priority over minor effect sounds.
* Event sounds must not interrupt combat processing.

---

# Automated Unit Test Coverage

## Requirement

Create or update unit tests to validate all newly introduced combat sound functionality.

The sound system must be designed to support testing without requiring actual audio playback devices.

Audio playback should be abstracted behind an interface that can be mocked or substituted during testing.

### Sound Registry Tests

Verify:

* Sound identifiers resolve correctly.
* Sound asset mappings are valid.
* Missing sound identifiers are handled gracefully.
* Duplicate sound registrations are detected.

### Minor Effect Sound Tests

Verify:

* Burn effect requests BurnTick sound.
* Poison effect requests PoisonTick sound.
* Frost effect requests FrostTick sound.
* Shock effect requests ShockTick sound.
* Arcane effect requests ArcaneEffect sound.
* Holy effect requests HolyEffect sound.
* Shadow effect requests ShadowEffect sound.
* Bleed effect requests BleedTick sound.

### Major Event Sound Tests

Verify:

* Critical Hit requests CriticalHit sound.
* Fumble requests Fumble sound.
* Perfect Parry requests PerfectParry sound.
* Perfect Dodge requests PerfectDodge sound.
* Counter Attack requests CounterAttack sound.
* Killing Blow requests KillingBlow sound.
* Resurrection requests Resurrection sound.

### Playback Service Tests

Verify:

* Audio playback requests are executed asynchronously.
* Playback does not block combat processing.
* Multiple playback requests can be queued safely.
* Playback failures do not crash combat execution.
* Missing audio files are handled gracefully.

### Performance Tests

Verify:

* Rapid effect triggering does not create excessive allocations.
* Rapid combat events do not create playback bottlenecks.
* Combat execution timing remains unaffected by audio requests.

---

# Reqnroll Regression Test Coverage

## Requirement

Create or update Reqnroll feature files to validate combat sound behavior through end-to-end combat execution scenarios.

These tests should verify that combat events trigger the correct sound requests through the sound service.

Actual WAV playback does not need to be verified.

The tests should verify that the correct sound identifiers are requested at the correct combat events.

---

# Reqnroll Scenario 1 - Burn Tick Sound

**Given** a character is affected by Burn

**When** Burn damage triggers

**Then** the BurnTick sound should be requested

**And** damage should be applied normally

---

# Reqnroll Scenario 2 - Poison Tick Sound

**Given** a character is affected by Poison

**When** Poison damage triggers

**Then** the PoisonTick sound should be requested

**And** damage should be applied normally

---

# Reqnroll Scenario 3 - Critical Hit Sound

**Given** a combat attack results in a Critical Hit

**When** damage is resolved

**Then** the CriticalHit sound should be requested

**And** combat should continue normally

---

# Reqnroll Scenario 4 - Perfect Parry Sound

**Given** a Perfect Parry occurs

**When** the parry resolves

**Then** the PerfectParry sound should be requested

**And** damage mitigation should occur normally

---

# Reqnroll Scenario 5 - Fumble Sound

**Given** an attacker rolls a Fumble

**When** the combat event resolves

**Then** the Fumble sound should be requested

**And** combat should continue normally

---

# Reqnroll Scenario 6 - Multiple Simultaneous Effects

**Given** multiple status effects trigger during the same combat cycle

**When** combat effects are processed

**Then** the corresponding sound requests should be generated

**And** combat processing should complete successfully

**And** no combat exceptions should occur

---

# Reqnroll Scenario 7 - Killing Blow Sound

**Given** an attack defeats a target

**When** the target dies

**Then** the KillingBlow sound should be requested

**And** death processing should complete normally

---

# Reqnroll Scenario 8 - Missing Sound Asset

**Given** a configured sound asset is unavailable

**When** the sound is requested

**Then** combat execution should continue

**And** an appropriate warning should be logged

**And** no combat exception should occur

---

# Reqnroll Scenario 9 - New Sound Registration

**Given** a new combat sound is registered

**When** the sound is referenced by combat logic

**Then** the sound should resolve through the centralized sound registry

**And** no combat engine modifications should be required

---

# Acceptance Criteria

### Scenario 1

**Given** a Burn effect triggers

**When** burn damage is applied

**Then** the burn sound plays

**And** combat execution continues normally

### Scenario 2

**Given** a Poison effect triggers

**When** poison damage is applied

**Then** the poison sound plays

**And** no delay is introduced into combat processing

### Scenario 3

**Given** a Perfect Parry occurs

**When** the parry resolves

**Then** the perfect parry sound plays

**And** the sound resembles a strong metallic weapon clash

### Scenario 4

**Given** a Fumble occurs

**When** the combat event resolves

**Then** the fumble sound plays

**And** the sound is clearly distinguishable from a critical hit

### Scenario 5

**Given** multiple combat effects trigger rapidly

**When** sounds are played

**Then** audio playback remains stable

**And** combat performance is unaffected

### Scenario 6

**Given** new combat effects are added in the future

**When** new sounds are configured

**Then** no combat engine modifications are required

**And** the centralized sound registry supports the new sounds

---

# Validation Requirements

Perform the following verification:

* [ ] Burn sound plays when burn damage triggers.
* [ ] Poison sound plays when poison damage triggers.
* [ ] Frost sound plays when frost effects trigger.
* [ ] Shock sound plays when electrical effects trigger.
* [ ] Arcane sound plays when arcane effects trigger.
* [ ] Holy sound plays when holy effects trigger.
* [ ] Shadow sound plays when shadow effects trigger.
* [ ] Bleed sound plays when bleed effects trigger.
* [ ] Critical hit sound plays correctly.
* [ ] Fumble sound plays correctly.
* [ ] Perfect parry sound plays correctly.
* [ ] Perfect dodge sound plays correctly.
* [ ] Counter attack sound plays correctly.
* [ ] Killing blow sound plays correctly.
* [ ] Resurrection sound plays correctly.
* [ ] Unit tests created for sound registry.
* [ ] Unit tests created for sound playback service.
* [ ] Unit tests created for effect sound routing.
* [ ] Unit tests created for combat event sound routing.
* [ ] Unit tests created for missing sound asset handling.
* [ ] Reqnroll scenarios created for combat sounds.
* [ ] Reqnroll scenarios created for effect sounds.
* [ ] Reqnroll scenarios created for missing sound asset handling.
* [ ] Existing combat regression suite passes.
* [ ] Existing status effect regression suite passes.
* [ ] Existing turn meter regression suite passes.
* [ ] Existing combat lifecycle regression suite passes.
* [ ] Simultaneous sound playback remains stable.
* [ ] No combat delays are introduced by audio playback.
* [ ] Audio assets are loaded through centralized configuration.
* [ ] No hardcoded file paths exist inside combat logic.

## Deliverables

1. Root cause analysis of the current audio limitations.
2. Description of the implemented sound architecture.
3. List of selected WAV assets and their sources.
4. List of affected files.
5. Sound registry design overview.
6. Unit test execution results.
7. Reqnroll execution results.
8. Regression testing results.
9. Test coverage summary for the combat audio system.
10. Confirmation that all acceptance criteria pass.
11. Confirmation that all combat sounds are loaded through centralized configuration.
12. Confirmation that future combat sounds can be added without modifying combat engine logic.
13. Confirmation that all existing regression tests continue to pass.
