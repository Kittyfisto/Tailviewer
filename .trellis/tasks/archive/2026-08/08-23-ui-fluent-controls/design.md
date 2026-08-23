# 设计：Fluent UI 控件视觉化

> 本文档把 prd.md 的 D1–D6 决策点落实为可执行设计。D1–D6 为规划期识别的**待批准决策**，实现前必须由主负责人拍板；§9 保留证据并标注待批准状态。

## 1. 边界：什么能改 / 什么不能改

### 可改（视图/主题层）
- `src/Tailviewer/Themes/Fluent.xaml`（新增，token 资源）。
- `src/Tailviewer/Themes/Constants.xaml`（merged dictionaries 新增 `Fluent.xaml`，一处）。
- `src/Tailviewer/Themes/MetrolibTheme.xaml`（4 个已拥有文本框模板加圆角/密度；`FlatButton`/`FlatToggleButton` 族样式加 `BorderRadius`/`Padding` setter；新增 CheckBox/ComboBox 模板——见 §4）。
- `src/Tailviewer/Ui/LogView/ToolbarToggleButtonStyle.xaml`、`Ui/DataSourceTree/TreeViewItemStyle.xaml`、`Ui/Menu/MenuItemTemplates.xaml`、`Ui/DataSourceTree/DataSourcesControl.xaml` 等自研模板（加圆角/密度/字号）。
- 仅在 D5 需要发布新 `Color` 键时：`src/Tailviewer/Ui/ThemeManager.cs`、`Ui/SemanticPalette.cs`、`App.cs`（应用作用域初始占位）。

### 不可改（红线，违反即回滚）
- `src/Tailviewer/BusinessLogic/**`。
- `src/Tailviewer/Ui/**/*ViewModel*.cs`、`IMainWindowViewModel.cs`、`AbstractMainPanelViewModel.cs`。
- `LogEntryListView.cs`、`TextCanvas.cs`、`TextLine.cs`、`TextSegment.cs`、`TextBrushes.cs`、`DataSourceCanvas.cs`、`AbstractLogColumnPresenter.cs`（渲染器红线）。
- `ThemePalette.cs` 现有契约（`Compute(Color)` 的 Primary*/Secondary* 语义与测试基线不变）。
- `tools/generate_localization.py` 生成产物（除非确需新字符串）。

## 2. Fluent token 资源（`Themes/Fluent.xaml`）

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">
    <sys:Double x:Key="ControlCornerRadius">4</sys:Double>
    <sys:Double x:Key="SurfaceCornerRadius">8</sys:Double>
    <sys:Double x:Key="ControlHeightStandard">32</sys:Double>
    <sys:Double x:Key="ControlHeightCompact">24</sys:Double>
    <sys:Double x:Key="FontSizeCaption">12</sys:Double>
    <sys:Double x:Key="FontSizeBody">14</sys:Double>
    <sys:Double x:Key="FontSizeSubtitle">20</sys:Double>
