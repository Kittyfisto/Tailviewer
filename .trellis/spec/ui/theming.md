# Theming & Accent Color

Tailviewer's accent/theme color is user-configurable and updates the whole UI live
(no restart). All accent colors derive from **one** base color (default `#FF222280`,
`UISettings.DefaultThemeColor`). The UI theme mode is persisted independently as
`Light`, `Dark`, or `System`.

## Architecture

```
UISettings.ThemeColor + ThemeMode
    -> ThemeManager.Apply(Color, ThemeMode)
                                       |  ResolveDarkMode (System -> provider)
                                       v
                            ApplyCore(Color, bool) -> Application.Current.Resources
                                       |                 ["Primary", "PrimaryLight", ...]
                                       +--> TextBrushes.UpdateTheme/UpdateNeutral
                                       +--> ThemeChanged
                                              |              |
                                              v              v
                                   LogEntryListView rebuilds   settings ColorPickers
                                   theme-aware TextBrushes
                                                           |
                                                           v
                                              XAML brushes bind via {DynamicResource}
```

- `src/Tailviewer/Ui/ThemePalette.cs` — pure helper `ThemePalette.Compute(Color)` derives
  the shade palette (light / lighter / dark / separator) by blending the base toward
  white/black. Pure & unit-testable (`ThemePaletteTest`).
- `src/Tailviewer/Ui/ThemeManager.cs` — `Apply(Color, ThemeMode)` is the startup and
  live-update entry point. `ThemeMode` controls the neutral UI (Light / Dark / System),
  and the effective light/dark result selects the matching log-level defaults. All
  callers publish the semantic resources, update the shared brushes, and raise
  `ThemeChanged`.
  Resource publication is a no-op when `Application.Current` is null (tests / design
  time), but brush updates and the event still run.
- `src/Tailviewer/Themes/Constants.xaml` — defines `SolidColorBrush` keys
  (`PrimaryBrush`, `SecondaryBrush`, …) whose `Color` is `{DynamicResource Primary}` etc.
  The `Color` keys themselves live **only** at application scope, set by `ThemeManager`.
- `src/Tailviewer/Ui/LogView/TextBrushes.cs` — static brushes used by `FormattedText`
  (line numbers, character code, selection). Every shared brush is frozen and
  `UpdateTheme(Color)` / `UpdateNeutral(bool)` replace the references instead of
  mutating a `SolidColorBrush`; this prevents WPF cross-thread ownership failures.
  Each `TextBrushes` instance receives the active internal `LogLevelPalette` and owns
  frozen level brushes. `LogEntryListView` rebuilds the instance and visible lines
  after `ThemeChanged`.

## Theme mode and system provider

- `ThemeMode.Light` always resolves to light; `ThemeMode.Dark` always resolves to dark;
  `ThemeMode.System` calls the injected `ISystemThemeProvider.IsDark()`.
- `UISettings.Save` writes `thememode`. `Restore` accepts only defined enum values;
  an invalid present value falls back to `Light`, while a missing `thememode` migrates
  the legacy `darkmode` attribute (`true` -> `Dark`, otherwise `Light`).
- `WindowsSystemThemeProvider` reads the Windows `AppsUseLightTheme` registry value,
  marshals `UserPreferenceChanged` to `Application.Current.Dispatcher`, and is disposed
  from `App.Exit`. Do not subscribe to `SystemEvents` from a static constructor.

## Log-level default palette

`LogLevelDefaults.Light` and `.Dark` are the single source for the seven level defaults.
The exact pairs are:

| Level | Light foreground / background | Dark foreground / background |
|---|---|---|
| TRACE | `#9AA5B1` / transparent | `#5B6472` / transparent |
| DEBUG | `#6B7A8F` / transparent | `#7E8C9E` / transparent |
| OTHER | `#7C6E9E` / transparent | `#8E7FAE` / transparent |
| INFO | `#333333` / transparent | `#DCDCDC` / transparent |
| WARNING | `#8A4D00` / `#FFF3D6` | `#F5D77E` / `#5A4700` |
| ERROR | `#B3261E` / `#FDECEA` | `#FFB4AB` / `#6B1F1F` |
| FATAL | `#FFFFFF` / `#C5221F` | `#FFFFFF` / `#9E1B1B` |

`LogLevelSettings.IsCustom` is persisted as `iscustom`. Legacy level nodes without that
attribute are custom when their stored pair differs from the new Light pair. A non-custom
level gets frozen brushes from the active `LogLevelPalette`; a custom level owns frozen
brushes from its stored colors. When a settings ColorPicker changes only one field for
the first time, its ViewModel must seed the other stored field from the current effective
palette default before setting `IsCustom=true`; otherwise a stale Light value can leak
into the selected palette.

### Theme-following log-level palette contract

- The settings page exposes only the overall `ThemeMode` selector; there is no separate
  log-level palette selector or persisted palette choice.
- `ThemeManager.Apply(Color, ThemeMode)` resolves `System` through
  `ISystemThemeProvider`, then sets `CurrentLogLevelPalette` to `Dark` for an effective
  dark theme and `Light` otherwise.
- Changing the overall theme updates an existing log viewer and log-level settings
  through `ThemeChanged`, so default colors always follow the visible theme.
- The settings reset action restores `UISettings.DefaultThemeColor`, calls
  `ILogViewerSettings.RestoreDefaultColors()` to clear every `IsCustom` flag and restore
  the Light persisted baseline, saves the settings, and reapplies the current
  `ThemeMode` on the Dispatcher so both Light and Dark defaults become visible
  immediately.

