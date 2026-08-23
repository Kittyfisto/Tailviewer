# 设计：日志级别默认色阶（双套重设计 + 自定义保留）

## 1. 边界

### 可改
- `src/Tailviewer/Settings/LogLevelSettings.cs`（`IsCustom`）、`Settings/LogViewerSettings.cs`（默认色阶来源 + 迁移）。
- 新增 `src/Tailviewer/Settings/LogLevelDefaults.cs`（Light/Dark 双套 + `For(bool)` + `Get(LevelFlags)`）。
- `src/Tailviewer/Ui/LogView/TextBrushes.cs`（等级默认静态可变画刷 + `UpdateLevelDefaults`）。
- `src/Tailviewer/Ui/ThemeManager.cs`（`Apply(Color,bool)` 末尾追加 `UpdateLevelDefaults` + 触发 `ThemeChanged`；三态签名由依赖子任务已提供）。
- `src/Tailviewer/Ui/Settings/LogLevelSettingsViewModel.cs`（有效色解析 + 置自定义 + 主题刷新）。

### 不可改（红线）
- `src/Tailviewer/BusinessLogic/**`。
- `ThemePalette`/`SemanticPalette` 值；`TextBrushes.UpdateTheme`/`UpdateNeutral` 逻辑。
- 渲染算法/分页/虚拟化/命中测试/事件坐标。

## 2. 数据模型

### 2.1 `LogLevelDefaults`（新增，纯数据）
```csharp
public sealed class LogLevelDefaults
{
    public LogLevelSettings Other, Trace, Debug, Info, Warning, Error, Fatal;
    public static LogLevelDefaults Light { get; }   // §3 新浅色值
    public static LogLevelDefaults Dark { get; }    // §3 新深色值
    public static LogLevelDefaults For(bool darkMode) => darkMode ? Dark : Light;
    public LogLevelSettings Get(LevelFlags level);
}
```

### 2.2 `LogLevelSettings`（新增 `IsCustom`）
- `public bool IsCustom { get; set; }`（默认 false）。
- `Save`：写 `iscustom`；`Restore`：读 `iscustom`（无此属性时不在本层推断，由 `LogViewerSettings.Restore` 迁移）。
- `Clone` 复制 `IsCustom`；新增 `Matches(Color fg, Color bg)` 供迁移比较。

### 2.3 `LogViewerSettings`
- `Default*` 静态属性 → `LogLevelDefaults.Light.Get(level).Clone()`（保持「每次访问返回新实例」语义，值为新浅色）。
- 构造：从 `LogLevelDefaults.Light` 克隆（`IsCustom=false`）。
- `Restore`：逐等级恢复后，若未显式写 `iscustom`（旧文件），迁移推断 `IsCustom = !_x.Matches(LogLevelDefaults.Light.Get(level))`。
- 新增 `ApplyThemeDefaults(bool darkMode)`：对 `IsCustom == false` 的等级，把 FG/BG 设为 `LogLevelDefaults.For(darkMode).Get(level)` 克隆（保持 `IsCustom=false`）。

## 3. 双套专业默认色阶（精确值 + 对比）

### 浅色（Light）—— 白/中性底、灰紫/灰蓝低等级、克制的琥珀/橙、淡红/深红、红底白字 FATAL

| 等级 | FG | BG | 说明 |
|---|---|---|---|
| TRACE | `#9AA5B1` | Transparent | 灰蓝最浅（最低） |
| DEBUG | `#6B7A8F` | Transparent | 灰蓝 |
| OTHER | `#7C6E9E` | Transparent | 灰紫 |
| INFO | `#333333` | Transparent | 中性正文（默认） |
| WARNING | `#8A4D00` | `#FFF3D6` | 深琥珀字 / 淡琥珀底 ≈6.0:1 (AA) |
| ERROR | `#B3261E` | `#FDECEA` | 深红字 / 淡红底 ≈5.6:1 (AA) |
| FATAL | `#FFFFFF` | `#C5221F` | 白字 / 红底 ≈5.8:1 (AA) |

### 深色（Dark）—— 无霓虹、低等级无背景、暗金+淡黄 WARNING、暗红+珊瑚 ERROR、深红+白 FATAL

| 等级 | FG | BG | 说明 |
|---|---|---|---|
| TRACE | `#5B6472` | Transparent | 灰（最低） |
| DEBUG | `#7E8C9E` | Transparent | 灰蓝 |
| OTHER | `#8E7FAE` | Transparent | 灰紫 |
| INFO | `#DCDCDC` | Transparent | 中性浅灰正文（=TextPrimary） |
| WARNING | `#F5D77E` | `#5A4700` | 淡黄字 / 暗金底 ≈6.3:1 (AA) |
| ERROR | `#FFB4AB` | `#6B1F1F` | 珊瑚字 / 暗红底 ≈6.7:1 (AA) |
| FATAL | `#FFFFFF` | `#9E1B1B` | 白字 / 深红底 ≈8.0:1 (AA/AAA) |

