# Modernize repo: remove legacy solution and make VS2026 build

## Goal

清理仓库中历史遗留的旧解决方案及其相关内容，并把项目统一到 .NET Framework 4.8，使 `Tailviewer.sln` 能在 VS 2026 中成功编译并运行主程序。

## Background（已确认事实）

- 仓库包含两个解决方案：`Tailviewer.sln`（当前主用，VS2019/VS16）与 `Tailviewer-Complete.sln`（历史遗留，VS2017/VS15）。
- `Tailviewer-Complete.sln` 引用了大量磁盘上已不存在的项目（`Tailviewer.Test`、`Tailviewer.AcceptanceTests`、`Tailviewer.Archiver.Test`、`Tailviewer.Scripthost`、`Tailviewer.Analysis.*` 等），并引用 `playground\` 下的实验项目。
- `playground\` 目录（10 个子目录，含 `Live` 残留的 `FileLogger.txt`、`SQLiteLogger.db`）未被主解决方案引用，属于实验性遗留内容。
- 主解决方案各项目大多已升级到 v4.8（提交 `3ddf2935 refactor: NET v4.8`），但 `src\Tailviewer\Tailviewer.csproj` 与 `src\Installer\Installer.csproj` 仍是 v4.7.1。
- 项目使用旧式（非 SDK）csproj + `packages.config`；NuGet 包目录 `/packages` 已被 `.gitignore` 排除且当前不存在（需 NuGet restore）。
- 强命名密钥 `sig\key.snk` 存在；`bin\` 为空（尚未构建）。
- VS 2026 中当前已打开 `Tailviewer.sln`。
- 第三方依赖：log4net 2.0.12、Metrolib 0.3.0.162、MMQ 1.0.15、Ookii.Dialogs.Wpf 1.1.0、SharpRemote 0.6.184、System.Threading.Extensions 2.0.72、Extended.Wpf.Toolkit 4.0.1、Moq 4.7.127（测试）。

## 关键决策

- **目标框架**：保持 .NET Framework 4.8（用户已确认）。不迁移到现代 .NET（net8.0-windows）。
- **执行中新增决策（用户已确认）**：VS 2026 不加载旧式 WPF 项目，故将两个 WPF 项目
  `src\Tailviewer` 与 `src\Installer` 迁移为 **SDK 风格 + PackageReference**（目标仍为
  `net48`，`UseWPF=true`）。其余项目保持旧式 csproj + packages.config。

## 范围内（In Scope）

- 删除 `Tailviewer-Complete.sln` 与 `Tailviewer-Complete.sln.DotSettings`。
- 删除 `playground\` 整个目录（实验项目及 `Live` 残留文件）。
- 将 `src\Tailviewer\Tailviewer.csproj` 与 `src\Installer\Installer.csproj` 迁移为 SDK 风格
  （`Microsoft.NET.Sdk.WindowsDesktop`，`net48` + `UseWPF`），packages.config 改为 PackageReference，
  并删除两个项目的 `packages.config`。
- 将 `src\Tailviewer\App.config` 与 `src\Installer\App.config` 的 `supportedRuntime` sku 升到 v4.8。
- 修复 `src\Tailviewer.Archiver\Plugins\PluginPacker.cs` 的插件框架上限（4.7.1 → 4.8），
  使 `.tvp` 插件打包在 4.8 下成功（Installer 依赖该产物）。
- 更新 `README.md` 中关于 .NET 4.7.1 的说明为 4.8。
- 更新 `.trellis/spec/build/index.md` 与 `.trellis/spec/index.md` 中关于旧解决方案与 4.7.1 的描述。
- 使 `Tailviewer.sln` 在 VS 2026 中编译成功（含 NuGet restore），主程序 `Tailviewer` 可启动运行。

## 范围外（Out of Scope）

- 迁移到现代 .NET（net8.0-windows）。
- 修改源代码注释/测试描述中的 "4.7.1" 字样（如 `src\Tailviewer.Archiver.Tests\PluginPackerTest.cs:102-117`，属行为描述，不影响构建）。
- 处理构建中可能暴露的功能性编译错误之外的大规模重构。

## Requirements

- R1：仓库中不再存在 `Tailviewer-Complete.sln`、`Tailviewer-Complete.sln.DotSettings` 及 `playground\` 目录。
- R2：`Tailviewer.sln` 中所有项目目标框架统一为 .NET Framework 4.8（主程序与 Installer 不再为 4.7.1）。
- R3：`Tailviewer.sln` 在 VS 2026 中能完成 restore 并成功编译。
- R4：主程序 `Tailviewer`（WinExe）可启动运行。
- R5：文档与 Trellis spec 中关于旧解决方案与 4.7.1 的描述已同步更新。

## Acceptance Criteria

- [x] `Tailviewer-Complete.sln`、`Tailviewer-Complete.sln.DotSettings` 与 `playground\` 目录已从仓库删除（`git status` 可见删除）。
- [x] `src\Tailviewer\Tailviewer.csproj`、`src\Installer\Installer.csproj` 及其 `App.config` 目标均为 net48 / v4.8。
- [x] `Tailviewer.sln` 在 VS 2026 中 Build 成功（0 失败）。
- [x] `Tailviewer` 可成功启动（Ctrl+F5 运行，进程启动并显示主窗口）。
- [x] `README.md`、`.trellis/spec/build/index.md`、`.trellis/spec/index.md` 中旧解决方案/4.7.1 描述已更新。

## Risks

- **NuGet restore**：`/packages` 为空且被 gitignore。若某第三方包（Metrolib、MMQ、SharpRemote、System.Threading.Extensions 等作者定制包）无法从当前 NuGet 源恢复，构建会失败，需向用户确认可用的包源。
- **VS 2026 对旧式 csproj/.NET Framework 的支持**：.NET Framework 4.8 定位包需随 VS 安装；若缺失需安装 "Developer Pack"。
- **删除操作不可逆**（虽可经 git 恢复）：删除 `playground\` 前已确认其不被主解决方案引用。
