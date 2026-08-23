# 研究：Fluent 设计 token 基线（圆角/密度/排版/状态色/无障碍）

调查日期：2026-08-23。目标是把 Tailviewer 的控件视觉向 Fluent UI 靠拢，同时**不破坏**已建立的语义 token 与强调色架构。token 取值参考 Fluent UI（Web/WinUI）公开设计规范，落地为 WPF `SolidColorBrush`/`CornerRadius`/`Thickness`/`FontSize` 常量。**不引入任何新依赖**（不用 MahApps / MaterialDesign / Fluent.Ribbon）。

## 1. 圆角（CornerRadius）

| 角色 | Fluent 值 | 应用 |
|---|---|---|
| 控件（按钮/输入框/勾选框/组合框） | `4` | FlatButton `BorderRadius`、文本框模板、CheckBox/ComboBox 模板 |
| 卡片/flyout/菜单弹层/滚动条滑块 | `8` | MenuItemTemplates popup Border、flyout 表面、FlatScrollBar 滑块 |
| 分段控件（OneWayToggle） | `4`（两端 4，中间 0） | 可选 |
| 顶部工具栏选中 pill（ToolbarToggleButton 选中态） | 全圆（`12` 或 `Height/2`） | 可选，属密度/形状视觉 |

> 说明：`CornerRadius` 是 WPF 原生类型，无需依赖；`FlatButton.BorderRadius` 是 Metrolib 自有 double 属性，setter 即可（见 control-inventory.md §4）。

## 2. 密度（Density）

| 角色 | Fluent 值 | 现状 | 目标 |
|---|---|---|---|
| 标准控件高 | `32`（Standard）/ `24`（Compact） | 文本框 26~32 不等；菜单项 32 | 统一为 32 标准 / 24 紧凑 |
| 按钮 padding | 标准 `8,5` / 紧凑 `6,3` | FlatButton 默认 | 统一 |
| 行高/间距 | `4` 间距网格 | 散落 4/6/8 | 收敛到 4 的倍数 |

> 注意：**不**改 `LogEntryListView` 的 33ms 虚拟化行高与命中测试——日志行密度由 `LogViewerSettings`/渲染器管理，属红线。密度 token 仅用于 chrome 控件。

## 3. 排版（Typography）

- WPF `net48` 默认字体即 **Segoe UI**（现代 Windows 系统字体映射），故**无需**强行改 FontFamily，重点是**显式统一字号/字重层级**，消除散落的 8/10/12/14/18/20/22/28/32 字号。
- 建议 token（Fluent type ramp 的近似）：
  - `Caption` 12 · `Body` 14 · `BodyStrong` 14 SemiBold · `Subtitle` 20 · `Title` 28/32（仅 About/错误占位用，可保留）
  - 现有 `FlatGroupBox` 标题模板 `FontSize=22` 收敛为 `20`（Subtitle）。
- 字号 token 通过 `SolidColorBrush` 之外的资源（`sys:Double` 或直接 `FontSize` setter）落地；**不**设全局隐式 `TextBlock`/`Control` 样式（会冲击布局与渲染器宿主，风险高）。

## 4. 状态色（accent / selection / error / warning / success）

### 强调色（保持既有契约，不改算法）
- 基色 `UISettings.DefaultThemeColor = #0047AB`，`ThemePalette.Compute` 派生 Primary*/Secondary*，`{DynamicResource}` 消费。**Fluent 化不改这套契约**（`ThemePaletteTest` 基线不变）。
- 仅**可选**新增「深色下强调色自动 lighten」的派生规则（dark-mode 任务的未来项），本任务默认不做，除非主负责人明确要求。

### 选中（Selection）
- 选中背景 = 强调色 + 白前景（既有，两态一致）。Fluent 化可在自定义模板（TreeViewItem/FlatListViewItem/FlatToggleButton 选中态）中引入 `8%` 强调色 tint 的未聚焦选中态（`SelectionUnfocused`），但**默认复用**现有 `SurfaceMuted` 语义键，避免新增键与风险。

### 错误/警告/成功（severity，对比现有值）
| 语义 | 现有（Tailviewer 散落） | Fluent 推荐 | 建议 |
|---|---|---|---|
| 错误红 | `#E81123`（Windows 红） | `#C42B1C`（Fluent 红） | **保留 `#E81123`**，避免大面积改 severity 与 `LogViewerSettings` 默认级别色 |
| 警告黄 | `#FFC300` | `#9D5D00`（深橙） | 保留 `#FFC300` |
| 成功绿 | 无统一 | `#0F7B0F` | 仅新建 `Success` 语义键，不迁移现有 |

> 结论：severity 语义色**保持不动**（与 dark-mode 任务红线一致）。Fluent 化不重定义错误红/黄，避免破坏 `LogViewerSettings` 默认级别色与既有视觉。此点记为决策点 D3 供主负责人确认。

## 5. 无障碍（Accessibility）

- **对比度**：深/浅两态下 `TextPrimary/TextSecondary` 对 `Surface` 已达标（dark-mode design.md §7：深色 15.3:1 / 6.5:1）。Fluent 化新增的圆角/密度不改前景/背景对比，仅需对新模板的 `Disabled`（`DisabledForeground`）与 `SelectionUnfocused` 校验对比。
- **焦点环**：Fluent 强调可见焦点。现状多处 `FocusVisualStyle="{x:Null}"`（FlatToggleButton、文本框、TreeViewItem 等）。**决策点**：是否为本任务恢复一套 Fluent 焦点环（1px 强调色外描边）。恢复会影响键盘交互一致性，需主负责人拍板；默认**保持现状**（不改键盘行为），仅在新模板（CheckBox/ComboBox）内实现焦点环。
- **键盘/鼠标交互**：不改任何命令、键位、`Click=` 事件、`ContextMenu` 行为、拖拽。

## 6. token 落地载体（不新增依赖）

- 新增 `Themes/Fluent.xaml`（或并入 `Semantic.xaml`）声明：
  - 圆角资源：`<sys:Double x:Key="ControlCornerRadius">4</sys:Double>` 或直接 `CornerRadius` 常量；
  - 密度资源：`<sys:Double x:Key="ControlHeightStandard">32</sys:Double>`、`<sys:Double x:Key="ControlHeightCompact">24</sys:Double>`；
  - 字号资源：`<sys:Double x:Key="FontSizeCaption">12</sys:Double>` 等。
- 经 `Themes/Constants.xaml` merged dictionaries 进入资源图（与 `Semantic.xaml` 同路径），**不改 `App.cs` 合并字典**（除非需应用作用域发布新 `Color` 键）。

## 7. 结论

- Fluent 化是**视觉 token 落地 + 少量模板替换**，不引入依赖、不改渲染器/逻辑/MVVM。
- 最低风险增量 = 圆角（FlatButton setter + 已拥有文本框模板）+ 密度/字号 token + CheckBox/ComboBox 两处模板替换。
- severity 色保持不动；强调色契约保持不动；焦点环作为独立决策点。