- 区分度：`TRACE < DEBUG < OTHER < INFO < WARNING < ERROR < FATAL` 由灰度/色相阶梯保证（TRACE→DEBUG→OTHER→INFO 前景由浅灰到深灰/正文递增；WARNING→ERROR→FATAL 背景由琥珀→红→深红递进，前景由深字→珊瑚→白字递进）。
- INFO 保持中性、无背景（浅色 `#333333`、深色 `#DCDCDC`）。
- 对比度按 WCAG 相对亮度近似计算；WARNING/ERROR/FATAL ≥4.5:1（AA）。TRACE/DEBUG 为刻意弱化（可扫读性），不做正文对比要求。

## 4. 渲染机制（`TextBrushes`）

- 新增静态可变画刷组（`SolidColorBrush`，不 Freeze）：
  - `LevelDefaultForegroundBrushes[LevelFlags]`、`LevelDefaultBackgroundBrushes[LevelFlags]`、`LevelDefaultAlternateBrushes[LevelFlags]`。
- 静态构造：以 `LogLevelDefaults.Light` 初始化（保证首次 `UpdateLevelDefaults` 之前已正确）。
- `UpdateLevelDefaults(bool darkMode)`：对每个 `LevelFlags`，取 `LogLevelDefaults.For(darkMode).Get(level)`，更新静态画刷 `.Color`；`LevelDefaultAlternateBrushes[level]`：透明背景 → 指向静态 `AlternatingBackgroundBrush`；否则 → 同等级默认背景画刷。
- 实例构造改为 `LevelFlags` 单循环：
  - `settings != null && settings.<level>.IsCustom` → `CreateBrush(存储 FG/BG)` + `GetAlternatingBrush(存储 BG)`（用户色，冻结）。
  - 否则（非自定义/无 settings）→ 引用静态 `LevelDefault*Brushes[level]`（主题可变）。
- 结果：主题切换只改静态画刷 `.Color`，实例引用随之变化；渲染热路径零新增分配/查找。

## 5. 主题联动与设置页刷新

- `ThemeManager.Apply(Color, bool darkMode)`（依赖子任务已就位，本子任务在此追加）：
  1. `TextBrushes.UpdateNeutral(darkMode)`（既有）。
  2. `TextBrushes.UpdateLevelDefaults(darkMode)`（新增）。
  3. 触发 `ThemeManager.ThemeChanged?.Invoke(darkMode)`（新增事件）。
- `LogLevelSettingsViewModel`：
  - 构造注入 `LevelFlags level` + `LogLevelSettings` + `IApplicationSettings`。
  - `ForegroundColor get => IsCustom ? 存储FG : LogLevelDefaults.For(ThemeManager.CurrentDarkMode).Get(level).ForegroundColor;`（BackgroundColor 同理）。
  - setter：写存储色 + `IsCustom = true` + `EmitPropertyChanged()` + `SaveAsync()`。
  - 订阅 `ThemeManager.ThemeChanged`：`EmitPropertyChanged(nameof(ForegroundColor))`/`nameof(BackgroundColor)`。
  - `SettingsFlyoutViewModel` 构造各等级 ViewModel 时传入对应 `LevelFlags`。

## 6. 兼容性与回滚

- 旧文件无 `iscustom`：迁移推断保留既有用户定制。
- 回滚点：R1 撤销 `LogLevelDefaults`/`IsCustom`/`TextBrushes` 等级画刷/`ThemeChanged`/`LogLevelSettingsViewModel` 有效色解析，恢复旧浅色默认 + 冻结用户色。

## 7. 测试

- `LogLevelDefaultsTest`：Light/Dark 全 7 等级 FG/BG 逐项断言（§3 精确值）；`For(bool)` 选择；`Get(LevelFlags)` 索引。
- `LogLevelSettingsTest`：`IsCustom` Clone/Roundtrip/缺省；`Matches`。
- `LogViewerSettingsTest`：`TestConstruction`/`TestRestoreFromEmpty` 更新为新浅色默认；旧文件迁移推断（自定义 vs 默认）；`ApplyThemeDefaults` 只改非自定义等级。
- `TextBrushesTest`：自定义等级 → 冻结用户色；非自定义等级 → 引用静态默认画刷；`UpdateLevelDefaults` 翻转静态画刷色（新浅/深值）。
