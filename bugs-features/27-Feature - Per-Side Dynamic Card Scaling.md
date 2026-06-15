# Feature — Per-Side Dynamic Character Card Scaling for Clash Battles

Project: Dark Orb

Priority: Medium

Type: Feature

Status: Draft

Dependencies: Clash/Duel mode (hero team size 1–6, enemy team size 1–26)

---

## Objective

When the number of characters on a side exceeds what fits at full card size, wrap cards into multiple columns and scale them uniformly so all cards fit the screen without scrolling. Each side scales independently — a 4-hero party keeps larger cards while a 26-enemy party arranges into 2 columns of 13 and scales down proportionally.

---

## Current State

**CharacterCard.axaml:5** — Fixed card size:
```xml
<UserControl ... Width="380" MinHeight="180">
```

**MainWindow.axaml:270-283, 302-315** — Both sides use `ScrollViewer` + vertical `StackPanel`:

```xml
<ScrollViewer Grid.Column="0" VerticalScrollBarVisibility="Auto">
  <ItemsControl ItemsSource="{Binding Heroes}">
    <ItemsPanelTemplate><StackPanel Spacing="4" /></ItemsPanelTemplate>
    <ItemTemplate><views:CharacterCard Margin="2" /></ItemTemplate>
  </ItemsControl>
</ScrollViewer>
```

Same pattern for enemies in column 4. For parties larger than 2–3 characters, cards overflow and scrolling is needed.

---

## Proposed Approach

### Overview

Replace the single-column `StackPanel` per side with a layout that:

1. **Determines column count** per side based on character count and available width
2. **Computes a uniform scale** for that side so all cards (across N columns × M rows) fit within the available height
3. **Applies `LayoutTransform`** with `ScaleTransform` to the items panel
4. **Disables scrollbars** when all cards fit; re-enables only at extreme sizes

### Phase 1: Column count logic

Each side occupies a `*`-sized column (~half the screen minus the center 76px). With `CardWidth = 380` at full scale, the column count is:

| Characters | Columns | Rows per column |
|------------|---------|-----------------|
| 1–4        | 1       | 1–4             |
| 5–8        | 2       | 3–4             |
| 9–14       | 2       | 5–7             |
| 15–20      | 2–3     | 5–10            |
| 21–26      | 2       | 11–13           |

The column count is computed from available width and card width:

```csharp
private int ComputeColumnCount(int characterCount, double availableWidth)
{
    if (characterCount <= 4) return 1;
    var maxCols = (int)(availableWidth / (CardWidthAtFullScale + CardSpacing));
    var cols = Math.Min(maxCols, (int)Math.Ceiling(characterCount / 4.0));
    return Math.Clamp(cols, 1, 3);
}
```

The rationale: for 26 enemies, `maxCols` at 1920px viewport width will be ~2 (since each side gets ~900px — enough for 2 × 380px + spacing). This gives 2 columns × 13 rows.

### Phase 2: Scale computation in ViewModel

Add to `MainWindowViewModel`:

```csharp
// Per-side scale factors
private double _heroSideScale = 1.0;
public double HeroSideScale
{
    get => _heroSideScale;
    set => SetField(ref _heroSideScale, Math.Clamp(value, 0.35, 1.0));
}

private double _enemySideScale = 1.0;
public double EnemySideScale
{
    get => _enemySideScale;
    set => SetField(ref _enemySideScale, Math.Clamp(value, 0.35, 1.0));
}

// Per-side column counts
private int _heroColumns = 1;
public int HeroColumns
{
    get => _heroColumns;
    set => SetField(ref _heroColumns, value);
}

private int _enemyColumns = 1;
public int EnemyColumns
{
    get => _enemyColumns;
    set => SetField(ref _enemyColumns, value);
}
```

Scale constants:

```csharp
private const double CardWidthAtFullScale = 380.0;
private const double CardHeightAtFullScale = 240.0;
private const double CardSpacing = 8.0;  // 4 StackPanel spacing + 2+2 Margin
private const double MinScale = 0.35;
```

Recalculation method:

```csharp
public void RecalcSideLayout(double availableWidth, double availableHeight)
{
    HeroColumns = ComputeColumnCount(Heroes.Count, availableWidth);
    EnemyColumns = ComputeColumnCount(Enemies.Count, availableWidth);

    HeroSideScale = ComputeSideScale(availableHeight, Heroes.Count, HeroColumns);
    EnemySideScale = ComputeSideScale(availableHeight, Enemies.Count, EnemyColumns);
}

private int ComputeColumnCount(int characterCount, double availableWidth)
{
    if (characterCount <= 4) return 1;
    var maxPossibleCols = (int)(availableWidth / (CardWidthAtFullScale + CardSpacing));
    var sensibleCols = (int)Math.Ceiling(characterCount / 5.0);
    return Math.Clamp(sensibleCols, 1, Math.Max(1, maxPossibleCols));
}

private double ComputeSideScale(double availableHeight, int count, int cols)
{
    if (count <= 0 || cols <= 0) return 1.0;

    var rows = (int)Math.Ceiling((double)count / cols);
    var neededHeight = rows * CardHeightAtFullScale + (rows - 1) * CardSpacing;

    if (neededHeight <= availableHeight) return 1.0;
    return Math.Max(MinScale, availableHeight / neededHeight);
}
```

