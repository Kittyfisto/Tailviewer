# Fluent UI 控件视觉化（圆角 / 密度 / 排版 / 状态色，不重排不重写渲染器）

## Goal

在任务 08-22-ui-semantic-layout（语义化布局）与 08-23-ui-dark-mode（深色模式）建立的语义 token 与主题管线之上，把**应用实际使用的控件**视觉向 Fluent UI 靠拢：圆角、密度、排版、状态色与焦点/无障碍一致性。这是**视觉/控件样式工作，不是 UI 重架构**。

约束边界（红线）：
- **保留**自定义高性能日志渲染器（`LogEntryListView`/`TextCanvas`/`DataSourceCanvas`/`AbstractLogColumnPresenter`/`TextBrushes`，33ms 虚拟化 + `FormattedText` 自绘）——本任务**不改其形状/算法/分页/虚拟化/命中测试/事件坐标**（dark-mode 已完成其主题画刷化，到此为止）。
- **保留**现有布局/语义（`MainWindow` 三栏、`LogViewerControl` 五行、搜索行、数据源树/侧栏）。
- **保留** MVVM 边界、命令/键位绑定、`XAML Click=` 事件、键盘/鼠标交互、拖拽、`FocusVisualStyle` 现状（除非决策点明确恢复焦点环）。
- **不引入新依赖**（不用 MahApps / MaterialDesign / Fluent.Ribbon / FluentWPF），只用 WPF + Metrolib + 既有 Extended.Wpf.Toolkit / Ookii。
- **不重新生成本地化**（预期零新字符串；若确有新字符串，最小化并重跑 `generate_localization.py`）。

## 决策点（D1–D6）

> **批准状态：主负责人已于 2026-08-23 批准全部 D1–D6，并扩展交付范围（非 MVP-only）。**
> 批准纪要（负责人补充，作为本任务执行的最终范围依据）：
> - 阶段 A 全部完成（token + 已拥有模板 + 自研模板圆角/密度/排版 + CheckBox/ComboBox）。
> - 阶段 B 中**实际使用**的 `FlatScrollBar`、`FlatTabControl`/`FlatTabItem`、`FlatToggleButton` 族、`OneWayToggle` 必须处理：能做安全 Fluent 处理则做；模板替换风险过高时**保留行为优先**，记录 out-of-scope 与已采用的安全替代。
> - 阶段 C 只做**实际可见且低风险**的 `FlatGroupBox`、`FlatContextMenu`、`FlatProgressBar`/`CircularProgressBar` 等；未实际使用或无法安全证明的项目记录 out-of-scope。
> - 红线（渲染器文件、`BusinessLogic/**`、ViewModel、`ThemePalette` 契约、Installer、插件、本地化生成产物）零改动；不引入依赖。
> - severity 红/黄与既有 accent/`ThemePalette` 不变；不自动提亮深色 accent；保持全局 `FocusVisualStyle` 策略；`Fluent.xaml` 经 `Constants.xaml` 接入且不声明 `Color`；不设全局隐式 `TextBlock`/`Control` 样式。

- **D1 范围与分期**：采用「共享 token + 已拥有模板 + CheckBox/ComboBox」为 **MVP（阶段 A）**；`FlatScrollBar`/`FlatTabControl`/`FlatToggleButton` 圆角/`OneWayToggle`/`FlatGroupBox` 作为**阶段 B/C 可选项**。批准 MVP 边界即视为批准本任务的执行范围。
- **D2 模板替换红线扩展**：dark-mode 只允许「换色」，本任务扩展为「对**少数**缺乏 setter 形状钩子的控件整体替换 `ControlTemplate`」——明确清单：`ComboBox`、`CheckBox`、`FlatScrollBar`、`FlatTabControl`/`FlatTabItem`、`FlatToggleButton` 族圆角、`OneWayToggle`、`FlatGroupBox`（各阶段见 design.md）。替换必须保留 `PART_*` 具名部件、VisualStateManager、`TemplateBinding`（见 research/metrolib-wpf-compat.md）。
- **D3 状态色**：severity 红/黄**保持不动**（`#E81123`/`#FFC300`，与 `LogViewerSettings` 默认级别色一致）；强调色契约与 `ThemePalette` 基线不变；**不做**深色下强调色自动 lighten。
- **D4 焦点环**：是否为本任务恢复 Fluent 焦点环（1px 强调色外描边）。默认**保持现状**（`FocusVisualStyle=null` 不变，仅在新模板 CheckBox/ComboBox 内实现焦点态）。如批准恢复，则需覆盖全部可聚焦控件并回归键盘交互。
- **D5 token 载体**：新增 `Themes/Fluent.xaml`（圆角/密度/字号资源）经 `Constants.xaml` merged dictionaries 进入资源图；**不改 `App.cs` 合并字典**（除非需发布新 `Color` 键，此时走 `ThemeManager.Apply` + `SemanticPalette`）。
- **D6 全局样式风险**：**不**设全局隐式 `TextBlock`/`Control` 样式；排版/密度只作用于列明的控件与模板（避免冲击布局与渲染器宿主）。

