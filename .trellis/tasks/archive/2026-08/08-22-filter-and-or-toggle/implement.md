# Implement — Add AND/OR toggle for log filter logic

Ordered checklist. Each item is a review gate; verify with the listed command before
moving on.

## 1. Core (Tailviewer.Core)

- [ ] 1.1 Add `src/Tailviewer.Core/Settings/FilterCombinationMode.cs` with
      `public enum FilterCombinationMode { And = 0, Or = 1 }`.
- [ ] 1.2 `src/Tailviewer.Core/Filters/Filter.cs`:
      - refactor `Create(IEnumerable<ILogEntryFilter>)` → call new
        `Create(IEnumerable<ILogEntryFilter>, FilterCombinationMode)` (default `And`);
      - add the mode-aware overload returning `AndFilter`/`OrFilter`;
      - add `Create(LevelFlags, ILogEntryFilter timeFilter,
        IEnumerable<ILogEntryFilter> quickFilters, FilterCombinationMode)`.

## 2. Data source layer (Tailviewer)

- [ ] 2.1 `IDataSource.cs`: add `TimeFilter` and `FilterCombinationMode`.
- [ ] 2.2 `AbstractDataSource.cs`: implement both; update `CreateFilteredLogFile()` to
      `Filter.Create(LevelFilter, _timeFilter, _quickFilterChain, FilterCombinationMode)`.
- [ ] 2.3 `FolderDataSource.cs`: delegate both to `_mergedDataSource`; refresh
      `_filteredLogSourceProxy.InnerLogSource` on filter-affecting setters.

## 3. Settings (persistence)

- [ ] 3.1 `src/Tailviewer/Settings/DataSource.cs`: add field, XML save/restore
      (`filtercombinationmode`), and `Clone()` copy.

## 4. View models + UI

- [ ] 4.1 `QuickFiltersSidePanelViewModel.cs`: split `CreateFilterChain()` (text only)
      + new `CreateTimeFilter()`; add `FilterCombinationMode` property bound to current
      data source, raising `OnFiltersChanged`.
- [ ] 4.2 `IDataSourceViewModel.cs` + `AbstractDataSourceViewModel.cs`: pass through
      `TimeFilter` and `FilterCombinationMode`.
- [ ] 4.3 `LogViewMainPanelViewModel.cs`: in `OnFiltersChanged()` and
      `ChangeDataSource()` also assign `TimeFilter` and `FilterCombinationMode`.
- [ ] 4.4 `QuickFiltersSidePanelDataTemplate.xaml`: add AND/OR toggle in the header.
- [ ] 4.5 Localization: add strings to `Strings.resx` + `Strings.zh-CN.resx`, run
      `python tools/generate_localization.py`.

## 5. Tests

- [ ] 5.1 Core: `FilterTest` / `OrFilterTest` for the new overloads (AND vs OR; level
      and time always AND).
- [ ] 5.2 Settings: `DataSourceTest` (or `ApplicationSettingsTest`) for
      `FilterCombinationMode` round-trip + default `And` on missing attribute.
- [ ] 5.3 Ui: `QuickFiltersSidePanelViewModelTest` for the new property and
      `CreateFilterChain()` excluding the time filter.

## 6. Validation

- [ ] 6.1 `dotnet build Tailviewer.sln` (no errors).
- [ ] 6.2 `dotnet test src/Tailviewer.Core.Tests/Tailviewer.Core.Tests.csproj`.
- [ ] 6.3 `dotnet test src/Tailviewer.Tests/Tailviewer.Tests.csproj`.

## Rollback points

- R1 (before 1.2): pure additive enum; nothing to roll back.
- R2 (after 2.x): revert `Filter.Create` overloads and restore the old
  `CreateFilteredLogFile()` expression; keep enum/UI only if desired.
- R3 (after 3.x): settings attribute is ignored by old builds; remove field + XML.