### Phase 3: Multi-column layout in XAML

Replace the `ScrollViewer` + `StackPanel` per side with a `WrapPanel`-based layout wrapped in a scaling container:

```xml
<Grid Grid.Column="0">
  <ScrollViewer VerticalScrollBarVisibility="Disabled"
                HorizontalScrollBarVisibility="Disabled">
    <ItemsControl ItemsSource="{Binding Heroes}">
      <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
          <WrapPanel Orientation="Horizontal" />
        </ItemsPanelTemplate>
      </ItemsControl.ItemsPanel>
      <ItemsControl.ItemTemplate>
        <DataTemplate>
          <views:CharacterCard Margin="2" />
        </DataTemplate>
      </ItemsControl.ItemTemplate>
      <ItemsControl.LayoutTransform>
        <ScaleTransform ScaleX="{Binding HeroSideScale}"
                        ScaleY="{Binding HeroSideScale}" />
      </ItemsControl.LayoutTransform>
    </ItemsControl>
  </ScrollViewer>
</Grid>
```

Key differences from current:
- `StackPanel` → `WrapPanel` with `Orientation="Horizontal"` — cards flow left-to-right, wrapping to new rows
- Added `LayoutTransform` with `ScaleTransform` bound to the side's scale
- Scrollbars disabled (all cards fit by design; ScrollViewer kept as safety net)

Apply the same pattern for enemies (col 4) with `EnemyColumns` / `EnemySideScale`.

### Phase 4: Wiring in code-behind

Call `RecalcSideLayout` from `PopulateCharacterCards` and on window resize:

```csharp
// In PopulateCharacterCards, after populating Heroes/Enemies:
var availableWidth = CombatCardsColumn.ActualWidth; // or similar binding
var availableHeight = CombatCardsRow.ActualHeight;
_vm.RecalcSideLayout(availableWidth, availableHeight);
```

Hook `SizeChanged` on the combat phase grid row to reflow:

```csharp
private void OnCombatCardAreaSizeChanged(object? sender, SizeChangedEventArgs e)
{
    if (_vm.IsCombatPhase)
        _vm.RecalcSideLayout(e.NewSize.Width, e.NewSize.Height);
}
```

---

## Examples

| Heroes | Enemies | Hero layout | Enemy layout | Hero scale | Enemy scale |
|--------|---------|-------------|--------------|------------|-------------|
| 1      | 1       | 1 col × 1 row | 1 col × 1 row | 1.0 | 1.0 |
| 4      | 6       | 1 col × 4 rows | 2 cols × 3 rows | 1.0 | ~0.8 |
| 6      | 12      | 2 cols × 3 rows | 2 cols × 6 rows | 1.0 | ~0.5 |
| 4      | 26      | 1 col × 4 rows | 2 cols × 13 rows | 1.0 | ~0.4 |

At 1920×1080 with ~600px available height for the card area:
- 6 heroes in 2×3: needs 240×3 + 8×2 = 736px — scales to ~0.82
- 26 enemies in 2×13: needs 240×13 + 8×12 = 3216px — scales to ~0.4 (min = 0.35, fits)

---

## Files to Modify

| File | Changes |
|------|---------|
| `BattleArena.Gui/ViewModels/MainWindowViewModel.cs` | Add `HeroSideScale`, `EnemySideScale`, `HeroColumns`, `EnemyColumns`, `RecalcSideLayout()`, `ComputeColumnCount()`, `ComputeSideScale()` |
| `BattleArena.Gui/Views/MainWindow.axaml` | Replace col 0 `StackPanel` with `WrapPanel` + `LayoutTransform`; same for col 4 |
| `BattleArena.Gui/Views/MainWindow.axaml.cs` | Call `RecalcSideLayout()` in `PopulateCharacterCards`; add `SizeChanged` handler |
| `BattleArena.Gui/Views/CharacterCard.axaml` | Possibly reduce `MinHeight` from 180 to improve compact layout appearance |

---

## Acceptance Criteria

- [ ] 1v1 duel: both cards at full scale, single column (no change)
- [ ] 4 heroes vs 6 enemies: heroes 1 col × 4 rows full scale; enemies 2 cols × 3 rows scaled to fit
- [ ] 4 heroes vs 26 enemies: heroes 1 col × 4 rows full scale; enemies 2 cols × 13 rows scaled to ~0.4
- [ ] 6 heroes vs 12 enemies: heroes 2 cols × 3 rows; enemies 2 cols × 6 rows; both sides fit
- [ ] Resizing the window reflows columns and scales
- [ ] All card content readable at minimum scale
- [ ] No change to combat simulation, playback, or presenter
- [ ] All 719 tests pass
