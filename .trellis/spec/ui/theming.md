# Theming & Accent Color

Tailviewer's accent/theme color is user-configurable and updates the whole UI live
(no restart). All accent colors derive from **one** base color (default `#0047AB`,
`UISettings.DefaultThemeColor`).

## Architecture

```
UISettings.ThemeColor  ->  ThemeManager.Apply(Color)  ->  Application.Current.Resources
                              |                                ["Primary", "PrimaryLight", ...]
                              +--> TextBrushes.UpdateTheme     ["Secondary", "SecondaryLight", ...]
                                   (mutable SolidColorBrush)           |
                                                                       v
                                                          XAML brushes bind via {DynamicResource}
```

- `src/Tailviewer/Ui/ThemePalette.cs` — pure helper `ThemePalette.Compute(Color)` derives
  the shade palette (light / lighter / dark / separator) by blending the base toward
  white/black. Pure & unit-testable (`ThemePaletteTest`).
- `src/Tailviewer/Ui/ThemeManager.cs` — `Apply(Color)` writes the palette into
  `Application.Current.Resources` under the well-known `Color` keys, then calls
  `TextBrushes.UpdateTheme`. No-op when `Application.Current` is null (tests / design time).
- `src/Tailviewer/Themes/Constants.xaml` — defines `SolidColorBrush` keys
  (`PrimaryBrush`, `SecondaryBrush`, …) whose `Color` is `{DynamicResource Primary}` etc.
  The `Color` keys themselves live **only** at application scope, set by `ThemeManager`.
- `src/Tailviewer/Ui/LogView/TextBrushes.cs` — static brushes used by `FormattedText`
  (line numbers, character code, selection). The three theme-derived brushes are mutable
  `SolidColorBrush`es updated by `UpdateTheme(Color)`.

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
> `{x:Static}` reference on a dependency property. A frozen brush is read-only, so a
> later `TextBrushes.UpdateTheme` that sets `.Color` throws
> `InvalidOperationException` ("cannot set a property on a read-only object").
>
> - Theme-derived brushes that must change at runtime: bind in XAML via
>   `{DynamicResource PrimaryBrush}` (application-scope resource), OR keep them as
>   C#-only mutable brushes consumed by `FormattedText` (which does not freeze them).
> - Never expose a theme-derived brush through `{x:Static}` in XAML.

> **Warning: `DynamicResource` on a brush `Color` only works if the `Color` key is not
> defined locally.** `Constants.xaml` must not re-declare the accent `Color` keys, or a
> merged local dictionary will shadow the application-scope value.

## Tests

- `Ui/ThemePaletteTest` — palette derivation (base == primary, fixed foregrounds,
  deterministic shades).
- `Ui/TextBrushesTest` — defaults follow `UISettings.DefaultThemeColor`; `UpdateTheme`
  recolors the three derived brushes.
- `Settings/UISettingsTest` — `#0047AB` default, save/restore roundtrip, invalid-value
  fallback, clone.