## 已确认事实（仓库证据，2026-08-23）

| # | 事实 | 证据 |
|---|------|------|
| F1 | 无 `App.xaml`，启动 `Bootstrapper`；`App` 构造发布语义中性 `Color` 键 + 合并 Metrolib Generic + `MetrolibTheme.xaml` | `App.cs` |
| F2 | `ThemeManager.Apply(Color primary, bool darkMode)` 单入口发布强调色 + 中性色 + `TextBrushes.UpdateTheme/UpdateNeutral`；`SemanticPalette.Compute(bool)` 纯函数 | `Ui/ThemeManager.cs`、`Ui/SemanticPalette.cs` |
| F3 | `Semantic.xaml` 只声明 Brush（`{DynamicResource}`），`Color` 键只在应用作用域 | `Themes/Semantic.xaml` |
| F4 | `FlatButton` 公开 `BorderRadius`/`HoveredBackground`/`PressedBackground`；`ToggleButtonBase`（FlatToggleButton 族）**无** `BorderRadius`；`OneWayToggle`/`FlatScrollBar`/`FlatTabControl` 等无圆角 DP | 反射 `Metrolib.dll`（control-inventory.md §4） |
| F5 | `EditorTextBox`/`PathChooserTextBox`/`FilterTextBox`/`FlatPasswordBox` 的 ControlTemplate **已在 `MetrolibTheme.xaml` 被重写**（含 VSM + focus/hover Border），Tailviewer 已拥有 | `MetrolibTheme.xaml` |
| F6 | `ComboBox` ×2、`CheckBox` ×2 为 WPF 默认 Aero2 模板，方形边框/字面勾选色（深色不可读，dark-mode 已注明遗留） | `SettingsControl.xaml` |
| F7 | 自研模板：`ToolbarToggleButton`（方形 Border）、`TreeViewItemStyle`（方形选中块）、`MenuItemTemplates`（方形 popup Border）、`DataSourcesControl`/`SidePanelControl`/`DataSourcesToggleButton` | 各 `Ui/*.xaml` |
| F8 | 字号散落 8/10/12/14/18/20/22/28/32；`FlatGroupBox` 标题 22；无统一 FontFamily（默认 Segoe UI） | grep `FontSize` |
| F9 | severity 色散落 `#E81123`/`#FFC300`；强调色默认 `#0047AB`；`LogViewerSettings` 默认级别色含黑/白/黄/红 | `LogViewerSettings.cs`、各 XAML |
| F10 | 渲染器红线：`TextCanvas`/`DataSourceCanvas`/`AbstractLogColumnPresenter`/`TextBrushes`/`LogEntryListView` 已主题画刷化（dark-mode D2），渲染热路径零每帧资源查找 | dark-mode design.md §6 |

## 需求