## Rules

1. **Accent brushes are `{DynamicResource}`, never `{StaticResource}`.** A
   `StaticResource` captures the value once at parse time and will not follow a theme
   change. When migrating existing references, grep for
   `StaticResource (Primary|Secondary)` and flip them.
2. **Derive, never hardcode.** New accent shades go through `ThemePalette.Compute`; do
   not hardcode blue variants (`#2196f3`, `DodgerBlue`, `43,145,175`, …).
3. **Apply on the Dispatcher.** When the color is changed from within a control's own
   callback (e.g. the settings `ColorPicker`), defer `ThemeManager.Apply` with
   `Application.Current.Dispatcher.BeginInvoke(...)`. Applying synchronously mutates
   application resources mid-layout and can re-enter the rendering pass.
4. **Startup wiring** happens in `App.StartApplication` right after `new App()` (so
   `Application.Current` is set) and before `window.Show()`.

## Overriding Metrolib control colors

Metrolib controls hardcode the accent blue (`#3998D6` and friends) in
`Metrolib.Constants.cs` / `Metrolib/Themes/Constants.xaml` and reference it via
`{StaticResource}` **inside their own compiled theme dictionaries**. Redefining
those resource keys at app scope does **not** reach them. To re-theme an external
(Metrolib) control:

1. Put an **implicit style in `Application.Current.Resources`** (not Tailviewer's
   theme dictionary — theme styles lose to app resources). In this project that
   means adding it to `Themes/MetrolibTheme.xaml`, merged in `App`'s constructor
   right after Metrolib's `Themes/Generic.xaml`.
2. Base it on the control's own theme style with
   `BasedOn="{StaticResource {x:Type controls:X}}"` — this resolves Metrolib's
   style (the app implicit style is not yet in scope while parsing), then re-set
   only the color-bearing properties to `{DynamicResource Primary*Brush}`.
3. Accent brushes must be `{DynamicResource}` (live theme change); neutral grays
   may be `{StaticResource}`.

### Metrolib color-property model

| Control family | Color properties | Icon draw |
| --- | --- | --- |
| `FlatButton` + icon buttons (`FlatIconButton`, `AddButton`, `CloseButton`, `MoreButton`, `RemoveButton`, `TrashButton`, `DownloadButton`, `FolderOpenButton`, `RefreshButton`) | `NormalForeground`, `HoveredForeground`, `PressedForeground` | `Foreground` (`Icon` geometry) |
| `FlatToggleButton` + subclasses (`EditToggleButton`, `PluginToggleButton`, …) | `NormalForeground`, `HoveredForeground`, `PressedForeground`, `CheckedForeground`, `HoveredAndCheckedForeground`, `CheckedBackground`, `DisabledOverlayBrush` | `Foreground` |
| `OneWayToggle` | checked `Background`/`BorderBrush` via style triggers | — |
| `PinToggleButton` | `*Foreground`, `InvertedForeground`, `CheckedBackground` | `Foreground` (`Icons.Pin`/`PinOff`) |
| `PathChooserTextBox` / `FilterTextBox` / `EditorTextBox` / `FlatPasswordBox` | focus border is a named `Border` hardcoded to `BorderBrushSelected` → must re-template and swap `focusBorder.BorderBrush` to `{DynamicResource PrimaryBrush}` | inner `MoreButton`/`CloseButton` |

> **Exception: `ExpanderToggleButton` is NOT a `FlatToggleButton`.** It has no
> `NormalForeground`/`CheckedForeground`; it draws via `Foreground` +
> `InvertedForeground` and is neutral (dark gray) by default. Setting
> `NormalForeground` on it fails the build with `MC4005`.

## Gotchas

> **Warning: `{x:Static TextBrushes.Xxx}` freezes the brush.**
>
> WPF freezes a `Freezable` (SolidColorBrush) when it is assigned through a
> `{x:Static}` reference on a dependency property. A frozen brush must never be
> mutated in place. All shared `TextBrushes` updates therefore create and assign a
> new frozen brush, so a later theme change cannot read or write a brush owned by
> another thread.
>
> - Theme-derived brushes that must change at runtime: bind in XAML via
>   `{DynamicResource PrimaryBrush}` (application-scope resource), or replace the
>   C# brush reference with a newly created frozen brush before redrawing.
> - Never expose a theme-derived brush through `{x:Static}` in XAML.

> **Warning: `DynamicResource` on a brush `Color` only works if the `Color` key is not
> defined locally.** `Constants.xaml` must not re-declare the accent `Color` keys, or a
> merged local dictionary will shadow the application-scope value.

## Tests

- `Ui/ThemePaletteTest` — palette derivation (base == primary, fixed foregrounds,
  deterministic shades).
- `Ui/TextBrushesTest` — defaults follow `UISettings.DefaultThemeColor`; `UpdateTheme`
  replaces frozen derived brushes; light/dark level palette selection creates the
  expected frozen defaults and custom brushes remain unchanged.
- `Settings/UISettingsTest` — `#FF222280` default, three-mode save/restore, legacy
  `darkmode` migration, invalid-value fallback, clone.
- `Settings/LogLevelDefaultsTest` and `Settings/LogViewerSettingsTest` — exact Light/Dark
  pairs, legacy `iscustom` inference, and restoration of all default colors.
- `Ui/Settings/LogLevelSettingsViewModelTest` — first custom edit seeds the unmodified
  field from the active theme and calls `SaveAsync`.
- `Ui/SettingsMainPanelViewModelTest` — the settings reset command restores the accent
  color and theme-following log-level state.
