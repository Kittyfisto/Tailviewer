# Add UI localization: English baseline + runtime language switch + zh-CN

## Goal

为 Tailviewer 主程序（`src/Tailviewer`）引入 UI 本地化能力：建立英文资源基线，提供运行时语言切换框架，并交付简体中文（zh-CN）翻译。让用户可在设置中切换界面语言，主程序 UI 文本随语言改变。

## Background（已确认事实）

- 主程序目前**没有任何本地化设施**：无 `.resx`（仅 `Installer` 有一个自用 `Resources.resx`），无 `.xlf`，无 `CultureInfo`/`CurrentUICulture` 切换逻辑，无语言设置项。
- 所有 UI 文本硬编码：50 个 XAML（`src/Tailviewer/Ui/**`，约 3771 行，~115 处 `Text/Header/Title/Content/ToolTip` 硬编码）+ 88 个 ViewModel/code-behind（`src/Tailviewer/**/*.cs`）中的数百处字符串字面量。
- `Language.cs` 只是英式复数工具（`Pluralize` 追加 `s`），非本地化。
- 设置持久化：`ApplicationSettings`（`src/Tailviewer/Settings/ApplicationSettings.cs`）手写 XML 序列化，每个子设置类实现 `Save(XmlWriter)`/`Restore(XmlReader)`；新增语言设置需遵循该模式。
- 设置 UI：`src/Tailviewer/Ui/Settings/SettingsControl.xaml` + `SettingsFlyoutViewModel.cs`，用 `FlatGroupBox` 分组；新增语言下拉框可在此追加一组。
- 启动流程：`App.StartApplication`（`src/Tailviewer/App.cs`）先 `settings.Restore()`，再构建 `ServiceContainer`/`MainWindow`；语言必须在构建 UI 前应用。
- `App` 无 `App.xaml`，是代码构建（`App.cs` 构造函数里 `Resources.MergedDictionaries.Add(...)`），资源字典可代码合并/替换。
- 项目已有「重启后生效」先例：设置里的 `Default text file encoding` 提示 "Changing this value requires tailviewer to be restarted in order to take effect."（`SettingsControl.xaml`）。

## Requirements

- R1：新增语言设置项，可持久化到 `settings.xml`（跟随现有 `ApplicationSettings` 的 XML 保存/恢复模式）。
- R2：设置 UI 中新增语言选择控件（`SettingsControl.xaml` + `SettingsFlyoutViewModel`）。
- R3：建立英文资源基线（作为默认/回退语言），把现有硬编码 UI 文本提取为资源。
- R4：提供运行时语言切换框架，使 UI 文本可随所选语言呈现。
- R5：交付简体中文（zh-CN）资源，覆盖提取出的 UI 文本。
- R6：主程序启动时按已保存语言应用资源。

## Acceptance Criteria

- [ ] 设置中可选择界面语言，选择被保存并在重启后恢复。
- [ ] 界面语言可切换为中文（zh-CN）且主要界面文本显示中文。
- [ ] 切回英文时界面文本恢复英文。
- [ ] 未翻译/缺失的文本回退到英文，不出现空文本或键名。
- [ ] 主程序在 VS 2026 中编译通过且可运行。

## Out of Scope

- 第三方组件（Metrolib、Xceed WPF Toolkit）内部文本的本地化。
- 插件（Serilog、LogLevelPlugin 等）与 Installer 的本地化。
- 日志输出、命令行输出、导出文件内容的本地化。
- 除 en / zh-CN 外的其他语言。

## 关键决策

- **生效方式**：切换语言后**重启生效**（用户已确认）。与现有 `Default text file encoding`
  设置同一 UX 先例；启动时读设置 → 设 `CurrentUICulture` → 加载对应资源字典。
- **范围**：全量主程序 UI（用户已确认）。50 个 XAML + 88 个 ViewModel 的全部 UI 字符串
  一次性提取为英文基线并翻译为 zh-CN。
- **翻译来源**：zh-CN 文本由 AI 机翻占位（用户已确认），结构化存储、可后续替换校对。
