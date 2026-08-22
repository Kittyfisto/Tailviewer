# Implement — Unify remaining icon colors to theme color

Ordered checklist. Each item is independently buildable/verifiable.

## 1. Neutral disabled resource + new override dictionary

1.1 Create `src/Tailviewer/Themes/MetrolibTheme.xaml`:
- MergedDictionaries → `pack://application:,,,/Tailviewer;component/Themes/Constants.xaml`.
- `<SolidColorBrush x:Key="DisabledForegroundBrush" Color="#B7B7B7" />`.
- Namespaces: `controls` = `clr-namespace:Metrolib.Controls;assembly=Metrolib`,
  `metrolib` = `clr-namespace:Metrolib;assembly=Metrolib` (only if Icon re-base fallback
  needed), `converters` = `clr-namespace:Metrolib.Converters;assembly=Metrolib` (for
  PathChooserTextBox re-template).

1.2 Add the R1 button-family overrides (`AddButton`, `CloseButton`, `MoreButton`,
`RemoveButton`, `TrashButton`, `DownloadButton`, `FolderOpenButton`, `RefreshButton`,
`FlatIconButton`) — `BasedOn="{StaticResource {x:Type controls:X}}"` + the three
foreground setters + disabled trigger (per design).

1.3 Add the R2 toggle-family overrides (`FlatToggleButton`, `EditToggleButton`,
`ExpanderToggleButton`, `PluginToggleButton`) — foreground/checked-background setters +
`DisabledOverlayBrush` + disabled trigger.

1.4 Add `OneWayToggle` derived style with checked-state triggers (R3).

1.5 Add `PinToggleButton` white-override style (R5).

1.6 Add `PathChooserTextBox` re-template (focus border → `{DynamicResource PrimaryBrush}`,
watermark/disabled → neutral). Keep everything else identical to Metrolib's template;
declare the `EmptyToVisibleConverter` locally if referenced.

## 2. Wire the dictionary into the app

2.1 In `src/Tailviewer/App.cs` constructor, after the existing Metrolib
`Themes/Generic.xaml` merge, add:
`Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Tailviewer;component/Themes/MetrolibTheme.xaml") });`

## 3. Textbox focus-border re-templates (R6 "含聚焦")

3.1 Re-template `FilterTextBox` (focus border → `PrimaryBrush`).
3.2 Re-template `EditorTextBox` (same).
3.3 Re-template `FlatPasswordBox` (same).
3.4 Re-template `SuggestionInputControl` (same).
> Each template is copied from Metrolib v0.3.0.162 and only the `focusBorder.BorderBrush`
> (and blue disabled/watermark brushes where present) are swapped to theme/neutral.

## 4. Tailviewer-owned color swaps

4.1 `Ui/DataSourceTree/TreeViewItemStyle.xaml:63` → `{DynamicResource PrimaryBrush}`.
4.2 `Ui/ActionCenter/ActionCenterControl.xaml:40` → `{DynamicResource PrimaryBrush}`.
4.3 `Ui/AutoUpdateControl.xaml:15` → `{DynamicResource PrimaryBrush}`.
4.4 `Ui/ImageLabel.xaml:15` `#DAECF7` → `#E0E0E0`.
4.5 `Ui/SidePanel/HyperlinkRun.cs` `UpdateForeground()` → theme-derived colors.

## 5. Validation

5.1 Build the solution (`Tailviewer.sln`, net48 WPF): zero errors/warnings.
5.2 Grep gate — confirm no blue keys/hexes remain in Tailviewer source:
`rg -ni "3998D6|2061A1|0B4680|C9DAEB|DAECF7|BackgroundBrushSelected|BorderBrushSelected|ForegroundBrushSelected|ForegroundBrushHovered|ForegroundBrushPressed|BorderBrushDisabled|ForegroundBrushDisabled|ForegroundBrushAccent" src/Tailviewer`
Expected hits: only the neutralized/re-templated items (none referencing the blue).
5.3 Run the UI smoke check: launch the app, change theme color in Settings, and verify
AC1–AC3 (icons/toggles/focus border/selection recolor live; pin stays white).

## Rollback

- R1–R5 + textbox re-templates: delete the `MetrolibTheme.xaml` merge line in `App.cs`
  (and the file if desired).
- R6–R8 swaps: revert the individual `.xaml`/`.cs` hunks.

## Gate before `task.py start`

- [ ] All of §1–§4 implemented.
- [ ] §5.1 and §5.2 pass.
- [ ] Review gate on `design.md` mapping table matches implementation.
