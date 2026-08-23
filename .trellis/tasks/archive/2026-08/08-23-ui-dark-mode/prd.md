# 深色模式（语义 Token + 日志渲染器参与）

## Goal

在任务 08-22-ui-semantic-layout 建立的语义色基础之上，实现一个**可用**的深色模式：切换后整个应用（窗口 chrome、日志查看区、设置/侧栏/About、以及自定义高性能日志渲染器）即时切换为深色，无需重启，且不破坏现有的强调色（ThemeColor）实时切换能力。

约束边界：
- **保留** WPF / Metrolib、MVVM 无框架结构、`LogEntryListView`/`TextCanvas`/`DataSourceCanvas` 的自绘高性能渲染与 33ms 虚拟化机制。
- **不做** Fluent 全控件重排/重绘（留给后续任务）、不做与主题无关的重构。
- 语义 `Color`/`Brush` 资源是任务 1 的产出，本任务以其为基础扩展，不推翻。

## 已批准决策（主负责人 2026-08-23 审核通过）

> 以下决策已由主负责人拍板，作为本任务的执行边界；实现时不得静默扩大范围。

- D1 **激活/持久化模型**：两态 Light/Dark 切换 + 即时生效 + 持久化，默认浅色；**不做**跟随系统（Light/Dark/System）模式。
- D2 **日志渲染器参与（红线扩展，已批准）**：批准 `TextCanvas`、`DataSourceCanvas`、`AbstractLogColumnPresenter`、`TextBrushes`，以及 `LogEntryListView` 的**最小分隔条变更**。允许的改动**仅限**主题感知的画刷来源/更新，以及重绘所需的最小失效（invalidate）；**不得**改动渲染算法、分页、虚拟化、命中测试、事件/坐标逻辑或用户数据语义。
- D3 **ColorByLevel 交互**：保留用户配置的各级别前景/背景值；仅默认（非 ColorByLevel）渲染颜色随主题切换。
- D4 **深色调色板基线**：批准文档化基线——`Surface #1E1E1E`、`SurfaceMuted #2D2D30`、`TextPrimary #DCDCDC`、`TextSecondary #9E9E9E`、`Divider #3F3F46`、`TitleBar #252526`、`OverlayBackground #99000000`，及 design.md §4 记录的渲染器各值。
- D5 **Metrolib 覆盖深度**：批准对**应用实际使用的** Metrolib 控件做定向中性色覆盖。仅改颜色/资源；**不得**改动控件形状、布局、模板（超出最小样式 setter）、视觉状态机、动画或 Fluent 样式。
- D6 **设置入口**：设置页「主题」分组，紧邻 ThemeColor；本任务**不做** Header 快捷开关。
- D7 **默认值/迁移**：默认浅色；旧设置文件缺失 `darkmode` 属性时回退浅色，无 schema 迁移。

### 主负责人附加护栏

- 保留现有强调色 `ThemePalette`/`ThemeManager` 契约与实时强调色切换。
- 不改 `BusinessLogic/` 或无关 ViewModel。
- 除新增深色模式字符串外，**不重新生成本地化**（不触及其他字符串）。
- 渲染热路径保持**零**每帧 `DynamicResource` 查找、**零**新增分配。
- 任何计划项若会超出上述边界，标记为 out of scope，而非静默扩大。

## 非目标（本任务不做）

- 不做 Fluent 全控件重绘、不改控件形状/视觉状态机。
- 不做「跟随系统」主题（Light/Dark/System）、不做自动定时切换。
- 不改 `BusinessLogic/`、不改任何 ViewModel（除设置页 `SettingsFlyoutViewModel` 新增一个布尔属性——见 §需求）、不改本地化逻辑（仅新增字符串键并重新生成）。
- 不改 `src/Installer`、`tools/`、`.github/`、插件项目。

## 已确认事实（仓库证据，2026-08-23 复核）

