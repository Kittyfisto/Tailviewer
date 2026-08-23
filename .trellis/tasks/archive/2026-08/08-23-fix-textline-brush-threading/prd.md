# 修复 TextLineTest WPF 画刷跨线程 CI 失败

## Goal

修复 CI Build & Test 中 TextLineTest.TestBackgroundBrush1 和 TestForegroundBrush1 的跨线程 SolidColorBrush 访问失败，保持主题切换和日志渲染行为正确。

## Requirements

- 找出并修复 `TextBrushes` 共享可变 `SolidColorBrush` 导致的 WPF Freezable 跨线程访问异常。
- 主题或日志配色更新后，已有日志查看器必须获得新画刷并重绘；不能以 `[Apartment]`、串行化或排除测试作为唯一修复。
- 共享默认画刷应可安全跨线程读取，自定义级别颜色和现有渲染行为保持不变。

## Acceptance Criteria

- [x] `TextLineTest.TestForegroundBrush1` 与 `TestBackgroundBrush1` 在 NUnit 并行 worker 环境通过。
- [x] 画刷线程归属问题有回归验证；主题/配色切换后现有视图仍能刷新。
- [x] 相关 `TextBrushes`/日志视图测试通过，未引入禁用测试或无关 CI 改动。

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
