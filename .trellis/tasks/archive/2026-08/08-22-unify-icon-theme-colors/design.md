# Design — Unify remaining icon colors to theme color

## Problem

The theme system only recolors elements that reference the `Primary*`/`Secondary*`
resource keys via `{DynamicResource}`. Metrolib's controls reference their own
hardcoded blue constants via `{StaticResource}` from inside Metrolib's compiled
theme dictionaries, so app-level resource redefinition cannot reach them. Each
affected control type must be overridden at the application-resource level.

## Key mechanism

Implicit styles in `Application.Current.Resources` take precedence over theme
styles regardless of which assembly declares the control. We add a single new
resource dictionary, `Themes/MetrolibTheme.xaml`, merged into `Application.Current.Resources`
right after Metrolib's `Themes/Generic.xaml` in `App`'s constructor.

Each override uses `BasedOn="{StaticResource {x:Type controls:X}}"` to inherit
Metrolib's existing template/visual structure, then re-sets only the color-bearing
properties to theme brushes. `{StaticResource {x:Type X}}` resolves Metrolib's
theme style (the app implicit style is not yet in scope while parsing), so there is
no self-reference. Fallback if a control ever throws a circular-reference: re-base on
the parent type and re-apply the `Icon` setter explicitly.

## Color mapping (semantic)

| Metrolib original (blue) | Theme resource | Notes |
| --- | --- | --- |
| `NormalForeground` = `BorderBrushSelected` `#3998D6` | `PrimaryBrush` | idle icon/chevron |
| `HoveredForeground` = `#2061A1` | `PrimaryDarkBrush` | darker on hover (preserves original darken metaphor) |
| `PressedForeground` = `#0B4680` | `PrimaryDarkBrush` | darkest available shade |
| `CheckedBackground` = `BackgroundBrushSelected` `#3998D6` | `PrimaryBrush` | selected/on state fill |
| `CheckedForeground` / `HoveredAndCheckedForeground` = white | `PrimaryForegroundBrush` | white (palette constant) |
| disabled `BorderBrushDisabled`/`ForegroundBrushDisabled` `#C9DAEB` | new neutral `DisabledForegroundBrush` `#B7B7B7` | neutral gray, not a theme tint |
| selection highlight `BackgroundBrushSelected` (Tailviewer XAML) | `PrimaryBrush` | tree/banner backgrounds |

`PinToggleButton` is special-cased to use `PrimaryForegroundBrush` (white) for all
foreground states and `Transparent` checked background, per requirement R5.

## Override list

### Icon buttons (R1) — flat button family
Types: `AddButton`, `CloseButton`, `MoreButton`, `RemoveButton`, `TrashButton`,
`DownloadButton`, `FolderOpenButton`, `RefreshButton`, `FlatIconButton`.

