# Unify remaining icon colors to theme color

## Goal

After the theme color was made user-configurable, many UI icons and accent
elements still render the original hardcoded sky blue (`#3998D6`) from the
`Metrolib` control library instead of the user-selected theme color. Unify all
remaining blue accents to the theme color (with live update on theme change).

## Background / Confirmed Facts

- The theme system is: `UISettings.ThemeColor` → `ThemeManager.Apply(Color)` →
  `Application.Current.Resources["Primary"|"PrimaryLight"|"PrimaryDark"|...]` +
  `TextBrushes.UpdateTheme`. Accent brushes (`PrimaryBrush`, `PrimaryDarkBrush`,
  `PrimaryForegroundBrush`, …) are `SolidColorBrush`es whose `Color` is
  `{DynamicResource Primary}` (see `.trellis/spec/ui/theming.md`).
- The remaining blue comes from `Metrolib` (NuGet `Metrolib 0.3.0.162`, a
  compiled DLL). Its `Constants.cs` hardcodes `ForegroundColorAccent=#3998D6`,
  `Hovered=#2061A1`, `Pressed=#0B4680`, `Disabled=#C9DAEB`; its `Themes/Constants.xaml`
  defines `BackgroundBrushSelected/BorderBrushSelected/ForegroundBrushSelected=#3998D6`,
  `BorderBrushDisabled=#C9DAEB`, etc.
- Metrolib control templates reference these keys via `{StaticResource}`, which is
  resolved inside Metrolib's own merged dictionaries. Redefining those keys at
  app level therefore does **not** reach them; each affected control must be
  overridden via an app-level implicit style (or re-templated).
- Color-bearing dependency properties on Metrolib controls:
  `NormalForeground`, `HoveredForeground`, `PressedForeground`, `CheckedForeground`,
  `HoveredAndCheckedForeground`, `CheckedBackground`, `HoveredBackground`,
  `PressedBackground`, `InvertedForeground`, `DisabledOverlayBrush` (all are
  `TemplateBinding`/`Self`-binding driven, so setters work).
- Metrolib controls are in another assembly, so app-level overrides must live in
  `Application.Current.Resources` (implicit styles there beat theme styles), not in
  Tailviewer's own theme dictionary.

## Requirements

- R1. All Metrolib flat icon buttons used in the app render their icon in the theme
  color (`Primary`) when idle, and a darker theme shade on hover/press:
  `AddButton`, `CloseButton`, `MoreButton`, `RemoveButton`, `TrashButton`,
  `DownloadButton`, `FolderOpenButton`, `RefreshButton`, and `FlatIconButton`.
- R2. All Metrolib flat toggle buttons render their unchecked icon in `Primary` and
  their checked background in `Primary` with white foreground:
  `FlatToggleButton`, `EditToggleButton`, `ExpanderToggleButton`, `PluginToggleButton`.
- R3. `OneWayToggle` checked segment (background + border) uses `Primary` instead of
  `#3998D6`.
- R4. `PathChooserTextBox` trailing icon (the inner `MoreButton`) uses `Primary`, and
  its focus border uses `Primary` instead of `#3998D6`.
- R5. **Exception:** `PinToggleButton` icon is white in every state (normal, hover,
  press, checked), matching the adjacent white text on the themed panel header —
  never theme-colored, never blue.
- R6. Selection/highlight accents themed: data-source tree selected-item background
  (`TreeViewItemStyle`) and the action-center / auto-update banner backgrounds use
  `Primary`.
- R7. Disabled accents use a neutral gray (not `#C9DAEB`): disabled icons/overlays of
  the controls in R1–R4 and the `HyperlinkRun` disabled state.
- R8. `HyperlinkRun` (side-panel links) renders normal/hover/press/disabled states
  derived from the theme color instead of `Metrolib.Constants.ForegroundBrush*`.
- R9. Everything updates live when the user changes the theme color in Settings
  (no restart), consistent with the existing theme behavior.

## Acceptance Criteria

- [ ] AC1. With a non-default theme color selected, every control listed in R1–R8
      shows the theme color (or white/neutral-gray where specified) — no visible
      `#3998D6`/`#2061A1`/`#0B4680`/`#C9DAEB` accent remains.
- [ ] AC2. `PinToggleButton` icon is white and visually consistent with the adjacent
      white text on the data-source header, in both pinned and unpinned states.
- [ ] AC3. Changing the theme color in Settings recolors all affected elements
      immediately (live preview), including toggles, icons, selection highlight and
      focus border.
- [ ] AC4. No remaining hardcoded Metrolib blue references in Tailviewer's own source
      (verified by grep for `3998D6|2061A1|0B4680|C9DAEB|DAECF7` and the
      `BackgroundBrushSelected|BorderBrushSelected|ForegroundBrush*` keys in Tailviewer
      `.xaml`/`.cs`).
- [ ] AC5. Solution builds with zero errors/warnings; app starts and renders without
      XAML resource-resolution exceptions.

## Out of Scope

- Metrolib's internal grays, neutral borders/hover backgrounds, and the standard WPF
  `CheckBox`/`Button` system theme are left as-is (they are not sky-blue).
- Rebuilding/upgrading the `Metrolib` package itself.
- Non-accent structural/layout changes.
