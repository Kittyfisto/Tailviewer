# Design — Add AND/OR toggle for log filter logic

## Overview

Introduce a per-data-source `FilterCombinationMode` (And/Or) that controls how the
active text quick filters combine. The level filter and time filter are separated from
the quick-filter chain so they can always be AND-ed, regardless of the mode.

## Current data flow

1. `QuickFiltersSidePanelViewModel.CreateFilterChain()` returns text quick filters +
   time filter as one flat `List<ILogEntryFilter>`.
2. `LogViewMainPanelViewModel.OnFiltersChanged()` / `ChangeDataSource()` assign it to
   `IDataSource.QuickFilterChain`.
3. `AbstractDataSource.CreateFilteredLogFile()` computes
   `Filter.Create(levelFilter, _quickFilterChain)`, which AND-s everything.

## Target data flow

1. `QuickFiltersSidePanelViewModel.CreateFilterChain()` returns only text quick
   filters; a new `CreateTimeFilter()` returns the time filter separately.
2. `LogViewMainPanelViewModel` assigns both `QuickFilterChain` and `TimeFilter`.
3. `AbstractDataSource.CreateFilteredLogFile()` computes
   `Filter.Create(levelFilter, _timeFilter, _quickFilterChain, FilterCombinationMode)`:
   `levelFilter AND timeFilter AND combine(textQuickFilters, mode)`.

## Contracts

### `FilterCombinationMode` (new enum, `Tailviewer.Core`)

```csharp
public enum FilterCombinationMode
{
    And = 0,
    Or = 1
}
```

File: `src/Tailviewer.Core/Settings/FilterCombinationMode.cs` (beside `FilterMatchType.cs`).

### `Filter` (Tailviewer.Core)

- Refactor `Create(IEnumerable<ILogEntryFilter>)` to delegate to a new
  `Create(IEnumerable<ILogEntryFilter>, FilterCombinationMode)`.
- New overload:
  `Create(LevelFlags levelFilter, ILogEntryFilter timeFilter, IEnumerable<ILogEntryFilter> quickFilters, FilterCombinationMode mode)`
  returning `Create([levelFilter, timeFilter, combine(quickFilters, mode)])`; nulls are
  filtered out by `Create`.
- `combine(filters, Or)` returns `new OrFilter(filters)`; `And` returns `new AndFilter(filters)`.
  Single-filter and empty cases behave exactly like today.

### `IDataSource`

Add:

```csharp
ILogEntryFilter TimeFilter { get; set; }
FilterCombinationMode FilterCombinationMode { get; set; }
```

### `AbstractDataSource`

- `TimeFilter`: stores `_timeFilter`, then `CreateFilteredLogFile()`.
- `FilterCombinationMode`: getter reads `_settings.FilterCombinationMode`; setter writes
  it and calls `CreateFilteredLogFile()` (mirrors `LevelFilter`).
- `CreateFilteredLogFile()` uses the new `Filter.Create` overload.

### `DataSource` (settings)

- New public field `FilterCombinationMode FilterCombinationMode` defaulting to `And`.
- Persist as XML attribute `filtercombinationmode` via `WriteAttributeEnum` /
  `ReadContentAsEnum`.
- Copy in `Clone()`.

### `FolderDataSource`

Delegate `TimeFilter` and `FilterCombinationMode` to `_mergedDataSource`; setters that
change filtering must refresh `_filteredLogSourceProxy.InnerLogSource` (same pattern as
`LevelFilter`).

### `MergedDataSource`

No change: inherits both members from `AbstractDataSource`.

### View model / UI

- `QuickFiltersSidePanelViewModel`:
  - `CreateFilterChain()` returns text filters only.
  - `CreateTimeFilter()` returns `_timeFilters.CreateFilter()`.
  - New `FilterCombinationMode` property reads/writes
    `CurrentDataSource?.DataSource?.FilterCombinationMode`, falls back to `And`, and
    raises `OnFiltersChanged` on change.
- `IDataSourceViewModel` / `AbstractDataSourceViewModel`: add `TimeFilter` and
  `FilterCombinationMode` pass-throughs (mirror `QuickFilterChain`).
- `LogViewMainPanelViewModel.OnFiltersChanged()` / `ChangeDataSource()`: assign
  `TimeFilter` and `FilterCombinationMode` in addition to `QuickFilterChain`.
- `QuickFiltersSidePanelDataTemplate.xaml`: add an AND/OR toggle in the header row (the
  `*` column). Two-state control bound to `FilterCombinationMode`; label via
  `Strings.FilterCombinationMode` / tooltip.
- `Strings.resx` + `Strings.zh-CN.resx`: add `FilterCombinationMode` (and tooltip
  string if needed); regenerate `Strings.cs` with `tools/generate_localization.py`.

## Trade-offs

- Separating the time filter into its own `IDataSource` property is more invasive than
  a single boolean toggle, but it is required to honor D1 (time filter always AND)
  cleanly and keeps `OrFilter`/`AndFilter` semantics unambiguous.
- Storing the mode per data source matches existing settings conventions and allows
  per-source preferences at the cost of one extra serialized attribute.

## Compatibility

- Backward compatible: missing `filtercombinationmode` attribute defaults to `And`.
- `Filter.Create(LevelFlags, andFilters, orFilters)` keeps its existing signature;
  the previously-unused `orFilters` parameter may now be honored or left untouched —
  the new overload is added rather than changing existing call sites' behavior.

## Rollback

- Revert is localized: remove the enum/field/UI, restore `CreateFilterChain()` to
  append the time filter, and restore the old `CreateFilteredLogFile()` call. No data
  migration beyond ignoring the new attribute (old builds will not read it).

## Verification

- `src/Tailviewer.Core.Tests/Filters/OrFilterTest.cs` already covers `OrFilter`.
- Add tests for the new `Filter.Create` overloads (AND vs OR; level/time always AND).
- Extend `src/Tailviewer.Tests/Settings/DataSourceTest.cs` (or `ApplicationSettingsTest.cs`)
  for `FilterCombinationMode` round-trip.
- Extend `src/Tailviewer.Tests/Ui/QuickFiltersSidePanelViewModelTest.cs` for the new
  property and that `CreateFilterChain()` excludes the time filter.
- Build: `dotnet build src/Tailviewer.sln` (or `Tailviewer.sln`), then run the touched
  test projects.
