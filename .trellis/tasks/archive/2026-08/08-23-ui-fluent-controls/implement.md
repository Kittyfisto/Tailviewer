# 实施计划：Fluent UI 控件视觉化

> 每个阶段独立可验证/可回滚。遵守 design.md §1 红线清单。
> **前置：主负责人已于 2026-08-23 批准 D1–D6，并扩展交付范围为非 MVP-only**（阶段 A 全部；阶段 B 实际使用控件必须处理，风险过高则记录 out-of-scope；阶段 C 只做实际可见且低风险项）。
> 状态：规划完成，已批准，已激活（`task.py start`）。

## 阶段 0：前置确认（主负责人）

- [x] D1 范围与分期：阶段 A 全部；阶段 B 实际使用控件必须处理；阶段 C 仅实际可见低风险项。
- [x] D2 模板替换红线扩展：批准列明清单，保留 PART_*/VSM/TemplateBinding；无法从工作区验证编译模板时保留行为优先。
- [x] D3 状态色：severity 红/黄不动；强调色契约不动；不做深色强调色 lighten。
- [x] D4 焦点环：默认保持现状，CheckBox/ComboBox 模板内自绘焦点态。
- [x] D5 token 载体：新增 `Fluent.xaml`，经 `Constants.xaml`，不改 `App.cs` 合并字典；不声明 `Color` 键。
- [x] D6 全局样式：不设全局隐式 TextBlock/Control 样式。

## 阶段 1：Fluent token 资源（回滚点 R1）

1. [x] 新增 `src/Tailviewer/Themes/Fluent.xaml`：圆角 4/8、密度 32/24、字号 12/14/20。圆角以 `CornerRadius` 类型声明（非 `sys:Double`，因 `Border.CornerRadius` 无 double 隐式转换），密度/字号用 `sys:Double`。
2. [x] `Themes/Constants.xaml` merged dictionaries 追加 `Fluent.xaml`（一处）。
3. [x] 验证：构建 warning-free；`Fluent.xaml` 无 TODO、无本地 `Color` 键。

## 阶段 2：圆角 + 密度 setter（低风险，回滚点 R2）

4. [x] `MetrolibTheme.xaml`：`FlatButton` 族（FlatIconButton/AddButton/CloseButton/MoreButton/RemoveButton/TrashButton/DownloadButton/FolderOpenButton/RefreshButton）隐式样式追加 `BorderRadius=4`。**Padding 未全局设置**（多处实例已本地指定 `Padding="0"`/`Width/Height=26`，全局 setter 会改变图标按钮尺寸，违反「不改布局」）。
5. [x] `MetrolibTheme.xaml`：4 个已拥有文本框模板（`PathChooserTextBox`/`FilterTextBox`/`EditorTextBox`/`FlatPasswordBox`）的 `normalBorder`/`hoverBorder`/`focusBorder` 加 `CornerRadius=4`。**Height 未统一改动**（Filter/FlatPassword 已 32；PathChooser/Editor 沿用实例高度，避免改变设置页/树编辑行布局）。
6. [x] `Ui/LogView/ToolbarToggleButtonStyle.xaml`：模板 Border 加 `CornerRadius=ControlCornerRadius`。
7. [x] `Ui/Menu/MenuItemTemplates.xaml`：两个 popup Border 加 `CornerRadius=SurfaceCornerRadius(8)`。
8. [x] `Ui/DataSourceTree/DataSourcesControl.xaml` 根 Border、`TreeViewItemStyle.xaml` 选中 Border 加 `CornerRadius`。
9. [x] 验证：构建 warning-free；`rg "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)(Brush|\b)"` 无回归（仅 `SurfaceCornerRadius` token 引用，属 CornerRadius 非主题色）。

## 阶段 3：排版 token（回滚点 R2）

10. [x] `MetrolibTheme.xaml`：`FlatTextBlock` 隐式样式 `FontSize=FontSizeBody(14)`；`FlatGroupBox` 标题字号 22→20（`SettingsControl.xaml` 两处 HeaderDataTemplate，用 `FontSizeSubtitle` token）。菜单/列表/树模板字号未逐处改（沿用既有 12/14，避免无依据的重排）。
11. [x] 不设全局隐式 TextBlock/Control 样式（D6）；构建通过（1024 目检受限于无交互 GUI，以 `SettingsControlTest` + 运行时模板加载测试佐证不裁切/不抛异常）。

## 阶段 4：CheckBox + ComboBox 模板替换（回滚点 R3，依赖 D2）

