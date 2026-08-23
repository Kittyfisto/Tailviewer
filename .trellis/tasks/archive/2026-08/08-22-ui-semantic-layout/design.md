# 设计：语义化 UI 设计基础 + 主界面重排

> 方向 A 已获主负责人批准（2026-08-22）。本文档把批准决策落实为可执行设计，语义资源给出具体基线值。

## 1. 边界：什么能改 / 什么不能改

### 可改（视图层，仅本任务范围）
- `src/Tailviewer/Ui/MainWindow.xaml`（头部 + 主内容骨架）。
- `src/Tailviewer/Ui/MainWindow.xaml.cs`（焦点转发桥接，见 §5）。
- `src/Tailviewer/Ui/LogView/LogViewerControl.xaml`（新增搜索行 + 中性色迁移）。
- `src/Tailviewer/Ui/LogView/LogViewerControl.cs`（新增 `FocusSearch`/`FocusFindAll` 公开方法）。
- `src/Tailviewer/Themes/Semantic.xaml`（新增，见 §4）。
- `src/Tailviewer/Themes/Constants.xaml`（merged dictionaries 新增 `Semantic.xaml`，一处新增）。
- `src/Tailviewer/App.cs`（在应用作用域发布语义中性 `Color` 键，见 §4）。

### 不可改（红线，违反即回滚）
- `src/Tailviewer/BusinessLogic/**`。
- `src/Tailviewer/Ui/**/*ViewModel*.cs`、`IMainWindowViewModel.cs`、`AbstractMainPanelViewModel.cs`。
- `src/Tailviewer/Ui/LogView/LogEntryListView.cs`、`TextCanvas.cs`、`TextLine.cs`、`TextSegment.cs`、`TextBrushes.cs`、`DataSource/DataSourceCanvas.cs`。
- `src/Tailviewer/Ui/ThemeManager.cs`、`ThemePalette.cs`（仅新增键也不改现有契约）。
- `tools/generate_localization.py` 生成产物（`Strings*.resx`、`Strings.cs`）。

## 2. 布局重排（方向 A，已批准）

现状 Header（`MainWindow.xaml:66-167`）：
```
[logo 32][Menu][SearchTextBox 200][FilterTextBox 200][title suffix *][ActionCenter 30]
```
重排后：
```
Header:   [logo 32][Menu][title suffix *][ActionCenter 30]
LogViewerControl Row1（新增搜索行）:
  [SearchTextBox 200][FilterTextBox 200]   ← 自 Header 移入，绑定见 §3
```

- Header 列数 6 → 4，删除两列搜索框；标题后缀列（star）获得空间，1024 宽度不再拥挤。
- 搜索行利用 `LogViewerControl.xaml` 现有**空置的 Row1（Height=Auto）**，不改 Row0(50px) 工具栏与 Row2(3*) 日志区结构，`LogEntryListView` 零接触。
- 顶部工具栏 `Row0` 与日志区之间新增一条搜索行，语义上搜索属于日志操作。

## 3. 绑定契约（不改 ViewModel）

`LogViewerControl` 的 DataContext = `LogViewMainPanelViewModel`（由 `LogViewMainPanelDataTemplate.xaml` 隐式模板承载，该 VM 由 `MainWindowViewModel.LogViewPanel` 提供）。故新搜索行绑定为：

| 控件 | 属性 | 绑定（DataContext 相对） | 原 Header 绑定 |
|------|------|--------------------------|----------------|
| `SearchTextBox` (x:Name=`PART_SearchBox`) | `Text` | `{Binding Search.Term, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}` | `LogViewPanel.Search.Term` |
| | `OccurenceCount` | `{Binding Search.ResultCount}` | `LogViewPanel.Search.ResultCount` |
| | `CurrentOccurenceIndex` | `{Binding Search.CurrentResultIndex, Mode=TwoWay}` | `LogViewPanel.Search.CurrentResultIndex` |
| | `Visibility` | `{Binding CurrentDataSource, Converter=NullToCollapsedConverter}` | `LogViewPanel.CurrentDataSource` |
| `FilterTextBox` (x:Name=`PART_FindAllBox`) | `FilterText` | `{Binding FindAll.SearchTerm, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}` | `LogViewPanel.FindAll.SearchTerm` |
| | `Visibility` | `{Binding CurrentDataSource, Converter=NullToCollapsedConverter}` | `LogViewPanel.CurrentDataSource` |