| # | 事实 | 证据 |
|---|------|------|
| F1 | 无 `App.xaml`，启动对象 `Bootstrapper`；`App` 构造中发布语义 `Color` 键到应用作用域，再合并 Metrolib `Generic.xaml` + `MetrolibTheme.xaml` | `App.cs:41-52` |
| F2 | `ThemeManager.Apply(Color)` 只发布强调色（Primary*/Secondary*）并调用 `TextBrushes.UpdateTheme`；启动时在 `App.StartApplication` 调用一次 | `ThemeManager.cs`、`App.cs:246` |
| F3 | 语义 Brush（`SurfaceBrush` 等）`Color` 用 `{DynamicResource}`，`Color` 键**仅**在应用作用域发布；`Semantic.xaml` 不本地声明 `Color` 键 | `Semantic.xaml`、`App.cs` |
| F4 | `Constants.xaml` 仍有本地声明的中性 `Color` 键：`TitleBar #EAEDF2`、`OverlayBackground #55000000`、`UnfocusedOverlay #30FFFFFF`，且对应 Brush 用 `{StaticResource}` | `Constants.xaml:18-24` |
| F5 | 设置持久化：`UISettings`（`Language`/`ThemeColor`）经 `ApplicationSettings.Save/Restore` 写入 `<ui>` 元素；`SaveAsync` 防抖；属性可缺省回退 | `UISettings.cs`、`ApplicationSettings.cs` |
| F6 | 设置页 `SettingsFlyoutViewModel.ThemeColor` setter 已示范"即时应用 + `SaveAsync` + Dispatcher 延迟"模式 | `SettingsFlyoutViewModel.cs:248-270` |
| F7 | 日志渲染器三处硬编码 `Brushes.White` 背景：`TextCanvas.OnRender`、`DataSourceCanvas.OnRender`、`AbstractLogColumnPresenter<T>.OnRender` | 各 `OnRender` |
| F8 | `TextBrushes` 静态画刷：选中背景=强调色（可变）、选中前景=白、未聚焦选中=#D7D7D7、高亮前景=黑/背景=#FFFF4D、行号前景=强调色（可变）、数据源文件名前景=#808080；交替行色 `GetAlternatingColor` 返回 `#E8F1F7` | `TextBrushes.cs` |
| F9 | `LogEntryListView` 分隔条 `Fill = Color.FromRgb(225,228,232)` 为代码内置 | `LogEntryListView.cs:194` |
| F10 | 默认级别色：Other/Info 前景 `Colors.Black`、背景 `Transparent`；Warning/Error/Fatal 前景白/背景黄红；Trace/Debug 前景 #808080 | `LogViewerSettings.cs` |
| F11 | 硬编码中性色散落点：设置页 `#333333`/`#6B6B6B`、`EmptyStateStyle` `#A0A0A0`、`ImageLabel` `#E0E0E0`/`#B7B7B7`、ActionCenter `#333333`、`QuickFilter` `#444444`、`MetrolibTheme.xaml` `DisabledForegroundBrush #B7B7B7`、FilterTextBox 水印 `#717171` | 见 `research/hardcoded-color-inventory.md` |
| F12 | 强调色 `Secondary = primary`，`MainWindow` 背景与 `LogViewerControl` 工具栏均用 `{DynamicResource SecondaryBrush}`（即强调色底），文字用 `PrimaryForegroundBrush`（白） | `ThemePalette.cs`、`MainWindow.xaml:66`、`LogViewerControl.xaml` |
| F13 | `SecondaryForegroundBrush`（`SecondaryForeground=Black`）当前**无消费方** | grep 仅 `Constants.xaml:37` 定义 |

## 需求

