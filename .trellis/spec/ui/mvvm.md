# MVVM Conventions

Tailviewer uses hand-written MVVM. There is no framework; follow the existing patterns.

## View models

- View models live under `src/Tailviewer/Ui/`, grouped by feature area:
  `Ui/Menu/`, `Ui/LogView/`, `Ui/DataSourceTree/`, `Ui/SidePanel/`, `Ui/ActionCenter/`, etc.
- Namespaces mirror `Tailviewer.Ui.*` (e.g. `Tailviewer.Ui.Menu`).
- A view model is a plain `sealed class` (or an `abstract` base such as
  `AbstractMainPanelViewModel`) implementing the relevant `I*ViewModel` interface
  from `src/Tailviewer/Ui/` or `src/Tailviewer.Api/Ui/`.

## `INotifyPropertyChanged`

Implement it directly with a `PropertyChanged` event and a
`EmitPropertyChanged([CallerMemberName] string propertyName = null)` helper. Setters
guard against no-op assignments before raising the event:

```csharp
// src/Tailviewer/Ui/AbstractMainPanelViewModel.cs
public event PropertyChangedEventHandler PropertyChanged;

protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public ISearchViewModel Search
{
    get { return _search; }
    protected set
    {
        if (value == _search)
            return;
        _search = value;
        EmitPropertyChanged();
    }
}
```

This exact shape (event + `EmitPropertyChanged` + guarded setter) repeats across the
codebase; use it rather than introducing a base-class library.

## Commands

- Commands are injected through the constructor as `System.Windows.Input.ICommand`.
  Do not create per-property `RelayCommand` factories; there is no command base class.
- Menus build dynamic command hierarchies out of small view models:
  `CommandMenuViewModel`, `ParentMenuViewModel`, `ToggleMenuViewModel`,
  `KeyBindingCommand`, `AbstractMenuViewModel` (`src/Tailviewer/Ui/Menu/`).
- Example: `src/Tailviewer/Ui/Menu/FileMenuViewModel.cs` composes
  `CommandMenuViewModel`s from constructor-injected `ICommand`s and assigns `Header`
  / `Icon` (from `Metrolib.Icons`).

## View <-> view model binding

- Views are `UserControl`s / `Window`s in `*.xaml` with a `*.xaml.cs` code-behind
  (e.g. `Ui/MainWindow.xaml` + `Ui/MainWindow.xaml.cs`,
  `Ui/LogView/LogViewerControl.xaml` + `Ui/LogView/LogViewerControl.cs`).
- Data templates live beside the view model they render
  (e.g. `Ui/LogView/LogViewMainPanelDataTemplate.xaml`), listed as `<Page>` items in
  the csproj.
- Custom converter classes live in `src/Tailviewer/Ui/Converters/` and are declared in
  XAML resource dictionaries such as `src/Tailviewer/Themes/Converters.xaml`
  (which re-exports `Metrolib.Converters`).

## What not to do

- Do not put business logic in view models or code-behind; it belongs in
  `src/Tailviewer/BusinessLogic/`.
- Do not introduce an MVVM framework or a `ViewModelBase` from a new library.
- Do not wire logic through XAML event handlers (`Click=`, `Loaded=`) when a command
  or binding expresses the same intent.
