# Implement — uifix maintenance pass

## Order
1. Crash fix + test (A)
2. Filter AND/OR default → OR + tests (D)
3. UI/theme XAML (B1–B6)
4. Release workflow (C)
5. Build/test + review

## A. Crash
- Edit `src/Tailviewer/Ui/DataSourceTree/DataSourcesControl.cs`
  `PartDataSourcesOnMouseMove`: guard `treeViewItem != null`, use
  `DataContext as DataSourcesViewModel`, guard `source != null`.
- Add null-source `CanBeDragged` regression test to
  `src/Tailviewer.Tests/Ui/DataSourcesViewModelTest.cs`.

## D. Filter AND/OR
- `src/Tailviewer/Settings/DataSource.cs` ctor → `FilterCombinationMode.Or`.
- `src/Tailviewer/Ui/SidePanel/QuickFilters/FilterCombinationModeToggleButton.cs`
  `Mode` DP → Or; `IsAndChecked` → false; `IsOrChecked` → true.
- `src/Tailviewer/Ui/SidePanel/QuickFilters/QuickFiltersSidePanelViewModel.cs`
  fallback → `?? FilterCombinationMode.Or`.
- Update `src/Tailviewer.Tests/Settings/DataSourceTest.cs`
  (`TestDefaultFilterCombinationMode`, `TestRestoreWithoutFilterCombinationMode`
  description + assertion) and
  `src/Tailviewer.Tests/Ui/QuickFiltersSidePanelViewModelTest.cs`
  (`TestFilterCombinationMode`).

## B. UI/theme
- `src/Tailviewer/Themes/MetrolibTheme.xaml`: OneWayToggle default setters + checked
  foreground; NotificationBadge red style.
- `src/Tailviewer/Ui/MainWindow.xaml.cs`: `OnApplyTemplate` remove inactive overlay.
- `src/Tailviewer/Ui/Converters/DoubleLessThanToCollapsedConverter.cs`: new converter.
- `src/Tailviewer/Ui/LogView/LogViewerControl.xaml`: WrapPanel → StackPanel; name the
  central Border; wrap search boxes with width-collapse trigger + register converter.
- `src/Tailviewer/Ui/DataSourceTree/DataSourcesToggleButton.xaml`: pin title TextBlock
  Foregrounds.
- `src/Tailviewer/Ui/LogView/ToolbarToggleButtonStyle.xaml`: selected/unselected triggers.
- `src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksDataTemplate.xaml`: BasedOn.

## C. Release
- `.github/workflows/release.yml`: remove GitVersion install/execute; add
  `Derive version from tag` step (validate + export `RELEASE_VERSION`); use
  `RELEASE_VERSION` in inject/zip/setup steps.

## Verification
- `dotnet build` / `msbuild` the solution (or the affected projects) Release.
- Run the affected NUnit test fixtures: `DataSourceTest`,
  `QuickFiltersSidePanelViewModelTest`, `DataSourcesViewModelTest`.
- `git diff --stat` + `git status` review. Confirm no temp artifacts remain under
  `.trellis` and the task directory is intact.
