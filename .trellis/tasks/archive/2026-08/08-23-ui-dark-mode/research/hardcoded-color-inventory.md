# 研究：硬编码中性色清单（深色模式迁移目标）

调查日期：2026-08-23。范围 `src/Tailviewer`，仅列**中性色**（需随主题翻转）；severity 语义色（红/黄/绿）与强调色**不列**（保持不变）。

## XAML

| 位置 | 值 | 迁移目标 |
|---|---|---|
| `Themes/Constants.xaml:18` | `UnfocusedOverlay #30FFFFFF`（本地 Color） | 迁应用作用域 + `{DynamicResource}` |
| `Themes/Constants.xaml:19` | `TitleBar #EAEDF2`（本地 Color） | 迁应用作用域 + `{DynamicResource}` |
| `Themes/Constants.xaml:21` | `OverlayBackground #55000000`（本地 Color） | 迁应用作用域 + `{DynamicResource}` |
| `Themes/MetrolibTheme.xaml:10` | `DisabledForegroundBrush #B7B7B7` | `TextSecondary`/新 `Disabled` 语义键，`{DynamicResource}` |
| `Themes/MetrolibTheme.xaml:369` | FilterTextBox 水印 `Foreground="#717171"` | `TextSecondaryBrush` |
| `Themes/MetrolibTheme.xaml:452` | FilterTextBox 无效态 `Background="#E81123"` | 语义错误色（保持红色系，非中性，可不动） |
| `Ui/EmptyStateStyle.xaml:7` | `Foreground #A0A0A0` | `TextSecondaryBrush` |
| `Ui/ImageLabel.xaml:15,18` | `#E0E0E0`（浅）/`#B7B7B7`（深） | `TextSecondary`/`Disabled` 语义键 |
| `Ui/About/AboutFlyoutDataTemplate.xaml:8` | `Foreground #333333` | `TextPrimaryBrush` |
| `Ui/ActionCenter/BugTemplate.xaml:23` | `Fill #E81123` | 保持（错误红） |
| `Ui/ActionCenter/NotificationTemplate.xaml:36,40,44` | `#333333`/`#FFC300`/`#E81123` | 中性 `#333333`→`TextPrimaryBrush`；黄/红保持 |
| `Ui/ActionCenter/ExportTemplate.xaml:28` | `Fill #333333` | `TextPrimaryBrush` |
| `Ui/QuickFilter/FilterToggleButton.xaml:14` | `Fill #444444` | `TextPrimaryBrush` |
| `Ui/Settings/SettingsControl.xaml:14` | `Foreground #333333` | `TextPrimaryBrush` |
| `Ui/Settings/SettingsControl.xaml:14` | `Background White` | `SurfaceBrush` |
| `Ui/Settings/SettingsControl.xaml:54,377,401` | `#6B6B6B` | `TextSecondaryBrush` |

## C#

| 位置 | 值 | 迁移目标 |
|---|---|---|
| `Ui/LogView/AbstractLogColumnPresenter.cs:100` | `Brushes.White`（OnRender 背景） | `TextBrushes.CanvasBackgroundBrush`（D2） |
| `Ui/LogView/DataSource/DataSourceCanvas.cs:85` | `Brushes.White`（OnRender 背景） | `TextBrushes.CanvasBackgroundBrush`（D2） |
| `Ui/LogView/DataSource/DataSourceCanvas.cs:224` | `Brushes.Black`（默认前景） | `TextBrushes.DefaultForegroundBrush`（D2） |
| `Ui/LogView/TextCanvas.cs:285` | `Brushes.White`（OnRender 背景） | `TextBrushes.CanvasBackgroundBrush`（D2） |
| `Ui/LogView/TextBrushes.cs:31-43` | 选中/高亮/未聚焦选中/文件名前景硬编码 | 见 design.md §6.2 可变画刷化（D2） |
| `Ui/LogView/TextBrushes.cs:166` | `#E8F1F7`（交替行） | `TextBrushes.AlternatingBackgroundBrush`（D2） |
| `Ui/LogView/LogEntryListView.cs:194` | `Color.FromRgb(225,228,232)`（分隔条） | 主题可变画刷（D2） |
| `Ui/SidePanel/HyperlinkRun.cs:144` | `DisabledBrush #B7B7B7` | `TextSecondary` 语义键（C# 侧可选） |

## 结论

- 中性色迁移量大但机械；severity 色（`#E81123`/`#FFC300`）与强调色不动。
- C# 渲染器侧的迁移全部落入 D2 红线扩展范围，需主负责人批准；XAML 侧（设置页/About/ActionCenter/EmptyState/ImageLabel）属于"视图层迁移"，不触碰红线。
- `HyperlinkRun` 是 WPF `Run` 子类，若迁移需谨慎（冻结画刷/静态字段）；可延后或仅接受浅色禁用态，标记为已知限制。
