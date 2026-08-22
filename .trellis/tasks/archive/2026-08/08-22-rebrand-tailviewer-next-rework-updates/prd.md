# Rebrand to Tailviewer NEXT and rework update mechanism

## Goal

把该 fork 的产品标识与更新机制从上游 `Kittyfisto/Tailviewer` 迁移到本项目 `brofea/Tailviewer-NEXT`：

1. 面向用户的显示名与可执行文件名从 "Tailviewer" 改为 "Tailviewer NEXT"。
2. 应用内「检查更新 + 自动下载安装」保留，仅把来源从上游改为本 fork 的 GitHub Releases。
3. 移除软件的更新日志（changelog）功能（About Changelog 选项卡 + 启动 What's new 通知 + 数据）。

## Background / Confirmed Facts

- 解决方案 `Tailviewer.sln`，`.NET Framework 4.8` WPF；主程序 SDK-style（`src/Tailviewer/Tailviewer.csproj`，`AssemblyName=Tailviewer`，输出 `bin\Tailviewer.exe` + `App.config`→`Tailviewer.exe.config`）；安装器 `src/Installer/Installer.csproj`（`AssemblyName=Tailviewer-setup`，把 `bin\*.exe/*.dll/*.tvp` 作为 `EmbeddedResource` 嵌入）。
- 品牌常量集中在 `src/Tailviewer/Constants.cs`（`ApplicationTitle`/`MainWindowTitle`/`ProjectPage`/`GithubPage`/`ReportBugPage`/`AppDataLocalFolder`/`MyDocumentsFolder`）与 `src/Installer/Constants.cs`（`ApplicationTitle`/`ApplicationExecutable`/`MainWindowTitle`/`Publisher`/`DefaultApplicationFolder`）。
- 用户可见 "Tailviewer" 标注：窗口标题、About 页（`TailviewerControl.xaml` 绑定 `Strings.Tailviewer`）、注册表 DisplayName、快捷方式、`Strings.resx`/`Strings.zh-CN.resx` 的 `<data name="Tailviewer">`、`AssemblyProduct("Tailviewer")`、`MainWindowViewModel` 的反馈链接、`App.cs` 日志。
- 自动更新：`src/Tailviewer/BusinessLogic/AutoUpdates/AutoUpdater.cs` 当前从 `https://kittyfisto.github.io/Tailviewer/downloads/version.xml` 读版本（`Server`+`VersionFile`），从 `https://github.com/Kittyfisto/Tailviewer/releases/download/`（`DownloadServer`）下载，`StartInstallation` 以 `update "{ApplicationFolder}"` 调起安装器。
- changelog：`Changelog.cs`（静态数据）+ `About` 弹层 Changelog 选项卡（`AboutFlyoutDataTemplate.xaml`、`ChangelogControl.xaml(.cs)`、`ChangelogViewModel.cs`）+ 启动 What's new 通知（`App.cs` `Change.Merge(Changelog.MostRecentPatches)`、`ActionCenterViewModel` 无参构造 `Changelog.MostRecent`）+ `Change.cs`/`ChangeViewModel.cs`/`ChangeTemplate.xaml`。
- 技术身份若改名会破坏：`pack://application:,,,/Tailviewer;component/...`（51 处）、强名称身份、`.tvp` 插件、`SingleApplicationHelper` 进程名匹配、`App.config` binding redirect。
- `Strings.zh-CN.resx` 为 **UTF-8 BOM + GBK 正文** 混合编码（中文需按 GBK/936 读写）。
- 兄弟任务 `08-22-github-actions-ci-release`（规划中）产出 `Tailviewer-setup-<version>.exe` 与 `Tailviewer-portable-<version>.zip` 并创建 GitHub Release，是本任务 R2 的更新来源。

## Requirements

