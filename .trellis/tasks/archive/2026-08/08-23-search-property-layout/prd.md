# 搜索框与属性页布局修正

## Goal

1. 把日志界面两个搜索框（Search 与 Find All）从独立的 `Grid.Row=1` 搜索行**移入顶部标题网格（`Grid.Row=0` 工具栏网格）的右侧**，并**删除**旧 `Grid.Row=1` 搜索行（而非仅翻转列）。
2. 两个搜索框用**显式共享视觉 setter/资源**统一（同为 `SearchTextBox`，共用样式资源），移除旧的左侧对齐样式与 FilterTextBox 过滤视觉。
3. 修正属性页**整页**前景：根前景、Properties 头、属性名、属性值统一用 `TextPrimaryBrush`（DynamicResource），深色下可读。

> 依赖：无（独立于另外两个子任务；不触碰 `ThemeManager`）。可与 `08-23-theme-mode-and-log-colors` 并行。

## 已确认事实

| # | 事实 | 证据 |
|---|---|---|
| F1 | 顶部工具栏网格 `Grid.Row=0` 三列 `Auto | * | Auto`：col0 数据源选择器（宽 300）、col1 中央工具栏、col2 SidePanelControl（最右） | `LogViewerControl.xaml:71-199` |
| F2 | 搜索行在独立 `Grid.Row=1`（`Auto | Auto | *`，两框靠左） | `LogViewerControl.xaml:201-235` |
| F3 | Search 用 `SearchTextBox`；Find All 用 `FilterTextBox`（视觉不一致） | `LogViewerControl.xaml:211-234` |
| F4 | 行定义 5 行：`50 | Auto | 3* | Auto | Auto`；Row 2 主内容/遮罩/数据源面板、Row 3 底部信息条 | `LogViewerControl.xaml:62-69` |
| F5 | `IFindAllViewModel` 有 `Search`（`ILogSourceSearch.Count`），无 `Count` 属性；`AbstractDataSourceViewModel.Update()` 周期性调 `_search.Update()` | `IFindAllViewModel.cs`、`AbstractDataSourceViewModel.cs:784-802` |
| F6 | 属性页根 Border 无前景；Properties 头用 `TextSecondaryBrush`；`DisplayName` 与 `Value` 均无前景（深色下默认黑不可读） | `PropertiesSidePanelDataTemplate.xaml` |
| F7 | `SearchTextBox` 成员：`Watermark`/`OccurenceCount`/`CurrentOccurenceIndex`/`RequiresExplicitSearchStart` | `bin/Metrolib.xml` |

## 需求

- R1 移入顶部网格：`LogViewerControl.xaml` 顶部网格改为四列 `Auto | * | Auto | Auto`，新增 col2 为搜索容器（右对齐），col3 保持 SidePanelControl；删除旧 `Grid.Row=1` 搜索行。
- R2 行重排：RowDefinitions 由 `50 | Auto | 3* | Auto | Auto` 改为 `50 | 3* | Auto | Auto`（删除搜索行）；`Grid.Row` 由 2/3 递减为 1/2（主内容、遮罩、数据源面板、底部信息条）。
- R3 统一视觉（显式共享资源）：`UserControl.Resources` 新增 `Style x:Key="LogSearchTextBoxStyle" TargetType="SearchTextBox"`，集中 Height=24、Width=200、Padding=2、VerticalAlignment=Center、RequiresExplicitSearchStart=False、AcceptsTab=True；两个框都引用该样式，移除旧的左侧边距/对齐与 FilterTextBox 过滤视觉。
- R4 Find All 换型：`FilterTextBox` → `SearchTextBox`，`Text` 绑 `FindAll.SearchTerm`、`OccurenceCount` 绑 `FindAll.Count`、`Watermark` 用 `Strings.FindAllInLogFile`；`CurrentOccurenceIndex` 不绑。
- R5 计数：`IFindAllViewModel` 新增 `int Count { get; }`；`FindAllViewModel` 实现 `Count`（`FindAll.Search?.Count ?? 0`）+ `Update()`；`AbstractDataSourceViewModel.Update()` 追加 `_findAll.Update()`。
- R6 属性页整页前景：根 Border 加 `Foreground="{DynamicResource TextPrimaryBrush}"`；Properties 头由 `TextSecondaryBrush` 改 `TextPrimaryBrush`；`DisplayName` 与 `Value` 加 `Foreground="{DynamicResource TextPrimaryBrush}"`。
- R7 红线：不碰 `BusinessLogic/**`、不改渲染算法/虚拟化、不引入新第三方依赖；保留 Search 与 Find All 既有绑定/行为（含 `FindAll.Show`/`CloseCommand` 语义）。

## 验收标准

- [ ] 两个搜索框位于顶部网格（`Grid.Row=0`）右侧，与左上数据源选择器、中央工具栏、最右 SidePanelControl 同排不重叠；旧 `Grid.Row=1` 搜索行已删除。
- [ ] 两个框视觉统一（共享 `LogSearchTextBoxStyle` + `SearchTextBox` 搜索视觉 + 水印 + 计数）；计数随 Find All 结果显示。
- [ ] Find All 输入仍即时触发「Find all」结果视图；清空后隐藏（`Show = !string.IsNullOrEmpty(value)` 不变）；Search 绑定（Term/ResultCount/CurrentResultIndex）不变。
- [ ] 属性页根/头/名/值在浅色与深色下均清晰可读（`TextPrimaryBrush`）。
- [ ] 解决方案构建 warning-free；目标 UI STA（`LogViewerControlTest`/`MainWindowTest`）通过；`git diff --check` 通过。
- [ ] `IFindAllViewModel` 变更后，任何模拟/实现该接口的测试同步编译通过。

## 非目标

- 不改 Find All 结果视图（`PART_FindAllView`）本身、不改 `FindAll.Show/CloseCommand` 语义。
- 不做 Search 与 Find All 之外的搜索 UI 改动。

## Notes

- `SearchTextBox.CurrentOccurenceIndex` 对 Find All 不适用（同时展示全部匹配，无单一游标），不绑定；其「上一处/下一处」按钮外观存在但无导航行为——已知限制。
- 若 `SearchTextBox` 在 Find All 场景的导航 chrome 表现异常，回退方案：保留统一视觉（同高/宽/边距/水印/右对齐）但 Find All 仍用 `FilterTextBox`——记录为文档化回退，不静默切换。