- R1 **Fluent token 资源**：新增 `Themes/Fluent.xaml`（圆角 `4/8`、密度 `32/24`、字号层级 `12/14/20`），经 `Constants.xaml` merged dictionaries 进入资源图；不本地声明 `Color` 键；不改 `App.cs` 合并字典。
- R2 **圆角**：`FlatButton` 族（约 30 实例）`BorderRadius=4` setter；4 个已拥有文本框模板 + 自研 `ToolbarToggleButton`/`MenuItemTemplates` popup/`DataSourcesControl` 根加 `CornerRadius`。
- R3 **密度**：已拥有文本框模板统一 `Height`/`Padding`；`FlatButton` 族 `Padding`；菜单项 `Padding`（自研模板）；不触碰日志行密度。
- R4 **排版**：显式统一字号层级（标题 20、正文 14、说明 12），作用于 `FlatTextBlock` 样式、`FlatGroupBox` 标题模板、菜单/列表/树模板；**不设全局隐式样式**。
- R5 **模板替换（D2 批准范围内）**：`ComboBox`、`CheckBox` 两处（MVP）替换为 Fluent 模板（圆角、语义前景/背景、焦点态、勾选=强调色）；保留 `PART_*`/VSM/`TemplateBinding`。
- R6 **状态色一致性**：新模板的状态色全部走语义/强调 `{DynamicResource}`；severity 红/黄不动；选中=强调色+白前景不变；未聚焦选中复用 `SurfaceMuted`。
- R7 **可访问性**：深/浅两态下新模板前景/背景对比达标；禁用态用 `DisabledForeground`；焦点行为按 D4（默认不改）。
- R8 **无回归**：渲染器文件零改动；无 `{StaticResource Primary*|Surface*|TextPrimary*}` 回归；构建 warning-free；主题（强调色 + 深色）实时切换不退化。
- R9 **图标一致性**：统一 24px 画布 + 语义前景色（不改 `Metrolib.Icons` 几何）。

## 非目标（本任务不做）

- 不改日志渲染器（形状/算法/分页/虚拟化/命中测试/事件坐标/用户数据语义）。
- 不做「跟随系统」主题、不做窗口 chrome 的 Fluent 化（标题栏圆角/系统级）。
- 不改 `BusinessLogic/**`、不改任何 ViewModel、不改命令/键位/事件绑定、不改本地化（除可能的 0~2 个新字符串外）。
- 不改 `src/Installer`、`tools/`、`.github/`、插件项目。
- 不引入任何新 NuGet/UI 框架依赖。
- 不重排布局（三栏/五行结构不动）。

## 验收标准

- [ ] `Themes/Fluent.xaml` 已创建并接入资源图；所有 token 有具体值（无 TODO），不本地声明 `Color` 键。
- [ ] `FlatButton` 族、4 个已拥有文本框、自研 `ToolbarToggleButton`/菜单 popup/`DataSourcesControl` 根呈现 Fluent 圆角（4/8），浅/深两态一致。
- [ ] `ComboBox`、`CheckBox` 在设置页呈现 Fluent 圆角/语义前景背景/强调色勾选/焦点态；键盘交互（Tab/空格切换/下拉）不退化。
- [ ] 密度/字号 token 落地后，设置页/菜单/列表/树在 1024 最小宽度下不重叠、不裁切（STA 测试 + 目检）。
- [ ] 强调色实时切换（`rg "StaticResource Primary"` 为空）与深色切换（`rg "StaticResource Surface|TextPrimary"` 为空）无回归。
- [ ] 渲染器红线零改动：`git diff --name-only` 不含 `LogEntryListView.cs`/`TextCanvas.cs`/`DataSourceCanvas.cs`/`AbstractLogColumnPresenter.cs`/`TextBrushes.cs`；不含 `BusinessLogic/**`；不含 ViewModel。
- [ ] 解决方案构建 warning-free（`net48` + `TreatWarningsAsErrors`）。
- [ ] 目标 UI STA 单测通过（`MainWindowTest`/`LogViewerControlTest`/`LogEntryListViewTest`/`TextCanvasTest`，参考前两任务）。
- [ ] `git diff --check` 通过。

## Notes

- 本任务是 UI 主题现代化序列的第 3 个交付物（第 1 个 08-22-ui-semantic-layout、第 2 个 08-23-ui-dark-mode 均已归档）。当前以独立任务（`parent: null`）形态创建，与前两任务一致；如需 parent/child 树可后续 `task.py add-subtask` 挂接。
- 规划期已产出 research/control-inventory.md、research/fluent-design-tokens.md、research/metrolib-wpf-compat.md；设计见 design.md，执行计划见 implement.md。
