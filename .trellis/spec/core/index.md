# Core Engine & API Guidelines

Guidance for working in `src/Tailviewer.Api` and `src/Tailviewer.Core` — the
non-UI logic of Tailviewer. `Tailviewer.Api` is the public plugin contract;
`Tailviewer.Core` is the log engine that implements it.

## Index

| Guide | Covers |
|-------|--------|
| [Architecture & abstractions](./architecture.md) | Layering, descriptors, buffers, log sources, columns/properties |
| [Error handling](./error-handling.md) | The no-throw contract, `NoThrow*` decorators, exceptions |
| [Logging](./logging.md) | log4net usage pattern |

## Dependency rules

- `Tailviewer.Api` has **no project references** — it is the leaf contract every
  plugin compiles against.
- `Tailviewer.Core` references only `Tailviewer.Api`
  (`src/Tailviewer.Core/Tailviewer.Core.csproj`).
- Plugins reference `Tailviewer.Api` only; they must never reference
  `Tailviewer.Core` or `Tailviewer` internals.
- UI code (`src/Tailviewer`) may use everything, but engine code must stay
  UI-free. `Tailviewer.Core` and `Tailviewer.Api` must not depend on WPF-specific
  view models or controls (they only reference `PresentationCore`/`PresentationFramework`
  for a handful of types).
