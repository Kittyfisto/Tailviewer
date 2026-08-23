# 重做深色主题、日志等级配色与日志界面布局

## Goal

在已归档 08-23-ui-dark-mode（两态 Light/Dark + 语义 Token + 日志渲染器参与）基础之上：把主题选择升级为 **Light/Dark/System 三态**；**重设计**日志等级浅/深两套专业默认色阶（同时保留用户自定义色）；**重设计**默认强调色（弃用遗留 `#0047AB`）；修正日志主界面两处布局：属性页整页前景与搜索框移入顶部网格右侧。

本任务为 **parent 任务**：拥有源需求集合、子任务映射、跨子任务验收标准与最终集成评审。无直接 src 实现工作。

## 源需求集合

- R1 **主题模式三态**：设置页「主题」分组提供 Light / Dark / System 三选；切换即时生效、持久化、无需重启。
- R2 **System 跟随**：System 模式跟随操作系统主题（Windows 深/浅），系统主题变化实时跟随；Light/Dark 不受系统主题影响。
- R3 **日志等级默认色阶（重设计）**：为 OTHER/TRACE/DEBUG/INFO/WARNING/ERROR/FATAL 定义**精确、经测试**的浅色与深色两套默认色（非保留旧浅色）；非自定义等级在主题切换时自动翻转。
- R4 **保留用户自定义色**：用户显式配置的等级前景/背景在两套主题下原样保留，不被主题切换改写。
- R5 **默认强调色（重设计）**：弃用遗留 `#0047AB`，采用专业、可访问的新默认色，记录精确值与浅/深对比行为；保留用户自定义 ThemeColor。
- R6 **属性页整页前景修正**：属性页根前景、Properties 头、属性名、属性值统一用 `TextPrimaryBrush`（DynamicResource），深色下可读。
- R7 **搜索框**：两个搜索框（Search 与 Find All）移入**顶部网格（Grid.Row=0 工具栏网格）右侧**，删除旧 `Grid.Row=1` 搜索行；Find All 与 Search 用显式共享视觉 setter/资源统一。
- R8 **保留既有架构**：保留深色语义资源架构（`Color` 键仅在应用作用域、Brush 用 `{DynamicResource}`、`ThemePalette`/`SemanticPalette`/`TextBrushes` 可变画刷管线、渲染热路径零每帧资源查找/零新增分配）。

## 子任务映射

| 子任务 | 拥有需求 |
|---|---|
| `08-23-theme-mode-and-log-colors` | R1, R2, R5, R8（三态 + 系统检测 + 默认强调色 + `ThemeManager` 签名） |
| `08-23-log-level-default-colors` | R3, R4, R8（双套色阶 + `IsCustom` + `TextBrushes` 等级画刷 + 钩子） |
| `08-23-search-property-layout` | R6, R7（属性页整页前景 + 搜索框移入顶部网格右侧） |

## 跨子任务验收标准

- [ ] 三子任务完成后，`ThemeManager.Apply(Color, ThemeMode)` 单入口：一次调用解析有效 darkMode，一致驱动中性色、强调色（新默认）、渲染器中性画刷**与**非自定义等级默认色翻转。
- [ ] Light/Dark/System 三态下，非自定义日志等级默认色按新色阶可读（对比度达标），自定义色不变。
- [ ] 属性页整页前景与两个搜索框在浅/深两态正确；搜索框位于顶部网格右侧、Find All 视觉统一、旧搜索行已删除。
- [ ] 解决方案 `net48` 构建 warning-free（`TreatWarningsAsErrors`）；`git diff --check` 通过。
- [ ] 红线审计：不触碰 `BusinessLogic/**`；渲染热路径零新增每帧资源查找/分配。
- [ ] 强调色实时切换回归：`rg "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer` 为空（或仅剩非主题键）。

## 实施顺序（依赖）

1. `08-23-search-property-layout`（独立、不触碰 `ThemeManager`；可与 2 并行）。
2. `08-23-theme-mode-and-log-colors`（先于 3：提供 `ThemeMode`、有效 darkMode 解析信号、新默认强调色）。
3. `08-23-log-level-default-colors`（依赖 2 的解析信号；等级默认色钩子由本任务在 `Apply(Color, bool)` 内实现，无缺失方法/`#if` 桩）。

> 依赖写在各子任务 `prd.md`/`implement.md` 内，不以树形位置隐含依赖。

## Notes

- 本 parent 不做实现；集成评审在三个子任务归档前完成（见 `implement.md`）。
- 源需求 R1–R8 完整细节与验收由各子任务 `prd.md` 承载；本文件只做集合与映射。
