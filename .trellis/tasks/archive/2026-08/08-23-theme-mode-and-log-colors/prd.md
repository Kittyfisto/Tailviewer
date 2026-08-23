# 主题模式与日志配色（Light / Dark / System 三态 + 新默认强调色）

## Goal

把已归档 08-23-ui-dark-mode 的**两态 Light/Dark**升级为 **Light/Dark/System 三态**：设置页三选、即时生效、持久化、System 跟随系统主题并实时响应。同时**重设计默认强调色**（弃用遗留 `#0047AB`，改用专业可访问的新默认，保留用户自定义 ThemeColor）。保留既有深色语义资源架构与强调色实时切换能力，不改渲染算法/虚拟化/命中测试。

> 依赖：本子任务先于 `08-23-log-level-default-colors`（为其提供 `ThemeMode` + 有效 darkMode 解析信号）。
> 干净顺序契约：本子任务**不引用** `TextBrushes.UpdateLevelDefaults`、**不引入** `#if` 桩或缺失方法；等级默认色钩子完全由依赖子任务在 `ThemeManager.Apply(Color, bool)` 末尾追加。

## 已确认事实（仓库证据）

| # | 事实 | 证据 |
|---|---|---|
| F1 | 两态现状：`UISettings.DarkMode`(bool) + `ThemeManager.Apply(Color, bool)` + `SemanticPalette.Compute(bool)` + `TextBrushes.UpdateNeutral(bool)` | `UISettings.cs`、`ThemeManager.cs`、`SemanticPalette.cs`、`TextBrushes.cs` |
| F2 | 设置页主题分组现为 `CheckBox IsChecked="{Binding DarkMode}"` + `ThemeColor` ColorPicker | `SettingsControl.xaml:314-341` |
| F3 | `SettingsFlyoutViewModel.DarkMode`/`ThemeColor` setter 已示范「写设置 + `SaveAsync()` + `Dispatcher.BeginInvoke(ThemeManager.Apply(...))`」 | `SettingsFlyoutViewModel.cs:248-297` |
| F4 | 启动应用点 `ThemeManager.Apply(settings.Ui.ThemeColor, settings.Ui.DarkMode)`；`App` 构造用 `SemanticPalette.Compute(false)` 占位 | `App.cs:44-52, 252` |
| F5 | `UISettings.Save` 写 `darkmode` bool；`Restore` 读 `darkmode` | `UISettings.cs` |
| F6 | 默认强调色 `UISettings.DefaultThemeColor = #0047AB`；`UISettingsTest.TestDefaultThemeColorIs0047AB` 硬编码断言该值 | `UISettings.cs`、`UISettingsTest.cs` |

## 需求

- R1 新增 `ThemeMode { Light, Dark, System }`（`Tailviewer.Settings`）；`UISettings.DarkMode`（bool）替换为 `UISettings.ThemeMode`（默认 `Light`）。
- R2 持久化与迁移：`Save` 写 `thememode`；`Restore` 先读 `thememode`（`Enum.TryParse`，非法回退 Light），缺失时回读旧 `darkmode`（`true`→Dark，否则 Light），再缺省回退 Light。`Clone` 复制。
- R3 系统检测（可测试）：新增 `ISystemThemeProvider { bool IsDark(); event Action Changed; }`；默认 `WindowsSystemThemeProvider` 读 `HKCU\...\Themes\Personalize` 的 `AppsUseLightTheme` DWORD（0=深色/1=浅色，缺失=浅色）。`ResolveDarkMode(ThemeMode)` 纯函数：`Dark→true`、`Light→false`、`System→provider.IsDark()`。
- R4 系统跟随：`WindowsSystemThemeProvider` 订阅 `SystemEvents.UserPreferenceChanged`，仅在 `ThemeMode == System` 且解析出的有效 darkMode 实际变化时，经 Dispatcher 重新 `ThemeManager.Apply`。非 System 不响应。
- R5 主题管线：`ThemeManager.Apply(Color primary, ThemeMode mode)` 解析有效 darkMode 后复用既有 `Apply(Color, bool)`；`Apply(Color, bool)` 主体**不改**。更新 `App.cs:252` 与 `SettingsFlyoutViewModel` 调用点为三态签名。
- R6 设置入口：主题分组三选（ComboBox 或三 RadioButton），绑定 `ThemeMode`，替换 `DarkMode` 复选框。新增本地化键（Light/Dark/System），重跑 `python tools/generate_localization.py`。
- R7 **默认强调色（重设计）**：`UISettings.DefaultThemeColor` 由 `#0047AB` 改为 **`#0F62FE`**（IBM Carbon Blue 60）；`ThemePalette`/`PrimaryForeground=White` 契约不变；记录浅/深对比（见 design §3）；更新 `UISettingsTest.TestDefaultThemeColorIs0047AB` 为新值断言；用户自定义 ThemeColor 持久化不受影响（旧设置缺失 `themecolor` 时回退新默认）。
- R8 架构红线：`Color` 键只在应用作用域发布；Brush 用 `{DynamicResource}`；强调色实时切换不变；渲染热路径零每帧资源查找/分配。

## 验收标准

- [ ] 设置页主题分组三选（Light/Dark/System）；选择即全应用即时切换，无需重启。
- [ ] System 跟随系统主题；系统主题变化实时跟随；切回 Light/Dark 后不再跟随。
- [ ] 持久化：重启保持所选模式；旧 `darkmode` bool 迁移正确；无属性回退 Light。
- [ ] 默认强调色为 `#0F62FE`；白字对比 ≈5.0:1；强调色三模式实时切换；用户自定义 ThemeColor 保留。
- [ ] `UISettingsTest`：`ThemeMode` 默认/Clone/Roundtrip/缺省/旧 bool 迁移/非法回退；`TestDefaultThemeColor` 断言新值 `#0F62FE`。
- [ ] `ResolveDarkMode` 用注入的 `ISystemThemeProvider` 可单测（无全局注册表改动）；`WindowsSystemThemeProvider` 单独用抽象隔离测试。
- [ ] 解决方案构建 warning-free；`git diff --check` 通过；本地化三件套重生成且无手改。

## 非目标

- 不做日志等级默认色阶（子任务 `08-23-log-level-default-colors`）。
- 不改 `BusinessLogic/**`、不改无关 ViewModel、不改渲染算法/分页/虚拟化/命中测试/事件坐标。
- 不引入自动定时切换、不做浅色强调色对比度增强。

## Notes

- `SystemEvents.UserPreferenceChanged` 在非 UI 线程触发，须 Marshal 到 `Application.Current.Dispatcher`；`Application.Current` 为 null（测试/设计时）时 no-op（见 design §5 生命周期/线程模型）。
- `AppsUseLightTheme` 为 Windows 10/11 键；缺失（旧系统）按浅色，标记为已知限制。
