# 设计

## 已批准方向

OpenCode 只读规划确认了两个根因：设置模型目前只有 `ThemeMode`，而日志默认色由应用的有效深色状态隐式驱动；`TextBrushes` 则把非冻结的静态 `SolidColorBrush` 交给不同 NUnit worker/渲染线程共享。

本任务采用一个独立的 `LogLevelPalette`（Light/Dark）持久化字段。`ThemeMode` 继续只负责应用中性/强调色和 `System` 跟随逻辑；日志级别默认色由显式日志配色决定。缺失新字段的旧配置按 Light 默认，已明确选择 Dark 的旧配置迁移为 Dark，以减少既有深色用户的视觉回退；之后两者保持独立。

画刷采用“冻结后替换”模型：共享默认画刷全部使用冻结 `Freezable`，更新时发布新引用而非跨线程修改 `.Color`；日志视图通过已有设置/重绘入口重建 `TextBrushes` 并使画布失效。自定义颜色继续使用实例级冻结画刷。

## 关键约束

- 3 参数主题应用入口应同时接收应用主题模式与日志配色；已有便捷入口需保持兼容并明确其默认语义。
- 不通过 NUnit apartment 属性、跳过测试或修改 CI 测试集合规避问题。
- 任何新的设置文本遵循 `tools/generate_localization.py` 的单一来源；避免生成与本任务无关的本地化漂移。
- 修改 XAML 时保持 `DynamicResource` 主题绑定、现有 MVVM 和 .NET Framework 4.8 约束。

## 验收重点

父 Agent 将分别审阅：设置中是否真的有两个可见选择器、调色板是否独立持久化、旧配置行为、冻结画刷与日志视图刷新链路，以及完整 `Tailviewer.Tests.dll` 的 CI 等价结果。
