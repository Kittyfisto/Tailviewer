# PRD — uifix maintenance pass

Branch: `uifix`. Single bounded maintenance pass. No commit/push/publish; leave all
changes uncommitted for the parent to review.

## Acceptance criteria

### A. Crash (highest priority)
`src/Tailviewer/Ui/DataSourceTree/DataSourcesControl.cs` — `PartDataSourcesOnMouseMove`
(around line 417) throws `NullReferenceException` when `treeViewItem` is null while
`source` is a `FileDataSourceViewModel`. Fix the root cause safely (null/lifecycle/
event-ordering), do not introduce a new null crash, and add a focused regression test
where the project has a seam.

### B. UI/theme
1. `controls:OneWayToggle` (AND/OR toggle) must use a theme-aware dark/light background,
   not a fixed white background.
2. When the window is unfocused, the very top menu bar must retain normal theme
   appearance (remove the inactive lightening/color change).
3. When width is too narrow the two search boxes must yield space before the
   `ToolbarToggleButton` filter buttons; filter buttons never wrap.
4. In the left sidebar, keep both `CurrentDataSource` title `TextBlock` colors stable
   across the three-state transition and theme behavior, using existing theme resources.
5. In both dark and light themes, selected vs unselected log-level filter buttons must be
   clearly distinguishable.
6. Action Center notification badge must be red; Bookmark add button must use the theme
   color instead of the old sky-blue.

### C. Release Action
Tag `v1.2.0` must produce `1.2.0` artifacts (version injection, zip name, setup name,
release assets), not `1.2.1`. Fix the workflow/script; do not break branch/CI builds.

### D. Log filter setting
- AND/OR selection must be persisted in log-filter settings and restored (not reset to AND).
- Missing-value/default behavior must change from AND to OR.
- Existing saved settings without the new value must resolve to OR.
- UI binding/commands, model/settings serialization, and migration must stay coherent.

## Constraints
- .NET Framework 4.8 WPF MVVM (no MVVM framework); NUnit 3.12 + FluentAssertions 5.6 +
  Moq 4.7 tests.
- Follow `.trellis/spec/ui/theming.md` (accent brushes `{DynamicResource}`), testing and
  build conventions. UTF-8 BOM + tab indentation for `.cs`.
- `.cs` source files are transparently encrypted on disk (E-SafeNet); edit them via
  Python (plaintext) to avoid corruption. XAML/csproj/yml are plaintext.
- No unnecessary dependencies or broad refactors.
