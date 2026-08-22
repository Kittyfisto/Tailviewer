# 技术设计：GitHub Actions 测试工作流

## 目标

在 Windows runner 上完成「恢复 → 构建 → 跑测试 → 上传结果」的闭环，复用现有 NUnit 测试，不修改任何 csproj / 源码。

## 运行环境

| 项 | 选择 | 理由 |
| --- | --- | --- |
| runner | `windows-2022` | 预装 .NET Framework 4.8 + MSBuild，支持经典 csproj 与 WPF 引用 |
| 触发器 | `workflow_dispatch` + `pull_request: branches: [master]` | 用户要求：手动 + PR 到主分支 |
| 配置 | `Debug` | `Tailviewer.Formats.Serilog.Tests` 的 Release `OutputPath` 是 `bin\Release\`（csproj 缺陷），Debug 下所有项目统一输出到仓库根 `bin\` |

## 关键决策

### 1. 不构建整个解决方案，只构建测试项目

`Installer.csproj` 把 `..\..\bin\Tailviewer.exe`、`..\..\bin\Tailviewer.exe.config` 等作为
`EmbeddedResource` 嵌入。全新 CI 环境下 `bin/` 为空，且 `Tailviewer.exe.config` 依赖仓库外的
`GenerateRedirects.exe` 工具生成，直接 `msbuild Tailviewer.sln` 存在失败风险。

测试项目之间通过 `ProjectReference` 传递依赖：构建 `Tailviewer.Tests` 会级联构建
`Tailviewer`、`Tailviewer.Api`、`Tailviewer.Core`、`Tailviewer.Archiver` 及 `*Tests`。
因此按「测试项目」逐个 `msbuild` 即可获得全部所需产物，且不触碰 `Installer`。

v1 构建清单（7 个单元测试项目，排除 Acceptance.Tests）：

```
src\Tailviewer.Api.Tests\Tailviewer.Api.Tests.csproj
src\Tailviewer.Core.Tests\Tailviewer.Core.Tests.csproj
src\Tailviewer.Tests\Tailviewer.Tests.csproj
src\Tailviewer.System.Tests\Tailviewer.System.Tests.csproj
src\Tailviewer.Archiver.Tests\Tailviewer.Archiver.Tests.csproj
src\Tailviewer.PluginRepository.Tests\Tailviewer.PluginRepository.Tests.csproj
src\Plugins\Tailviewer.Formats.Serilog.Tests\Tailviewer.Formats.Serilog.Tests.csproj
```

### 2. 恢复 NuGet 包

- 经典项目用 `packages.config`，SDK-style 项目（Tailviewer）用 `PackageReference`。
- `nuget restore Tailviewer.sln` 可同时处理两者（`nuget.exe` 下载自官方 dist 地址）。
- 不依赖 `dotnet restore`（对 packages.config 的支持不可靠）。

### 3. 测试运行器

- 仓库未提交 NUnit runner/adapter，v1 用 `nuget install NUnit.ConsoleRunner -Version 3.12.0`
  临时下载到 `$env:RUNNER_TEMP`，执行 `nunit3-console.exe`。
- 版本对齐：runner 3.12.0 ↔ NUnit framework 3.12.0，支持 .NET Framework 4.8 测试程序集。
- 逐程序集传入测试 DLL（共享输出目录），`--result` 输出 NUnit3 XML，`--labels=Before`
  便于日志定位。`nunit3-console` 在测试失败时返回非零退出码，满足「失败即失败」。
- 注意：程序集名与项目名并不完全一致（由 `<AssemblyName>` 决定）：
  `Tailviewer.System.Tests` → `Tailviewer.SystemTests.dll`；
  `Tailviewer.Formats.Serilog.Tests` → `Tailviewer.Serilog.Test.dll`。工作流按实际 DLL 名传入。
- `Tailviewer.Serilog.Test.dll` 内的 `SerilogAcceptanceTest` 依赖未提交的
  `TestData\Formats\Serilog\Serilog.txt`，在干净 CI 必然失败，故用
  `--where "class != 'Tailviewer.Serilog.Test.SerilogAcceptanceTest'"` 跳过该验收测试，
  保留 `SerilogFileParserTest` 等纯单元测试。

### 4. 结果产物

- `actions/upload-artifact` 上传 `TestResult-*.xml`（`if: always()`，成功失败都保留）。

### 5. 首次 CI 运行暴露的既有测试缺陷（本次一并修复）

干净环境（全新 checkout + 4 worker）暴露了 4 个既有测试缺陷，属于测试代码本身问题，与工作流无关，本次一并修复：

| 测试 | 根因 | 修复 |
| --- | --- | --- |
| `TextBrushesTest.TestDefaultsFollowThemeColor` / `TestUpdateTheme` | WPF 画笔跨线程访问（缺 STA） | 加 `[Apartment(ApartmentState.STA)]`（与其余 UI 测试一致，复用共享 STA 线程） |
| `SettingsControlTest.TestChangeDataContext` | mock `IApplicationSettings` 未设置 `.Ui` → `applicationSettings.Ui.Language` NRE | Setup 中补 `_applicationSettings.Setup(x => x.Ui).Returns(new UISettings())` |
| `PluginArchiveLoaderTest.TestIgnoreIdenticalPlugin` | `InMemoryFilesystem` 只注册了 `PluginPath` 的盘符根，`DownloadedPluginsPath` 在另一盘符（CI 为 `C:\`）未注册 | Setup 中按需注册 `DownloadedPluginsPath` 的盘符根（去重） |

## 数据流

```
checkout → nuget restore → msbuild ×7 测试项目(Debug) → nuget install runner
        → nunit3-console bin\*.Tests.dll --result=TestResult-*.xml --labels=Before
          (Serilog 程序集附加 --where 跳过验收测试)
        → upload-artifact TestResult-*.xml
```

## 风险与回滚

| 风险 | 缓解 / 回滚 |
| --- | --- |
| `Tailviewer.Tests` 含 WPF/WpfUnit，CI 无显示环境 | windows runner 自带桌面会话；个别 STA 问题已通过测试加 `[Apartment(STA)]` 修复 |
| 绑定重定向缺失导致个别程序集加载失败 | 测试项目均含 `app.config`；若报加载错误，v2 显式生成 `*.dll.config` |
| runner 上 `msbuild` 默认版本不兼容经典 csproj | windows-2022 自带 VS Build Tools 的 MSBuild，兼容 v4.8 经典项目 |
| 回滚 | 删除 `.github/workflows/test.yml` 即可，无任何源码/项目文件改动 |