</ResourceDictionary>
```

- 经 `Themes/Constants.xaml` 的 merged dictionaries 进入资源图（`Constants.xaml` 已 merge `Semantic.xaml`，同路径追加 `Fluent.xaml`）。
- **不本地声明 `Color` 键**；若阶段 C 需要 `Success`/`SelectionUnfocused` 新 `Color` 键，走 `SemanticPalette` + `ThemeManager.Apply` + `App` 初始占位（D5）。
- 圆角引用方式：`CornerRadius="{StaticResource ControlCornerRadius}"`（`sys:Double` 可静态；圆角非实时主题切换对象）。字体/密度同理 `{StaticResource}`。

## 3. 分期与范围映射（D1）

### 阶段 A（MVP，低风险，建议首批准）
| 目标 | 载体 | 改动类型 |
|---|---|---|
| `FlatButton` 族圆角 4 + 密度 Padding | `MetrolibTheme.xaml` 隐式样式追加 `BorderRadius`/`Padding` setter | setter，零模板 |
| 4 个文本框圆角 4 + 统一 Height/Padding | `MetrolibTheme.xaml` 已拥有模板的 `normalBorder/hoverBorder/focusBorder` 加 `CornerRadius` | 已拥有模板内改 |
| `ToolbarToggleButton`/`LogLevelToggleButton` 圆角（选中 pill） | `ToolbarToggleButtonStyle.xaml` 自研模板 Border 加 `CornerRadius` | 自研模板 |
| `MenuItemTemplates` popup Border 圆角 8 | `MenuItemTemplates.xaml` | 自研模板 |
| `DataSourcesControl` 根 / `TreeViewItemStyle` 选中块圆角 | 各自研模板 | 自研模板 |
| **`ComboBox` 模板**（圆角 + 语义色 + 焦点环 + 下拉箭头语义色） | `MetrolibTheme.xaml` 新增隐式样式（BasedOn WPF 默认） | **模板替换** |
| **`CheckBox` 模板**（圆角方框 + 强调色勾选 + 语义前景） | `MetrolibTheme.xaml` 新增隐式样式 | **模板替换** |
| 字号层级（FlatTextBlock/FlatGroupBox 标题/菜单 12/14/20） | `MetrolibTheme.xaml` + 自研模板 | setter |

### 阶段 B（中风险，可选）
- `FlatScrollBar` 圆角滑块/轨道（模板替换，保留 `PART_Track`）。
- `FlatTabControl`/`FlatTabItem` 无边框 tab + 下划线选中（模板替换）。
- `FlatToggleButton` 族圆角（模板替换，保留 `ToggleButtonBase` 的 `CheckedBackground`/`DisabledOverlayBrush` TemplateBinding）。
- `OneWayToggle` 圆角分段（模板替换，保留 `IsChecked/IsPressed/HasRightBorder/HasLeftBorder`）。

### 阶段 C（低价值，默认不做）
- `FlatGroupBox` 圆角/语义边框（模板替换）。
- `FlatContextMenu`/`FlatProgressBar`/`CircularProgressBar` 圆角与密度。
- `Success`/`SelectionUnfocused` 新语义 `Color` 键。

## 4. 关键模板替换规格（阶段 A）

### 4.1 CheckBox（Fluent）
- 目标：圆角 4 方框（`Surface` 底 + `Divider` 边）+ 勾选态 `PrimaryBrush` 底 + 白勾；前景 `TextPrimary`；禁用用 `DisabledForeground`。
- 保留 WPF `BulletDecorator` 语义 + `ContentPresenter`（`RecognizesAccessKey=True`），不绑定业务。
- 勾选标记用 `Path`（`M...` 勾形）`Fill={DynamicResource PrimaryForegroundBrush}`；未勾选隐藏。
- 焦点态：`FocusVisualStyle` 由模板内 1px `PrimaryBrush` 描边实现（D4 决策：若默认不改全局焦点，则 CheckBox/ComboBox 模板内自绘焦点态，不引入全局焦点环）。
- **不**改 `SettingsControl.xaml` 中 `AlwaysOnTop`/`CheckForUpdates` 的绑定（`IsChecked` 双向绑定原样）。

### 4.2 ComboBox（Fluent）
- 目标：圆角 4 边框（`Divider` 默认 / `PrimaryBrush` 悬停聚焦）+ `Surface` 底 + `TextPrimary` 前景 + 下拉箭头语义色。
- **保留** `PART_EditableTextBox`/`PART_Popup`（本应用为非可编辑 ComboBox，仍按标准模板结构，确保下拉键盘导航与 `ItemsSource/SelectedItem` 绑定不退化）。
- `ToggleButton`（模板内）复用一个已主题化的按钮外观（或最小化 `ToggleButton` 模板），保留 `IsDropDownOpen` 绑定。
- `ItemsPresenter` 置于 `Popup` 内，popup `Border` 圆角 8 + `Surface` 底 + `Divider` 边。
- 语言/编码两个 ComboBox 的 `SelectedItem`/`DisplayMemberPath` 绑定原样保留。

> 通用约束（metrolib-wpf-compat.md §2）：替换必须保留 `PART_*` 具名部件、完整 `CommonStates`/`FocusStates` VSM、以及对 `FlatButton`/`ToggleButtonBase` 颜色 DP 的 `TemplateBinding`；颜色一律 `{DynamicResource}`。

## 5. 状态色与可访问性（D3/D4/D7）

- 强调色契约不变（`ThemePalette` 基线不变，`ThemePaletteTest` 不破）。
- severity 红 `#E81123`/黄 `#FFC300` **保持不动**；新模板不重定义。
- 选中 = 强调色 + 白前景；未聚焦选中复用 `SurfaceMuted`（不新增键）。
- 禁用 = `DisabledForeground`（语义键，深/浅由 `SemanticPalette` 提供）。
- 焦点环按 D4：默认保持 `FocusVisualStyle=null` 现状；CheckBox/ComboBox 模板内自绘焦点态。若主负责人批准恢复全局焦点环，则单独列为阶段 B 项。

