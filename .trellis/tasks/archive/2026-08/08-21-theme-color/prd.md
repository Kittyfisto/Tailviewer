# Add configurable theme color

## Goal

Let the user configure Tailviewer's accent/theme color (the blue `Primary`/`Secondary` color used across the UI) from the Settings flyout, using the same color picker control already used for log-level colors. The default theme color changes from `#2196f3` to `#0047AB`.

## Requirements

- R1: A new "Theme color" setting is available in the Settings flyout (visual grouping near the existing Language/Log Viewer sections).
- R2: The theme color picker reuses the exact same picker control (`Extended.Wpf.Toolkit` `ColorPicker`) already used for log-level foreground/background colors.
- R3: The chosen theme color is persisted to the application settings file (`settings.xml`) and restored on next startup, following the existing `UISettings` pattern (like `Language`).
- R4: The default theme color is `#0047AB`. Users upgrading from an older version (no stored value) must get `#0047AB`, not the old blue.
- R5: Changing the theme color updates the accent color throughout the UI (all `Primary*`/`Secondary*` brushes) without requiring an application restart.
- R6: The derived accent shades (light, lighter, dark, separator) are derived deterministically from the single chosen color, so the user only picks one color.
- R7: The UI must remain fully localized: the new label text exists in English (`Strings.resx`) and Simplified Chinese (`Strings.zh-CN.resx`).

## Non-goals (out of scope)

- Light/dark mode switching.
- A palette of preset theme colors (single free-form color picker only).
- Changing the fixed foreground colors (`PrimaryForeground` = white, `SecondaryForeground` = black) or non-accent colors (`TitleBar`, `OverlayBackground`, `UnfocusedOverlay`).

## Acceptance Criteria

- [ ] A "Theme color" setting with a `ColorPicker` appears in the Settings flyout.
- [ ] Fresh settings default the theme color to `#0047AB`.
- [ ] Changing the color in the picker immediately recolors the app's accent UI (side panel, log viewer chrome, menus, toggle buttons) without restart.
- [ ] The selected color survives an app restart (persisted in `settings.xml`).
- [ ] An existing settings file with no stored theme color falls back to `#0047AB` after restore.
- [ ] The new setting is localized in English and Simplified Chinese.
- [ ] Existing unit tests still pass and new tests cover: default value, save/restore roundtrip, invalid-value fallback, and shade derivation.

## Notes

- This is a complex task; `design.md` and `implement.md` are required before `task.py start`.