Set: `NormalForeground=PrimaryBrush`, `HoveredForeground=PrimaryDarkBrush`,
`PressedForeground=PrimaryDarkBrush`; style trigger `IsEnabled=False →
Foreground=DisabledForegroundBrush` (overrides the template's disabled trigger).

### Toggle buttons (R2) — flat toggle family
Types: `FlatToggleButton`, `EditToggleButton`, `ExpanderToggleButton`, `PluginToggleButton`.

Set: `NormalForeground=PrimaryBrush`, `HoveredForeground=PrimaryDarkBrush`,
`PressedForeground=PrimaryDarkBrush`, `CheckedForeground=PrimaryForegroundBrush`,
`HoveredAndCheckedForeground=PrimaryForegroundBrush`,
`CheckedBackground=PrimaryBrush`, `DisabledOverlayBrush=DisabledForegroundBrush`;
style trigger `IsEnabled=False → Foreground=DisabledForegroundBrush`.

### OneWayToggle (R3)
Derived style (BasedOn same type) that keeps the inherited template and adds
triggers overriding the checked-state blue:
- `IsChecked=True` (hovered or not) → `Background=PrimaryBrush`,
  `BorderBrush=PrimaryBrush`.
- `IsChecked=True && IsEnabled=False` → `Background/BorderBrush=DisabledForegroundBrush`.

### PinToggleButton (R5) — exception
`NormalForeground`, `HoveredForeground`, `PressedForeground`, `CheckedForeground`,
`HoveredAndCheckedForeground`, `InvertedForeground` → `PrimaryForegroundBrush`;
`CheckedBackground=Transparent`. Disabled state → `DisabledForegroundBrush`.

### PathChooserTextBox (R4)
Two parts:
1. Its trailing `MoreButton` is covered by the R1 `MoreButton` override.
2. Its focus border is a named `Border` hardcoded to `{StaticResource BorderBrushSelected}`
   inside Metrolib's template — must be re-templated. Copy Metrolib's
   `PathChooserTextBox` template and change only `focusBorder.BorderBrush` to
   `{DynamicResource PrimaryBrush}` (and watermark/disabled brushes to the neutral
   disabled gray where they were blue).

### Textbox focus borders (R6 / "含聚焦")
`FilterTextBox`, `EditorTextBox`, `FlatPasswordBox`, `SuggestionInputControl` all use
the same `focusBorder` with `{StaticResource BorderBrushSelected}`. Re-template each
the same minimal way (focus border → `PrimaryBrush`). This is mechanical and isolated;
each re-template is a separately verifiable chunk.

### Tailviewer-owned XAML/C# (R6–R8)
- `TreeViewItemStyle.xaml:63` selected background → `{DynamicResource PrimaryBrush}`
  (line 74 "unfocused" is a neutral gray, unchanged).
- `ActionCenterControl.xaml:40` and `AutoUpdateControl.xaml:15` banner background →
  `{DynamicResource PrimaryBrush}`.
- `ImageLabel.xaml:15` inverted text `#DAECF7` → neutral light gray `#E0E0E0`.
- `HyperlinkRun.cs` `UpdateForeground()` → derive from `ThemePalette.Compute(ThemeManager.CurrentPrimary)`:
  normal=`Primary`, hover/pressed=`PrimaryDark`, disabled=neutral gray.

## Data flow / live update

- XAML: `{DynamicResource Primary*Brush}` → `Constants.xaml` brush `Color={DynamicResource Primary*}`
  → `ThemeManager.Apply` rewrites the `Primary*` Color keys at app scope → whole tree updates live.
- `HyperlinkRun` recomputes on its next `UpdateForeground()` (mouse/state change); a
  theme change mid-hover picks up on the next state change. Acceptable minor edge case.
- `MetrolibTheme.xaml` merges `Constants.xaml` so its `{DynamicResource}` brush keys
  resolve in the same scope; the neutral `DisabledForegroundBrush` is defined locally
  (static, not theme-dependent).

## Compatibility / rollback

- Additive: one new XAML file + one `App.cs` line + a few `{StaticResource}→{DynamicResource}`
  swaps + one C# method body. No public API changes.
- Rollback: remove the `MetrolibTheme.xaml` merge in `App.cs` (reverts R1–R5 and the
  textbox re-templates); the remaining `.xaml`/`.cs` edits are simple color swaps.
- Existing Tailviewer-derived controls (`DataSourcesToggleButton`, `ToolbarToggleButton`,
  `FilterToggleButton`) already set their own colors and are unaffected by the new
  base-type overrides (their own setters win).

## Risks

- Same-type `BasedOn` could surface a circular-reference in a specific WPF version;
  mitigation documented above (re-base on parent type + explicit `Icon`).
- Re-templating the 4 textboxes + `PathChooserTextBox` duplicates Metrolib template
  markup; mitigated by changing only the focus-border/disabled brushes and keeping the
  rest byte-for-byte.
- The neutral disabled gray is a new single resource; if a future task wants a themed
  disabled tint, swap `DisabledForegroundBrush` to `PrimaryLighterBrush`.
