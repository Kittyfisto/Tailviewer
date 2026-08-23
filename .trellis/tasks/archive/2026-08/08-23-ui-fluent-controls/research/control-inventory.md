# 研究：真实控件清单与 Fluent 化改造面

调查日期：2026-08-23。范围 `src/Tailviewer`，只列**应用实际使用**的控件（grep `controls:` + 标准 WPF 控件 + Tailviewer 自研控件）。Metrolib 0.3.0.162（`C:\Users\admin\.nuget\packages\metrolib\0.3.0.162`），经反射确认公开依赖属性面。

## 1. Metrolib 控件使用频次（`controls:` 前缀，按出现次数降序）

FlatScrollViewer ×12 · FlatTextBlock ×11 · EditorTextBox ×10 · FlatGroupBox ×9 ·
CloseButton ×8 · OneWayToggle ×7 · FlatToggleButton ×5 · FlatTabItem ×5 ·
AddButton ×4 · OpenInNewHyperlink ×4 · EditToggleButton ×3 · FilterTextBox ×3 ·
FlatListView ×3 · PathChooserTextBox ×2 · MarkdownPresenter ×2 · TrashButton ×2 ·
CircularProgressBar ×2 · DownloadButton ×2 · FlatPasswordBox ×1 · RemoveButton ×1 ·
RefreshButton ×1 · PluginToggleButton ×1 · PinToggleButton ×1 · EditableTextBlock ×1 ·
MoreButton ×1 · MenuItemContentControl ×1 · FolderOpenButton ×1 · FlatContextMenu ×1 ·
FlatIconButton ×1 · SearchTextBox ×1 · ExpanderToggleButton ×1 · FlatProgressBar ×1 ·
EmailToggleButton（MainWindow）· NotificationBadge（MainWindow）· ChromelessWindow（根）·
FlatTreeView（DataSourcesControl，容器）· FlatTreeViewItem（TreeViewItemStyle 容器）

## 2. 标准 WPF / 第三方控件

- `ComboBox` ×2（SettingsControl：Language、TextFileEncodings）——WPF 默认 Aero2 模板，方形边框。
- `CheckBox` ×2（SettingsControl：AlwaysOnTop、CheckForUpdates）——Aero2 模板，勾选框为字面颜色，深色下不可读（dark-mode 任务已注明遗留）。
- `Menu`/`MenuItem`/`ContextMenu`（MainWindow Header + MenuItemTemplates.xaml 自研模板）。
- Xceed `ColorPicker`（设置页主题色）——第三方，仅换色不换模板。

## 3. Tailviewer 自研控件（模板在仓库内，Tailviewer 完全拥有）

| 控件 | 模板位置 | Fluent 改造面 |
|---|---|---|
| `ToolbarToggleButton` | `Ui/LogView/ToolbarToggleButtonStyle.xaml` | 自研模板，可加圆角/选中 pill |
| `LogLevelToggleButton` | `Ui/LogView/LogLevelToggleButtonStyle.xaml` | BasedOn ToolbarToggleButton，继承 |
| `DataSourceDisplayModeToggleButton` | `Ui/LogView/DataSourceDisplayModeToggleButton.xaml` | 内嵌 OneWayToggle ×2 |
| `DataSourcesToggleButton` | `Ui/DataSourceTree/DataSourcesToggleButton.xaml` | BasedOn FlatToggleButton（无 BorderRadius） |
| `DataSourcesControl` | `Ui/DataSourceTree/DataSourcesControl.xaml` | 自研模板，根 Border 无圆角 |
| `TreeViewItemStyle`（`DataSourceTreeViewItemStyle`） | `Ui/DataSourceTree/TreeViewItemStyle.xaml` | 自研 FlatTreeViewItem 模板，方形选中块 |
| `SidePanelControl` | `Ui/SidePanel/SidePanelControl.xaml` | 内嵌 FlatToggleButton 图标钮 |
| `FilterToggleButton` | `Ui/QuickFilter/FilterToggleButton.xaml` | BasedOn 标准 `Button` |
| `QuickFilterMatchTypeToggleButton` | `Ui/QuickFilterMatchTypeToggleButton.xaml` | 自研 |
| `FilterCombinationModeToggleButton` | `Ui/SidePanel/QuickFilters/FilterCombinationModeToggleButton.xaml` | 自研 |
| `EmptyState`/`ImageLabel`/`FlatImage`/`AutoUpdateControl`/`ActionCenterControl` | 各 `Ui/*.xaml` | 仅语义色已迁移，形状可后续 |

## 4. Metrolib 公开依赖属性面（反射结论，决定「setter 改形状」vs「换模板」）

- **`FlatButton`**（基类）公开 `BorderRadius`、`HoveredBackground`、`PressedBackground`、`NormalForeground/HoveredForeground/PressedForeground/InvertedForeground`。
  → `FlatIconButton`/`AddButton`/`CloseButton`/`MoreButton`/`RemoveButton`/`TrashButton`/`DownloadButton`/`FolderOpenButton`/`RefreshButton` 全部继承。
  **圆角可以纯 setter 完成（低风险），无需换模板。**
