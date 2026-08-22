# GitHub Actions CI + Release workflows

## Goal

为 Tailviewer 仓库新增一套 GitHub Actions CI/CD 工作流，替换现有 `test.yml`，并配套完成「portable 优先」的产物重构：
- `ci.yml`：PR 合入 master 前 + 手动触发时，并行执行 `test` 与 `build`。
- `release.yml`：仅 master 分支上的 `v` 开头 tag 触发，从 portable staging 目录生成 portable zip 与 installer，并创建 GitHub Release。
- 构建时通过 GitVersion 注入 3 段版本号 `x.x.x`。

## Background / Confirmed Facts

- 解决方案 `Tailviewer.sln`，全部项目 `.NET Framework 4.8`，WPF 应用。
- `Tailviewer` / `Installer` 为 SDK-style（`PackageReference`，`GenerateAssemblyInfo=false`，链接 `GlobalAssemblyInfo.cs`）；其余为经典 csproj + `packages.config`。
- 版本硬编码于 `src/GlobalAssemblyInfo.cs`（`AssemblyVersion/FileVersion/InformationalVersion` 均 `1.0.0.0`）；经典项目另有 `Properties/AssemblyInfo.cs`（同样 `1.0.0.0`）。应用经 `Assembly.GetName().Version`（`AssemblyVersion`）读取并显示。
- 所有项目强命名，key 文件 `sig/key.snk`（已提交，开发者 key，哈希与 `sig/DeveloperKey.snk` 一致），CI 无需恢复密钥。
- 所有项目统一输出到仓库根 `bin\`；`Installer.csproj` 在构建时把 `bin\*.exe/*.dll/Plugins\*.tvp` 等作为 `EmbeddedResource` 嵌入，安装时按 `InstallationFiles\` 前缀枚举并解压到 Program Files。
- `.sln` 的 `ProjectDependencies`：`Serilog → Archiver`、`Installer → Api/Core/Serilog/Archiver/Tailviewer`；插件 `PostBuildEvent` 以裸名调用 `archive.exe`。
- 现有 `.github/workflows/test.yml`：7 个测试工程 + NUnit.ConsoleRunner 3.12.0（Debug，跳过 `SerilogAcceptanceTest`）。
- 本地化：`zh-CN\Tailviewer.resources.dll` 为应用卫星资源；`cs-CZ/de/es/...` 目录含 `Xceed.Wpf.AvalonDock.resources.dll`（第三方 AvalonDock 卫星）；现有 Installer 均未嵌入（缺口）。
- 仓库当前无 git tag、无 GitVersion 配置；分支 `master` + `dev`；现有 CI 生态为 AppVeyor。

## Requirements

### 版本注入
- R1: 引入 GitVersion，从 git tag/历史自动推导 SemVer（`x.x.x`）。
- R2: 构建时注入：`AssemblyVersion` = `x.x.x.0`（.NET 强制 4 段），`AssemblyFileVersion` / `AssemblyInformationalVersion` = `x.x.x`（3 段）。
- R3: 应用显示版本统一为 `x.x.x`（去除尾部 `.0`，或改读 3 段文件/信息版本）。

### portable staging 与产物
- R4: 新增 portable staging 目录作为**唯一真实产物源**（完整运行时集：主程序/dll/config/插件 `.tvp` + 本地化卫星 `zh-CN\Tailviewer.resources.dll` 与 `Xceed.Wpf.AvalonDock.resources.dll` + LICENSE/Uninstall.cmd/Fonts/Icons/Plugins\Readme.txt）。
- R5: portable zip 与 Installer 都从 staging 目录生成；**先 portable，后 installer**。
- R6: `Installer.csproj` 改为嵌入 staging 目录（替代逐文件 `..\..\bin\` 清单），CI **不再执行** silentinstall。

### CI 工作流
- R7: `.github/workflows/ci.yml`：`workflow_dispatch` + `pull_request: branches: [master]`；含 `test` 与 `build` 两 job **并行**（无 `needs`）。
- R8: `test` job 复用现有 test.yml 逻辑；`build` job 构建主程序 + 按新流程产出并上传 installer artifact。
- R9: `.github/workflows/release.yml`：仅 `push: tags: ['v*']`，并校验 tag 提交在 master 上。
- R10: release 产出 `Tailviewer-NEXT-setup-<x.x.x>.exe` 与 `Tailviewer-NEXT-portable-<x.x.x>.zip`，创建 GitHub Release 上传两者。
- R11: **portable 失败则整体失败，不允许只发 installer**。
- R12: 删除 `.github/workflows/test.yml`。
- R13: 全程开发者 key 签名，无自定义 secret（仅默认 `GITHUB_TOKEN`）。

## Acceptance Criteria

- [ ] AC1: `ci.yml` / `release.yml` 存在且 YAML 有效；`ci.yml` 触发含 `workflow_dispatch` + `pull_request(master)`；`release.yml` 仅 `push.tags ['v*']` + master 守卫。
- [ ] AC2: `ci.yml` 含 `test`、`build` 两 job 且无 `needs`（并行）。
- [ ] AC3: GitVersion 配置就位；构建产物 `AssemblyVersion=x.x.x.0`、`FileVersion/InformationalVersion=x.x.x`；应用显示版本为 3 段。
- [ ] AC4: staging 目录生成且为完整运行时集（含本地化卫星）；portable zip 与 Installer 均从 staging 生成；CI 不执行 silentinstall。
- [ ] AC5: release 产物名为 `Tailviewer-NEXT-setup-<ver>.exe`、`Tailviewer-NEXT-portable-<ver>.zip`，并创建 GitHub Release 上传两者。
- [ ] AC6: portable 步骤失败导致 release 整体失败（无 installer-only 发布路径）。
- [ ] AC7: `test.yml` 已删除；工作流无 `secrets.` 引用（除默认 token）。

## Out of Scope

- 官方 key（`sig/sns`）签名；`Tailviewer.Acceptance.Tests` 接入；`SerilogAcceptanceTest`（缺提交 TestData）。
- AppVeyor 下线、README/CONTRIBUTING 徽章更新；多平台、构建缓存、覆盖率、PR 内联状态。
- 产品标识改名本身（`Tailviewer` → `Tailviewer NEXT` 由并行任务负责；本任务仅消费其产物名）。

## Key Decisions

| 决策 | 结论 |
| --- | --- |
| release 产出 | 构建 installer + portable zip，创建 GitHub Release 上传两者 |
| 版本注入 | GitVersion 自动推导；AssemblyVersion=x.x.x.0，File/Info 版=x.x.x |
| Installer 来源 | 改 `Installer.csproj` 嵌入 staging 目录 |
| staging 文件范围 | 完整运行时集（含本地化卫星） |
| 构建顺序 | 先 portable（staging→zip），再 installer（staging→嵌入）；CI 不执行 silentinstall |
| portable 失败 | 整体失败，禁止只发 installer |
| 产物命名 | `Tailviewer-NEXT-setup-<ver>.exe` / `Tailviewer-NEXT-portable-<ver>.zip` |
| 签名 key | 开发者 key（无 secret） |
| test.yml | 删除，并入 ci.yml |

## Cross-task Dependency

- 依赖并行任务「产品标识改名 Tailviewer → Tailviewer NEXT」已落地：可执行文件为 `bin\Tailviewer NEXT.exe`（`Tailviewer.exe` 保留为引用程序集），安装器仍为 `bin\Tailviewer-setup.exe`。本任务发布 artifact 按 `Tailviewer-NEXT-*` 命名。

## Risks / Deferred

- GitVersion 注入在「SDK + 经典 csproj + 显式 AssemblyInfo.cs/GlobalAssemblyInfo.cs」混合下的具体路径（GitVersion.MsBuild vs CLI + set-version 脚本）需实现前 spike 验证；MVP 行为不变。
- 官方 key 签名 deferred；并行改名任务的 `Installer.csproj` 变更与本任务 staging 嵌入改动需避免冲突（不同区域，风险低）。
