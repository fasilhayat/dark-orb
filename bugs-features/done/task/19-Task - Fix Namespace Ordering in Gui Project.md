# Task — Fix Namespace Ordering in Gui Project

Project: Dark Orb

Priority: Low

Type: Code Convention

Status: Draft

---

## Objective

Fix the systematic `using-before-namespace` convention violation across all 63 `BattleArena.Gui` source files to match the project-wide convention (`namespace` before `using`).

---

## Current State

The AGENTS.md convention states:

> `namespace` before `using` in hand-written code.

This is followed in:
- `BattleArena.Core` — all 69 files ✅
- `BattleArena.Application` — all 62 files ✅
- `BattleArena.Infrastructure` — all 19 files ✅
- `BattleArena.Presentation` — all 20 files ✅
- `BattleArena.UnitTests` — all files ✅

**But NOT followed in `BattleArena.Gui`** — all 63 `.cs` files place `using` directives _before_ the `namespace` declaration:

```csharp
// Current — violates convention
using System;
using System.IO;
using Avalonia;
using BattleArena.Application.Models;

namespace BattleArena.Gui.Views;

// Target — follows convention
namespace BattleArena.Gui.Views;

using System;
using System.IO;
using Avalonia;
using BattleArena.Application.Models;
```

### Files affected

All `.cs` files under `BattleArena.Gui/`:
- `Views/MainWindow.axaml.cs` (1496 lines)
- `ViewModels/MainWindowViewModel.cs` (1452 lines)
- `Presenters/AvaloniaCombatPresenter.cs` (1087 lines)
- `Presenters/AvaloniaSoundPlayer.cs`
- `BattleArenaApiClient.cs`
- `Rendering/*.cs` (6+ files)
- `Data/*.cs`
- `Models/*.cs`
- `Services/*.cs`
- `ViewModels/World/*.cs`, `ViewModels/WorldMap/*.cs`
- `Views/World/*.cs`, `Views/WorldMap/*.cs`

~63 files total.

---

## Proposed Solution

### Mechanical transformation

For each file, move the `using` block below the file-scoped `namespace` declaration:

```csharp
// Before
using System;
using System.Collections.Generic;

namespace BattleArena.Gui.Views;

// After
namespace BattleArena.Gui.Views;

using System;
using System.Collections.Generic;
```

For files that use top-level statements (none in Gui) or `namespace X { ... }` block syntax, wrap appropriately.

### Same-namespace usings

After moving usings, consider cleaning up `using BattleArena.Core.Entities` etc. when the namespace hierarchy provides implicit access. This is optional — minimal diff is preferred.

---

## Risks

- **Avalonia XAML code-behind files** (`*.axaml.cs`) — these use `namespace BattleArena.Gui.Views;` followed by a class. Moving `using` below namespace is safe because C# allows `using` inside a file-scoped namespace declaration.
- **No behavioral change** — pure cosmetic refactoring. The C# compiler generates identical IL regardless of `using` placement.
- **Merge conflicts** — this touches 63 files. Should be done in a dedicated commit with no other changes.

---

## Files to Modify

All ~63 `.cs` files in `BattleArena.Gui/` — every file that has `using` directives before the `namespace` declaration.

---

## Acceptance Criteria

- [ ] All `BattleArena.Gui` `.cs` files follow `namespace` before `using`
- [ ] Project builds without errors
- [ ] All 719 tests pass
- [ ] No behavioral changes in any file
