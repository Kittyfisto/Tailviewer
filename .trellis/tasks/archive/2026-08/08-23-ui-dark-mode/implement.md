# 实施计划：深色模式（语义 Token + 日志渲染器参与）

> 每个阶段独立可验证/可回滚。遵守 `design.md` §1 红线清单。
> **前置**：主负责人已于 2026-08-23 批准 D1–D7（含 D2 红线扩展与 D5 Metrolib 覆盖深度），并给出附加护栏（见 prd.md「已批准决策」）。
> 状态：规划完成，待 `task.py start` 激活；尚未实现。

## 阶段 0：前置确认（主负责人，已完成）

- [x] D1 激活模型：两态 toggle + 即时生效（默认浅色，不做跟随系统）。
- [x] D2 渲染器参与：批准扩展红线文件集（`TextCanvas.cs`/`DataSourceCanvas.cs`/`AbstractLogColumnPresenter.cs`/`TextBrushes.cs`/`LogEntryListView.cs` 最小分隔条）；仅画刷来源/更新 + 最小失效。
- [x] D3 ColorByLevel：用户显式色保持，仅默认前景/背景翻转。
- [x] D4 深色值：采用 design.md §4 基线。
- [x] D5 Metrolib 覆盖深度：定向中性色覆盖（非 Fluent 重绘，仅颜色/资源）。
- [x] D6 设置入口：设置页「主题」分组（紧邻 ThemeColor；无 Header 快捷开关）。
- [x] 附加护栏：保留强调色 ThemePalette/ThemeManager 契约与实时切换；不改 BusinessLogic/无关 ViewModel；仅新增深色字符串重生成本地化；渲染热路径零每帧 DynamicResource/零新增分配。

## 阶段 1：调色板与主题管线（回滚点 R1）

1. [ ] 新增 `src/Tailviewer/Ui/SemanticPalette.cs`：纯函数 `Compute(bool darkMode)` 返回中性 `Color`（design.md §4 基线）。
2. [ ] 扩展 `ThemeManager.Apply(Color primary, bool darkMode)`：保留强调色逻辑，新增中性 `Color` 键发布 + `TextBrushes.UpdateNeutral(darkMode)`；保留旧签名或改为唯一签名并更新两处调用点（`App.cs:246`、`SettingsFlyoutViewModel`）。
3. [ ] `App.cs`：语义 `Color` 键发布改由 ThemeManager 统一负责（App 构造可留初始占位，避免空资源）；启动应用点改为 `ThemeManager.Apply(settings.Ui.ThemeColor, settings.Ui.DarkMode)`。
4. [ ] `Constants.xaml`：`UnfocusedOverlay`/`TitleBar`/`OverlayBackground` 三个 `Color` 键移出（删除本地声明），`TitleBarBrush`/`OverlayBackgroundBrush` 等改 `Color="{DynamicResource ...}"`。
5. [ ] `Semantic.xaml`：新增 `TextPrimaryBrush`（`Color="{DynamicResource TextPrimary}"`）；确认全部 Brush 用 DynamicResource。
6. [ ] 验证：构建 warning-free；`rg "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer` 仅允许非主题键（如 `DisabledForegroundBrush` 迁移前的中间态）。

## 阶段 2：设置模型与持久化（回滚点 R2）

7. [ ] `UISettings`：新增 `DarkMode` 布尔（默认 false）+ `Save`/`Restore`/`Clone` 覆盖。
8. [ ] `SettingsFlyoutViewModel`：新增 `DarkMode` 属性，setter 写设置 + `SaveAsync()` + Dispatcher 延迟 `ThemeManager.Apply(ThemeColor, value)`。
9. [ ] `SettingsControl.xaml`：主题分组新增深色开关（`CheckBox` 或 `OneWayToggle`，绑定 `DarkMode`）。
10. [ ] `tools/generate_localization.py`：新增 `DarkMode`（英/zh-CN）字符串；重跑生成三件套（`Strings.resx`/`Strings.zh-CN.resx`/`Strings.cs`）。
11. [ ] 新增 `UISettingsTest` 深色字段用例（构造默认/Clone/Roundtrip/缺省回退）。
12. [ ] 验证：`dotnet build`；`UISettingsTest` 通过。

## 阶段 3：视图层中性色迁移（回滚点 R3）

13. [ ] `SettingsControl.xaml`：根 `Background White→SurfaceBrush`、`Foreground #333333→TextPrimaryBrush`、三处 `#6B6B6B→TextSecondaryBrush`。
14. [ ] 按 `research/hardcoded-color-inventory.md` 迁移 `EmptyStateStyle`、`ImageLabel`、`AboutFlyoutDataTemplate`、`ActionCenter` 中性色、`QuickFilter/FilterToggleButton`、`MetrolibTheme.xaml` 的 `DisabledForegroundBrush`/水印色；severity 色不动。
15. [ ] 每批构建；grep 校验无主题键 `{StaticResource}` 回归。

## 阶段 4：Metrolib 定向中性覆盖（回滚点 R3）

