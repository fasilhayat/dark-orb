## Future-Proofing Requirement - Configurable Transfer Visuals

The transfer animation system must be designed as a reusable resource-transfer framework rather than a Leech-specific implementation.

### Objective

Future spells should be able to reuse the same transfer animation system without requiring new animation implementations.

Examples:

- Leech
- Life Drain
- Soul Siphon
- Arcane Theft
- Divine Extraction
- Deity-specific drain abilities
- Future mana, health, turn meter, or resource transfer effects

---

### Visual Configuration Requirements

Transfer effects must support configurable:

- Transfer color
- Particle color
- Particle intensity
- Stream thickness
- Stream pulse speed
- Source glow color
- Destination glow color
- Transfer sound effect

The animation system must not hardcode Leech-specific colors or visual assets.

---

### Initial Configuration

Leech should use:

| Property | Value |
|-----------|---------|
| Transfer Color | Red |
| Source Glow | Red |
| Destination Glow | Red |
| Particle Effect | Red siphon particles |
| Stream Style | Continuous drain |

---

### Future Spell Examples

#### Life Drain

Visual theme:

- Dark crimson
- Blood-like energy flow
- Health transfer instead of mana transfer

#### Soul Siphon

Visual theme:

- Purple
- Shadow particles
- Ethereal energy stream

#### Arcane Theft

Visual theme:

- Blue
- Arcane spark particles
- Magical energy stream

#### Divine Extraction

Visual theme:

- Gold
- Holy particle effects
- Celestial energy transfer

---

### Architectural Requirement

The transfer system must be implemented as:

```text
Transfer Effect Renderer
```

with spell-specific configuration values.

It must NOT be implemented as:

```text
Leech Animation
Life Drain Animation
Soul Siphon Animation
Arcane Theft Animation
```

Separate implementations are prohibited.

---

### Reusability Goal

The same renderer should support:

```text
Mana → Mana transfer
Health → Health transfer
Turn Meter → Turn Meter transfer
Future resource systems
```

through configuration only.

---

### Acceptance Criteria

- [x] Transfer color configurable per spell
- [x] Particle effects configurable per spell
- [x] Source and destination glow configurable
- [x] Leech implemented using configuration values
- [x] No hardcoded Leech-specific rendering logic
- [x] Renderer reusable for future drain and siphon spells
- [x] Future resource-transfer effects require configuration only

---

## Implementation Summary

### Completed: June 7, 2026

Built a configurable transfer-effect framework replacing hardcoded Leech-specific rendering logic.

### New Files

| File | Location | Purpose |
|------|----------|---------|
| `TransferEffectConfig.cs` | `Presentation/` | Config model: color, particles, glow, stream, sound per effect |
| `TransferEffectRegistry.cs` | `Presentation/` | Central registry with `Leech` and `LeechMana` defaults |

### Modified Files

| File | Change |
|------|--------|
| `CombatPlaybackEngine.cs` | `EmitVisualEvents`: LeechTick case looks up config via `TransferEffectRegistry` instead of hardcoded `#cc44ff`/`#ff6644`. `GetPersistentColor`: replaced `"Leech"` hardcoded case with `TryGetTransferColor` fallback. `EmitCombatSounds`: uses `GetTransferSoundId` for config-driven sound. Added `"LeechMana"` to `PersistentEffectNames`. |
| `CombatSoundRegistry.cs` | Added `GetTransferSoundId(effectName, eventType)` - looks up sound from config, falls back to event-based lookup. |
| `AvaloniaCombatPresenter.cs` | `BuildLeechTickRow`: uses `TransferEffectRegistry.GetConfig().TransferColor` via `MakeBrush` instead of hardcoded `Magenta`/`Red`. Uses config `OverlayLabel` instead of hardcoded symbol. |

### How It Works

1. Each transfer effect (Leech, Life Drain, Soul Siphon, etc.) has a `TransferEffectConfig` entry in the registry
2. The config defines: TransferColor, ParticleColor, SourceGlowColor, DestinationGlowColor, SoundId, OverlayLabel, stream properties
3. `CombatPlaybackEngine.EmitVisualEvents` looks up the config by effect name and uses its values instead of hardcoded colors
4. `CombatSoundRegistry.GetTransferSoundId` uses the config's sound ID with fallback
5. The GUI `BuildLeechTickRow` renders log entries using config colors
6. Adding a new drain/siphon spell requires only a new registry entry - no code changes

### Future-Proof

To add a new transfer effect (e.g., "Soul Siphon"):
```csharp
TransferEffectRegistry.Register("SoulSiphon", new TransferEffectConfig {
    TransferColor = "#8844ff",
    ParticleColor = "#6622cc",
    SoundId = "SoulSiphon",
    OverlayLabel = "SOUL SIPHON"
});
```
Zero renderer changes needed.