# Implement: Configurable theme color

Ordered execution checklist. Run the validation gate at each marked step.

## 1. Settings layer

1.1 `src/Tailviewer/Settings/UISettings.cs`
- Add `public const string DefaultThemeColor = "#0047AB";`
- Add `public Color ThemeColor { get; set; }` (using `System.Windows.Media`), initialized to `#0047AB` in the constructor via `ColorConverter`/`ColorFromHex`.
- Update `Save` to `writer.WriteAttributeColor("themecolor", ThemeColor)`.
- Update `Restore` to read `themecolor` with `ReadAttributeAsColor(..., DefaultThemeColor, ...)`.
- Update `Clone()` to copy `ThemeColor`.

1.2 No interface change needed (`IApplicationSettings.Ui` already exposes `UISettings`).

**Gate**: build `Tailviewer` project.

## 2. Theme palette helper (pure, testable)

2.1 New `src/Tailviewer/Themes/ThemePalette.cs` (or `Ui/ThemePalette.cs`): a struct/class with the six derived `Color` values and a `static ThemePalette Compute(Color base)` implementing D2 lighten/darken blending.

**Gate**: compile.

## 3. ThemeManager service

3.1 New `src/Tailviewer/Themes/ThemeManager.cs`:
- `public static void Apply(Color base)` → computes `ThemePalette`, writes `Color` values into `Application.Current?.Resources` under keys `Primary`, `PrimaryLight`, `PrimaryLighter`, `PrimaryDark`, `PrimaryForeground`, `PrimarySeparator`, `Secondary`, `SecondaryLight`, `SecondaryLighter`, `SecondaryDark`, `SecondaryForeground`.

## 4. Resource dictionary restructure (`Themes/Constants.xaml`)

4.1 Remove the `Color` definitions for `Primary*`/`Secondary*` (keep `UnfocusedOverlay`, `TitleBar`, `OverlayBackground`).
4.2 Change the 11 `SolidColorBrush` definitions to bind `Color="{DynamicResource ...}"` instead of `StaticResource`.
4.3 Keep the merge of Metrolib Constants.

## 5. XAML reference migration

5.1 In the six files listed in D6, change all `{StaticResource <accent-brush>}` → `{DynamicResource <accent-brush>}` for the 11 brush keys (do NOT touch other keys like `ForegroundBrush`).

**Gate**: grep audit — no remaining `StaticResource` references to any of the 11 accent-brush keys.

## 6. Startup wiring (`App.cs`)

6.1 In `StartApplication`, after `var application = new App();` (and after `new App()` so `Application.Current` is set), call `ThemeManager.Apply(settings.Ui.ThemeColor);` before `window.Show()`.

## 7. View model (`SettingsFlyoutViewModel.cs`)

7.1 Add `public Color ThemeColor { get; set; }` backed by `_settings.Ui.ThemeColor`; on change set the setting, `ThemeManager.Apply(value)`, `EmitPropertyChanged()`, `_settings.SaveAsync()`.

## 8. Settings UI (`SettingsControl.xaml`)

8.1 Add a new `FlatGroupBox` with header `{x:Static loc:Strings.ThemeGroup}` containing a `ColorPicker` (`xctk:ColorPicker`) bound to `ThemeColor`, matching the log-level picker markup.

## 9. Localization

9.1 Add `ThemeColor` and `ThemeGroup` entries (English + Simplified Chinese) to the `STRINGS` dict in `tools/generate_localization.py`.
9.2 Run `python tools/generate_localization.py` to regenerate `Strings.resx`, `Strings.zh-CN.resx`, `Strings.cs`.

**Gate**: grep the three generated files for `ThemeColor`/`ThemeGroup`.

## 10. Tests

10.1 New `src/Tailviewer.Tests/Settings/UISettingsTest.cs` (or extend existing): default is `#0047AB`; save/restore roundtrip; restore from empty/invalid falls back to `#0047AB`; `Clone` copies the color.
10.2 New `src/Tailviewer.Tests/Themes/ThemePaletteTest.cs`: `Compute` returns base for `Primary`, white `PrimaryForeground`, black `SecondaryForeground`, and derived shades are deterministic and distinct from base.
10.3 Verify existing `SettingsMainPanelViewModelTest` still compiles/passes (view model constructor unchanged).

## Validation commands

- Build solution: `dotnet build Tailviewer.sln` (or via VS `VS_build_solution`). Primary project is `src/Tailviewer/Tailviewer.csproj`.
- Run tests: `dotnet test src/Tailviewer.Tests/Tailviewer.Tests.csproj` (NUnit 3.12). If the legacy non-SDK csproj isn't `dotnet test`-friendly, run the NUnit3 console/VS Test Explorer instead.
- Grep audit (step 5 gate): search XAML for `StaticResource Primary` / `StaticResource Secondary`.

## Rollback points

- After step 4 (resource restructure) — pure markup, revertible.
- After step 6 (startup wiring) — first end-to-end visible state; a full manual check happens here.
- After step 10 — final.

## Review gate (before `task.py start` acceptance)

- PRD + design + implement present (this file).
- All `[required]` planning steps complete.
