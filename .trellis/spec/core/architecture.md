# Core Architecture & Abstractions

The authoritative description of Tailviewer's core concepts is `docs/Architecture.md`.
This page turns those concepts into the concrete coding patterns you must follow when
editing `src/Tailviewer.Api` and `src/Tailviewer.Core`.

## Project layout

`src/Tailviewer.Core` groups files by concept, not by namespace. The `Compile` items
in `Tailviewer.Core.csproj` are the ground truth; representative groups:

- `Buffers/` — `LogBufferArray`, `LogBufferList`, `LogBufferView`, views (`SingleColumnLogBufferView`, `CombinedLogBufferView`)
- `Columns/` — `Columns`, `WellKnownColumnDescriptor`, `CustomColumnDescriptor`
- `Entries/` — `LogEntry`, `ReadOnlyLogEntry`, `AbstractLogEntry`, entry views
- `Filters/` — `Filter` base and subclasses (`SubstringFilter`, `RegexFilter`, `LevelFilter`, …) plus `Filters/ExpressionEngine/` for the expression parser/tokenizer
- `Parsers/` — `TimestampParser`, `DateTimeParser`, `LogLevelParser`
- `Properties/` — `Properties`, `PropertiesBufferList`, `WellKnownProperty`
- `Sources/` — log source implementations, split by strategy: `Sources/Text/`, `Sources/Merged/`, `Sources/Buffer/`, `Sources/Adorner/`
- `Settings/` — `QuickFiltersSettings`, `TimeFilterSettings`, etc.

## Core concepts

### Descriptors

Descriptors describe a property or column: its .NET type, default value, unique id,
and (de)serialization. They do **not** hold data. The interfaces are
`IReadOnlyPropertyDescriptor` / `IPropertyDescriptor` / `IColumnDescriptor` in
`src/Tailviewer.Api`.

### Well-known columns and properties

Well-known columns live in a `public static class Columns` with
`public static readonly IColumnDescriptor<T>` fields, initialized in a static
constructor via `WellKnownColumnDescriptor<T>(id, defaultValue)`:

```csharp
// src/Tailviewer.Core/Columns/Columns.cs
public static class Columns
{
    public static readonly IColumnDescriptor<string> RawContent;
    public static readonly IColumnDescriptor<LevelFlags> LogLevel;
    public static readonly IColumnDescriptor<DateTime?> Timestamp;
    public static readonly IReadOnlyList<IColumnDescriptor> Minimum;

    static Columns()
    {
        RawContent = new WellKnownColumnDescriptor<string>("raw_content");
        LogLevel = new WellKnownColumnDescriptor<LevelFlags>("log_level");
        Timestamp = new WellKnownColumnDescriptor<DateTime?>("timestamp");
        Minimum = new IColumnDescriptor[] { RawContent, LogLevel, Timestamp /* … */ };
    }
}
```

The same pattern applies to properties in `src/Tailviewer.Core/Properties/Properties.cs`.
Follow it exactly when adding a well-known column/property: a static readonly
`IColumnDescriptor<T>` field, a stable string id, and membership in `Minimum` when the
column is mandatory. Custom (plugin-specific) columns use `CustomColumnDescriptor<T>`
instead of `WellKnownColumnDescriptor<T>`.

### Buffers

Buffers are the in-memory backing stores for descriptors: `LogBufferArray` (fixed size),
`LogBufferList` (dynamic), and concurrent wrappers. They expose row *and* column access
and support slicing/copying between buffers. See `src/Tailviewer.Core/Buffers/`.

### Log sources

A log source provides log entries by index and notifies listeners of three changes
(`docs/Architecture.md`, "Log Sources"):

- **Append** — entries were added at the end.
- **Reset** — the source became empty (file moved/deleted).
- **Invalidate** — entries from an index onward were removed.

Implementations extend `AbstractLogSource` (`src/Tailviewer.Core/Sources/AbstractLogSource.cs`)
or wrap another source via `LogSourceProxy`. When writing a new source, extend the
abstract base and reuse its listener bookkeeping instead of re-implementing
`AddListener`/`RemoveListener`.

### Filters

`Filter` (`src/Tailviewer.Core/Filters/Filter.cs`) is the single entry point for
assembling `ILogEntryFilter`s — do not `new AndFilter(...)`/`new OrFilter(...)` from
other layers. Combination is controlled by `FilterCombinationMode`
(`src/Tailviewer.Core/Settings/FilterCombinationMode.cs`, `And = 0, Or = 1`):

```csharp
// AndFilter by default (behavior before the AND/OR feature existed):
ILogEntryFilter combined = Filter.Create(filters);
// Explicit mode:
ILogEntryFilter combined = Filter.Create(filters, FilterCombinationMode.Or);
```

The contract used by `AbstractDataSource.CreateFilteredLogFile()` is:

```csharp
Filter.Create(LevelFlags levelFilter,
              ILogEntryFilter timeFilter,
              IEnumerable<ILogEntryFilter> quickFilters,
              FilterCombinationMode mode)
```

- The level filter and the time filter are **always AND-ed**; only the quick filters
  participate in `mode`. This is a product decision (D1), not an implementation detail.
- All `Create` overloads filter out `null` entries, return `null` for an empty set, and
  return the single element directly when only one filter remains (which unwraps
  `NoFilter`).
- `quickFilters` may legitimately be `null` (no active quick filter). Guard it with
  `quickFilters ?? Enumerable.Empty<ILogEntryFilter>()` before handing it to an overload
  that does not already null-check — `Filter.Create(IEnumerable<ILogEntryFilter>)` itself
  does not tolerate a `null` argument.

### Namespaces vs. folders

Namespaces do **not** mirror folders. A file in `src/Tailviewer.Core/Columns/Columns.cs`
declares `namespace Tailviewer.Core`, not `Tailviewer.Core.Columns`. Keep this convention:
put the `// ReSharper disable once CheckNamespace` comment at the top and use the
package-level namespace (`Tailviewer.Core` / `Tailviewer.Api`).

### Deprecation

Mark APIs that are scheduled for removal with `[WillBeRemoved(reason, issueUrl)]`
(`src/Tailviewer.Api/WillBeRemoved.cs`). See `Columns.LogEntryIndex` /
`Columns.LineNumber` in `src/Tailviewer.Core/Columns/Columns.cs` for usage.
