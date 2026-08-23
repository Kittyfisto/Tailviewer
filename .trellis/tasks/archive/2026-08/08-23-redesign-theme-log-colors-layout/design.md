# 设计：重做深色主题、日志等级配色与日志界面布局（parent 集成视角）

> parent 无直接 src 实现。本文档锁定三个子任务共享的契约与边界。

## 1. 共享契约（子任务间接口）

### 1.1 主题模式解析（子任务 2 产出，子任务 3 消费）

```csharp
// Tailviewer.Settings
public enum ThemeMode { Light, Dark, System }

// Tailviewer.Ui
public interface ISystemThemeProvider
{
    bool IsDark();
    event Action Changed;      // 已 marshal 到 UI Dispatcher
}

public static class ThemeManager
{
    public static ISystemThemeProvider SystemThemeProvider { get; set; } // 测试注入点，默认 Windows 实现
    public static bool ResolveDarkMode(ThemeMode mode);
    public static void Apply(Color primary, ThemeMode mode);   // 解析后委托 Apply(Color, bool)
    public static void Apply(Color primary, bool darkMode);    // 既有下游；子任务 2 不改其主体
}
```

- `ThemeManager.Apply(Color, bool)` 的既有下游（中性色发布 + `TextBrushes.UpdateNeutral`）在子任务 2 中**保持不变**。
- 子任务 2 **不引用** `TextBrushes.UpdateLevelDefaults`；等级默认色钩子完全由子任务 3 实现（见 1.2）。无 `#if` 桩、无缺失方法。

### 1.2 等级默认色钩子（子任务 3 产出，子任务 2 无依赖）

```csharp
// TextBrushes（子任务 3 新增）
public static void UpdateLevelDefaults(bool darkMode);

// ThemeManager（子任务 3 在 Apply(Color, bool) 末尾追加）
TextBrushes.UpdateLevelDefaults(darkMode);
ThemeChanged?.Invoke(darkMode);   // 子任务 3 新增事件，供设置页等级 ColorPicker 刷新
```

- 子任务 3 消费「有效 darkMode 已解析」这一事实；`Apply(Color, ThemeMode)` → `Apply(Color, bool)` →（子任务 3 落地后）等级默认色钩子。
- 干净顺序契约：子任务 2 落地后 `Apply(Color, bool)` 主体不变、可独立编译；子任务 3 落地后在 `Apply(Color, bool)` 末尾追加钩子，亦可独立编译。

### 1.3 默认强调色（子任务 2 产出）

- `UISettings.DefaultThemeColor`：`#0047AB` → **`#0F62FE`**（IBM Carbon Blue 60）。`ThemePalette`/`PrimaryForeground=White` 契约不变。
- 对比（见子任务 2 `design.md` §3）：白字 on `#0F62FE` ≈ 5.0:1（AA）。浅/深两态下强调色实时切换不变；用户自定义 ThemeColor 持久化不变。

## 2. 子任务边界（避免冲突）

| 文件 | 子任务 2（theme-mode） | 子任务 3（log-level-defaults） | 子任务 4（search-property-layout） |
|---|---|---|---|
| `ThemeManager.cs` | 加 `ResolveDarkMode`/`Apply(Color, ThemeMode)`/`SystemThemeProvider`；**不改** `Apply(Color,bool)` 主体 | 在 `Apply(Color,bool)` 末尾追加 `UpdateLevelDefaults` + `ThemeChanged` | 不碰 |
| `UISettings.cs` | `DarkMode`→`ThemeMode` + 迁移 + `DefaultThemeColor` 新值 | 不碰 | 不碰 |
| `TextBrushes.cs` | 不碰 | 新增等级默认画刷 + `UpdateLevelDefaults` | 不碰 |
| `LogLevelSettings.cs`/`LogViewerSettings.cs` | 不碰 | `IsCustom` + 双套新色阶 + 迁移 | 不碰 |
| `SettingsFlyoutViewModel.cs` | `ThemeMode` 属性 + 选项集合 | 不碰（依赖 `SaveAsync` 模式） | 不碰 |
| `SettingsControl.xaml` | 主题分组三选 | 不碰 | 不碰 |
| `LogViewerControl.xaml` | 不碰 | 不碰 | 搜索框移入 Row 0、删 Row 1、统一视觉 |
| `PropertiesSidePanelDataTemplate.xaml` | 不碰 | 不碰 | 整页前景 |

> 子任务 2 与 3 都改 `ThemeManager.cs`：按 §1.1/1.2 顺序实现（先 2 后 3）。

## 3. 数据流

```
UISettings.ThemeMode (Light/Dark/System)   UISettings.DefaultThemeColor = #0F62FE
        │  ThemeManager.Apply(ThemeColor, ThemeMode)
        ├─> ResolveDarkMode(mode) ── System 时经 SystemThemeProvider.IsDark()
        ├─> 发布 Primary*/Secondary*（强调色，新默认派生）
        ├─> SemanticPalette.Compute(darkMode) 发布中性 Color 键（既有）
        ├─> TextBrushes.UpdateTheme(primary)（既有）
        ├─> TextBrushes.UpdateNeutral(darkMode)（既有）
        └─> TextBrushes.UpdateLevelDefaults(darkMode)（子任务 3）→ 静态等级默认画刷翻转
                    └─> ThemeChanged(darkMode) → 设置页等级 ColorPicker 刷新
```

## 4. 兼容性与回滚形状

- `net48` + `UseWPF` + `TreatWarningsAsErrors` 零警告。
- `thememode` 缺省迁移自旧 `darkmode` bool；`iscustom` 缺省按「存储值 ≠ 浅色默认」推断。
- 回滚点（逐子任务）：见各子任务 `implement.md`；parent 层回滚=按序撤销子任务 3→2→4。

## 5. 集成评审清单（parent 负责，子任务全归档前执行）

- [ ] `rg "DarkMode" src/Tailviewer` 无残留；`rg "#0047AB|00, 0x47, 0xAB"` 无遗留默认强调色。
- [ ] `ThemeManager.Apply(Color, ThemeMode)` 唯一入口；`Apply(Color,bool)` 内等级钩子就位（无 `#if` 桩）。
- [ ] `TextBrushes` 等级默认画刷为静态可变；非自定义等级无冻结用户色拷贝。
- [ ] 三态下设置页/渲染器/属性页/搜索框目检通过；新默认强调色浅/深对比达标。
