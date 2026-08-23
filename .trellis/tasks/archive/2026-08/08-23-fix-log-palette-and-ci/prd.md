# 修正日志配色选择与 CI 画刷稳定性

## Goal

在设置菜单中提供可见的浅色与深色日志配色选项，并修复 Build & Test 中 TextLineTest 的 WPF 画刷跨线程失败；完成验证后归档并推送。

## Requirements

- 在设置菜单中同时提供两类互相独立的选择：应用主题模式 `浅色 / 深色 / 跟随系统`，以及日志级别默认配色 `浅色 / 深色`。用户必须能在界面上区分这两个选择。
- 日志配色方案必须持久化；新配置默认使用浅色方案，已有深色主题配置在没有新字段时应尽量保持原有深色日志观感。无效值必须安全回退到浅色方案。
- 非自定义日志级别使用所选方案的 `LogLevelDefaults.Light` 或 `LogLevelDefaults.Dark`；自定义级别颜色不因应用主题或日志配色切换而被覆盖。
- 切换日志配色后，设置页预览/颜色编辑器和已经打开的日志查看器都要使用新方案，不需要重启应用。
- 修复 GitHub Actions `Build & Test` 中 `TextLineTest.TestForegroundBrush1` 与 `TestBackgroundBrush1` 的 WPF `SolidColorBrush` 跨线程异常。修复必须解决共享画刷的线程归属问题，不得跳过、禁用或弱化测试。
- 保持 .NET Framework 4.8、现有 MVVM/XAML、本地化和主题切换约定，不引入新的 UI 框架或无关重构。

## Acceptance Criteria

- [x] 设置菜单能同时看到应用主题模式和日志配色方案两个控件；前者有三个选项，后者恰好有浅色和深色两个选项。
- [x] 两套日志默认色值与 `LogLevelDefaults.Light/Dark` 一致，选择日志配色不改变应用中性主题，切换应用主题也不会隐式覆盖用户选择的日志配色。
- [x] 新字段可保存、恢复、克隆；缺失字段和非法值的兼容行为有测试覆盖；自定义日志级别仍保持原值。
- [x] 主题/日志配色切换后，现有日志视图可继续渲染并显示最新画刷，相关画刷可安全地从其他 NUnit worker 线程读取。
- [x] `Tailviewer.Tests.dll` 中原先失败的两个 `TextLineTest` 已通过，未通过项未通过排除测试来隐藏；相关项目 Debug 构建无错误。
- [x] `git diff --check` 与 Trellis 质量检查通过，工作区只包含本任务范围内的变更。

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