- `Search` 为 `ISearchViewModel`（Term/ResultCount/CurrentResultIndex，`IDataSourceViewModel.Search`）。
- `FindAll` 为 `IFindAllViewModel`（`SearchTerm`），与 FindAll 结果视图（`DataSource.FindAll.Show` 驱动，`LogViewerControl.xaml:278`）共享同一对象；`SearchTerm` setter 已触发 `PropertyChanged` 并翻转 `Show`，生命周期一致，无需新逻辑。
- 不再走 `MainWindow` 的 `LogViewPanel.*` 路径，也不引入新 ViewModel 属性。

## 4. 语义资源（具体基线值，等价复用现有颜色，DynamicResource 化）

`Semantic.xaml` 经 `Themes/Constants.xaml` 的 merged dictionaries 进入资源图（`App`/`MainWindow`/`LogViewerControl` 均经由 `Constants.xaml` 获得，**不直接改 `App.cs` 合并字典**）。镜像强调色架构（`theming.md`）：`Color` 键**不在** `Semantic.xaml` 本地声明（本地声明会遮蔽应用作用域值，阻断运行时替换），而是在 `App` 构造中发布到 `Application.Current.Resources`；`Semantic.xaml` 只声明 Brush，其 `Color` 与所有消费者都用 `{DynamicResource}`，以便后续深色模式实时替换。

```xml
<!-- Themes/Semantic.xaml -->
<SolidColorBrush x:Key="SurfaceBrush" Color="{DynamicResource Surface}" />
<SolidColorBrush x:Key="SurfaceMutedBrush" Color="{DynamicResource SurfaceMuted}" />
<SolidColorBrush x:Key="TextSecondaryBrush" Color="{DynamicResource TextSecondary}" />
<SolidColorBrush x:Key="DividerBrush" Color="{DynamicResource Divider}" />
```

```csharp
// App 构造：应用作用域发布默认 Color（与 ThemeManager 发布强调色同构）
Resources["Surface"]        = Color.FromRgb(0xFF, 0xFF, 0xFF); // #FFFFFF
Resources["SurfaceMuted"]   = Color.FromRgb(0xD8, 0xD8, 0xD8); // #D8D8D8
Resources["TextSecondary"]  = Color.FromRgb(0xA0, 0xA0, 0xA0); // #A0A0A0
Resources["Divider"]        = Color.FromRgb(0x80, 0x80, 0x80); // #808080
```

迁移映射（仅本任务范围，已实现）：

| 位置 | 现值 | 迁移为 |
|------|------|--------|
| `LogViewerControl.xaml` 根 `Background` | `White` | `{DynamicResource SurfaceBrush}`（根元素不能用 StaticResource 解析自身资源，实测修正） |
| 错误占位 Ellipse `Fill` | `#D8D8D8` | `{DynamicResource SurfaceMutedBrush}` |
| 错误占位消息/动作 `Foreground` ×2 | `#A0A0A0` | `{DynamicResource TextSecondaryBrush}` |
| 错误占位图标 Path `Fill` | `White` | `{DynamicResource SurfaceBrush}` |
| FindAll 头 TextBlock `Foreground` | `White`（on Primary） | `{DynamicResource PrimaryForegroundBrush}`（强调前景，随主题实时切换） |
| `MainWindow.xaml` flyout 分隔 `Rectangle.Fill` | `Gray` | `{DynamicResource DividerBrush}` |

