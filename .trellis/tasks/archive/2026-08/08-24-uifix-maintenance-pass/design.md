# Design — uifix maintenance pass

## A. Crash
`PartDataSourcesOnMouseMove` dereferences the cached `SelectedTreeViewItem`
(assigned only in `PartDataSourcesOnSelectedItemChanged`). `GetTreeViewItem` legitimately
returns null when the container is not realized (virtualization recycle, pre-template,
selection clear). Fix:

- Guard `treeViewItem != null` before `IsMouseOver`.
- Harden the `DataContext` cast to the `as` + null-check pattern already used by
  `PartDataSourcesOnDrop` (`DataContext as DataSourcesViewModel`), and guard
  `source != null` before `DragLayer.DoDragDrop`.

Regression seam: `src/Tailviewer.Tests/Ui/DataSourcesViewModelTest.cs` (headless) already
covers `CanBeDragged`. Add a null-source `CanBeDragged` test there. The private mouse
handler has no UI test seam; the guard is verified by build + code inspection.

## B. UI/theme
1. OneWayToggle: Metrolib's compiled default background is white (`#FFFFFFFF`); the
   existing `MetrolibTheme.xaml` style only overrides the checked state. Add theme-aware
   default setters (`Background=SurfaceMutedBrush`, `BorderBrush=DividerBrush`,
   `Foreground=TextPrimaryBrush`) and set `Foreground=PrimaryForegroundBrush` in the
   checked triggers.
2. Inactive menu lightening: Metrolib `ChromelessWindow` template draws a white
   `#50FFFFFF` `Rectangle` over `PART_TitleBar` (overlay). Remove it in
   `MainWindow.OnApplyTemplate` (find `PART_TitleBar`, remove white-translucent
   `Rectangle` children). Removal from the visual tree is robust even if the compiled
   control toggles the overlay's visibility on activate/deactivate.
3. Toolbar layout (`LogViewerControl.xaml`): change central `WrapPanel` →
   `StackPanel Orientation="Horizontal"` so filter buttons never wrap. Collapse the
   search-box `StackPanel` via a `DataTrigger` on the central toolbar `Border.ActualWidth`
   using a new `DoubleLessThanToCollapsedConverter` (threshold ~560px).
4. `DataSourcesToggleButton.xaml`: pin both title `TextBlock` `Foreground` to
   `{DynamicResource PrimaryForegroundBrush}` so state transitions stop changing them.
5. `ToolbarToggleButtonStyle.xaml`: replace the unselected `TextPrimaryBrush` foreground
   (dark-on-accent, poor in light mode) and add an `IsChecked=True` trigger
   (`Background=PrimaryForegroundBrush`, `Foreground=PrimaryBrush`) — white pill + accent
   icon, clearly distinct from the unselected white-on-transparent in both themes.
6. `MetrolibTheme.xaml`: add implicit `NotificationBadge` style (`Background` red,
   `Foreground` white). `BookmarksDataTemplate.xaml`: add
   `BasedOn="{StaticResource {x:Type controls:FlatIconButton}}"` to the inline bookmark
   `FlatIconButton.Style` so the theme accent color applies instead of the sky-blue.

## C. Release
Root cause: GitVersion Mainline computes the *next* version (predictive) and on a
detached tag HEAD applies a +1 branch bump → `v1.2.0` yields `1.2.1`. Fix: derive the
version from the tag itself (`GITHUB_REF_NAME` minus leading `v/V`) and use it for
injection + artifact names; remove the GitVersion install/execute steps. Add a
`RELEASE_VERSION` env var step with `\d+\.\d+\.\d+` validation (regression guard).
`ci.yml` never runs GitVersion/set-version → unaffected.

## D. Filter AND/OR
Persistence already exists (`DataSource.Save` writes `filtercombinationmode`, `Restore`
reads it; `DataSourceSettings.Restore` constructs `new DataSource()` per element). The
bug is the default is AND in three places. Change default to OR in:
- `DataSource` ctor (`FilterCombinationMode = Or`)
- `FilterCombinationModeToggleButton` (`Mode=Or`, `IsAndChecked=false`, `IsOrChecked=true`)
- `QuickFiltersSidePanelViewModel` getter fallback (`?? Or`)

Missing attribute then resolves to the `DataSource` ctor default (OR) — the migration
path. Leave `FilterCombinationMode` enum values and `Filter.Create(IEnumerable)` helper
default unchanged (independent low-level default). Update the three default assertions in
`DataSourceTest` and `QuickFiltersSidePanelViewModelTest`.
