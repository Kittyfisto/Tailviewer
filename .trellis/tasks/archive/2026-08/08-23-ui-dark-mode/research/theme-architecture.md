# 研究：主题/设置架构与资源链

调查日期：2026-08-23（dark mode 任务规划阶段）

## 结论

1. **启动与资源合并链**
   - 无 `App.xaml`；启动对象 `Bootstrapper`（`Tailviewer.csproj` `<StartupObject>`）。
   - `App` 构造函数（`App.cs:41-52`）顺序：
     1. 应用作用域发布语义 `Color` 键：`Surface`/`SurfaceMuted`/`TextSecondary`/`Divider`。
     2. `Resources.MergedDictionaries.Add(Metrolib Generic.xaml)`。
     3. `Resources.MergedDictionaries.Add(Tailviewer MetrolibTheme.xaml)`。
   - `MetrolibTheme.xaml` 内部 merge `Themes/Constants.xaml`；`Constants.xaml` 内部 merge Metrolib `Constants.xaml` + `Themes/Semantic.xaml`。

2. **主题应用管线**
   - `ThemeManager.Apply(Color primary)`（`Ui/ThemeManager.cs`）：设置 `CurrentPrimary`、`TextBrushes.UpdateTheme(primary)`，把 `ThemePalette.Compute(primary)` 的 Primary*/Secondary* `Color` 写入 `Application.Current.Resources`；`Application.Current == null` 时 no-op（测试/设计时）。
   - 启动调用点：`App.StartApplication` 中 `ThemeManager.Apply(settings.Ui.ThemeColor)`（`App.cs:246`），发生在 `new App()` 之后、`window.Show()` 之前。
   - 设置页 `SettingsFlyoutViewModel.ThemeColor` setter（`SettingsFlyoutViewModel.cs:248-270`）：写 `_settings.Ui.ThemeColor` → `SaveAsync()` → `Dispatcher.BeginInvoke(ThemeManager.Apply)`（避免控件回调中同步改资源重入渲染）。

3. **语义资源所有权规则（任务 1 已建立，本任务扩展）**
   - `Semantic.xaml` 只声明 `SolidColorBrush`，`Color` 一律 `{DynamicResource}`；`Color` 键只在应用作用域发布（`App.cs`）。
   - 原因：`Constants.xaml` 本地声明 `Color` 键会遮蔽应用作用域值，阻断运行时替换（`theming.md` 陷阱）。
   - **遗留问题**：`Constants.xaml:18-24` 仍本地声明 `UnfocusedOverlay`/`TitleBar`/`OverlayBackground` 三个 `Color` 键，且 `TitleBarBrush`/`OverlayBackgroundBrush` 用 `{StaticResource}`。深色模式必须把它们迁到应用作用域并改 `{DynamicResource}`。

4. **设置持久化**
   - `UISettings`（`Settings/UISettings.cs`）：`Language`（string，默认 "en"）、`ThemeColor`（`Color`，默认 `#0047AB`）；`Save(XmlWriter)` 写属性、`Restore(XmlReader)` 按名回退、`Clone`。
   - `ApplicationSettings.Save()`：写 `<ui>` 元素调 `_ui.Save(writer)`；`Restore` 按元素名分发到 `_ui.Restore`。`SaveAsync()` 经 `_saveTask` 串行防抖 + `AllowSave` 门控。
   - 新增 `DarkMode` 字段只需：`UISettings` 属性 + Save/Restore/Clone + 测试；`ApplicationSettings` 无需结构改动。

5. **渲染器画刷现状（D2 相关）**
   - `TextCanvas.OnRender`：`DrawRectangle(Brushes.White)`（`TextCanvas.cs:285`）。
   - `DataSourceCanvas.OnRender`：`DrawRectangle(Brushes.White)`（`DataSourceCanvas.cs:85`）；`CreateFormattedText` 默认前景 `Brushes.Black`。
   - `AbstractLogColumnPresenter<T>.OnRender`：`DrawRectangle(Brushes.White)`（`AbstractLogColumnPresenter.cs:100`）。
   - `TextBrushes`（`Ui/LogView/TextBrushes.cs`）：
     - 静态可变画刷（`UpdateTheme` 改 `.Color`）：`SelectedBackgroundBrush`（=强调色）、`LineNumberForegroundBrush`（=强调色）、`DataSourceCharacterCodeForegroundBrush`（=强调色）。
     - 静态不可变画刷：`SelectedForegroundBrush=White`、`SelectedUnfocusedBackgroundBrush=#D7D7D7`、`HighlightedForegroundBrush=Black`、`HighlightedBackgroundBrush=#FFFF4D`、`HighlightedSelectedForegroundBrush=Black`、`HighlightedSelectedBackgroundBrush=#FF9632`、`DataSourceFilenameForegroundBrush=#808080`。
     - 实例级 `_foregroundBrushes`/`_backgroundBrushes`/`_alternateBackgroundBrushes` 来自 `ILogViewerSettings`（用户可配置级别色）；无 settings 时回退 `Black`/`White`。
     - `GetAlternatingColor`：对 `White`/透明背景返回 `#E8F1F7`，否则原样。
   - `LogEntryListView` 构造分隔条：`Fill = new SolidColorBrush(Color.FromRgb(225,228,232))`（`LogEntryListView.cs:194`）。

6. **强调色在深色下的关键事实**
   - `ThemePalette.Secondary = primary`；`MainWindow` 背景、`LogViewerControl` 工具栏 `Background={DynamicResource SecondaryBrush}`（强调色底），文字 `PrimaryForegroundBrush`（白）。因此深色模式不改变这两处"强调色底 + 白字"的组合。
   - `SecondaryForeground = Black` 对应的 `SecondaryForegroundBrush` 当前无消费方（grep 仅定义处）。

## 涉及文件（只读证据）

- src/Tailviewer/App.cs（构造 + StartApplication 主题应用点）
- src/Tailviewer/Ui/ThemeManager.cs、ThemePalette.cs
- src/Tailviewer/Themes/Constants.xaml、Semantic.xaml、MetrolibTheme.xaml
- src/Tailviewer/Settings/UISettings.cs、ApplicationSettings.cs
- src/Tailviewer/Ui/Settings/SettingsFlyoutViewModel.cs、SettingsControl.xaml
- src/Tailviewer/Ui/LogView/TextCanvas.cs、TextBrushes.cs、TextLine.cs、AbstractLogColumnPresenter.cs、DataSource/DataSourceCanvas.cs、LogEntryListView.cs
- .trellis/spec/ui/theming.md（DynamicResource 规则、冻结画刷陷阱、Metrolib 覆盖规则）
