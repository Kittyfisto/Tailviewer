# UI Project Structure

The WPF app (`src/Tailviewer`) separates concerns into top-level folders; the csproj
`Compile`/`Page` lists are authoritative.

## Top-level folders

| Folder | Contents |
|--------|----------|
| `BusinessLogic/` | Non-UI logic: data sources, searches, filters, action center, plugins, exporters. No XAML. |
| `Ui/` | View models, controls, converters, data templates, popups. |
| `Settings/` | Plain-C# settings models (`MainWindowSettings`, `LogViewerSettings`, `ApplicationSettings`, …). |
| `Collections/` | Observable-collection helpers (`CompoundObservableCollection`, `FilteringObservableCollection`, `ProjectingObservableCollection`). |
| `Themes/` | XAML resource dictionaries: `Generic.xaml`, `Constants.xaml`, `Converters.xaml`. |
| `Resources/` | Fonts, icons, images. |

## `BusinessLogic/` vs `Ui/`

- Anything that reads files, parses logs, runs searches, or manages state goes in
  `BusinessLogic/` and must not reference WPF view types.
- `Ui/` view models consume `BusinessLogic/` services through interfaces
  (e.g. `BusinessLogic/Sources/ILogSourceFactory.cs`, `BusinessLogic/DataSources/IDataSources.cs`).
- The `NoThrow*` wrappers in `BusinessLogic/Sources/` isolate plugin failures
  (see [Error Handling](../core/error-handling.md)).

## Settings

- Settings are plain POCO classes with `I*Settings` interfaces, persisted via
  `IApplicationSettings` (`src/Tailviewer/Settings/IApplicationSettings.cs`).
- Nested settings group under feature folders: `Settings/Bookmarks/`,
  `Settings/Dashboard/`, `Settings/CustomFormats/`.
- When a new setting is added, update the owning settings class **and** its consumer;
  grep for the constant/field before changing values (see
  [Code Reuse Thinking Guide](../guides/code-reuse-thinking-guide.md)).

## Localization

The single source of truth for UI strings is the `STRINGS` dict in
`tools/generate_localization.py` — a PascalCase key mapped to an `(English, zh-CN)`
tuple. After adding/editing an entry, regenerate the artifacts:

```bash
python tools/generate_localization.py
```

This rewrites `src/Tailviewer/Localization/Strings.resx`, `Strings.zh-CN.resx`, and the
strongly-typed `Strings.cs`. Never hand-edit those three files; edit the dict and
regenerate. XAML references strings via `{x:Static loc:Strings.<Key>}`.

## Adding a new UI feature

Follow the existing feature shape: a `BusinessLogic/` service + interface, a
`Ui/<Feature>/` view model + `I*ViewModel` interface, a `*DataTemplate.xaml`, and a
registration into the parent view model (e.g. `MainWindowViewModel` /
`AbstractMainPanelViewModel`).
