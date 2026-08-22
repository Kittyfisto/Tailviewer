# GitHub Actions 测试工作流

## Goal

为 Tailviewer-NEXT 仓库新增一个 GitHub Actions 工作流，复用现有的 NUnit 测试项目，在 CI 中自动构建并运行测试。触发方式为手动触发（`workflow_dispatch`）以及向主分支（`master`）发起 Pull Request 时触发。

## Background / Confirmed Facts

- 仓库远端为 `git@github.com:brofea/Tailviewer-NEXT.git`，默认分支为 `origin/master`。
- 所有项目目标框架为 `.NET Framework 4.8`（见 `.trellis/spec/build/index.md`）。
- `Tailviewer` / `Installer` 为 SDK-style `PackageReference`；其余项目（含全部测试项目）为经典 non-SDK csproj + `packages.config` + `packages/` 目录（`HintPath` 指向 `..\..\packages\`）。
- `packages/` 被 `.gitignore` 忽略（`/packages`），CI 必须先 `nuget restore`。
- 所有程序集强命名，密钥 `sig/key.snk` 已提交（`git ls-files sig` 确认），无需在 CI 中恢复密钥。
- 测试框架为 NUnit 3.12.0（`packages/NUnit.3.12.0`），但仓库未提交 NUnit 控制台 runner 或 test adapter，需要在 CI 中临时下载。
- 所有项目统一输出到 `..\..\bin\`（共享输出目录），测试 DLL 会落到仓库根 `bin/`。
- 测试项目清单：`Tailviewer.Api.Tests`、`Tailviewer.Core.Tests`、`Tailviewer.Tests`、`Tailviewer.System.Tests`、`Tailviewer.Archiver.Tests`、`Tailviewer.PluginRepository.Tests`、`Tailviewer.Formats.Serilog.Tests`、`Tailviewer.Acceptance.Tests`。
- `Tailviewer.Acceptance.Tests` 依赖 WPF/WpfUnit 与 Installer，属于 UI/系统级验收测试，风险较高，v1 暂不纳入。
- `Installer.csproj` 将 `bin/*.exe`、`bin/*.dll` 等作为 `EmbeddedResource` 嵌入，直接构建整个解决方案存在构建顺序/文件缺失风险，v1 只构建测试项目而非整个解决方案。

## Requirements

- R1: 新增一个工作流文件 `.github/workflows/test.yml`。
- R2: 工作流可被手动触发（`workflow_dispatch`）。
- R3: 工作流在向 `master` 分支发起 PR 时触发（`pull_request`，`branches: [master]`）。
- R4: 工作流在 Windows runner 上运行，恢复 NuGet 包后以 Release 配置构建测试项目。
- R5: 工作流使用 NUnit 控制台 runner 运行现有单元测试程序集，测试失败时任务失败。
- R6: 工作流产出并上传测试结果（便于查看失败详情）。

## Acceptance Criteria

- [ ] AC1: `.github/workflows/test.yml` 存在且 YAML 语法有效。
- [ ] AC2: `on` 块同时包含 `workflow_dispatch` 与 `pull_request`（target `master`）。
- [ ] AC3: 工作流包含 checkout、NuGet 恢复、MSBuild 构建、NUnit 运行、结果上传步骤。
- [ ] AC4: 测试命令覆盖现有单元测试程序集（不含 `Tailviewer.Acceptance.Tests`；`SerilogAcceptanceTest` 因缺少提交的 TestData 而跳过）。
- [ ] AC5: 任一测试程序集失败会导致整个工作流失败（非零退出码向上传递）。

## Out of Scope

- `Tailviewer.Acceptance.Tests`（WPF/UI/系统级验收测试）的接入。
- `SerilogAcceptanceTest`（依赖未提交的 `TestData\Formats\Serilog\Serilog.txt`），后续补提交测试数据后再纳入。
- 构建整个解决方案（含 `Installer`）与打包/发布。
- 代码覆盖率、缓存优化、多平台（Linux/macOS）支持。
- 测试结果的 PR 内联注释 / status check 集成（v1 仅上传产物）。
