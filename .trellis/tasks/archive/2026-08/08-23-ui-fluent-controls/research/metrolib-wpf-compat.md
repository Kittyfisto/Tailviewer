# 研究：WPF / Metrolib 兼容性与模板替换方法

调查日期：2026-08-23。为 Fluent 化（形状/密度/排版）提供 WPF/Metrolib 兼容性边界与实施方法，避免踩坑。

## 1. 模板替换 vs 样式 setter（Metrolib 已编译主题字典）

- Metrolib 控件在**编译进 `Metrolib.dll` 的主题字典**（`Themes/Generic.xaml`）内用 `{StaticResource}` 硬编码形状与颜色（`theming.md` 已注明）。
- **颜色**可在应用作用域用隐式样式 `BasedOn="{StaticResource {x:Type controls:X}}"` 重设（dark-mode 任务已做）。
- **形状（圆角/密度）**只有两条路：
  1. 控件公开了对应 DP（如 `FlatButton.BorderRadius`）→ setter 改，低风险；
  2. 否则必须**整体替换 `ControlTemplate`**——一旦替换，必须完整重写该控件的模板（含 VisualStateManager、`PART_*` 具名部件、动画），否则行为退化。

## 2. 替换模板的兼容性红线

- **保留具名部件**：WPF 约定控件通过 `PART_*` 查找部件（如 `ScrollBar` 的 `PART_Track`、`TextBox` 的 `PART_ContentHost`、`ComboBox` 的 `PART_EditableTextBox`/`PART_Popup`）。换模板必须保留这些名字，否则键盘/滚动/编辑行为退化。
- **保留 VisualStateManager**：`CommonStates`（Normal/MouseOver/Pressed/Disabled）与 `FocusStates`（Focused/Unfocused）必须齐全，否则悬停/按下/禁用/焦点视觉与可访问性回归。
- **保留 `TemplateBinding` 的 DP**：Metrolib 控件的颜色属性（`NormalForeground`/`HoveredBackground`/`CheckedBackground`/`DisabledOverlayBrush` 等）必须在新模板中继续 `TemplateBinding`，否则现有 `MetrolibTheme.xaml` 的颜色覆盖失效、主题切换失效。
- **`BasedOn` 链**：现有隐式样式已 `BasedOn` Metrolib 样式；换模板时新样式应**继续 `BasedOn` 原样式**，仅覆盖 `Template` + 形状 setter，避免丢失既有颜色/行为 setter。

## 3. DynamicResource 行为（浅/深 + 强调色实时切换）

- 所有语义/强调色画刷在新模板中一律 `{DynamicResource}`（`theming.md` 规则），确保深色开关与强调色切换实时生效。
- 中性色（`Surface/TextPrimary/Divider/...`）也走 `{DynamicResource}`（dark-mode 已建立）。
- **陷阱**：新模板内若用 `{StaticResource}` 引用语义/强调画刷，会冻结并失去实时切换（`theming.md` 冻结画刷陷阱）。
- **`Color` 键只在应用作用域**：新 `Color` 键（如 `Success`、`SelectionUnfocused`）必须在 `App` 构造 + `ThemeManager.Apply` 发布，`Semantic.xaml`/`Fluent.xaml` 不本地声明 `Color` 键（`theming.md` 遮蔽陷阱）。

## 4. 性能约束

- 渲染热路径（`TextCanvas.OnRender`/`TextLine.Render`/`LogEntryListView.OnTimer`/虚拟化）**零改动**——Fluent 化只改 chrome 控件模板，不进入逐行渲染。
- chrome 层的 `{DynamicResource}` 数量可控（一次性查找，非每帧）；主题切换仍是一次性发布。
- 新模板中避免每帧动画开销过大；焦点环/悬停动画用 VisualStateManager Storyboard（一次性，非每帧轮询）。

## 5. 兼容性清单（net48 + UseWPF + TreatWarningsAsErrors）

- `net48` 支持 `CornerRadius`、`sys:Double` 资源、VisualStateManager、`DynamicResource`——均原生，无兼容问题。
- `TreatWarningsAsErrors`：换模板时避免未使用的 `x:Name`、未绑定的 `TemplateBinding`、`MC4005`（对不存在的属性 setter，如给 `ExpanderToggleButton` 设 `NormalForeground`）。
- 新模板必须 `x:Class` 无关（纯 XAML 资源字典），不得引入 code-behind（MVVM 边界）。
- 不改 csproj（SDK 项目 XAML 自动 glob，`Themes/Fluent.xaml` 会作为 Page 自动包含；若使用 `sys:` 需在根元素加 `xmlns:sys="clr-namespace:System;assembly=mscorlib"`）。

## 6. 回滚形状

- 换模板的控件：删除新 `<Style TargetType=...>` 块即回到 Metrolib/WPF 默认模板（因新样式是覆盖，删除即还原）。
- 圆角 setter（`BorderRadius`）：删除 setter 即还原方形。
- token 资源（`Fluent.xaml`）：从 `Constants.xaml` merged dictionaries 移除 + 删除文件即还原。
- 任何模板替换若触碰 §2 红线（丢 `PART_*`、丢 VisualStateManager、冻结画刷、丢 `TemplateBinding`），立即回滚该控件模板到最近提交并回到 Plan 修订。

## 7. 证据文件

- `.trellis/spec/ui/theming.md`（DynamicResource 规则、冻结画刷陷阱、Metrolib 覆盖方法）
- `src/Tailviewer/Themes/MetrolibTheme.xaml`（4 个文本框已拥有模板，作为换模板范式）
- `src/Tailviewer/Ui/Menu/MenuItemTemplates.xaml`（自研 MenuItem 模板范式）
- `src/Tailviewer/Ui/DataSourceTree/TreeViewItemStyle.xaml`（自研 FlatTreeViewItem 模板范式）
- 反射：`Metrolib.dll` 公开属性面（见 control-inventory.md §4）
