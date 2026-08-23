# 设计：深色模式（语义 Token + 日志渲染器参与）

> 本文档把 prd.md 的 D1–D7 决策点落实为可执行设计，并给出每个决策的推荐与证据。
> **所有 D1–D7 决策已于 2026-08-23 由主负责人批准**（见 prd.md「已批准决策」），含 D2 红线扩展与 D5 Metrolib 覆盖深度。§10 保留证据表并标注批准结果。

## 1. 边界：什么能改 / 什么不能改

### 可改（视图层 + 设置模型 + 主题管线）
- `src/Tailviewer/App.cs` — 应用作用域发布完整中性 `Color` 键（浅/深两套由 ThemeManager 负责，App 构造只做初始占位或保持现状）。
- `src/Tailviewer/Ui/ThemeManager.cs` — 扩展为同时应用"强调色 + 深色模式"，并更新渲染器画刷。
- `src/Tailviewer/Ui/ThemePalette.cs` 或新增 `src/Tailviewer/Ui/SemanticPalette.cs` — 纯函数计算深/浅中性调色板（可单测）。
- `src/Tailviewer/Themes/Semantic.xaml`、`Constants.xaml`、`MetrolibTheme.xaml` — 语义 Brush 扩展与 Metrolib 定向中性覆盖。
- `src/Tailviewer/Settings/UISettings.cs`、`ApplicationSettings.cs`（序列化透传）— 新增深色标志。
- `src/Tailviewer/Ui/Settings/SettingsFlyoutViewModel.cs`、`SettingsControl.xaml` — 新增开关 + 即时应用。
- **依赖 D2 批准**：`TextCanvas.cs`、`DataSourceCanvas.cs`、`AbstractLogColumnPresenter.cs`、`TextBrushes.cs`、`LogEntryListView.cs`（仅背景/分隔条画刷，不改虚拟化/分页/命中测试）。
- `tools/generate_localization.py`（新增字符串）与其生成产物（`Strings*.resx`/`Strings.cs`）。

### 不可改（红线，违反即回滚）
- `src/Tailviewer/BusinessLogic/**`。
- `src/Tailviewer/Ui/**/*ViewModel*.cs`（除 `SettingsFlyoutViewModel` 新增一个布尔属性）、`IMainWindowViewModel.cs`、`AbstractMainPanelViewModel.cs`。
- `LogEntryListView`/`TextCanvas`/`DataSourceCanvas`/`TextLine`/`TextSegment` 的**渲染算法、分页、虚拟化、命中测试、事件与坐标系逻辑、用户数据语义**——仅允许替换背景/前景画刷来源 + 重绘所需的最小失效（D2 批准范围）。
- `ThemePalette` 强调色现有契约（`Compute(Color)` 的 Primary*/Secondary* 语义不变，测试基线不变）；`ThemeManager` 的强调色行为与实时切换能力不变（仅追加中性色与深色参数，不改既有键名/语义）。

### 主负责人附加护栏（超出即 out of scope）
- 保留强调色 `ThemePalette`/`ThemeManager` 契约与实时切换。
- 不改 `BusinessLogic/` 或无关 ViewModel。
- 仅新增深色模式字符串并重生成本地化；不触及其他字符串。
- 渲染热路径零每帧 `DynamicResource` 查找、零新增分配。

## 2. 激活与持久化（D1，推荐：二态 + 即时生效）

现状：`UISettings` 只有 `Language`/`ThemeColor`；`ApplicationSettings.Save` 写 `<ui>` 元素属性；`Restore` 按属性名回退默认。

推荐方案：
- `UISettings` 新增 `public bool DarkMode { get; set; }`，构造默认 `false`（浅色，保持现有行为）。
- `Save` 写 `writer.WriteAttributeString("darkmode", XmlConvert.ToString(DarkMode))`；`Restore` 读 `"darkmode"`（`reader.ReadAttributeAsBool` 或等价，缺省/非法回退 `false`）；`Clone` 复制。
- 启动应用：`App.StartApplication` 中 `ThemeManager.Apply(settings.Ui.ThemeColor, settings.Ui.DarkMode)`（替换现有单参调用）。
- 切换：`SettingsFlyoutViewModel.DarkMode` setter 写设置 + `SaveAsync()` + `Dispatcher.BeginInvoke(ThemeManager.Apply(...))`（镜像 `ThemeColor` setter 的延迟模式，避免在控件回调中同步改资源重入渲染）。

**备选（不推荐本任务做）**：Light/Dark/System 三态需监听 OS 主题变化（注册表/`SystemParameters`），跨 .NET Framework 4.8 且引入"用户设置 + 系统"两层优先级，复杂度明显上升；作为后续任务更合适。

## 3. 资源所有权与更新机制