16. [ ] 在 `MetrolibTheme.xaml` 加隐式样式（`BasedOn="{StaticResource {x:Type controls:X}}"`）覆盖 `FlatGroupBox`、`FlatTabControl`、`FlatScrollBar`、`FlatContextMenu`/`Menu`、`ComboBox`、文本框族（`EditorTextBox`/`PathChooserTextBox`/`FilterTextBox`/`FlatPasswordBox`）的 `Background`/`Foreground`/`BorderBrush`/水印为 `{DynamicResource}` 语义键。
17. [ ] 数据源树/侧栏 `SeparatorBrush` 来源改为动态中性键（或新增 `DividerBrush` 替代），消除浅色边框残留。
18. [ ] 验证：构建通过；人工/STA 目检深色下无"白底控件悬空"。

## 阶段 5：渲染器参与（回滚点 R4，依赖 D2）

19. [ ] `TextBrushes`：新增可变画刷组 `CanvasBackgroundBrush`/`DefaultForegroundBrush`/`DefaultBackgroundBrush`/`SelectedUnfocusedBackgroundBrush`（改可变）/`AlternatingBackgroundBrush`/`DataSourceFilenameForegroundBrush`（改可变）；新增 `UpdateNeutral(bool darkMode)` 一次性更新；`ForegroundBrush`/`BackgroundBrush` 非 ColorByLevel 分支改用默认画刷；`GetAlternatingColor` 改用 `AlternatingBackgroundBrush`。
20. [ ] `TextCanvas.OnRender`、`DataSourceCanvas.OnRender`、`AbstractLogColumnPresenter<T>.OnRender`：`Brushes.White` → `TextBrushes.CanvasBackgroundBrush`；`DataSourceCanvas` 默认前景 `Brushes.Black` → `TextBrushes.DefaultForegroundBrush`。
21. [ ] `LogEntryListView` 分隔条：硬编码 `225,228,232` → 主题可变画刷（浅/深值见 design.md §4）。
22. [ ] 验证：构建 warning-free；STA 单测（`LogEntryListViewTest`/`TextCanvasTest`）通过；深色下渲染器目检可读。

## 阶段 6：回归与收尾

23. [ ] 运行目标 UI STA 单测：`MainWindowTest`、`LogViewerControlTest`、`LogEntryListViewTest`、`TextCanvasTest` + `UISettingsTest` + 新增中性调色板单测。
24. [ ] 强调色回归：浅/深两态切换强调色实时生效（`rg "StaticResource Primary"` 为空）。
25. [ ] 红线审计：`git diff --name-only` 仅含 D2 批准的渲染器文件 + 视图/设置/主题文件；无 `BusinessLogic/**`、无 ViewModel（除 `SettingsFlyoutViewModel`）。
26. [ ] `git diff --check` 通过。
27. [ ] 深色/浅色目检清单：主窗口、日志渲染器、设置页、侧栏、About、ActionCenter、flyout 遮罩、滚动条、菜单、组合框均正确。

## 验证命令

```bash
dotnet build src/Tailviewer/Tailviewer.csproj
dotnet build src/Tailviewer.Tests/Tailviewer.Tests.csproj

python tools/generate_localization.py
git status --porcelain   # 确认 Strings*.resx / Strings.cs 已重新生成

rg -n "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer  # 应为空

# 目标 UI STA（nunit3-console 3.12.0，参考任务 1）
nunit3-console.exe bin\Tailviewer.Tests.dll "--where=test =~ 'MainWindowTest' or test =~ 'LogViewerControlTest' or test =~ 'LogEntryListViewTest' or test =~ 'TextCanvasTest' or test =~ 'UISettingsTest'"

git diff --check
git diff --name-only   # 红线文件审计
```

## Review Gates

- G1（阶段 1 后）：`SemanticPalette` 纯函数可单测；`ThemeManager.Apply(color, darkMode)` 单入口；`Color` 键只在应用作用域；无主题键 `{StaticResource}` 回归。
- G2（阶段 2 后）：深色属性 roundtrip + 缺省回退；开关即时切换无重启；本地化三件套已重生成。
- G3（阶段 3/4 后）：无"白底控件悬空"；severity 色与强调色绘制未动；未引入 Fluent 重绘。
- G4（阶段 5 后）：渲染热路径零新增每帧资源查找/分配；STA 渲染单测通过。
- G5（阶段 6 后）：构建 warning-free + 目标 STA 通过 + 红线审计通过。

## 回滚点

- R1：撤销调色板/主题管线（恢复浅色语义键 + `Constants.xaml` 本地 `Color` 键 + 单参 `Apply`）。
- R2：撤销设置模型与开关。
- R3：撤销视图层迁移与 Metrolib 覆盖。
- R4：撤销渲染器画刷替换，恢复 `Brushes.White` 与硬编码画刷。
- 越界即回滚：任一步触碰 §1 红线（超出 D2 批准范围），立即 revert 到最近回滚点并回到 Plan。
