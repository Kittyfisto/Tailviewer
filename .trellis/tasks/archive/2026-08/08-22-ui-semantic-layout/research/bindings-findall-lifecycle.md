# 研究：LogViewerControl 绑定契约与 FindAll 生命周期

调查日期：2026-08-22（方向 A 批准后，实现前补充确认）

## 结论

1. **DataContext 链路**：`MainWindow` 的 `ContentPresenter`(PartContent) 绑定 `LogViewPanel`（`LogViewMainPanelViewModel`），经 `LogViewMainPanelDataTemplate.xaml` 的 `DataTemplate DataType=LogViewMainPanelViewModel` 实例化 `LogViewerControl`。因此 `LogViewerControl.DataContext == LogViewMainPanelViewModel`。
   - 证据：`LogViewerControl.xaml` 已用 `{Binding CurrentDataSource}`、`{Binding SidePanels}`、`{Binding SelectedSidePanel}`、`{Binding DataSources}`（均为 LogViewMainPanelViewModel 成员）。

2. **Search 路径**：`LogViewMainPanelViewModel.Search` 类型为 `ISearchViewModel`（`ChangeDataSource` 中 `Search = value?.Search`），暴露 `Term`/`ResultCount`/`CurrentResultIndex`。`IDataSourceViewModel.Search` 同为 `ISearchViewModel`（`IDataSourceViewModel.cs:136`）。
   - 故新 `SearchTextBox` 绑定 `{Binding Search.Term}` 等，等价于原 `LogViewPanel.Search.*`。

3. **FindAll 路径与生命周期**：`LogViewMainPanelViewModel.FindAll` 类型 `IFindAllViewModel`（`FindAll = value?.FindAll`）。实现类为 `AbstractDataSourceViewModel` 的嵌套 `FindAllViewModel`，**每个数据源持有一个**，其 `SearchTerm` getter/setter 代理到 `_dataSource.FindAllFilter`，setter 触发 `PropertyChanged` 并翻转 `Show`。
   - `LogViewerControl.xaml` 的 FindAll 结果视图绑定 `{Binding DataSource.FindAll.Show/LogSource/...}`（ElementName=This 的 `DataSource` DP，其值 = `LogViewerViewModel.DataSource` = 同一 `IDataSourceViewModel`）。
   - 因此 `FilterTextBox.FilterText = {Binding FindAll.SearchTerm}` 与 FindAll 结果视图引用**同一对象**，输入即可打开/关闭面板，无生命周期缺口，无需新增逻辑。

4. **焦点桥接可行工具**：`FindChildrenOfType<T>`（Metrolib 扩展）已在 `MainWindow.xaml.cs:195` 与 `AutoPopup.cs:63`（`Application.Current?.MainWindow?.FindChildrenOfType<TextCanvas>()`）使用，可沿视觉树查找 `LogViewerControl`。空安全 `FirstOrDefault()?.` 处理模板未生成场景。

5. **App 字典合并点**：`App` 构造函数 `Resources.MergedDictionaries` 依次合并 Metrolib Generic.xaml、`Themes/MetrolibTheme.xaml`（后者内部 merge `Constants.xaml`）。`Semantic.xaml` 应在此之后合并（app 级资源），或并入 `Constants.xaml` 的 MergedDictionaries；本实现采用 App 构造处新增一行，最小侵入。

## 涉及文件（只读证据）

- src/Tailviewer/Ui/LogView/LogViewMainPanelDataTemplate.xaml
- src/Tailviewer/Ui/LogView/LogViewMainPanelViewModel.cs
- src/Tailviewer/Ui/LogView/AbstractDataSourceViewModel.cs（FindAllViewModel 嵌套类，~503-570 行）
- src/Tailviewer/Ui/LogView/ISearchViewModel.cs
- src/Tailviewer/Ui/DataSourceTree/IDataSourceViewModel.cs（Search/FindAll 定义，136-138 行）
- src/Tailviewer/App.cs（字典合并，43-44 行）
- src/Tailviewer/Ui/MainWindow.xaml.cs（FindChildrenOfType 用法，195 行）
