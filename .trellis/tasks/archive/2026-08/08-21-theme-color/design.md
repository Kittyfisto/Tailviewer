# Design: Configurable theme color

## Context

Tailviewer is a .NET Framework 4.8 WPF app. The accent color is defined entirely in `src/Tailviewer/Themes/Constants.xaml`:

- `Primary` / `PrimaryLight` / `PrimaryLighter` / `PrimaryDark` / `PrimaryForeground` / `PrimarySeparator` (current default `#2196f3`)
- `Secondary` / `SecondaryLight` / `SecondaryLighter` / `SecondaryDark` / `SecondaryForeground` (currently mirrors Primary)

These `Color` keys are wrapped into `SolidColorBrush` keys (`PrimaryBrush`, `SecondaryBrush`, ...) and consumed in ~8 Tailviewer XAML files via `{StaticResource ...}` / `{DynamicResource ...}`. Metrolib (external NuGet lib) does NOT define these keys — verified by inspecting `Metrolib.dll` resources — so the accent color is 100% Tailviewer-owned. That makes the change self-contained.

Settings are persisted as XML via `ApplicationSettings` (`settings.xml`). `UISettings` already holds `Language` and is the natural home for a `ThemeColor` property. The settings flyout is `SettingsFlyoutViewModel` + `SettingsControl.xaml`, which already uses `xctk:ColorPicker` for log-level colors.

## Key challenge

The theme brushes are referenced with `StaticResource` in most places, and the `Color` keys are defined inside a dictionary (`Constants.xaml`) that is merged into several *local* scopes. Two facts drive the design:

1. `StaticResource` resolves once at parse time — can't react to runtime changes.
2. `DynamicResource` walks up the scope chain; a local definition would shadow an application-level override.

## Design decisions

### D1. Single source of truth at Application scope

- Remove the `Color` definitions for `Primary*`/`Secondary*` from `Constants.xaml`.
- In `Constants.xaml`, keep the `SolidColorBrush` keys but bind their `Color` with `{DynamicResource Primary}` etc. (not `StaticResource`).
- Define the actual `Color` resources (`Primary`, `PrimaryLight`, ..., `Secondary`, ...) at **Application scope** (`Application.Current.Resources`) at startup and whenever the setting changes.

Because the `Color` keys no longer exist in any local merged dictionary, `DynamicResource` on the brush's `Color` resolves to the Application-level value, and all brushes update in place when the Application-level `Color` changes. `PrimaryForeground` (white) and `SecondaryForeground` (black) are set as fixed values, unchanged from today.

### D2. Derived shade palette computed from one color

New pure static helper `ThemePalette.Compute(Color)` (and a small `ThemePalette` struct/class) that maps a base color to the six/five derived colors using lighten/darken blending (blend toward white/black by fixed factors):

- `Primary` = base
- `PrimaryLight` = lighten(base, 0.35)
- `PrimaryLighter` = lighten(base, 0.55)
- `PrimaryDark` = darken(base, 0.30)
- `PrimaryForeground` = white
- `PrimarySeparator` = `PrimaryDark`
- `Secondary` = base, `SecondaryLight` = `PrimaryLight`, `SecondaryLighter` = `PrimaryLighter`, `SecondaryDark` = `PrimaryDark`, `SecondaryForeground` = black

Keeping the helper pure makes it unit-testable. The exact blend factors are chosen to visually match the existing blue palette proportions (light = base→white 35%, lighter = 55%, dark = base→black 30%).

### D3. Theme application service

New `ThemeManager` (in `Tailviewer` UI layer) with:
- `void Apply(Color base)` — computes the palette and writes the `Color` values into `Application.Current.Resources` under the known keys.
- Called once during startup (in `App.StartApplication`, after `new App()` and before `window.Show()`), and again from the settings view model whenever the color changes (live update, no restart).

`ThemeManager` reads `Application.Current` defensively (null → no-op) so it stays safe in tests/design-time.

### D4. Settings persistence

`UISettings` gains:
- `public const string DefaultThemeColor = "#0047AB";` (or a `Color DefaultThemeColor` via `ColorConverter`)
- `public Color ThemeColor { get; set; }` initialized to `#0047AB`
- Save via `writer.WriteAttributeColor("themecolor", ThemeColor)` (existing `Core` extension)
- Restore via `reader.ReadAttributeAsColor("themecolor", Log, DefaultThemeColor, out ...)` with invalid-value fallback to default
- `Clone()` copies `ThemeColor`

This mirrors `LogLevelSettings` exactly (which already round-trips `Color` through XML).

### D5. View model + UI

- `SettingsFlyoutViewModel` gains `Color ThemeColor { get; set; }` backed by `_settings.Ui.ThemeColor`; on change it calls `_settings.SaveAsync()` **and** `ThemeManager.Apply(value)` for live preview.
- `SettingsControl.xaml` gains a new `FlatGroupBox` (header `Strings.ThemeGroup` or reuse `Appearance` grouping) containing a label + `xctk:ColorPicker SelectedColor="{Binding ThemeColor}" AvailableColorsSortingMode="HueSaturationBrightness"` — identical markup to the log-level pickers.
- New localization keys added to `STRINGS` in `tools/generate_localization.py` (single source of truth), regenerating `Strings.resx`, `Strings.zh-CN.resx`, and `Strings.cs`. Keys: `ThemeColor` (en "Theme color" / zh "主题色") and `ThemeGroup` (en "Theme" / zh "主题").

### D6. XAML reference migration

Change every accent-brush reference from `{StaticResource ...}` to `{DynamicResource ...}` for the 11 keys: `PrimaryBrush`, `PrimaryLightBrush`, `PrimaryLighterBrush`, `PrimaryDarkBrush`, `PrimaryForegroundBrush`, `PrimarySeparatorBrush`, `SecondaryBrush`, `SecondaryLightBrush`, `SecondaryLighterBrush`, `SecondaryDarkBrush`, `SecondaryForegroundBrush`. Affected files (from grep):

- `Ui/SidePanel/SidePanelControl.xaml` (already partially Dynamic)
- `Ui/MainWindow.xaml`
- `Ui/Menu/MenuItemTemplates.xaml`
- `Ui/DataSourceTree/DataSourcesToggleButton.xaml`
- `Ui/LogView/LogViewerControl.xaml`
- `Ui/LogView/ToolbarToggleButtonStyle.xaml`

## Data flow

```
settings.xml <-> UISettings.ThemeColor <-> SettingsFlyoutViewModel.ThemeColor
                                                     |
                                                     v
                                              ThemeManager.Apply
                                                     |
                          Application.Current.Resources["Primary" | "PrimaryLight" | ...]
                                                     |
                                        DynamicResource bindings on brushes
                                                     |
                                           all accent UI elements
```

## Compatibility / rollback

- Old settings files without `themecolor` fall back to `#0047AB` on restore (acceptance criterion).
- The XAML changes are purely markup reference style changes; no behavioral change to layout.
- Rollback = revert the XAML/`UISettings`/`ThemeManager` changes; no migration or data-format breakage (new attribute is additive).

## Risks & mitigations

- **Risk**: A missed `StaticResource` reference means part of the UI keeps the old color. **Mitigation**: grep audit step in `implement.md` (assert zero remaining `StaticResource` for the 11 keys) plus a manual visual pass.
- **Risk**: `DynamicResource` on brush `Color` fails if a `Color` key is still defined locally. **Mitigation**: remove all local `Color` definitions for the accent keys; verify via grep.
- **Risk**: `Color` set at Application scope before any window exists. **Mitigation**: call `ThemeManager.Apply` right after `new App()` construction (Application.Current available) and before `window.Show()`.