12. [x] `MetrolibTheme.xaml` 新增 `CheckBox` 隐式样式：圆角 4 方框 + 强调色勾选/未定态 + 语义前景/禁用 + 模板内焦点描边（`FocusVisualStyle=null`）；保留 `BulletDecorator`+`ContentPresenter`（`RecognizesAccessKey`）与 `IsChecked` 绑定。
13. [x] `MetrolibTheme.xaml` 新增 `ComboBox` 隐式样式：圆角 4 边框 + 语义前景/背景 + 强调色悬停/聚焦 + 语义下拉箭头 + popup 圆角 8；保留 `PART_Popup`/`PART_EditableTextBox`/`ItemsPresenter`/`IsDropDownOpen` ToggleButton；`SelectedItem`/`DisplayMemberPath` 绑定零改动。
14. [x] 验证：构建 warning-free；`SettingsControl.xaml` 绑定零改动；运行时模板加载测试（临时 `FluentThemeLoadTest`，已删）证明 CheckBox/ComboBox/FlatProgressBar/CircularProgressBar/FlatTextBlock 模板可 `ApplyTemplate` 不抛异常。

## 阶段 4b：阶段 B/C 实际使用控件处理（负责人批准范围）

> 执行策略：先做**真实使用覆盖审计**（grep 仓库 XAML），再对每个实际使用的 Metrolib 控件判定「安全 setter/已拥有模板」vs「需编译模板替换」。
> 能从工作区安全证明的做；否则**保留行为优先**，记录 out-of-scope 与已采用的安全替代（写入覆盖审计表）。

15. [x] 使用覆盖审计：grep 每个目标控件真实实例数（见下方覆盖审计表）。
16. [x] 可安全处理项落地：
    - [x] `FlatProgressBar`(1)/`CircularProgressBar`(2)：新增隐式样式 `Foreground={DynamicResource PrimaryBrush}`（仅状态色，零模板）。
    - [x] `FlatGroupBox`(9) 标题字号 22→20（改 `SettingsControl.xaml` HeaderDataTemplate；不换模板）。
    - [x] `FlatTabControl`/`FlatTabItem`/`FlatToggleButton` 族/`OneWayToggle`/`FlatScrollBar`/`FlatContextMenu`：Metrolib 编译模板无法从工作区验证 → 保留行为优先，记录 out-of-scope + 安全替代（见下表）。
17. [x] 覆盖审计表写入 implement.md（见下方「覆盖审计表」）。

### 覆盖审计表（真实使用覆盖，2026-08-23 grep 仓库）

| 控件 | 真实实例 | 处理 | 说明 |
|---|---|---|---|
| `FlatButton` 族（9 种图标钮） | 22 | **已 Fluent** | `BorderRadius=4` setter（FlatButton 公开 DP，零模板） |
| 文本框 ×4（PathChooser/Filter/Editor/FlatPassword） | 18 | **已 Fluent** | 已拥有模板 `normalBorder/hoverBorder/focusBorder` 加 `CornerRadius=4` |
| `ToolbarToggleButton`/`LogLevelToggleButton` | 自研模板 | **已 Fluent** | Border `CornerRadius=4` |
| `MenuItemTemplates` popup | 自研模板 | **已 Fluent** | popup Border `CornerRadius=8` |
| `DataSourcesControl` 根 / `TreeViewItemStyle` 选中 | 自研模板 | **已 Fluent** | Border `CornerRadius` |
| `CheckBox` | 7（设置页 4 + 树/过滤/高亮各 1） | **已 Fluent** | 标准 WPF 模板替换（可验证，保留 Content/IsChecked） |
| `ComboBox` | 3（设置页 2 + 自定义格式 1） | **已 Fluent** | 标准 WPF 模板替换（可验证，保留 PART_*/绑定） |
| `FlatTextBlock` | 11 | **已 Fluent** | `FontSize=14`（正文 token） |
| `FlatGroupBox` | 9 | **部分** | 标题 22→20 已做；外框圆角需换编译模板 → out-of-scope（安全替代：无边框现状 + 语义 Foreground） |
| `FlatProgressBar` | 1 | **已 Fluent（状态色）** | `Foreground=PrimaryBrush`；圆角轨道需换模板 → out-of-scope |
| `CircularProgressBar` | 2 | **已 Fluent（状态色）** | `Foreground=PrimaryBrush`；本身即环形，无圆角需求 |
| `FlatTabControl`/`FlatTabItem` | 2 控件 / 5 项（About + TimeChooser） | **保留行为** | 无边框+下划线选中需完整重写编译模板（`IsSelected` 双向绑定 + 容器生成逻辑）→ out-of-scope；安全替代：保留语义 `Background/Foreground` setter |
| `FlatToggleButton` 族（Flat/Edit/Plugin/Pin/Email + DataSourcesToggleButton） | 12 | **保留行为** | 无 `BorderRadius` DP；圆角需替换 `ToggleButtonBase` 编译模板（状态色 DP 的 TemplateBinding + 子类图标模板无法验证）→ out-of-scope；安全替代：保留语义 `CheckedBackground/HoveredBackground/Foreground` setter |
| `OneWayToggle` | 7 | **保留行为** | 无圆角 DP；分段边框逻辑（`HasLeftBorder/HasRightBorder/IsPressed`）在编译模板内 → out-of-scope；安全替代：保留选中态 `PrimaryBrush` 背景/边框 setter |
| `FlatScrollBar` | 0 直接（FlatScrollViewer ×12 内部使用） | **保留行为** | 需替换完整 ScrollBar 模板（PART_Track/Thumb/RepeatButton + 双向朝向 + 命令）；无法从工作区验证 Metrolib 编译模板 → out-of-scope；安全替代：保留 `Foreground=SurfaceMuted` 语义滑块色 |
| `FlatContextMenu`（Metrolib 编译控件） | 2（MainWindow + LogViewerControl） | **保留行为** | Metrolib 编译模板（AnchorAlignment/IsFirstItemHovered）→ out-of-scope |
| 自研 `ContextMenu`（`FlatContextMenu` 样式，非编译控件） | 3（数据源树 File/Folder/Merged 模板） | **已 Fluent** | 自研 `ContextMenu` 样式（`MenuItemTemplates.xaml`），popup Border 已 `CornerRadius=8` |

