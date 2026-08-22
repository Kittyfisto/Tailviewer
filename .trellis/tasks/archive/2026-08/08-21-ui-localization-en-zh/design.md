# Design — UI 本地化（en 基线 + zh-CN + 重启生效）

## 目标架构

用 .NET 标准资源机制（`.resx` + `ResourceManager` + 卫星资源）实现本地化，单一机制同时覆盖 XAML 与 C# 字符串。语言选择持久化到现有 `settings.xml`，启动时应用 `CurrentUICulture`，切换后重启生效。

## 资源存储与命名

| 文件 | 说明 |
|------|------|
| `src/Tailviewer/Properties/Strings.resx` | 英文基线（中性资源，默认/回退） |
| `src/Tailviewer/Properties/Strings.zh-CN.resx` | 简体中文卫星资源 |

- ManifestResourceName：`Tailviewer.Properties.Strings`（resx 位于 `Properties/`，`RootNamespace=Tailviewer`）。
- 卫星资源机制原生支持「zh-CN 缺失 key → 自动回退英文」，无需额外代码。

## 强类型访问类

`src/Tailviewer/Properties/Strings.cs`（脚本从 `Strings.resx` 生成一次并提交）：

```csharp
namespace Tailviewer.Properties
{
    public static class Strings
    {
        private static readonly System.Resources.ResourceManager _rm =
            new System.Resources.ResourceManager("Tailviewer.Properties.Strings",
                typeof(Strings).Assembly);

        public static string ExportFolder => _rm.GetString("ExportFolder") ?? "ExportFolder";
        // ... 每个 key 一个属性
    }
}
```

- XAML 访问：`{x:Static properties:Strings.ExportFolder}`（`xmlns:properties="clr-namespace:Tailviewer.Properties"`）。
- C# 访问：`Strings.ExportFolder`。
- 回退：`GetString` 在 zh-CN 缺 key 时返回中性（英文）值；`?? key` 仅兜底极端情况（资源文件缺失）。

## 语言设置

遵循现有 `ApplicationSettings` 分层，新增 `src/Tailviewer/Settings/UISettings.cs`：

- 字段 `string Language`（默认 `"en"`；可选值 `"en"`、`"zh-CN"`）。
- 实现 `Save(XmlWriter)` / `Restore(XmlReader)` / `Clone()`，写入 `<ui><language>en</language></ui>`。
- 在 `ApplicationSettings` 增加 `UISettings Ui` 属性，并在其 `Save()`/`Restore()`/`Clone()` 中接线（参考现有 `export`/`logfile` 的接线方式）。

## 启动应用语言

`App.StartApplication`（`src/Tailviewer/App.cs`）中，`settings.Restore(...)` 之后、构建任何 ViewModel/窗口之前：

```csharp
var culture = new System.Globalization.CultureInfo(settings.Ui.Language);
System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
```

`x:Static` 与 C# 属性读取发生在 UI 构建期，此时文化已设置，文本即为所选语言。

## 设置 UI

- `SettingsControl.xaml`：新增一个 `FlatGroupBox Header="Language"`，内含 `ComboBox`
  （ItemsSource 绑定 `Languages`，SelectedItem 绑定 `Language`），下方 `Run` 提示
  "Changing the language requires tailviewer to be restarted to take effect."。
- `SettingsFlyoutViewModel`：新增
  - `IReadOnlyList<string> Languages`（`"en"`、`"zh-CN"` 的展示名映射，如 `LanguageViewModel`）
  - `string Language` getter/setter（写入 `_settings.Ui.Language` + `SaveAsync()`）。

## 字符串提取

- XAML（50 个文件，~115 处 `Text/Header/Title/Content/ToolTip` 硬编码）→ 逐处替换为
  `{x:Static properties:Strings.<Key>}`（`Header` 等非 string 属性经 `x:Static` 返回 string 可直接用；
  复合格式字符串如带 `Run` 的多行说明，按语义拆为多个 key）。
- C#（88 个 ViewModel/code-behind，数百处字符串字面量）→ 替换为 `Strings.<Key>`。
- `Language.cs` 的 `Pluralize`/`Count` 保持不动（英式复数逻辑，与本地化正交；中文本地化字符串
  不经过它）。

## 兼容性与回滚

- 纯新增：不改动 `ApplicationSettings` 已有字段语义；新增 `<ui>` 节点，旧 settings.xml 无此节点时
  `Restore` 保持默认 `"en"`（向后兼容）。
- 回滚：全部为文件新增/替换，git 可整体回滚；语言设置读取失败（非法值）时回退 `"en"`。
- 不引入新 NuGet 依赖；不触碰 SDK-style 项目的资源 glob 默认行为（显式声明两个 resx 以防歧义）。

## 权衡

- 选 `.resx` + 强类型而非 WPF `DynamicResource`：因「重启生效」无需热切换；`x:Static` 足够，
  且同一机制覆盖 C# 字符串，无需为 ViewModel 文本另建通知/刷新框架。
- 强类型类用脚本生成而非依赖 VS 自定义工具：保证 `dotnet build`/MSBuild CLI 可复现构建。
