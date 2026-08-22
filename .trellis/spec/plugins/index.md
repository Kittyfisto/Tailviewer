# Plugin Development Guidelines

Tailviewer is extended by plugins: zip archives (`.tvp`) containing one or more .NET
assemblies that implement Tailviewer's plugin interfaces. Authoritative docs live in
`docs/DevelopingPlugins.md` and `docs/DevelopingFileFormatPlugins.md`.

## Plugin contracts (`src/Tailviewer.Api`)

- `IPlugin` is a tag interface (`src/Tailviewer.Api/Plugins/IPlugin.cs`) — every plugin
  implements it plus at least one capability interface:
  - `ILogFileFormatMatcherPlugin`
  - `ITextLogFileParserPlugin`
  - `ILogFileOutlinePlugin`
  - `ILogFileIssuesPlugin`
  - `IFileFormatPlugin`
  - `ICustomDataSourcePlugin`
- Plugin-facing contracts (e.g. `IServiceContainer`, `ILogFile`, `ILogSource`,
  `IPropertyDescriptor`, `IColumnDescriptor`) all live in `Tailviewer.Api`. A plugin
  references `Tailviewer.Api` **only**.

## Assembly-level attributes

Plugins describe themselves via assembly attributes (declared in
`src/Tailviewer.Api/Plugins/`): `PluginId`, `PluginVersion`, `PluginAuthor`,
`PluginWebsite`, `PluginDescription`. Real usage in
`src/Plugins/Tailviewer.Formats.Serilog/Properties/AssemblyInfo.cs`:

```csharp
[assembly: PluginId("Tailviewer.Formats", "Serilog")]
[assembly: PluginVersion(1, 0, 0, 0)]
[assembly: PluginWebsite("https://github.com/Kittyfisto/Tailviewer/tree/master/src/Plugins/Tailviewer.Formats.Serilog")]
[assembly: PluginAuthor("Simon Mießler")]
[assembly: PluginDescription("Parses log files in Serilog's default text format.")]
```

`PluginId` takes a namespace + name so two plugins with the same display name can be
distinguished (`src/Tailviewer.Api/Plugins/PluginIdAttribute.cs`). Always set at least
`PluginId`, `PluginVersion`, and `PluginAuthor`; also set `PluginWebsite` and
`PluginDescription` to avoid the packer warnings.

## Example plugins

The repo ships working plugins to copy from:

- `src/Plugins/Tailviewer.Formats.Serilog/` — a Serilog format plugin, plus
  `src/Plugins/Tailviewer.Formats.Serilog.Tests/SerilogAcceptanceTest.cs`.
- `src/Plugins/Tailviewer.PluginCreator/` — plugin template/creator tool.
- `examples/Tailviewer.LogLevelPlugin/` — a minimal example.

When adding a new bundled plugin, follow the Serilog project's structure: a matcher,
a parser (`SerilogEntryParser`), a format (`SerilogFileFormat`), and the `*Plugin`
entry points.

## Building/archiving

`src/Tailviewer.Archiver` builds `.tvp` zip archives from an assembly (invoked by the
`archive.exe` build tool described in `docs/DevelopingPlugins.md`). Plugin download and
update is handled by `src/Tailviewer.PluginRepository` (client) and
`src/Tailviewer.PluginRepository.Service` (service).

## Isolation

Plugin code is untrusted: the host wraps every plugin surface in a `NoThrow*` decorator
so a plugin exception cannot crash Tailviewer (see [Error Handling](../core/error-handling.md)
and `src/Tailviewer/BusinessLogic/Sources/NoThrowLogSource.cs`).