- 语义 Brush 及消费者全部 `{DynamicResource}`（chrome 层，性能可忽略）；强调色仍走 `ThemePalette` + `{DynamicResource}`（`theming.md` 规则不变）。
- `SeparatorBrush`（数据源树 spacer/侧栏边框）来自 Metrolib Constants，保持原样；`LogEntryListView.cs` 的内置 `225,228,232` 分隔条属红线，本次不迁移。

## 5. 焦点桥接（关键兼容性修正）

`MainWindow.xaml.cs` 原逻辑 `PartSearchBox.Focus()` / `PartFindAllBox.Focus()` 依赖 Header 命名域，重排后失效。改为最小 code-behind 转发：

1. `LogViewerControl`（视图层，非 ViewModel）新增：
   ```csharp
   public void FocusSearch()   => PART_SearchBox?.Focus();
   public void FocusFindAll()  => PART_FindAllBox?.Focus();
   ```
   `PART_SearchBox`/`PART_FindAllBox` 为 `InitializeComponent` 生成的命名字段；模板未生成时字段为 null，`?.` 安全空操作。

2. `MainWindow.xaml.cs` 改为：
   ```csharp
   private void FocusLogFileSearch()  => FindLogViewer()?.FocusSearch();
   private void FocusLogFileSearchAll() => FindLogViewer()?.FocusFindAll();

   private LogViewerControl FindLogViewer()
       => this.FindChildrenOfType<LogViewerControl>().FirstOrDefault();
   ```
   - `FindChildrenOfType<T>` 为 Metrolib 已有扩展（`MainWindow.xaml.cs:195`、`AutoPopup.cs:63` 已使用），沿视觉树递归查找。
   - 模板尚未生成 / 当前内容不是日志页 / 无实例 → `FirstOrDefault()` 返回 null → `?.` 空操作，不抛异常。
   - 本应用主内容恒为 `LogViewPanel`（日志页），flyout 为覆盖层，日志 `LogViewerControl` 始终在树中；该防御逻辑覆盖启动时序与未来页面化。

3. 不改任何 ViewModel、不改命令绑定；四个全局快捷键的 `InputBinding` 与命令 DP 原样保留。

## 6. 性能基线

- 渲染热路径（`TextCanvas.OnRender`、`TextLine.Render`、`LogEntryListView.OnTimer`）零改动。
- 新增 `{DynamicResource}` 仅用于 chrome（头部/搜索行/错误占位），不在逐行渲染路径；渲染热路径零改动。
- 主题实时切换仍走 `ThemeManager.Apply` 一次性发布；语义 `Color` 键在应用作用域一次性发布，Brush `Color` 经 `{DynamicResource}` 跟随，不引入每帧计算。
- `FindLogViewer()` 仅由快捷键触发（低频），不进入布局/渲染循环。

## 7. 兼容性

- `net48` + `UseWPF` + `TreatWarningsAsErrors` 下零警告。
- `Semantic.xaml` 不抢占 Metrolib 隐式样式，不重复声明强调 `Color` 键、也不本地声明语义 `Color` 键（`Color` 键仅在应用作用域发布，避开 `theming.md` 陷阱）。
- 强命名与输出目录不变；SDK 项目 XAML 自动 glob，无需改 csproj。
- 文案未变，无需重新生成本地化。

## 8. 回滚形状

- R1（资源层）：移除 `Constants.xaml` 中 `Semantic.xaml` merge + 删除 `Semantic.xaml` + 移除 `App.cs` 中语义 `Color` 发布 + 还原被迁移 XAML 的颜色 → 零残留。
- R2（中性色迁移）：按文件 revert。
- R3（重排+桥接）：revert `MainWindow.xaml`（恢复 Header 搜索框）、`MainWindow.xaml.cs`（恢复 `PartSearchBox.Focus()`）、`LogViewerControl.xaml/.cs`（移除搜索行与焦点方法）。
- 任一步触碰 §1 红线即回滚到最近回滚点并回到 Plan 修订。
