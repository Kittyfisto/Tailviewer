# Logging

Tailviewer logs with **log4net** (package `log4net.3.3.0`, referenced from every
project). There is no custom logging abstraction — call log4net directly.

## Logger declaration pattern

Declare one static logger per class using the declaring type:

```csharp
private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
```

This is the universal pattern, e.g. `src/Tailviewer/BusinessLogic/Sources/NoThrowLogSource.cs`
and `src/Tailviewer.Core` classes. Requires `using log4net;` and
`using System.Reflection;`.

## Usage

- Use the format overloads (`Log.WarnFormat`, `Log.InfoFormat`, `Log.ErrorFormat`, …)
  rather than string concatenation, so messages are lazily formatted.
- Include identifying context in the message — e.g. the plugin name when blaming a
  plugin: `Log.WarnFormat("Plugin {0} threw an exception: {1}", _pluginName, exception);`
- The log level should reflect severity: `Warn` for tolerated failures (plugin threw,
  fallback applied), `Error`/`Fatal` for defects, `Info`/`Debug` for lifecycle events.

## Configuration

- log4net is bound through `App.config` assembly binding redirects
  (`src/Tailviewer/App.config` redirects `log4net` to `3.3.0.0`).
- Do not add new logging frameworks; keep log4net as the single logger.
