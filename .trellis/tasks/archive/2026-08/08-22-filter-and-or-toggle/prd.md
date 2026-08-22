# Add AND/OR toggle for log filter logic

## Goal

Let the user choose how the active quick filter entries combine. Today they are
always AND-ed: a log entry is shown only if it matches every active filter. Add a
toggle so the user can switch to OR: an entry is shown if it matches any active filter.

## Background / Confirmed Facts

- Quick filters are assembled as a flat `List<ILogEntryFilter>` and combined with an
  `AndFilter` in `Filter.Create(IEnumerable<ILogEntryFilter>)`
  (`src/Tailviewer.Core/Filters/Filter.cs:73-77`).
- The data source combines the per-source level filter with that chain in
  `AbstractDataSource.CreateFilteredLogFile()`:
  `Filter.Create(levelFilter, _quickFilterChain)` (`src/Tailviewer/BusinessLogic/DataSources/AbstractDataSource.cs`)
  — level filter, time filter and all quick filters are AND-ed together.
- An `OrFilter` already exists and is unit-tested but is not wired into the filter
  assembly path (`src/Tailviewer.Core/Filters/OrFilter.cs`,
  `src/Tailviewer.Core.Tests/Filters/OrFilterTest.cs`).
- `Filter.Create(LevelFlags, andFilters, orFilters)` already declares an unused
  `orFilters` parameter (`Filter.cs:141-155`).
- The quick filter chain (text filters + time filter) is produced by
  `QuickFiltersSidePanelViewModel.CreateFilterChain()`
  (`src/Tailviewer/Ui/SidePanel/QuickFilters/QuickFiltersSidePanelViewModel.cs`).
- Per-data-source settings are persisted in `Tailviewer.Settings.DataSource`
  (`src/Tailviewer/Settings/DataSource.cs`); the level filter lives there.
- Quick filters are application-wide; their per-source activation is stored per data
  source (`ActivatedQuickFilters`).
- `MergedDataSource` extends `AbstractDataSource`; `FolderDataSource` delegates filter
  members to its inner `MergedDataSource`.

## Requirements

- R1: A user-facing toggle in the quick filter side panel switches the combination
  operator applied to the active text quick filter entries between AND (current
  behavior) and OR.
- R2: In OR mode, a log entry is shown when it matches at least one active text quick
  filter entry.
- R3: In AND mode, behavior is unchanged from today.
- R4: The mode is per data source, persisted, and restored on next startup
  (consistent with `LevelFilter` / `ActivatedQuickFilters`).
- R5: Toggling the mode re-evaluates the currently displayed log immediately.
- R6: The level filter and the time filter always combine via AND, regardless of the
  selected mode.
- R7: Settings files saved before this feature load without error and default to AND.

## Acceptance Criteria

- [ ] AC1: With two active quick filters "foo" and "bar", AND mode shows only lines
        containing both; OR mode shows lines containing either.
- [ ] AC2: Toggling AND/OR updates the filtered log view without restart.
- [ ] AC3: The selected mode is per data source and survives an application restart.
- [ ] AC4: A settings file saved before this feature still loads and defaults to AND.
- [ ] AC5: Level filter and time filter remain AND-ed with the quick filter result in
        both modes.
- [ ] AC6: Unit tests cover the OR combination path (reusing `OrFilter`), the new
        `Filter.Create` overloads, and the setting's serialization round-trip.

## Out of Scope

- Changing the level filter or time filter combination semantics (always AND).
- Applying the mode to merged-child data sources individually beyond what the
  per-source setting already provides.

## Key Decisions

- D1: The OR toggle applies only to the text quick filter entries; level filter and
  time filter always AND.
- D2: The mode is stored per data source in `Tailviewer.Settings.DataSource`, not
  globally.