- R1 新增**深色语义调色板**：在应用作用域按主题发布中性 `Color` 键（浅色=现有值，深色=新基线），并把 `Constants.xaml` 中本地声明的 `TitleBar`/`OverlayBackground`/`UnfocusedOverlay` 迁到应用作用域（Brush 用 `{DynamicResource}`），保持"`Color` 键只在应用作用域、Brush 用 DynamicResource"规则。
- R2 新增 `TextPrimary` 语义角色（默认前景），替换设置页/控件硬编码的 `#333333` 等默认前景；所有新/改语义 Brush 与消费方用 `{DynamicResource}`。
- R3 激活与持久化：`UISettings` 新增深色模式标志（默认浅色），Save/Restore/Clone 覆盖，`ApplicationSettings` `<ui>` 元素序列化；启动时与 `ThemeColor` 一同应用。
- R4 即时切换：设置页「主题」分组新增深色开关，切换即 `ThemeManager` 应用新主题（Dispatcher 延迟，同强调色模式），无需重启。
- R5 渲染器参与（**D2 已批准**）：`TextCanvas`/`DataSourceCanvas`/`AbstractLogColumnPresenter` 的背景由可变画刷提供并随主题切换；`TextBrushes` 新增"默认前景/背景、未聚焦选中背景、交替行色、分隔条"等主题相关画刷，在 `ThemeManager` 切换时一次性更新，不在 `OnRender` 每帧做资源查找。仅允许画刷来源/更新与重绘所需的最小失效；不改渲染算法/分页/虚拟化/命中测试/事件坐标。
- R6 强调色兼容：深色/浅色两态下，强调色切换仍实时生效；`PrimaryForegroundBrush`（白）等强调色消费方行为不变。
- R7 可访问性：深色下默认前景/次要前景对深色表面满足可读对比（≥4.5:1 常规文字，见 design.md §7）。
- R8 不回退：所有主题相关键/画刷不得出现 `{StaticResource Primary*}`/`{StaticResource Surface*}` 回归；渲染热路径零新增分配/每帧资源查找。
- R9 本地化：新增"深色模式"字符串键（英/zh-CN），重跑 `python tools/generate_localization.py` 生成三件套。

## 验收标准

- [ ] 设置页新增深色开关；开启后整个应用（窗口 chrome、日志查看区含渲染器、设置/侧栏/About/flyout、Metrolib 控件中性色）即时切换为深色，无需重启；关闭即恢复浅色。
- [ ] 深色状态持久化：重启后保持上次选择；旧设置文件缺失属性时回退浅色（`UISettingsTest` 覆盖 roundtrip/缺省）。
- [ ] 强调色在浅色/深色下均实时切换（无 `{StaticResource Primary*}` 回归）。
- [ ] 日志渲染器：深色下画布深底、默认前景浅色可读；`ColorByLevel` 用户显式色不变；选中/高亮/悬停/未聚焦选中在深色下均可见。
- [ ] 可访问性目检：深色下默认/次要文字对比达标（人工项 + design.md §7 数值）。
- [ ] 红线审计：仅 D2 批准文件（`TextCanvas`/`DataSourceCanvas`/`AbstractLogColumnPresenter`/`TextBrushes`/`LogEntryListView` 最小分隔条）被触碰，且只改画刷来源/更新与最小失效；不改 `BusinessLogic/**`、不改 ViewModel（`SettingsFlyoutViewModel` 仅新增布尔属性）、不改渲染算法/分页/虚拟化/命中测试/事件坐标。
- [ ] 解决方案构建 warning-free（`net48` + `TreatWarningsAsErrors`）。
- [ ] 目标 UI STA 单测通过（`MainWindowTest`/`LogViewerControlTest`/`LogEntryListViewTest`/`TextCanvasTest`）+ 新增 `UISettingsTest` 深色字段 + 中性调色板单测。
- [ ] `git diff --check` 通过；`generate_localization.py` 三件套已重新生成且无手改。

## Notes

- 本任务是 UI 主题现代化序列的第 2 个交付物（第 1 个已归档：08-22-ui-semantic-layout，第 3 个为后续 Fluent 全控件化）。当前以**独立任务**（`parent: null`）形态创建，与任务 1 一致；如主负责人希望建立正式 parent/child 树，可后续 `task.py add-subtask` 挂接，不影响本任务验收。
