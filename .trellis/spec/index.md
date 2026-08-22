# Tailviewer Coding Specs

Project-specific coding guidance for AI agents and new contributors. Each layer below
has an `index.md` that points to concrete rules backed by real source files.

## Repository at a glance

Tailviewer is a Windows desktop log-file viewer written in C# on the .NET Framework.
It is **not** a web app: there is no HTTP backend/frontend split. The code is layered
as a set of Visual Studio projects under `src/`:

| Project | Role | Target |
|---------|------|--------|
| `src/Tailviewer.Api` | Public plugin/extension API (interfaces, descriptors, plugin attributes) | .NET 4.8 |
| `src/Tailviewer.Core` | Log engine: buffers, sources, filters, parsers, columns/properties | .NET 4.8 |
| `src/Tailviewer` | WPF application (MVVM view models + XAML views + settings) | .NET 4.8 |
| `src/Tailviewer.Archiver` | Builds `.tvp` plugin zip archives | .NET 4.8 |
| `src/Tailviewer.PluginRepository(.Service)` | Plugin download/update client and service | .NET 4.8 |
| `src/Tailviewer.PluginCreator`, `src/Plugins/Tailviewer.Formats.Serilog` | Example plugins | .NET 4.8 |
| `src/Installer` | Application installer | .NET 4.8 |
| `src/*.Tests`, `src/Tailviewer.Acceptance.Tests`, `src/Tailviewer.System.Tests` | Test projects | — |

Reference docs: `docs/Architecture.md` (core concepts), `docs/DevelopingPlugins.md`,
`docs/DevelopingFileFormatPlugins.md`, `CONTRIBUTING.md` (PR rules), `README.md`.

## Spec layers

| Layer | Directory | Covers |
|-------|-----------|--------|
| Engine & API | [core/](./core/index.md) | `Tailviewer.Api` + `Tailviewer.Core`: descriptors, buffers, sources, filters, error handling, logging |
| WPF UI | [ui/](./ui/index.md) | `src/Tailviewer`: MVVM, view models, XAML, converters, settings |
| Plugins | [plugins/](./plugins/index.md) | Plugin authoring, `IPlugin` contracts, example plugins |
| Testing | [testing/](./testing/index.md) | NUnit + FluentAssertions + Moq conventions shared by all test projects |
| Build & projects | [build/](./build/index.md) | `.csproj`, `packages.config`, signing, warnings-as-errors, output layout |
| Thinking guides | [guides/](./guides/index.md) | Cross-cutting "did you think of that" checklists |

## Source-file conventions that apply everywhere

- Source files are UTF-8 **with BOM** and use **tab** indentation (see `src/Tailviewer.Core/Filters/NoFilter.cs`).
- XML doc comments are used pervasively: `/// <summary>`, `/// <inheritdoc />`, `/// <param>`.
- When a file's namespace does not match its folder path, the file starts with
  `// ReSharper disable once CheckNamespace` (see `src/Tailviewer.Core/Columns/Columns.cs`,
  which lives in `Columns/` but declares `namespace Tailviewer.Core`).
- The project ships ReSharper settings in `Tailviewer.sln.DotSettings` / `Tailviewer.sln.DotSettings`;
  match the surrounding style rather than introducing a new one.
