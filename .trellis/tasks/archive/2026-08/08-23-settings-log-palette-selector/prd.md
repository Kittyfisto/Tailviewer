# 设置菜单显式提供浅色与深色日志配色

## Goal

新增持久化的日志配色方案选择，并让日志级别默认颜色根据所选浅色/深色方案生效；设置页面需要明确展示两个方案。

## Requirements

- 新增独立的日志默认配色选择，选项只有浅色和深色；不复用或改变已有应用主题模式的三选项语义。
- 将选择保存到 UI 设置并在启动时恢复；缺失字段默认浅色，旧的深色主题配置尽量迁移为深色日志配色，非法值回退浅色。
- 非自定义级别从所选方案读取默认前景/背景色；用户自定义级别保持自定义值。
- 设置选择生效时更新日志查看器和设置页的有效颜色，并覆盖必要的本地化文本与测试。

## Acceptance Criteria

- [x] Settings XAML 明确显示“应用主题模式”和“日志配色方案”两个独立选择器，日志配色方案恰好含浅色/深色。
- [x] Light/Dark 选择分别对应 `LogLevelDefaults.Light/Dark`，且与应用 `ThemeMode` 解耦。
- [x] UISettings 的构造、Clone、Save、Restore、旧配置兼容及 VM setter 持久化均有测试或可审查证据。
- [x] 自定义颜色不被方案切换覆盖，相关 UI/日志视图能实时使用新方案。

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