## 6. 性能约束

- 渲染热路径零改动（§1 红线）。
- 新增 `{DynamicResource}` 仅在 chrome 控件模板；主题切换仍是一次性发布，无每帧计算。
- 圆角/密度/字号用 `{StaticResource sys:Double}`（非实时切换对象，无性能与冻结问题）。
- 模板内 VSM Storyboard 为一次性悬停/聚焦动画，不进入布局循环。

## 7. 兼容性

- `net48` + `UseWPF` + `TreatWarningsAsErrors` 零警告。
- `Fluent.xaml` 不抢占 Metrolib 隐式样式、不重复声明强调/语义 `Color` 键。
- 新模板不引入 code-behind（纯 XAML 资源字典）。
- SDK 项目 XAML 自动 glob，`Fluent.xaml` 无需改 csproj。

## 8. 回滚形状

- R1 token 层：`Constants.xaml` 移除 `Fluent.xaml` merge + 删除文件。
- R2 圆角/密度/字号 setter：逐个删除 setter 即还原。
- R3 模板替换：删除对应 `<Style>` 块即回退到 Metrolib/WPF 默认模板。
- 任一步触碰 §1 红线（改渲染器/BusinessLogic/ViewModel/命令事件），立即回滚到最近回滚点并回到 Plan 修订。

## 9. 决策结论（2026-08-23 已批准）

| 决策 | 结论 | 状态 |
|---|---|---|
| D1 范围与分期 | 阶段 A 全部完成；阶段 B 实际使用控件必须处理（风险过高则保留行为+记录 out-of-scope）；阶段 C 只做实际可见且低风险项 | **已批准（负责人扩展为非 MVP-only）** |
| D2 模板替换红线扩展 | 批准列明清单（ComboBox/CheckBox/FlatScrollBar/FlatTabControl+Item/FlatToggleButton 族/OneWayToggle/FlatGroupBox），保留 PART_*/VSM/TemplateBinding；无法从工作区验证编译模板时保留行为优先 | **已批准** |
| D3 状态色 | severity 红/黄不动；强调色契约不动；不做深色强调色 lighten | **已批准** |
| D4 焦点环 | 默认保持现状；CheckBox/ComboBox 模板内自绘焦点态 | **已批准** |
| D5 token 载体 | 新增 `Fluent.xaml`，经 `Constants.xaml`；不改 `App.cs` 合并字典；`Fluent.xaml` 不声明 `Color` 键 | **已批准** |
| D6 全局样式 | 不设全局隐式 TextBlock/Control 样式 | **已批准** |

> 上述决策批准后实现不得静默偏离；任何超出边界的计划项标记 out of scope。
> 执行约定（负责人 2026-08-23）：优先共享 semantic/DynamicResource 样式与已拥有模板；必要的少量模板替换必须保留 PART_*、VisualStateManager、TemplateBinding、键盘鼠标/命令/绑定行为；不得改布局结构或日志行密度。