### 3.1 所有权（与任务 1 一致并扩展）
```
UISettings.ThemeColor ──┐
                        ├─> ThemeManager.Apply(color, darkMode)
UISettings.DarkMode  ───┘         │
        ┌─────────────────────────┼──────────────────────────────┐
        │ 应用作用域 Color 键        │ 渲染器可变画刷（C#）             │
        │  Primary*/Secondary*（既有）│  TextBrushes.UpdateTheme     │
        │  Surface/SurfaceMuted/    │   + 新增 UpdateNeutral(dark)  │
        │  TextPrimary/TextSecondary│                                │
        │  Divider/TitleBar/        │                                │
        │  OverlayBackground/       │                                │
        │  UnfocusedOverlay         │                                │
        └─────────────────────────┴──────────────────────────────┘
                        │
                        v
        XAML Brush（{DynamicResource}）—— chrome 层实时跟随
```
- `Semantic.xaml`/`Constants.xaml` 只声明 `SolidColorBrush`，其 `Color` 一律 `{DynamicResource}`；`Color` 键只在应用作用域发布（`App` 构造 + `ThemeManager`）。
- `Constants.xaml` 现有的 `TitleBar`/`OverlayBackground`/`UnfocusedOverlay` 本地 `Color` 键**必须迁出**：`Color` 键移到应用作用域，`TitleBarBrush`/`OverlayBackgroundBrush` 等改为 `Color="{DynamicResource ...}"`（否则深色无法替换）。`UnfocusedOverlay` 是 `#30FFFFFF`（白 19% 透明）语义上偏浅色遮罩，深色可保持不变或略加深（D4）。

### 3.2 调色板助手（纯函数，可单测）
新增 `SemanticPalette`（`Compute(bool darkMode)`）返回中性 `Color` 集合，镜像 `ThemePalette` 风格：
```csharp
public sealed class SemanticPalette
{
    public Color Surface, SurfaceMuted, TextPrimary, TextSecondary, Divider, TitleBar, OverlayBackground;
    public static SemanticPalette Compute(bool darkMode) { ... }
}
```
`ThemeManager.Apply(Color primary, bool darkMode)`：
1. `CurrentPrimary = primary`，计算 `ThemePalette.Compute(primary)`，发布 Primary*/Secondary*（现状不变）。
2. `CurrentDarkMode = darkMode`，计算 `SemanticPalette.Compute(darkMode)`，发布中性 `Color` 键。
3. `TextBrushes.UpdateTheme(primary)`（现状）+ `TextBrushes.UpdateNeutral(darkMode)`（新增，见 §6）。

> 单一入口保证浅色/深色/强调色三者一致性，避免"改了强调色但忘了中性色"的漂移。

## 4. 深色调色板基线（D4，主负责人可调）

| 角色 | 键 | 浅色（现有） | 深色（推荐基线） | 说明 |
|---|---|---|---|---|
| 表面 | `Surface` | `#FFFFFF` | `#1E1E1E` | 主背景（LogViewerControl 根、渲染器画布） |
| 弱表面 | `SurfaceMuted` | `#D8D8D8` | `#2D2D30` | 占位/弱表面 |
| 主前景（新） | `TextPrimary` | `#333333` | `#DCDCDC` | 默认文字（设置页/控件/渲染器默认前景） |
| 次要前景 | `TextSecondary` | `#A0A0A0` | `#9E9E9E` | 提示/次要文字 |
| 分隔线 | `Divider` | `#808080` | `#3F3F46` | 分隔/边框 |
| 标题栏 | `TitleBar` | `#EAEDF2` | `#252526` | 底部信息条 |
| 遮罩 | `OverlayBackground` | `#55000000` | `#99000000` | flyout 遮罩 |
| 禁用前景 | `DisabledForegroundBrush` | `#B7B7B7` | `#6B6B6B` | MetrolibTheme 覆盖 |
| 渲染器未聚焦选中 | —（C#） | `#D7D7D7` | `#3F3F46` | `TextBrushes` |
| 渲染器交替行 | —（C#） | `#E8F1F7` | `#252526` | `TextBrushes.GetAlternatingColor` |

对比度（相对 `Surface #1E1E1E`，WCAG 相对亮度）：
- `TextPrimary #DCDCDC` ≈ 15.3:1（AA/AAA 达标）。
- `TextSecondary #9E9E9E` ≈ 6.5:1（AA 达标）。
- 强调色交互见 §7。

## 5. 覆盖范围映射（逐文件）

### 5.1 chrome（主窗口 + 日志查看区）
- `MainWindow.xaml`：flyout 分隔 `DividerBrush` 已 DynamicResource；`OverlayBackgroundBrush` 由 §3.1 迁移后随主题变化；Header `ForegroundBrushInverted`（白）在强调色底上保持不变。
- `LogViewerControl.xaml`：根 `SurfaceBrush` 已就绪；底部 `TitleBarBrush` 改 DynamicResource 后随主题变化。

