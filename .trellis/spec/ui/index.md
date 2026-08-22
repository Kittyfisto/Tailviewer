# WPF UI Guidelines

Guidance for working in `src/Tailviewer`, the WPF application. Tailviewer uses raw
WPF MVVM — there is **no** MVVM framework (no Prism, Caliburn.Micro, ReactiveUI, or
CommunityToolkit.Mvvm).

## Index

| Guide | Covers |
|-------|--------|
| [MVVM conventions](./mvvm.md) | View-model patterns, `INotifyPropertyChanged`, commands |
| [Project structure](./project-structure.md) | `BusinessLogic` vs `Ui` vs `Settings`, XAML and converters |
| [Theming & accent color](./theming.md) | `ThemeManager`/`ThemePalette`, `DynamicResource` accent brushes, frozen-brush gotcha, overriding Metrolib control colors |

## Key facts

- Startup object is `Tailviewer.Bootstrapper` (`src/Tailviewer/Bootstrapper.cs`), set via
  `<StartupObject>` in `src/Tailviewer/Tailviewer.csproj` — there is no `App.xaml`.
- The `Metrolib` library (same author) supplies base controls, `Icons`, and common
  converters; it is referenced by both `Tailviewer` and `Tailviewer.Core`.
- Third-party UI packages in use: `Extended.Wpf.Toolkit` (Xceed), `Ookii.Dialogs.Wpf`.
