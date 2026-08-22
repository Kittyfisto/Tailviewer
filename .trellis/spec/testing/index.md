# Testing Guidelines

Conventions shared by every `*.Tests` project. All test projects use
**NUnit 3.12**, **FluentAssertions 5.6**, and **Moq 4.7** (see the `packages.config`
/ `*.csproj` of any test project, e.g. `src/Tailviewer.Core.Tests/Tailviewer.Core.Tests.csproj`).

## Project layout

- Each production project has a sibling test project named `<Project>.Tests`
  (`Tailviewer.Core` → `Tailviewer.Core.Tests`, `Tailviewer.Api` → `Tailviewer.Api.Tests`).
- There are also `Tailviewer.Acceptance.Tests` and `Tailviewer.System.Tests`.
- Test files mirror the source tree by folder and name: source `Filters/NoFilter.cs` →
  test `Filters/NoFilterTest.cs` (see `src/Tailviewer.Core.Tests/Filters/`).

## Test structure

```csharp
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Filters
{
    [TestFixture]
    public sealed class NoFilterTest
    {
        [Test]
        public void TestPassesFilter1()
        {
            var filter = new NoFilter();
            filter.PassesFilter(new LogEntry()).Should().BeTrue();
            filter.PassesFilter(new LogEntry[0]).Should().BeTrue();
        }
    }
}
```

- Mark fixtures with `[TestFixture]` and tests with `[Test]`.
- Name test classes `<Subject>Test` and test methods `Test<Behavior>`.
- Common attributes: `[Description("…")]` to explain intent, `[Issue("https://…")]` to
  link a regression test to its GitHub issue, and `[Range(from, to)]` / `[Values(…)]`
  for parameterized tests (see `src/Tailviewer.Core.Tests/Sources/AbstractLogSourceTest.cs`).

## Fluent assertions with reasons

Every assertion should state **why** it must hold, in text, per `CONTRIBUTING.md`:

```csharp
logFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(3);
buffer[offset].Should().Be(42, "because we've specified an offset and thus values before that offset shouldn't have been touched");
new Action(() => logFile.GetColumn(null, Core.Columns.Index, new LogLineIndex[0], 0))
    .Should().Throw<ArgumentNullException>();
```

Use `.Should().Throw<ArgumentException>("reason")` for exception tests.

## Shared test suites via abstract base classes

Behavior that must hold for every implementation of an interface is expressed once in an
`abstract` test base class with virtual factory methods, then inherited by per-implementation
fixtures. Example: `AbstractLogSourceTest` in
`src/Tailviewer.Core.Tests/Sources/AbstractLogSourceTest.cs` defines tests that call
`CreateEmpty()` / `CreateFromContent(...)`, which concrete fixtures override to build
`InMemoryLogSource`, `FilteredLogSource`, `MergedLogSource`, etc.

When adding a new `ILogSource`/`ILogFile` implementation, inherit the matching abstract
test base and implement its factory methods so the shared contract is verified for free.

## Mocking

Use Moq for collaborators where a real instance is awkward. Prefer real implementations
of pure types (`LogEntry`, buffers, filters) over mocks.

## Localization-safe assertions

UI text is localized through `Tailviewer.Localization.Strings` (generated from
`Strings.resx`); the resolved value changes with `CurrentUICulture`. Never hardcode
English UI text in assertions:

```csharp
// Wrong — fails when CurrentUICulture resolves a localized value:
_menu.Help.Items.First(x => x.Header == "Show Log");

// Right — culture-independent:
_menu.Help.Items.First(x => x.Header == Strings.MenuShowLog);
_viewModel.InstallTitle.Should().Be(Strings.Install);
```

Do **not** pin tests to `en-US` (`CultureInfo.DefaultThreadCurrentUICulture = …`) to
make hardcoded strings pass; reference `Strings.<Key>` instead so tests pass under any
culture (local zh-CN and CI en-US).

## PR rules (from `CONTRIBUTING.md`)

- Bugfixes require a test that reproduces the bug (unless infeasible).
- New classes/methods require tests.
- Keep pull requests small and single-purpose.