### 5.2 设置页 + flyout
- `SettingsControl.xaml`：根 `Background="White"` → `{DynamicResource SurfaceBrush}`、`Foreground="#333333"` → `{DynamicResource TextPrimaryBrush}`；三处 `#6B6B6B` → `{DynamicResource TextSecondaryBrush}`。
- 其他 flyout/数据模板内硬编码中性色（`EmptyStateStyle`、`ImageLabel`、`About`、`ActionCenter`、`QuickFilter`、`HyperlinkRun` 禁用色）按 `research/hardcoded-color-inventory.md` 迁移到语义 Brush；**severity 语义色**（红 `#E81123`/黄 `#FFC300`/绿等）保持不动。

### 5.3 Metrolib 定向中性覆盖（D5）
Metrolib 控件在其编译主题字典内用 `{StaticResource}` 硬编码浅色（`theming.md` 已注明）。深色下必须对这些**中性表面/前景/边框**做定向覆盖，否则出现"白底控件挂在深色窗口上"。方法沿用 `theming.md` 的规则：在 `MetrolibTheme.xaml`（`App` 构造中紧跟 Metrolib Generic 之后合并，赢过主题字典）加隐式样式，`BasedOn="{StaticResource {x:Type controls:X}}"`，只重设中性色属性为 `{DynamicResource ...}`。

覆盖清单（中性色，非 Fluent 重绘）：
- `FlatGroupBox` 标题/背景、`FlatTabControl` 背景与未选中前景、`FlatScrollBar` 轨道/滑块中性色、`Menu`/`FlatContextMenu` 背景与前景、数据源树/侧栏边框（`SeparatorBrush` 来源改为动态中性键）、`ComboBox`/`EditorTextBox`/`PathChooserTextBox`/`FilterTextBox`/`FlatPasswordBox` 的 `Background`/`Foreground`/`BorderBrush`/水印色、`DisabledForegroundBrush`。
- **明确不做**：不改控件形状、圆角、图标绘制、视觉状态动画、强调色绘制（这些仍走现有 Primary*/Secondary* DynamicResource）。

> 边界判定口诀：**换"颜色"= 本任务；换"形状/状态机/布局"= 后续 Fluent 任务**。

## 6. 渲染器参与设计（D2，红线扩展，需批准）

目标：深色下日志主区域不再出现大面积 `Brushes.White`，且默认文字可读；**渲染热路径零新增分配、零每帧资源查找**。

### 6.1 现状问题
`TextCanvas.OnRender`/`DataSourceCanvas.OnRender`/`AbstractLogColumnPresenter<T>.OnRender` 均 `DrawRectangle(Brushes.White)`。`TextBrushes` 的默认前景是 `settings.Other.ForegroundColor`（默认 `Black`）；若只把背景变深而前景不动，文字不可见。

### 6.2 方案：主题感知可变画刷 + 默认色翻转
- 在 `TextBrushes` 增加静态可变画刷组（`SolidColorBrush`，不 Freeze）：
  - `CanvasBackgroundBrush`（画布背景，浅色白 / 深色 `Surface`）。
  - `DefaultForegroundBrush`（非 ColorByLevel 的默认前景，浅色黑 / 深色 `TextPrimary`）。
  - `DefaultBackgroundBrush`（非 ColorByLevel 的默认背景，浅色透明 / 深色透明；供交替行与选中补白）。
  - `SelectedUnfocusedBackgroundBrush` 改为可变（浅 `#D7D7D7` / 深 `#3F3F46`）。
  - `AlternatingBackgroundBrush`（浅 `#E8F1F7` / 深 `#252526`，替代 `GetAlternatingColor` 对白/透明的硬编码返回）。
  - `DataSourceFilenameForegroundBrush` 改为可变（浅 `#808080` / 深 `#9E9E9E`）。
- `ThemeManager` 新增调用 `TextBrushes.UpdateNeutral(bool darkMode)`，一次性 `.Color =` 更新上述可变画刷（与 `UpdateTheme` 同构）。
- 三处 `OnRender` 的 `Brushes.White` 改为 `TextBrushes.CanvasBackgroundBrush`（静态引用，无每帧查找/分配）。
- `TextBrushes.ForegroundBrush`/`BackgroundBrush` 的**非 ColorByLevel 分支**改用 `DefaultForegroundBrush`/`DefaultBackgroundBrush`；**ColorByLevel 分支仍走 `_foregroundBrushes`/`_backgroundBrushes`（用户显式色，D3 保持）**。
- `LogEntryListView` 分隔条 `Color.FromRgb(225,228,232)` → 一个主题可变画刷（浅 `#E1E4E8` / 深 `#3F3F46`），仅在构造时创建、切换时更新颜色，不重建控件。