- **`ToggleButtonBase`**（`FlatToggleButton` 及 `EditToggleButton`/`PluginToggleButton`/`PinToggleButton`/`EmailToggleButton`/`DataSourcesToggleButton` 的基类）公开
  `NormalForeground/HoveredForeground/PressedForeground/CheckedForeground/HoveredAndCheckedForeground/CheckedBackground/PressedBackground/HoveredBackground/DisabledOverlayBrush/InvertedForeground`，**没有 `BorderRadius`**。
  → 圆角需换模板（或接受方形）。注意 `ExpanderToggleButton` 不在此列（`theming.md` 已注明，它不是 FlatToggleButton）。
- **`OneWayToggle`**：`IsChecked/IsPressed/HasRightBorder/HasLeftBorder`，无圆角。→ 换模板或接受方形。
- **`FlatScrollBar`**：仅 `Thickness`。→ 换模板改滑块形状。
- **`FlatTabControl`/`FlatTabItem`/`FlatGroupBox`/`FlatListView`/`FlatListViewItem`/`FlatTreeViewItem`/`FlatContextMenu`/`FlatProgressBar`/`CircularProgressBar`/`FlatTextBlock`/`SearchTextBox`/`EditorTextBox`/`PathChooserTextBox`**：无圆角类属性。→ 换模板。
- **`EditorTextBox`/`PathChooserTextBox`/`FilterTextBox`/`FlatPasswordBox`** 的 ControlTemplate **已在 `MetrolibTheme.xaml` 内被 Tailviewer 重写**（dark-mode 任务产物，含 focus/hover Border 与 VisualStateManager）。这些模板 Tailviewer 已拥有，**直接在其上加 `CornerRadius` 即低风险**，无需再「换模板」。

## 5. 低风险 vs 需模板替换 的分类结论

### 低风险（共享资源 / 样式 setter / 已拥有的模板）
1. **圆角**：`FlatButton` 族（约 30 个实例）`BorderRadius` setter；4 个已拥有的文本框模板 `CornerRadius`。
2. **密度**：4 个已拥有文本框模板的 `Height/Padding`；`FlatButton` 族 `Padding`；菜单项 `Padding`（自研模板）。
3. **排版**：新增 `FontFamily`/字号 token 资源，应用到 `FlatTextBlock`、`FlatGroupBox` 标题模板、菜单模板（自研）；不设全局隐式 `TextBlock` 样式（风险高）。
4. **状态色**：强调色/选中/错误色走语义键 + `{DynamicResource}`（沿用 dark-mode 架构，不改算法）。
5. **图标一致性**：统一 24px 画布 + 语义前景色（已有 `Metrolib.Icons`，只统一颜色与尺寸，不改几何）。

### 需模板替换（中高风险，需主负责人批准红线的扩展）
| 控件 | 原因 | Fluent 目标 |
|---|---|---|
| `ComboBox` | WPF 默认 Aero2，方形+系统边框 | 圆角边框 + 语义前景/背景 + 焦点环 + 展开箭头语义色 |
| `CheckBox` | Aero2 勾选框字面颜色，深色不可读 | 圆角方框 + 强调色勾选 + 语义前景 |
| `FlatScrollBar` | 仅 `Thickness`，形状无法 setter 改 | 圆角轨道/滑块，语义色 |
| `FlatTabControl`/`FlatTabItem` | 无圆角属性 | 无边框 tab + 下划线选中 + 语义色 |
| `FlatToggleButton` 族圆角 | 无 `BorderRadius` | 圆角选中 pill（或接受方形，见决策点） |
| `OneWayToggle` | 无圆角属性 | 圆角分段控件（或接受方形） |
| `FlatGroupBox` | 无圆角属性 | 标题 + 圆角/语义边框（或保持） |

### 明确不动（红线，与 dark-mode 任务一致）
- `LogEntryListView`/`TextCanvas`/`DataSourceCanvas`/`AbstractLogColumnPresenter`/`TextBrushes`：自定义高性能日志渲染器（33ms 虚拟化 + `FormattedText` 自绘）。dark-mode 已完成其主题画刷化；**本任务不改其形状/算法/分页/虚拟化/命中测试/事件坐标**。若任何 Fluent 化想动渲染器，一律 out of scope。
- `BusinessLogic/**`、所有 ViewModel、MVVM 边界、命令/键位绑定、`XAML Click=` 事件处理。

## 6. 证据文件

- `src/Tailviewer/Themes/MetrolibTheme.xaml`（4 个文本框模板已拥有；FlatButton/FlatToggleButton 颜色覆盖）
- `src/Tailviewer/Ui/LogView/ToolbarToggleButtonStyle.xaml`、`LogLevelToggleButtonStyle.xaml`、`DataSourceDisplayModeToggleButton.xaml`
- `src/Tailviewer/Ui/DataSourceTree/{DataSourcesControl,DataSourcesToggleButton,TreeViewItemStyle}.xaml`
- `src/Tailviewer/Ui/SidePanel/SidePanelControl.xaml`、`Ui/Menu/MenuItemTemplates.xaml`
- `src/Tailviewer/Ui/Settings/SettingsControl.xaml`（ComboBox ×2、CheckBox ×2）
- 反射：`Metrolib.dll`（net45）公开属性面，见 §4。