## 阶段 5：回归与收尾

15. [x] 运行目标 UI STA 单测：`MainWindowTest`、`LogViewerControlTest`、`LogEntryListViewTest`、`TextCanvasTest`（+ `SettingsControlTest`/`ThemePaletteTest`/`SemanticPaletteTest`）：102 总、99 通过、0 失败、3 忽略（既有忽略）。末尾 `NUnitEngineUnloadException`（HRESULT 0x80131015）为既有跨 STA/Freezable 卸载基础设施问题，非测试失败。
16. [x] 强调色 + 深色回归：`rg "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)(Brush|\b)"` 为空。
17. [x] 红线审计：`git diff --name-only` 仅含 7 个视图/主题 XAML + 新增 `Fluent.xaml`；不含渲染器文件、`BusinessLogic/**`、ViewModel、`ThemePalette.cs`。
18. [x] `git diff --check` 通过（仅 LF→CRLF 提示）。
19. [x] 运行时资源加载验证：临时 `FluentThemeLoadTest`（已删除）证明 Fluent token + 全部新模板 `ApplyTemplate` 不抛异常；`SettingsControlTest` 证明 `SettingsControl.xaml`（含 `FontSizeSubtitle`）加载正常。

## 验证命令

```bash
dotnet build src/Tailviewer/Tailviewer.csproj
dotnet build src/Tailviewer.Tests/Tailviewer.Tests.csproj

rg -n "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer   # 应为空

# 目标 UI STA（nunit3-console 3.12.0，参考前两任务）
nunit3-console.exe bin\Tailviewer.Tests.dll "--where=test =~ 'MainWindowTest' or test =~ 'LogViewerControlTest' or test =~ 'LogEntryListViewTest' or test =~ 'TextCanvasTest'"

git diff --check
git diff --name-only   # 红线文件审计
```

## Review Gates

- G1（阶段 1 后）：`Fluent.xaml` 无 TODO、无本地 `Color` 键、经 `Constants.xaml` 进入资源图。
- G2（阶段 2/3 后）：圆角/密度/字号落地；无 `{StaticResource}` 主题键回归；1024 不重叠。
- G3（阶段 4 后）：CheckBox/ComboBox 保留 PART_*/VSM/TemplateBinding；绑定与键盘交互不退化。
- G4（阶段 5 后）：构建 warning-free + 目标 STA 通过 + 红线审计通过。

## 回滚点

- R1：移除 `Fluent.xaml` merge + 删除文件。
- R2：逐个删除圆角/密度/字号 setter。
- R3：删除 CheckBox/ComboBox 隐式样式块，回退默认模板。
- 越界即回滚：任一步触碰 §1 红线（改渲染器/BusinessLogic/ViewModel/命令事件），立即 revert 到最近回滚点并回到 Plan。
