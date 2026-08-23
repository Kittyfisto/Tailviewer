# 实施计划：日志级别默认色阶（双套重设计 + 自定义保留）

> 每个阶段独立可验证/可回滚。遵守 `design.md` §1 红线。
> 状态：规划完成，待 `task.py start`；尚未实现。
> 前置依赖：`08-23-theme-mode-and-log-colors` 已 `start` 且 `ThemeManager.Apply(Color, ThemeMode)` + `CurrentDarkMode` 就位（`Apply(Color, bool)` 主体未被其改动）。
> 干净顺序契约：本计划在 `Apply(Color, bool)` 末尾追加钩子；无 `#if` 桩。

## 阶段 0：前置确认

- [ ] 依赖子任务完成 `ThemeMode` 三态；`Apply(Color, bool)` 主体仍为既有实现。

## 阶段 1：双套色阶 + `IsCustom`（回滚点 R1）

1. [ ] 新增 `src/Tailviewer/Settings/LogLevelDefaults.cs`（Light/Dark 双套精确值，见 design §3；`For(bool)`；`Get(LevelFlags)`）。
2. [ ] `LogLevelSettings`：新增 `IsCustom` + `Matches`；`Save`/`Restore`/`Clone` 覆盖 `iscustom`。
3. [ ] `LogViewerSettings`：`Default*` → `LogLevelDefaults.Light.Get(level).Clone()`；构造从 `LogLevelDefaults.Light` 克隆；`Restore` 迁移推断（无 `iscustom` 时 `IsCustom = !Matches(Light 默认)`）；新增 `ApplyThemeDefaults(bool darkMode)`。
4. [ ] 验证：`dotnet build`；`LogLevelDefaultsTest`（新增）/`LogLevelSettingsTest`/`LogViewerSettingsTest` 更新为新浅色默认并通过。

## 阶段 2：渲染画刷（回滚点 R1）

5. [ ] `TextBrushes`：新增静态可变 `LevelDefaultForegroundBrushes`/`LevelDefaultBackgroundBrushes`/`LevelDefaultAlternateBrushes`（静态构造以 `LogLevelDefaults.Light` 初始化）+ `UpdateLevelDefaults(bool darkMode)`。
6. [ ] 实例构造改为 `LevelFlags` 单循环：自定义 → 冻结用户色；非自定义/无 settings → 静态默认画刷引用。
7. [ ] 验证：`TextBrushesTest` 更新/通过；构建 warning-free。

## 阶段 3：主题联动 + 设置页刷新（回滚点 R2）

8. [ ] `ThemeManager.Apply(Color, bool darkMode)`：`UpdateNeutral` 之后追加 `TextBrushes.UpdateLevelDefaults(darkMode)` + 触发 `ThemeManager.ThemeChanged(darkMode)`（新增事件）。
9. [ ] `LogLevelSettingsViewModel`：构造注入 `LevelFlags`；有效色 getter + setter 置 `IsCustom=true` + `SaveAsync()`；订阅 `ThemeManager.ThemeChanged` 刷新 FG/BG。
10. [ ] `SettingsFlyoutViewModel`：构造各等级 ViewModel 传入对应 `LevelFlags`。
11. [ ] 验证：构建；设置页等级 ColorPicker 主题切换后显示正确；改色持久化并置自定义。

## 阶段 4：回归与收尾

12. [ ] `rg "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer` 为空。
13. [ ] 目标单测：`LogLevelDefaultsTest`/`LogLevelSettingsTest`/`LogViewerSettingsTest`/`TextBrushesTest`/`UISettingsTest` + UI STA（`LogEntryListViewTest`/`TextCanvasTest`）。
14. [ ] `git diff --check`；`git diff --name-only` 红线审计（§1 可改文件 + 依赖子任务文件）。

## 验证命令

```bash
dotnet build src/Tailviewer/Tailviewer.csproj
dotnet build src/Tailviewer.Tests/Tailviewer.Tests.csproj
rg -n "iscustom|IsCustom" src/Tailviewer
rg -n "StaticResource (Primary|Secondary|Surface|TextPrimary|TextSecondary|Divider|TitleBar|OverlayBackground)" src/Tailviewer
git diff --check
```

## Review Gates

- G1：新浅/深色阶逐值正确；迁移推断正确（自定义保留 / 默认可翻转）。
- G2：非自定义等级引用静态可变默认画刷；渲染热路径零新增分配。
- G3：主题切换翻转非自定义等级色、保留自定义色；设置页 ColorPicker 刷新正确。
- G4：构建 warning-free + 单测通过 + 红线审计通过。

## 回滚点

- R1：撤销 `LogLevelDefaults`/`IsCustom`/`TextBrushes` 等级画刷，恢复旧浅色默认与冻结用户色。
- R2：撤销 `ThemeChanged`/`LogLevelSettingsViewModel` 有效色解析与主题订阅。
- 越界即回滚：触碰 `BusinessLogic/**` 或渲染算法立即 revert。
