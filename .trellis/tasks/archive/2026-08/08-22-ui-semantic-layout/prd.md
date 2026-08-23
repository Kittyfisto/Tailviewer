# 语义化 UI 设计基础 + 主界面重排

## Goal

在不迁移 UI 框架、不重写业务逻辑与日志渲染器的前提下，建立一套语义化设计资源（颜色）作为后续深色模式与全控件 Fluent 化的基础，并对主界面（窗口头部 + 日志查看区）做一次低风险重排：把拥挤的搜索/过滤框从窗口标题栏移入日志查看区，缓解 1024 最小宽度下的头部拥挤。

## 已批准决策（主负责人 2026-08-22 审核通过）

- A1：采用 design.md **方向 A**（渐进式收紧，保留三栏骨架与全部命令）。
- A2：语义迁移与重排范围**仅**覆盖 `MainWindow` + `LogViewerControl` 及其直接使用的样式资源；不扩展到设置页/侧栏/About。
- A3：不新增紧凑模式等用户可见设置。
- A4：保持 WPF/Metrolib、ViewModel/BusinessLogic、`LogEntryListView`/`TextCanvas`/`TextBrushes`/分页与虚拟化不变。
- A5：语义资源必须给出**可运行的具体基线值**，优先等价复用现有颜色；“不锁定值”不等于留下 TODO。
- A8（收尾修正）：语义资源是深色模式的基础。`Semantic.xaml` 四个语义 Brush 的 `Color` 引用及本任务范围内这些 Brush 的消费者全部使用 `{DynamicResource}`（仅 chrome，性能可忽略）；`Color` 键在 `App` 构造发布到应用作用域（镜像强调色架构），保持强调色规则与日志自绘红线不变。
- A6：搜索框移入 `LogViewerControl` 后，`MainWindow.xaml.cs` 不得再引用 `PartSearchBox`/`PartFindAllBox`（已不属于 MainWindow 命名域）；改为 `LogViewerControl` 暴露焦点方法，`MainWindow` 通过活动 `LogViewerControl` 转发，并正确处理模板尚未生成 / 找不到实例的情况。不改 ViewModel。
- A7：新位置 `SearchTextBox`/`FilterTextBox` 绑定基于 `LogViewerControl` 的 DataContext（= `LogViewMainPanelViewModel`）上的 `Search`/`FindAll`/`CurrentDataSource` 路径；FindAll 生命周期沿用既有（每数据源持有），并补回归验证。

## 非目标（本任务不做）

- 不迁移 UI 框架、不重写业务逻辑 / ViewModel / 日志分页与自绘渲染。
- 不做深色模式、不做全控件 Fluent 化。
- 不改 `src/Installer`、`tools/`、`.github/`、插件项目。

## 已确认事实（仓库证据）

| # | 事实 | 证据 |
|---|------|------|
| F1 | WPF + .NET Framework 4.8，SDK 主项目，`TreatWarningsAsErrors` | `src/Tailviewer/Tailviewer.csproj` |
| F2 | 启动对象 `Bootstrapper`，无 `App.xaml`；`App` 构造中合并 Metrolib Generic + `MetrolibTheme.xaml` | `src/Tailviewer/App.cs:43-44` |
| F3 | Metrolib 0.3.0.162 | csproj `PackageReference` |
| F4 | 主窗口 `ChromelessWindow`，`MinWidth=1024`，自绘 Header | `MainWindow.xaml:1-16` |
| F5 | Header 搜索/过滤框固定宽 200×2 + 菜单 + ActionCenter，1024 下拥挤 | `MainWindow.xaml:66-167` |
| F6 | 日志自绘 `TextCanvas.OnRender`（背景硬编码 `Brushes.White`） | `LogView/TextCanvas.cs` |
| F7 | `LogEntryListView` 纯代码 Grid + 33ms 虚拟化；其分隔条 `Color.FromRgb(225,228,232)` 为代码内置（本任务红线） | `LogView/LogEntryListView.cs` |
| F8 | 主题链路 `UISettings.ThemeColor → ThemeManager.Apply → Application.Resources → {DynamicResource}`；`TextBrushes` 用可变画刷 | `Ui/ThemeManager.cs`、`ThemePalette.cs`、`LogView/TextBrushes.cs` |
| F9 | 硬编码中性色散落：`#D8D8D8`、`#A0A0A0`、`White`、`Gray` 出现在 `LogViewerControl.xaml`/`MainWindow.xaml` | 各 XAML |
| F10 | `LogViewerControl` 经 `LogViewMainPanelDataTemplate` 承载，DataContext = `LogViewMainPanelViewModel`；其 `Search`(ISearchViewModel)、`FindAll`(IFindAllViewModel)、`CurrentDataSource` 均在 DataContext 路径上 | `LogViewMainPanelDataTemplate.xaml:18`、`LogViewMainPanelViewModel.cs`、`IDataSourceViewModel.cs:136-138` |
| F11 | `FindAllViewModel`（嵌套类）每数据源持有，`SearchTerm` setter 触发 PropertyChanged 并驱动 `Show` | `AbstractDataSourceViewModel.cs:505-570` |
| F12 | `FindChildrenOfType<T>` 扩展（Metrolib）已在 `MainWindow.xaml.cs`/`AutoPopup.cs` 使用 | `MainWindow.xaml.cs:195`、`AutoPopup.cs:63` |
| F13 | 快捷键 Ctrl+F / Ctrl+Shift+F / Ctrl+E / Ctrl+Shift+N 经 `Window.InputBindings` + 命令 DP | `MainWindow.xaml:18-27` |