- R1: 显示名改为 `Tailviewer NEXT`，可执行文件改为 `Tailviewer NEXT.exe`；程序集/命名空间、AppData、MyDocuments 目录保持 `Tailviewer`。
- R2: 自动更新来源改为 `GET https://api.github.com/repos/brofea/Tailviewer-NEXT/releases/latest`（JSON），下载用资产 `browser_download_url`；应用内「检查更新」菜单、设置勾选、自动下载安装逻辑保留。
- R3: 移除 changelog 功能：About Changelog 选项卡、启动 What's new 通知、`Changelog.cs` 数据、`Change`/`ChangeViewModel`/`ChangeTemplate` 及相关测试。

## Acceptance Criteria

- [ ] AC1: 窗口标题、About 页、注册表 DisplayName、快捷方式、安装器标题显示 `Tailviewer NEXT`；`Strings.resx`/`zh-CN.resx` 的 `Tailviewer` 值为 `Tailviewer NEXT`。
- [ ] AC2: 构建产物为 `bin\Tailviewer NEXT.exe` + `bin\Tailviewer NEXT.exe.config`；安装器嵌入并安装 `Tailviewer NEXT.exe`（原 `Tailviewer.exe` 保留为供测试项目引用的程序集，不随安装器/便携包分发）。
- [ ] AC3: AppData / MyDocuments 目录仍为 `...\Tailviewer`（未随显示名变更）。
- [ ] AC4: `SingleApplicationHelper` 能按 `Tailviewer NEXT` 进程名做单实例互斥。
- [ ] AC5: 反馈/仓库链接全部指向 `github.com/brofea/Tailviewer-NEXT`（含 issue 页）。
- [ ] AC6: `AutoUpdater` 不再引用 `kittyfisto.github.io` / `Kittyfisto/Tailviewer`；`BuildVersionCheckUri()` 返回 fork 的 releases/latest 端点；JSON 解析单测通过。
- [ ] AC7: About 弹层无 Changelog 选项卡；启动无 What's new 通知；`Changelog.cs`、`Change`、`ChangeViewModel`、`ChangeTemplate` 已删除；相关测试删除且全量编译通过。
- [ ] AC8: `Strings` 中无 `Changelog` 键（resx 两文件 + `Strings.cs`）。

## Out of Scope

- 上游 `Kittyfisto` 仓库本身。
- 兄弟任务 `08-22-github-actions-ci-release` 的工作流改动（本任务只消费其 Release 产物）。
- 安装器 `Tailviewer-setup.exe` 文件名改名（默认不动；如需改名另行确认并同步 CI-release）。
- 库 DLL（Api/Core/Archiver）的 `AssemblyProduct` 元数据（保持 `Tailviewer`）。
- GitHub API 认证 token（未认证限流 60 次/时，个人应用够用；deferred）。
- beta 通道更新（稳定版优先，`Beta` 置 null；deferred）。

## Key Decisions

| 决策 | 结论 |
| --- | --- |
| 品牌改名范围 | 显示名 + 可执行文件名 → `Tailviewer NEXT` / `Tailviewer NEXT.exe`；程序集/命名空间/AppData/MyDocuments 保持 `Tailviewer` |
| exe 重命名方式 | 仅改输出文件名（`AfterBuild` 复制+删旧），`AssemblyName` 保持 `Tailviewer`（保护 pack URI 与强名） |
| 安装目录 | `%ProgramFiles%\Tailviewer NEXT`（安装器显示名派生）；AppData/MyDocuments 保持 `Tailviewer` |
| Publisher | `brofea`（原 `Kittyfisto`） |
| 更新来源 | GitHub Releases API（JSON）+ 资产下载；`DataContractJsonSerializer`（net48 内建，无新增包） |
| 应用内更新 | 保留（换源） |
| changelog | 全量移除 |

## Risks / Deferred

- exe 文件名含空格：需实机 smoke test（`Process.GetProcessesByName`、安装器、更新拉起）。
- `Strings.zh-CN.resx` GBK 编码：必须脚本化 GBK 读写，避免中文损坏。
- GitHub API 未认证限流：个人应用可接受，token 化 deferred。
- beta 通道：deferred。
