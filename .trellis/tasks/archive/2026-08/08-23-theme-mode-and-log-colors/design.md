# 设计：主题模式三态 + 新默认强调色

## 1. 边界

### 可改
- `src/Tailviewer/Settings/ThemeMode.cs`（新增枚举）、`Settings/UISettings.cs`。
- `src/Tailviewer/Ui/ThemeManager.cs`（签名 + 解析 + provider 注入；`Apply(Color,bool)` 主体不改）。
- 新增 `src/Tailviewer/Ui/SystemThemeDetector.cs`（`ISystemThemeProvider` + `WindowsSystemThemeProvider`）。
- `src/Tailviewer/Ui/Settings/SettingsFlyoutViewModel.cs`、`Ui/Settings/SettingsControl.xaml`。
- `src/Tailviewer/App.cs`（启动调用点签名）。
- `tools/generate_localization.py`（新增 Light/Dark/System 键；如 `DarkMode` 键无引用则删除）及其三件套产物。

### 不可改（红线）
- `src/Tailviewer/BusinessLogic/**`。
- `ThemePalette`/`SemanticPalette` 现有值与契约；`TextBrushes`（本子任务不碰，`UpdateNeutral` 逻辑不变）。
- 渲染算法/分页/虚拟化/命中测试/事件坐标。

## 2. 枚举与解析

```csharp
namespace Tailviewer.Settings
{
    public enum ThemeMode { Light = 0, Dark = 1, System = 2 }
}

// Tailviewer.Ui
public interface ISystemThemeProvider
{
    bool IsDark();
    event Action Changed;
}

public static class ThemeManager
{
    public static ISystemThemeProvider SystemThemeProvider { get; set; } = new WindowsSystemThemeProvider();

    public static bool ResolveDarkMode(ThemeMode mode)
    {
        switch (mode)
        {
            case ThemeMode.Dark: return true;
            case ThemeMode.Light: return false;
            default: return SystemThemeProvider.IsDark();
        }
    }

    public static void Apply(Color primary, ThemeMode mode)
    {
        var darkMode = ResolveDarkMode(mode);
        Apply(primary, darkMode);   // 委托既有 Apply(Color, bool)，其主体不变
    }

    public static void Apply(Color primary, bool darkMode) { /* 既有逻辑，本子任务不改 */ }
}
```

- 三态解析在上游完成；`Apply(Color, bool)` 下游保持既有行为，与依赖子任务（等级默认色钩子）解耦。

## 3. 默认强调色（重设计）

| 项 | 旧 | 新 |
|---|---|---|
| `UISettings.DefaultThemeColor` | `#0047AB` | **`#0F62FE`**（IBM Carbon Blue 60） |
| `PrimaryForeground` | White | White（不变） |
| 白字 on 强调色对比（浅色表面） | ≈13.8:1 | **≈5.0:1**（AA 达标） |
| 深色表面行为 | 同左 | 强调色不变，白字 on `SecondaryBrush`(=#0F62FE) ≈5.0:1（AA） |

- 对比计算（WCAG 相对亮度，`#0F62FE` L≈0.160）：`(1.0+0.05)/(0.160+0.05) ≈ 5.0:1`。
- 迁移：旧设置文件缺失 `themecolor` 时 `Restore` 回退新默认 `#0F62FE`；已保存 `themecolor` 的用户值不受影响（保留用户自定义）。
- `grep` 指引：`rg -n "#0047AB|00, 0x47, 0xAB|0047AB" src/Tailviewer` 仅 `ThemePaletteTest` 用作任意测试输入可保留；`UISettingsTest.TestDefaultThemeColorIs0047AB` 必须更新。

## 4. 系统检测与线程/生命周期

```csharp
public sealed class WindowsSystemThemeProvider : ISystemThemeProvider, IDisposable
{
    public bool IsDark()  // 读 AppsUseLightTheme DWORD
    public event Action Changed;

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General) return;
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null) return;                 // 测试/设计时 no-op
        dispatcher.BeginInvoke(new Action(() => Changed?.Invoke()));
    }
}
```

- **线程模型**：`SystemEvents.UserPreferenceChanged` 在非 UI 线程触发；回调只读注册表 → 判定是否有变化 → `Dispatcher.BeginInvoke` 抛回 UI 线程再触发 `Changed`。UI 线程上才允许 `ThemeManager.Apply`（它改 `Application.Current.Resources`）。
- **生命周期**：`App.StartApplication` 注册一次系统事件（`SystemEvents.UserPreferenceChanged += ...`），应用退出时反注册（`IDisposable`）；**不在**任何类型静态构造器里订阅（避免测试进程泄漏）。`App` 内 `ThemeManager.SystemThemeProvider` 默认实例即真实 Windows 实现；测试可替换为 fake。
- **跟随判定**：`ThemeManager` 缓存 `CurrentThemeMode`；`Changed` 回调内 `if (CurrentThemeMode == ThemeMode.System)` 比较解析前后有效 darkMode，变化才 `Apply`。
- **可测试性**：`ResolveDarkMode` 不直接碰注册表——经 `SystemThemeProvider` 接口；单测注入 `FakeSystemThemeProvider`（可手动触发 `Changed`），无需全局注册表变更。

## 5. 设置页 UI

- `SettingsFlyoutViewModel`：`DarkMode` 布尔属性替换为 `ThemeMode`（`ThemeMode` 类型），setter 写设置 + `SaveAsync()` + Dispatcher 延迟 `ThemeManager.Apply(ThemeColor, value)`（镜像现状）。
- 新增 `ThemeModes` 只读选项集合（`DisplayName` 用 `Strings.ThemeModeLight/Dark/System`），ComboBox 绑定。
- `SettingsControl.xaml`：`CheckBox DarkMode` → 三选控件（ComboBox/RadioButton），绑定 `ThemeMode`。

## 6. 兼容性与回滚

- 回滚点 R1：撤销 `ThemeMode`/`ISystemThemeProvider`/`WindowsSystemThemeProvider`/`ThemeManager` 签名与注入，恢复 `UISettings.DarkMode` bool 与 `CheckBox`，恢复 `DefaultThemeColor = #0047AB`。
- 迁移不回写旧文件（只读旧 `darkmode`）；无 schema 迁移。
- 强调色既有单测（`ThemePaletteTest`）基线不变；`UISettingsTest.TestDefaultThemeColorIs0047AB` 改为新值断言。

## 7. 测试

- `UISettingsTest`：默认 `ThemeMode.Light`；`ThemeMode` Clone/Roundtrip/缺省回退；旧 `darkmode="true|false"` 迁移 Dark/Light；非法 `thememode` 回退 Light；`TestDefaultThemeColorIs0F62FE` 断言 `Color.FromRgb(0x0F, 0x62, 0xFE)`。
- 新增 `ThemeManagerTest`/`SystemThemeDetectorTest`：`ResolveDarkMode` 三态映射（注入 fake provider）；fake provider 触发 `Changed` 仅 System 模式跟随。
- 本地化安全断言引用 `Strings.ThemeMode*`，不硬编码英文。
