# 实施计划：重做深色主题、日志等级配色与日志界面布局（parent）

> parent 无直接 src 实现。本文件是集成顺序、门禁与评审清单。
> 状态：规划完成，待子任务 `task.py start`；尚未实现。

## 阶段 0：前置确认

- [x] 源需求 R1–R8 与子任务映射已写入本任务 `prd.md`。
- [x] 三子任务均已 `task.py create`（`--parent` 已挂接）。
- [ ] 子任务实现前，各子任务负责人确认其 `prd.md`/`design.md`/`implement.md` 与本文件一致。

## 阶段 1：按序启动子任务

1. [ ] `task.py start 08-23-search-property-layout`（独立，先做；可与阶段 2 并行）。
2. [ ] `task.py start 08-23-theme-mode-and-log-colors`（提供 `ThemeMode` + 解析信号 + 新默认强调色）。
3. [ ] `task.py start 08-23-log-level-default-colors`（依赖 2，后做；在 `Apply(Color,bool)` 内追加钩子）。

## 阶段 2：跨子任务集成评审（全部完成后）

4. [ ] `rg -n "DarkMode" src/Tailviewer` 无残留；`rg -n "#0047AB" src/Tailviewer` 无遗留默认强调色。
5. [ ] `rg "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer` 为空。
6. [ ] 确认 `ThemeManager.Apply(Color, ThemeMode)` 单入口：中性色 + 强调色 + 渲染器 + 等级默认色一致翻转；`Apply(Color,bool)` 内无 `#if` 桩。
7. [ ] 目检：三态切换、System 跟随、等级默认色翻转、自定义色保留、新强调色对比、属性页整页前景、搜索框右置且旧搜索行已删除。
8. [ ] 目标 UI STA 单测通过；`dotnet build` warning-free；`git diff --check`。

## Review Gates

- G1（子任务 4 后）：属性页/搜索框仅 XAML + `LogViewerControl`/`IFindAllViewModel` 变更，无 `ThemeManager` 触碰。
- G2（子任务 2 后）：`ResolveDarkMode` 可单测（注入 `ISystemThemeProvider`）；系统变化仅 System 模式生效；迁移正确；新强调色默认 + 对比达标。
- G3（子任务 3 后）：等级默认画刷静态可变；自定义色两态保留；`LogViewerSettingsTest`/`LogLevelSettingsTest` 更新为新浅色默认；渲染热路径零新增分配。
- G4（集成后）：阶段 2 全部通过。

## 回滚点

- R1：撤销子任务 4（搜索框/属性页 XAML 变更）。
- R2：撤销子任务 3（等级色阶 + `IsCustom` + `TextBrushes` 等级画刷 + 钩子）。
- R3：撤销子任务 2（`ThemeMode` 三态 + 系统检测 + `ThemeManager` 签名 + 新强调色默认），回退两态 `DarkMode` + `#0047AB`。
- 越界即回滚：任一子任务触碰其 `prd.md` 红线即 revert 到最近回滚点并回到 Plan。
