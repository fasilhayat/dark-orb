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

- [ ] Transfer color configurable per spell
- [ ] Particle effects configurable per spell
- [ ] Source and destination glow configurable
- [ ] Leech implemented using configuration values
- [ ] No hardcoded Leech-specific rendering logic
- [ ] Renderer reusable for future drain and siphon spells
- [ ] Future resource-transfer effects require configuration only