### 6.3 ColorByLevel 交互（D3）
- 用户通过设置页配置的各级别前景/背景（`LogViewerSettings`）**原样保留**；深色模式下这些显式色可能对比不足，但这是用户显式选择，本任务不擅自改用户数据。
- 默认（非 ColorByLevel）路径是深色模式可读性的关键，由 `DefaultForegroundBrush` 保证。

## 7. 强调色交互与可访问性（D7）

- 强调色 `Primary` 默认 `#0047AB`（深蓝）。深色表面 `#1E1E1E` 上，`PrimaryForegroundBrush`（白）落在 `PrimaryBrush` 底上，对比约 8.1:1（AA/AAA 达标）——保持现状即可。
- `MainWindow` 背景与 `LogViewerControl` 工具栏是 `SecondaryBrush`（= 强调色），文字 `PrimaryForegroundBrush`（白）：若用户把强调色调成很浅的颜色，白字对比不足。这是**既有行为**，非本任务引入；不扩大范围，但在验收中作为已知限制记录。
- `SecondaryForeground = Black`（F13）当前无消费方；深色模式下若未来被消费需重新评估，本任务不改其语义。
- 高亮 `#FFFF4D` + 黑前景、高亮选中 `#FF9632` + 黑前景在深/浅两态均可读，保持不变。
- 选择（选中）背景 = 强调色 + 白前景，两态一致，保持不变。

## 8. 性能约束

- 渲染热路径（`OnRender`/`TextLine.Render`/`LogEntryListView.OnTimer`/虚拟化）**零新增每帧 `DynamicResource` 查找、零新增对象分配**——所有主题相关画刷在 `ThemeManager` 切换时一次性更新，渲染仅读取静态 `Brush` 引用。
- XAML 中新增 `{DynamicResource}` 仅用于 chrome（窗口/设置/侧栏/flyout），不进入逐行渲染；主题切换仍是一次性发布，非每帧计算。
- `SemanticPalette.Compute` 为纯函数、只在切换时调用一次，无热路径开销。

## 9. 兼容性与回滚形状

- `net48` + `UseWPF` + `TreatWarningsAsErrors` 零警告。
- 深色属性可缺省（旧设置文件回退浅色），无 schema 迁移、无 `neededPatching` 需求。
- 强调色既有单测（`ThemePaletteTest`/`TextBrushesTest`/`UISettingsTest`）基线不破坏。

回滚点：
- **R1 资源层**：撤销 `App.cs`/`ThemeManager.cs` 中性键发布 + 撤销 `SemanticPalette` + 恢复 `Constants.xaml` 本地 `Color` 键与 `{StaticResource}` → 回到纯浅色。
- **R2 设置层**：撤销 `UISettings`/`ApplicationSettings`/`SettingsFlyoutViewModel`/`SettingsControl.xaml` 深色字段与开关。
- **R3 覆盖层**：撤销 `MetrolibTheme.xaml` 定向覆盖、撤销各 XAML 硬编码色迁移。
- **R4 渲染器层**：撤销 `TextCanvas`/`DataSourceCanvas`/`AbstractLogColumnPresenter`/`TextBrushes`/`LogEntryListView` 画刷替换，恢复 `Brushes.White` 与硬编码画刷。
- 任一步触碰 §1 红线（超出 D2 批准范围）即回滚到最近回滚点并回到 Plan 修订。

## 10. 决策结论（主负责人 2026-08-23 已批准）

| 决策 | 结论 | 证据 |
|---|---|---|
| D1 激活模型 | **两态 toggle + 即时生效 + 持久化，默认浅色；不做跟随系统** | 与 `ThemeColor` 实时切换架构同构（F2/F6） |
| D2 渲染器参与 | **参与（红线扩展已批准）**，仅画刷来源/更新 + 最小失效，不改渲染算法/分页/虚拟化/命中测试/事件坐标/用户数据语义 | 日志区是主内容，不参与则深色"不可用" |
| D3 ColorByLevel | **用户显式色保持**，仅默认（非 ColorByLevel）路径随主题翻转 | 用户显式配置不应被主题静默改写 |
| D4 深色值 | **§4 基线批准** | 对比达标（§7） |
| D5 Metrolib 覆盖深度 | **定向中性色覆盖**（应用实际使用的控件），仅颜色/资源，不改形状/布局/模板（最小 setter 之外）/状态机/动画/Fluent | 不覆盖则白底控件悬在深色窗口上 |
| D6 设置入口 | **设置页「主题」分组**，不做 Header 快捷开关 | 最小改动、可发现 |
| D7 默认值 | **默认浅色**，旧 XML 缺失 `darkmode` 回退浅色，无迁移 | 向后兼容 |

> 上述决策已生效，实现不得静默偏离；任何超出边界的计划项标记 out of scope。
