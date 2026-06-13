# D&D Game Overlay Layout Specification

## Root Layout

The game UI consists of four primary regions:

```text
+------------------------------------------------------+
|                                                      |
|                                                      |
|                                                      |
|                 WORLD VIEWPORT                       |
|                                                      |
|                                                      |
|                                                      |
|                                          PARTY PANEL |
+------------------------------------------------------+
|                ACTION BAR / HUD                      |
+------------------------------------------------------+
```

---

# 1. World Viewport

Purpose:

* Main gameplay area.
* Isometric map rendering.
* Characters, NPCs, monsters, effects.
* Ground decals and path indicators.
* Floating combat text.
* Selection circles.

Suggested Control:

```csharp
WorldViewportControl
```

Responsibilities:

```csharp
RenderMap()
RenderCharacters()
RenderMonsters()
RenderEffects()
RenderSelections()
RenderTooltips()
```

Anchoring:

```text
Fill remaining screen space.
```

---

# 2. Party Panel (Right Side)

Purpose:

* Display party members.
* Health/Mana/Stamina.
* Status effects.
* Character selection.

Suggested Width:

```text
280px - 360px
```

Layout:

```text
+----------------------+
| Character Portrait   |
| HP Bar               |
| Mana Bar             |
| Buff Icons           |
+----------------------+

(repeated)
```

Component:

```csharp
PartyPanel
```

Child Component:

```csharp
PartyMemberCard
```

Properties:

```csharp
Portrait
Health
Mana
Stamina
StatusEffects
Level
Class
```

---

# 3. Action Bar (Bottom)

Purpose:

* Combat abilities.
* Items.
* Quick slots.

Component:

```csharp
ActionBar
```

Layout:

```text
+------------------------------------------------+
| 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 0 |
+------------------------------------------------+
```

Properties:

```csharp
AbilityIcon
Cooldown
Charges
Hotkey
Tooltip
```

Supports:

```csharp
DragDrop
CooldownAnimations
HoverTooltips
```

---

# 4. Character Orb Cluster

Purpose:

* Display player resources.

Position:

```text
Bottom Left
```

Contains:

```csharp
HealthOrb
ManaOrb
```

Alternative:

```csharp
HealthBar
ManaBar
StaminaBar
```

---

# 5. Navigation Menu (Left Side)

Purpose:

* Open major game systems.

Component:

```csharp
NavigationRail
```

Buttons:

```text
Character
Inventory
Spellbook
Journal
Map
Crafting
Factions
Settings
```

Layout:

```text
Vertical
```

---

# 6. Utility Panel (Bottom Right)

Purpose:

* Secondary functions.

Contains:

```text
Compass
World Time
Weather
Notifications
```

Component:

```csharp
UtilityPanel
```

---

# 7. Tooltip Layer

Separate overlay.

Purpose:

```text
Item tooltips
NPC names
Monster names
Buff descriptions
Ability descriptions
```

Component:

```csharp
TooltipOverlay
```

Render Order:

```text
Always above game world.
```

---

# 8. Modal Layer

Purpose:

```text
Inventory
Character Sheet
Quest Log
Merchant Window
Crafting Window
```

Component:

```csharp
ModalHost
```

Behavior:

```text
Centered
Draggable
Resizable
```

---

# 9. Notification Layer

Purpose:

```text
Quest updates
Loot messages
Level up
System warnings
```

Component:

```csharp
NotificationOverlay
```

Position:

```text
Top Center
```

---

# 10. Screen Composition

```text
GameRoot
 ├── WorldViewport
 ├── NavigationRail
 ├── PartyPanel
 ├── ActionBar
 ├── CharacterOrbCluster
 ├── UtilityPanel
 ├── TooltipOverlay
 ├── NotificationOverlay
 └── ModalHost
```

---

# Art Direction

Theme:

```text
Dark Fantasy
Dungeons & Dragons
Baldur's Gate
Neverwinter Nights
Icewind Dale
```

Materials:

```text
Dark bronze
Aged gold
Iron
Worn stone
Leather
Runic engravings
```

Avoid:

```text
Modern UI
Flat design
Mobile-game styling
Neon sci-fi effects
```

Target:

```text
4K
2560x1440
1920x1080
Ultrawide
Resizable window
DPI aware
```
