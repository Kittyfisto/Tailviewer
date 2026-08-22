# Error Handling

## The no-throw contract

The public log interfaces (`ILogSource`, `ILogFile`, and related contracts in
`src/Tailviewer.Api`) promise **not to throw**. Third-party plugins implement these
interfaces, and Tailviewer cannot trust them. Therefore any call into a plugin-provided
implementation must be isolated so a plugin bug cannot crash the host.

## `NoThrow*` decorators

The local pattern is a decorator that wraps an untrusted implementation, catches every
exception, logs a warning blaming the plugin, and returns a safe default value.

Reference implementation: `src/Tailviewer/BusinessLogic/Sources/NoThrowLogSource.cs`.
Key points:

- Each public member wraps the inner call in `try/catch (Exception e)` and calls
  `BlameExceptionOnPlugin(e)` on failure.
- On failure it returns the descriptor's `DefaultValue` or an empty collection — never
  rethrows (except where the method's contract requires it, e.g. `GetAllProperties`).
- The logger is declared once per class:

```csharp
private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
// ...
private void BlameExceptionOnPlugin(Exception exception)
{
    Log.WarnFormat("Plugin {0} threw an exception: {1}", _pluginName, exception);
}
```

There is one `NoThrow*` wrapper per plugin surface, e.g.
`src/Tailviewer/BusinessLogic/Sources/NoThrowLogSource.cs`,
`src/Tailviewer/BusinessLogic/Sources/NoThrowLogFileFormatMatcher.cs`,
`src/Tailviewer/BusinessLogic/Sources/NoThrowLogEntryParser.cs`. When adding a new plugin
surface, add the matching `NoThrow*` wrapper.

## Argument validation

Validate arguments at the public boundary using the standard BCL exceptions with
`nameof(...)`. This is the repeated pattern in `src/Tailviewer.Core`:

```csharp
if (sourceIndices == null)
    throw new ArgumentNullException(nameof(sourceIndices));
if (destinationIndex < 0)
    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
if (destinationIndex + sourceIndices.Count > destination.Length)
    throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");
```

See `NoThrowLogSource.GetColumn` in `src/Tailviewer/BusinessLogic/Sources/NoThrowLogSource.cs`.

## Custom exception types

Domain errors are represented as dedicated exception types in `Tailviewer.Api` /
`Tailviewer.Core`, not as raw `Exception`:

- `SourceCannotBeAccessed` (`src/Tailviewer.Core/SourceCannotBeAccessed.cs`)
- `SourceDoesNotExist` (`src/Tailviewer.Core/SourceDoesNotExist.cs`)
- `NoSuchColumnException`, `NoSuchPropertyException`, `ColumnNotRetrievedException`
  (`src/Tailviewer.Api/`)
- `NoSuchServiceException` (`src/Tailviewer.Api/NoSuchServiceException.cs`)
- `ParseException` (`src/Tailviewer.Core/Filters/ExpressionEngine/ParseException.cs`)

When an operation can fail for a domain reason, prefer (or add) a typed exception over
`Exception`/`InvalidOperationException`.

## What not to do

- Do not let a plugin call bubble an exception out of a `NoThrow*` wrapper.
- Do not swallow exceptions silently — always log them with the offending plugin named.
- Do not return `null` where the contract allows an empty collection or `DefaultValue`.