## 需求

- R1 新增语义资源字典 `src/Tailviewer/Themes/Semantic.xaml`，经 `Themes/Constants.xaml` 的 merged dictionaries 进入资源图（不改 App.cs 合并字典）；仅覆盖本任务范围内的语义角色（表面/弱表面/次要前景/分隔线），给出具体基线值并等价复用现有颜色（见 design.md §4）。
- R2 将 `MainWindow.xaml`、`LogViewerControl.xaml` 范围内硬编码中性色迁移为语义资源引用；语义 Brush 及其消费者、强调色引用全部 `{DynamicResource}`（`theming.md`）。
- R3 重排主窗口头部：移除 Header 中的 `PartSearchBox`/`PartFindAllBox`，Header 仅保留 logo/菜单/标题后缀/ActionCenter；搜索/过滤框下移进 `LogViewerControl` 的搜索行。
- R4 迁移与重排遵守红线：不改 `BusinessLogic/`、不改任何 ViewModel、不改 `LogEntryListView`/`TextCanvas`/`TextLine`/`TextSegment`/`TextBrushes`/`DataSourceCanvas`。
- R5 保留 Ctrl+F / Ctrl+Shift+F / Ctrl+E / Ctrl+Shift+N 及菜单命令键位行为不变。
- R6 焦点桥接（A6/A7）：`LogViewerControl` 暴露 `FocusSearch()`/`FocusFindAll()`；`MainWindow` 经活动 `LogViewerControl` 转发，模板未生成/找不到实例时安全空操作；不改 ViewModel。
- R7 新搜索/过滤框绑定走 DataContext 的 `Search.Term/ResultCount/CurrentResultIndex` 与 `FindAll.SearchTerm`；`Visibility` 由 `CurrentDataSource` 空值折叠。

## 验收标准

- [x] `Semantic.xaml` 已创建并经 `Constants.xaml` merged dictionaries 进入资源图；所有键有具体值（无 TODO），值等价复用现有颜色。
- [x] `MainWindow.xaml` 头部在 1024 最小宽度下不重叠、不裁切（STA 测试通过；目检为人工项）。
- [x] 四个全局快捷键功能不变：Ctrl+F / Ctrl+Shift+F 经桥接聚焦到新位置的搜索/过滤框（`MainWindowTest` 覆盖 InputBinding 同步）。
- [x] 主题色切换仍实时生效：强调色引用无 `{StaticResource Primary*}` 回归。
- [x] 红线文件零改动：`BusinessLogic/**`、`Ui/**/*ViewModel*`、`LogEntryListView.cs`、`TextCanvas.cs`、`TextLine.cs`、`TextSegment.cs`、`TextBrushes.cs`、`DataSourceCanvas.cs`。
- [x] 解决方案构建 warning-free；目标 UI STA 单测（`MainWindowTest`、`LogViewerControlTest`、`LogEntryListViewTest`、`TextCanvasTest`）通过。
- [x] FindAll 回归：`FilterTextBox` 的 `FindAll.SearchTerm` 与 FindAll 结果视图共享同一 `IFindAllViewModel`（代码核实 + `LogViewerControlTest` 通过）。

## 验证证据（已执行）

- 构建：`dotnet build src/Tailviewer/Tailviewer.csproj` 与 `src/Tailviewer.Tests/Tailviewer.Tests.csproj` 均 0 警告 / 0 错误（`TreatWarningsAsErrors`）。
- 目标 UI STA 单测（nunit3-console 3.12.0，`--where` 过滤 4 fixture）：**94 总 / 91 通过 / 0 失败 / 3 跳过**。
- 全量 `Tailviewer.Tests.dll`：**2365 总 / 2295 通过 / 67 失败 / 3 跳过**；其中 66 为**基线失败**（`git stash` 干净基线复跑结果一致，集中在 `LogViewerViewModelTest`/`TextBrushesTest`/`TimeFiltersViewModelTest`/`FileDataSourceViewModelTest` 等，根因是 zh-CN 本地化文案不匹配 + 测试执行顺序依赖），另 1 个为 `LogEntryListViewTest.TestMouseWheelDown2` **偶发**（Freezable 跨线程竞态，隔离复跑 1/1 通过，与本次改动无关）。
- 资源解析修正：`LogViewerControl` 根 `Background` 由 `{StaticResource SurfaceBrush}` 改为 `{DynamicResource SurfaceBrush}`（根元素无法用 StaticResource 解析自身资源，实测 STA 测试 `XamlParseException` 后修正）。
- `git diff --check`：通过。

## 剩余限制（已知、本任务不解决）

- `LogEntryListView.cs` 内置的分隔条颜色 `225,228,232` 为代码内置，属红线文件，本次不迁移（记录于 design.md，供后续 Fluent 化任务处理）。
- 侧栏/设置页/About 的中性色与间距仍为硬编码，留待后续任务。
- `TextCanvas` 背景 `Brushes.White` 属自绘渲染路径，本次不改。
