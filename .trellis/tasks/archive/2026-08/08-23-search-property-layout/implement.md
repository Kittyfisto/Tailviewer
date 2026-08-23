# 实施计划：搜索框移入顶部网格 + Find All 统一视觉 + 属性页整页前景

> 独立任务；不触碰 `ThemeManager`。可回滚。
> 状态：规划完成，待 `task.py start`；尚未实现。

## 阶段 1：Find All 计数（回滚点 R1）

1. [ ] `IFindAllViewModel`：新增 `int Count { get; }`。
2. [ ] `AbstractDataSourceViewModel.FindAllViewModel`：实现 `Count`（`_dataSource.FindAllSearch?.Count ?? 0`）+ `public void Update()`（`EmitPropertyChanged(nameof(Count))`）；`AbstractDataSourceViewModel.Update()` 末尾追加 `_findAll.Update();`。
3. [ ] 验证：`dotnet build`；相关测试编译通过（Moq 对 `IFindAllViewModel.Count` setup 补齐）。

## 阶段 2：共享样式 + 顶部网格右置（回滚点 R1）

4. [ ] `LogViewerControl.xaml`：`UserControl.Resources` 新增 `LogSearchTextBoxStyle`（Height/Width/Padding/VerticalAlignment/RequiresExplicitSearchStart/AcceptsTab）。
5. [ ] 顶部网格改为四列 `Auto | * | Auto | Auto`；新增 col2 搜索容器 StackPanel（右对齐，`Visibility` 绑 `CurrentDataSource`），Search 与 Find All 引用共享样式；col3 为原 SidePanelControl。
6. [ ] Find All：`FilterTextBox` → `SearchTextBox`（`Text` 绑 `FindAll.SearchTerm`、`OccurenceCount` 绑 `FindAll.Count`、水印 `FindAllInLogFile`）。
7. [ ] 删除旧 `Grid.Row=1` 搜索网格。
8. [ ] 验证：`dotnet build` warning-free；目检两框右置、视觉统一、旧行已删。

## 阶段 3：行重排（回滚点 R1）

9. [ ] RowDefinitions：`50 | Auto | 3* | Auto | Auto` → `50 | 3* | Auto | Auto`；主内容/遮罩/`DataSourcesControl` 由 `Grid.Row=2` → `1`；底部信息条由 `Grid.Row=3` → `2`。
10. [ ] 验证：构建；目检主内容/遮罩/数据源面板/底部条位置正确。

## 阶段 4：属性页整页前景（回滚点 R2）

11. [ ] `PropertiesSidePanelDataTemplate.xaml`：根 Border 加 `Foreground="{DynamicResource TextPrimaryBrush}"`；Properties 头 `TextSecondaryBrush` → `TextPrimaryBrush`；`DisplayName` 与 `Value` 加 `Foreground="{DynamicResource TextPrimaryBrush}"`。
12. [ ] 验证：构建；深/浅两态属性页整页可读。

## 阶段 5：回归与收尾

13. [ ] 目标 UI STA：`LogViewerControlTest`/`MainWindowTest`（Find All 输入/清空/结果视图开关回归）。
14. [ ] `rg -n "PART_SearchBox|PART_FindAllBox|LogSearchTextBoxStyle|Grid.Row=\"1\"" src/Tailviewer/Ui/LogView/LogViewerControl.xaml` 核对改动点。
15. [ ] `git diff --check`；`git diff --name-only` 红线审计（仅 §1 可改文件）。

## 验证命令

```bash
dotnet build src/Tailviewer/Tailviewer.csproj
dotnet build src/Tailviewer.Tests/Tailviewer.Tests.csproj
git diff --check
git diff --name-only
```

## Review Gates

- G1：Find All 计数周期刷新且不引入 `BusinessLogic` 改动。
- G2：两搜索框并入顶部网格右侧、视觉统一、旧 `Grid.Row=1` 已删、行重排正确、Find All 输入/清空语义不变。
- G3：属性页整页深浅两态可读；语义 Token 用 `{DynamicResource}`。
- G4：构建 warning-free + UI STA 通过 + 红线审计通过。

## 回滚点

- R1：撤销 `Count`/`Update()`、共享样式、顶部网格四列、行重排与 Find All 换型，恢复三列顶部网格 + 旧 `Grid.Row=1` 搜索行（`FilterTextBox` 左置）。
- R2：撤销属性页整页前景。
- 越界即回滚：触碰 `BusinessLogic/**` 立即 revert。
