# 日志级别默认色阶（浅色/深色专业双套重设计 + 保留用户自定义）

## Goal

为 OTHER/TRACE/DEBUG/INFO/WARNING/ERROR/FATAL **重设计**浅色与深色两套专业默认色阶（非保留旧浅色值），精确、经测试、对比度达标；非自定义等级在主题切换时自动翻转；**用户显式配置的等级前景/背景在两套主题下原样保留**。渲染热路径保持零每帧资源查找/零新增分配。

> 依赖：本子任务后于 `08-23-theme-mode-and-log-colors`（消费其 `ThemeMode` 解析出的有效 `ThemeManager.CurrentDarkMode`）。
> 干净顺序契约：本子任务在 `ThemeManager.Apply(Color, bool)` 末尾追加 `TextBrushes.UpdateLevelDefaults(darkMode)` + 触发 `ThemeChanged(darkMode)`；依赖子任务不提供/不引用该钩子，无 `#if` 桩、无缺失方法。

## 已确认事实

| # | 事实 | 证据 |
|---|---|---|
| F1 | 等级默认色只有一套旧浅色：Other/Info FG Black BG Transparent；Trace/Debug FG #808080 BG Transparent；Warning FG White BG #FFC300；Error/Fatal FG White BG #E81123 | `LogViewerSettings.cs` |
| F2 | `LogLevelSettings` 仅 `ForegroundColor`/`BackgroundColor`，**无「是否自定义」标记**；`Save` 无条件写两属性 | `LogLevelSettings.cs` |
| F3 | `TextBrushes` 实例构造按 `ILogViewerSettings` 逐等级 `CreateBrush(settings.X.ForegroundColor)` 冻结；无主题感知的等级默认画刷 | `TextBrushes.cs:96-140` |
| F4 | 设置页等级颜色由 `LogLevelSettingsViewModel` 直接读写，setter 调 `SaveAsync()` | `LogLevelSettingsViewModel.cs` |
| F5 | `ThemeManager.CurrentDarkMode` 已在 `Apply` 记录 | `ThemeManager.cs` |
| F6 | `TextBrushes.UpdateNeutral(bool)` 已一次性更新中性静态可变画刷（不每帧查找/分配），为等级默认画刷提供同构范式 | `TextBrushes.cs:72-91` |
| F7 | `LogViewerSettingsTest`/`LogLevelSettingsTest`/`TextBrushesTest` 硬编码旧浅色默认值，须随新色阶更新 | 各 `*Test.cs` |

## 需求

- R1 新增 `LogLevelDefaults`：`Light`/`Dark` 双套（精确 hex，见 design §3）+ `For(bool darkMode)` + `Get(LevelFlags)`；纯数据可单测。
- R2 浅色默认色阶为**重设计值**（非保留旧值）；`LogViewerSettings.Default*` 语义改为返回 `LogLevelDefaults.Light` 对应等级克隆；更新 `LogViewerSettingsTest`/`LogLevelSettingsTest` 到新值。
- R3 自定义标记：`LogLevelSettings` 新增 `bool IsCustom`（默认 false），持久化 `iscustom`；用户改色即置 `IsCustom = true`。
- R4 迁移：旧设置文件无 `iscustom` 时，按「存储值 ≠ 新浅色默认」推断自定义（保留既有用户定制）；等于浅色默认视为非自定义（可随主题翻转）。
- R5 渲染：`TextBrushes` 新增**静态可变**等级默认画刷组与 `UpdateLevelDefaults(bool darkMode)`；非自定义等级引用静态默认画刷，自定义等级引用用户色冻结画刷。
- R6 主题联动：`ThemeManager.Apply(Color, bool)` 末尾追加 `TextBrushes.UpdateLevelDefaults(darkMode)` + 触发 `ThemeManager.ThemeChanged(darkMode)`；设置页等级 ColorPicker 据此刷新。
- R7 设置页：`LogLevelSettingsViewModel` 暴露「有效色」（`IsCustom ? 存储色 : 当前主题默认色`），setter 写存储色 + 置 `IsCustom=true` + `SaveAsync()`。
- R8 红线：渲染热路径零新增每帧资源查找/分配；不改渲染算法/分页/虚拟化/命中测试/事件坐标；不改 `BusinessLogic/**`。

## 验收标准

- [ ] 主题切换（Light↔Dark，含 System 解析）时，非自定义等级渲染色自动在新浅/深色阶间翻转，无需重启。
- [ ] 用户自定义过某等级颜色后，主题切换不改该等级颜色（浅/深两态保留用户值）。
- [ ] 新浅色默认色阶逐值正确（`LogViewerSettingsTest`/`LogLevelSettingsTest`/`TextBrushesTest` 更新为新值，`LogLevelDefaultsTest` 覆盖 Light/Dark 全值）。
- [ ] 旧设置文件（无 `iscustom`）迁移正确：与新浅色默认不同的持久化色识别为自定义并保留。
- [ ] 设置页等级 ColorPicker 主题切换后显示当前主题默认色（非自定义）或用户色（自定义）；改色后持久化并置自定义。
- [ ] `TextBrushes` 等级默认画刷为静态可变、非自定义等级无冻结用户色拷贝；渲染热路径零新增每帧查找/分配。
- [ ] 新增/更新单测：`LogLevelDefaultsTest`（Light/Dark 值 + `For` + `Get`）、`LogLevelSettingsTest`（`IsCustom` roundtrip/迁移）、`LogViewerSettingsTest`（迁移推断 + 新默认）、`TextBrushesTest`（自定义 vs 默认画刷选择）。
- [ ] 解决方案构建 warning-free；`git diff --check` 通过。

## 非目标

- 不做「恢复默认」重置按钮（可选后续，见 Notes）。
- 不改强调色契约、不改 `SemanticPalette` 中性色值、不改其它 ViewModel（除 `LogLevelSettingsViewModel`）。

## Notes

- 非自定义等级「恢复跟随主题」的唯一入口是置 `IsCustom=false`；本任务不新增 UI 重置按钮，已知限制。
- TRACE/DEBUG 低等级为刻意弱化（可扫读性），非长读正文；INFO 及以上满足 WCAG AA（见 design §3 对比）。
