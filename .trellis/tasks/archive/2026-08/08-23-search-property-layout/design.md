# 设计：搜索框移入顶部网格 + Find All 统一视觉 + 属性页整页前景

## 1. 边界

### 可改
- `src/Tailviewer/Ui/LogView/LogViewerControl.xaml`（顶部网格列、删除 Row 1、搜索容器 + 共享样式）。
- `src/Tailviewer/Ui/LogView/IFindAllViewModel.cs`（新增 `Count`）。
- `src/Tailviewer/Ui/LogView/AbstractDataSourceViewModel.cs`（`FindAllViewModel.Count` + `Update()`；`Update()` 追加 `_findAll.Update()`）。
- `src/Tailviewer/Ui/SidePanel/Property/PropertiesSidePanelDataTemplate.xaml`（整页前景）。

### 不可改
- `src/Tailviewer/BusinessLogic/**`（`FindAllFilter`/`FindAllSearch`/`FindAllLogSource` 语义不变）。
- 渲染算法/虚拟化/命中测试；其它搜索 UI；`ThemeManager`。

## 2. 顶部网格布局（`LogViewerControl.xaml`）

现状（行 71-199）三列 `Auto | * | Auto`；搜索行（行 201-235）独立 `Grid.Row=1`。

改为顶部网格四列，搜索容器并入 col2（右对齐），col3 保持 SidePanelControl：

```xml
<Grid Background="{DynamicResource SecondaryBrush}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />   <!-- col0 数据源选择器 -->
        <ColumnDefinition />                 <!-- col1 中央工具栏 -->
        <ColumnDefinition Width="Auto" />    <!-- col2 搜索容器（右对齐） -->
        <ColumnDefinition Width="Auto" />    <!-- col3 SidePanelControl -->
    </Grid.ColumnDefinitions>

    <!-- col0: DataSourcesToggleButton + PinToggleButton（不变） -->
    <!-- col1: Central Toolbar Border（不变） -->

    <!-- col2: 搜索容器（右对齐） -->
    <StackPanel Grid.Column="2"
                Orientation="Horizontal"
                VerticalAlignment="Center"
                Margin="0,0,8,0"
                Visibility="{Binding CurrentDataSource, Converter={StaticResource NullToCollapsedConverter}}">
        <controls:SearchTextBox x:Name="PART_SearchBox"
                                Style="{StaticResource LogSearchTextBoxStyle}"
                                Margin="0,0,8,0"
                                Text="{Binding Search.Term, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                OccurenceCount="{Binding Search.ResultCount}"
                                CurrentOccurenceIndex="{Binding Search.CurrentResultIndex, Mode=TwoWay}"
                                Watermark="{x:Static loc:Strings.SearchLogFile}" />
        <controls:SearchTextBox x:Name="PART_FindAllBox"
                                Style="{StaticResource LogSearchTextBoxStyle}"
                                Text="{Binding FindAll.SearchTerm, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                OccurenceCount="{Binding FindAll.Count}"
                                Watermark="{x:Static loc:Strings.FindAllInLogFile}" />
    </StackPanel>

    <!-- col3: SidePanelControl（原 col2，不变） -->
</Grid>
```

- 共享样式（`UserControl.Resources` 内新增）：

```xml
<Style x:Key="LogSearchTextBoxStyle" TargetType="{x:Type controls:SearchTextBox}">
    <Setter Property="Height" Value="24" />
    <Setter Property="Width" Value="200" />
    <Setter Property="Padding" Value="2" />
    <Setter Property="VerticalAlignment" Value="Center" />
    <Setter Property="RequiresExplicitSearchStart" Value="False" />
    <Setter Property="AcceptsTab" Value="True" />
</Style>
```

- **移除旧 Row 1**：删除整个 `Grid.Row=1` 搜索网格（行 201-235），旧左侧 `Margin="4,4,4,0"`、`*` 尾列、FilterTextBox 过滤视觉一并移除。

## 3. 行重排

- RowDefinitions：`50 | Auto | 3* | Auto | Auto` → `50 | 3* | Auto | Auto`（删搜索行 `Auto`）。
- 元素 `Grid.Row` 递减：
  - 主内容网格 `Grid.Row="2"` → `Grid.Row="1"`。
  - 遮罩 `Grid.Row="2"` → `Grid.Row="1"`。
  - `DataSourcesControl Grid.Row="2"` → `Grid.Row="1"`。
  - 底部信息条 `Grid.Row="3"` → `Grid.Row="2"`。
- 末尾 `Auto` 行（原 index 4 → 新 index 3）仍无内容，保持现状。

## 4. Find All 统一视觉 + 计数

- 控件 `FilterTextBox`（过滤视觉）→ `SearchTextBox`（搜索视觉），与 Search 统一。
- `Text` 绑 `FindAll.SearchTerm`（替代 `FilterText`）；`RequiresExplicitSearchStart="False"` 保持即时搜索。
- 计数：
  ```csharp
  // IFindAllViewModel
  int Count { get; }

  // FindAllViewModel
  public int Count => _dataSource.FindAllSearch?.Count ?? 0;
  public void Update() { EmitPropertyChanged(nameof(Count)); }

  // AbstractDataSourceViewModel.Update() 末尾追加（镜像 _search.Update()）：
  _findAll.Update();
  ```
- `CurrentOccurenceIndex` 不绑定（Find All 无单一游标）；导航按钮视觉存在但无导航行为（已知限制）。

## 5. 属性页整页前景（`PropertiesSidePanelDataTemplate.xaml`）

- 根 `Border`：加 `Foreground="{DynamicResource TextPrimaryBrush}"`（子元素默认继承）。
- `Properties` 头 TextBlock：`Foreground` 由 `TextSecondaryBrush` → `TextPrimaryBrush`。
- `DisplayName` TextBlock：加 `Foreground="{DynamicResource TextPrimaryBrush}"`（保留 `FontWeight="Bold"`）。
- `Value` ContentPresenter：加 `Foreground="{DynamicResource TextPrimaryBrush}"`。
- 规则依据：`theming.md` 要求语义 Brush 一律 `{DynamicResource}`；`TextPrimaryBrush` 已存在于 `Semantic.xaml`。

## 6. 兼容性与回滚

- 回滚点 R1：恢复三列顶部网格 + 恢复旧 `Grid.Row=1` 搜索行（`FilterTextBox` 左置）+ 撤销 `Count`/`Update()` 与行重排。
- 回滚点 R2：撤销属性页整页前景。
- 若 `SearchTextBox` 在 Find All 场景异常，回退方案（prd Notes）为文档化回退。

## 7. 测试

- `LogViewerControlTest`/`MainWindowTest`（UI STA）：搜索行并入顶部网格、Find All 输入/清空/结果视图开关回归。
- 若存在 `IFindAllViewModel` 的 Moq 实现，补充 `Count` 的 setup（编译即覆盖）。
