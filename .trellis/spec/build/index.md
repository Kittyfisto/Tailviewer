# Build & Project Conventions

How the solution is built and what every `.csproj` shares. The authoritative files are
the solution — `Tailviewer.sln` — and the csproj files under `src/`.

## Project format

- `src/Tailviewer` and `src/Installer` are **SDK-style** (`Microsoft.NET.Sdk.WindowsDesktop`,
  `<TargetFramework>net48</TargetFramework>`, `<UseWPF>true</UseWPF>`). Sources and
  XAML pages are globbed by default; new files need no csproj entry.
- All other projects are classic (non-SDK) `.csproj` with explicit
  `<Compile Include=…>` / `<Page Include=…>` items. **New source files in those
  projects must be added to the csproj** — they are not globbed.
- XAML views are `<Page>` items; their code-behind is `<Compile>` with
  `<DependentUpon>` pointing at the XAML.
- Target frameworks: all projects target `.NET Framework 4.8`
  (`src/Tailviewer/Tailviewer.csproj`, `src/Installer/Installer.csproj`,
  `Tailviewer.Api`, `Tailviewer.Core`, `Tailviewer.Archiver`, …).

## NuGet

- `src/Tailviewer` and `src/Installer` use SDK-style `PackageReference`.
- All other projects use `packages.config` + a `packages/` folder (not
  PackageReference). Reference paths use `HintPath` into `..\..\packages\`.
- When adding a dependency to a classic project, follow the
  `packages.config` + `<Reference>` + `HintPath` pattern seen in
  `src/Tailviewer.Core/Tailviewer.Core.csproj`.

## Warnings and XML docs

- `TreatWarningsAsErrors` is `true` for `Tailviewer.Api`, `Tailviewer.Core`, and
  `Tailviewer` (Debug and Release). Builds must be warning-free.
- Public libraries generate XML documentation (`DocumentationFile` →
  `..\..\bin\Tailviewer.Core.XML`). Keep XML docs on public members current.

## Assembly metadata & signing

- All assemblies are strong-named: `<SignAssembly>true</SignAssembly>` and
  `<AssemblyOriginatorKeyFile>../../sig/key.snk</AssemblyOriginatorKeyFile>`.
- Shared assembly info (`AssemblyVersion`, `AssemblyFileVersion`, company, copyright)
  lives in `src/GlobalAssemblyInfo.cs`, linked into every project as
  `Properties/GlobalAssemblyInfo.cs`. Do not duplicate version attributes per project.

## Output layout

- Every project outputs to a common `..\..\bin\` directory (see the
  `<OutputPath>` elements), so the app and its dependencies land together.
- The installer project (`src/Installer`) depends on the build outputs via solution
  project dependencies.

## Code style

- UTF-8 with BOM, tab indentation.
- XML doc comments on public API; `// ReSharper disable once CheckNamespace` where the
  namespace intentionally differs from the folder.
- ReSharper settings are committed as `Tailviewer.sln.DotSettings`.
