# 实施计划：主题模式三态 + 新默认强调色

> 每个阶段独立可验证/可回滚。遵守 `design.md` §1 红线。
> 状态：规划完成，待 `task.py start`；尚未实现。
> 前置依赖：无（先于 `08-23-log-level-default-colors`）。
> 干净顺序契约：本计划不引用 `TextBrushes.UpdateLevelDefaults`、无 `#if` 桩；`Apply(Color, bool)` 主体不改。

## 阶段 1：枚举、provider 与解析（回滚点 R1）

1. [ ] 新增 `src/Tailviewer/Settings/ThemeMode.cs`：`enum ThemeMode { Light, Dark, System }`。
2. [ ] 新增 `src/Tailviewer/Ui/SystemThemeDetector.cs`：`ISystemThemeProvider` + `WindowsSystemThemeProvider`（`IsDark()` 读 `AppsUseLightTheme`；`Changed` 经 `SystemEvents.UserPreferenceChanged` + Dispatcher marshal + `IDisposable`）。
3. [ ] `ThemeManager`：新增 `SystemThemeProvider`（可注入静态属性，默认 Windows 实现）、`ResolveDarkMode(ThemeMode)`、`Apply(Color, ThemeMode)`（委托既有 `Apply(Color, bool)`）、`CurrentThemeMode` 缓存；**不改** `Apply(Color, bool)` 主体。
4. [ ] 验证：`dotnet build src/Tailviewer/Tailviewer.csproj` warning-free；新增 `ThemeManagerTest`/`SystemThemeDetectorTest` 通过（fake provider，无注册表改动）。

## 阶段 2：设置模型、持久化与迁移（回滚点 R2）

5. [ ] `UISettings`：`DarkMode` → `ThemeMode`（默认 Light）；`Save`/`Restore`（含旧 `darkmode` 迁移）/`Clone`。
6. [ ] `App.cs:252`：`ThemeManager.Apply(settings.Ui.ThemeColor, settings.Ui.ThemeMode)`。
7. [ ] 更新 `UISettingsTest`：`TestConstruction`/`TestClone`/`TestRoundtrip`/`TestRestoreFromEmpty`/`TestRestoreDarkMode`/`TestRestoreFromInvalidDarkMode` → `ThemeMode` 语义 + 旧 bool 迁移 + 非法回退。
8. [ ] 验证：`dotnet build`；`UISettingsTest` 通过；`rg "DarkMode" src/Tailviewer` 归零（或仅历史注释）。

## 阶段 3：新默认强调色（回滚点 R1/R2）

9. [ ] `UISettings.DefaultThemeColor`：`#0047AB` → `#0F62FE`（`Color.FromRgb(0x0F, 0x62, 0xFE)`）。
10. [ ] 更新 `UISettingsTest.TestDefaultThemeColorIs0047AB` → `TestDefaultThemeColorIs0F62FE`（断言新值）。
11. [ ] `rg -n "#0047AB|0x47" src/Tailviewer`：确认仅 `ThemePaletteTest` 任意测试输入残留（可保留），无生产代码遗留。
12. [ ] 验证：构建；`UISettingsTest`/`ThemePaletteTest`/`TextBrushesTest` 通过（`TextBrushes` 默认画刷跟随 `DefaultThemeColor` 自动更新）。

## 阶段 4：设置入口 + 系统跟随（回滚点 R3）

13. [ ] `SettingsFlyoutViewModel`：`ThemeMode` 属性（写设置 + `SaveAsync()` + Dispatcher 延迟 Apply）+ `ThemeModes` 选项集合；删除 `DarkMode` 属性。
14. [ ] `SettingsControl.xaml`：主题分组 `CheckBox DarkMode` → 三选控件，绑定 `ThemeMode`。
15. [ ] 系统跟随：`App.StartApplication` 注册 `WindowsSystemThemeProvider`（`SystemEvents.UserPreferenceChanged`）→ 仅 `CurrentThemeMode == System` 且有效 darkMode 变化时重新 Apply；退出时反注册。
16. [ ] `tools/generate_localization.py`：新增 `ThemeModeLight`/`ThemeModeDark`/`ThemeModeSystem`（英/zh-CN），重跑生成三件套；删除无引用的 `DarkMode` 键。
17. [ ] 验证：`python tools/generate_localization.py`；构建 warning-free；`git status --porcelain` 确认三件套重生成。

## 阶段 5：回归与收尾

18. [ ] `rg "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer` 为空。
19. [ ] 目标 UI STA 单测：`MainWindowTest`/`SettingsControlTest`/`UISettingsTest` + 新增解析/检测单测。
20. [ ] `git diff --check`；`git diff --name-only` 红线审计（仅 §1 可改文件）。

## 验证命令

```bash
dotnet build src/Tailviewer/Tailviewer.csproj
dotnet build src/Tailviewer.Tests/Tailviewer.Tests.csproj
python tools/generate_localization.py
rg -n "DarkMode" src/Tailviewer
rg -n "#0047AB|0x47" src/Tailviewer
rg -n "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer
git diff --check
```

## Review Gates

- G1：`ResolveDarkMode` 经注入 `ISystemThemeProvider` 可单测；无全局注册表改动。
- G2：三态持久化 roundtrip + 旧 bool 迁移；`DarkMode` 残留归零；新强调色 `#0F62FE` 断言就位。
- G3：System 跟随系统主题、非 System 不跟随；本地化三件套重生成。
- G4：强调色三态实时切换回归；构建 warning-free。

## 回滚点

- R1：撤销枚举/检测 provider/`ThemeManager` 解析与注入，恢复两态 `Apply(Color, bool)` + `#0047AB`。
- R2：撤销 `UISettings` 字段与持久化迁移。
- R3：撤销设置入口三选与系统跟随、恢复 `CheckBox DarkMode`。
- 越界即回滚：触碰 `BusinessLogic/**` 或渲染器文件立即 revert。
