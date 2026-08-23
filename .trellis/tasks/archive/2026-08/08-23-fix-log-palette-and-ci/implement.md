# 实施计划

1. 在 `UISettings`/设置枚举中加入独立的日志配色状态、克隆/保存/恢复/兼容默认；补充 Light/Dark 选项的 ViewModel 与 Settings XAML 显示。
2. 让 `ThemeManager`、`LogLevelSettingsViewModel`、`App` 和 `TextBrushes` 以显式日志配色驱动非自定义级别，保留应用主题模式的独立语义。
3. 将共享画刷改为可跨线程读取的冻结引用并在主题/配色变化时替换；通过现有日志视图入口重建/刷新画刷，保留自定义画刷。
4. 添加/更新设置、配色、画刷和日志视图回归测试；必要时只更新本任务需要的本地化生成源和产物。
5. 运行聚焦 NUnit、Debug 构建、`git diff --check`，再运行 Trellis 质量检查和尽可能接近 CI 的完整测试集合。

## 不做

- 不修改 search box、页面布局或其他前序任务内容。
- 不修改 CI 来过滤失败测试，不做大范围画刷/渲染重构。
