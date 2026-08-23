# 已批准决策记录（2026-08-23）

主负责人审核并批准了 prd.md 的 D1–D7 决策。此文件为权威批准记录，供实现/检查子代理对齐。

## 批准结论

| 决策 | 结论 |
|---|---|
| D1 | 两态 Light/Dark 切换，即时生效，持久化，默认浅色；不做跟随系统模式 |
| D2 | 批准 `TextCanvas`/`DataSourceCanvas`/`AbstractLogColumnPresenter`/`TextBrushes` 参与 + `LogEntryListView` 最小分隔条变更。仅主题感知画刷来源/更新 + 重绘所需最小失效；不改渲染算法/分页/虚拟化/命中测试/事件坐标/用户数据语义 |
| D3 | 保留用户配置的 ColorByLevel 前景/背景；仅默认（非 ColorByLevel）渲染颜色随主题切换 |
| D4 | 批准基线调色板：`Surface #1E1E1E`、`SurfaceMuted #2D2D30`、`TextPrimary #DCDCDC`、`TextSecondary #9E9E9E`、`Divider #3F3F46`、`TitleBar #252526`、`OverlayBackground #99000000`，及 design.md §4 记录的渲染器各值 |
| D5 | 批准对应用实际使用的 Metrolib 控件做定向中性色覆盖；仅颜色/资源，不改形状/布局/模板（最小 setter 之外）/状态机/动画/Fluent |
| D6 | 设置页「主题」分组，紧邻 ThemeColor；本任务无 Header 快捷开关 |
| D7 | 默认浅色；旧 XML 缺失 `darkmode` 属性回退浅色，无 schema 迁移 |

## 附加护栏

- 保留强调色 `ThemePalette`/`ThemeManager` 契约与实时强调色切换。
- 不改 `BusinessLogic/` 或无关 ViewModel。
- 仅新增深色模式字符串并重生成本地化；不触及其他字符串。
- 渲染热路径零每帧 `DynamicResource` 查找、零新增分配。
- 任何超出边界的计划项标记 out of scope，不得静默扩大。

## 涉及文件（D2 红线扩展批准范围）

- `src/Tailviewer/Ui/LogView/TextCanvas.cs` — 仅背景画刷来源（`Brushes.White` → `TextBrushes.CanvasBackgroundBrush`）+ 最小失效。
- `src/Tailviewer/Ui/LogView/DataSource/DataSourceCanvas.cs` — 仅背景画刷来源 + 默认前景画刷来源。
- `src/Tailviewer/Ui/LogView/AbstractLogColumnPresenter.cs` — 仅背景画刷来源。
- `src/Tailviewer/Ui/LogView/TextBrushes.cs` — 仅新增/改造可变画刷与 `UpdateNeutral`，不改渲染算法。
- `src/Tailviewer/Ui/LogView/LogEntryListView.cs` — 仅分隔条画刷来源（最小变更）。
