# 技术设计：Tailviewer NEXT 品牌更新 + 更新机制重构

## 目标

在 `brofea/Tailviewer-NEXT` fork 上完成三项改动，且不破坏强命名程序集、`.tvp` 插件加载、单实例互斥与安装/更新链路。

## 关键决策

| 决策 | 结论 |
| --- | --- |
| 显示名 | `Tailviewer NEXT`（窗口标题、About、注册表 DisplayName、快捷方式、安装器标题） |
| 可执行文件名 | `Tailviewer NEXT.exe`（仅改输出文件名，`AssemblyName` 仍为 `Tailviewer`） |
| 程序集/命名空间 | 全部保持 `Tailviewer`（避免破坏 pack URI、强名称身份、插件兼容） |
| AppData / MyDocuments 目录 | 保持 `Tailviewer`（新增独立常量，不再由显示名派生） |
| 安装目录 | `%ProgramFiles%\Tailviewer NEXT`（由安装器显示名派生；运行时应用自发现其目录，一致） |
| Publisher（安装器/卸载） | `brofea`（原 `Kittyfisto`） |
| 仓库/反馈链接 | `GithubPage`/`ProjectPage`/`ReportBugPage` 与 `MainWindowViewModel` 内硬编码链接 → `brofea/Tailviewer-NEXT` |
| 自动更新来源 | GitHub Releases API：`GET https://api.github.com/repos/brofea/Tailviewer-NEXT/releases/latest`（JSON）；下载用资产 `browser_download_url` |
| 应用内更新能力 | 保留（Help 菜单 Check for updates、设置勾选、自动下载安装），仅换源 |
| changelog 功能 | 全量移除（About Changelog 选项卡 + 启动 What's new 通知 + `Changelog.cs` 数据 + 视图/模板） |

## R1：品牌标识（显示名 + 可执行文件名）

### 主程序 `src/Tailviewer/Constants.cs`

- `ApplicationTitle = "Tailviewer"` → `"Tailviewer NEXT"`（显示名）
- 新增 `private const string ApplicationFolderName = "Tailviewer";`（技术身份目录名，保持 `Tailviewer`）
- `MainWindowTitle = string.Format("Tailviewer, v{0}", ...)` → `string.Format("Tailviewer NEXT, v{0}", ...)`
- `AppDataLocalFolder = Path.Combine(..., ApplicationFolderName)`（原用 `ApplicationTitle`）
- `MyDocumentsFolder = Path.Combine(..., ApplicationFolderName)`（原用 `ApplicationTitle`）
- `ProjectPage`/`GithubPage`/`ReportBugPage` → `https://github.com/brofea/Tailviewer-NEXT`（issue 页为 `.../issues/new`）
- `ApplicationLogFile` 文件名保持 `Tailviewer.log` 字面量（不是显示名）

### 可执行文件重命名（AssemblyName 保持 `Tailviewer`）

- 在 `src/Tailviewer/Tailviewer.csproj` 增加一个 `AfterBuild` Target：把 `$(TargetPath)`（`bin\Tailviewer.exe`）与 `bin\Tailviewer.exe.config` 复制为 `bin\Tailviewer NEXT.exe` / `bin\Tailviewer NEXT.exe.config`。**只复制、不删除**原 `Tailviewer.exe`。
- 理由：`pack://application:,,,/Tailviewer;component/...`（51 处 XAML + `Resource.cs`）与强名称身份都依赖 `AssemblyName=Tailviewer`，不能改。且 `Installer`、`Tailviewer.Tests`、`Acceptance.Tests`、`System.Tests`、`Archiver.Tests`、`Serilog.Tests` 等多个项目 `ProjectReference` 到 `Tailviewer.csproj` 并把 `Tailviewer.exe` 作为引用程序集解析——若删除 `Tailviewer.exe` 会触发 CS0006。故保留原程序集，仅额外产出并分发 `Tailviewer NEXT.exe`。

### 单实例 `src/Tailviewer/SingleApplicationHelper.cs`

- `FindOtherTailviewers()` 中 `Process.GetProcessesByName("Tailviewer")` → `Process.GetProcessesByName("Tailviewer NEXT")`；保留 `Tailviewer.vshost` 分支。
- 说明：进程名 = exe 文件名去扩展名 = `Tailviewer NEXT`。IPC 队列名 `Kittyfisto.Tailviewer.SimpleIPC` 为内部值，非用户可见标注，保持不变。

### 安装器 `src/Installer/Constants.cs`

- `ApplicationTitle = "Tailviewer NEXT"`
- `ApplicationExecutable = "Tailviewer NEXT.exe"`（仍由 `$"{ApplicationTitle}.exe"` 派生，无需解耦）
- `MainWindowTitle = "Installing Tailviewer NEXT, v{0}"`
- `Publisher = "brofea"`
- `DefaultApplicationFolder = %ProgramFiles%\Tailviewer NEXT`（由 `ApplicationTitle` 派生，自动正确）

### 安装器打包 `src/Installer/Installer.csproj`

- `EmbeddedResource`：`..\..\bin\Tailviewer.exe` → `..\..\bin\Tailviewer NEXT.exe`；`..\..\bin\Tailviewer.exe.config` → `..\..\bin\Tailviewer NEXT.exe.config`

### 本地化 `Strings.resx` / `Strings.zh-CN.resx`

- `<data name="Tailviewer">` 值 → `Tailviewer NEXT`（两个文件）
- 注意：`Strings.zh-CN.resx` 实际为 **GBK 编码正文 + UTF-8 BOM**（非标准 UTF-8，中文按 GBK 编码）。编辑时必须用 GBK（codepage 936）读写，否则中文损坏。见「编码风险」。

### 程序集元数据（可选、低风险）

- 主程序 `src/Tailviewer/Properties/AssemblyInfo.cs` 与 `src/Installer/Properties/AssemblyInfo.cs`：`[assembly: AssemblyProduct("Tailviewer")]` → `"Tailviewer NEXT"`（Explorer 文件属性可见）。
- 库 DLL（Api/Core/Archiver）保持 `AssemblyProduct("Tailviewer")`（技术身份），作为范围备注。

### 其它用户可见/硬编码品牌点

- `src/Tailviewer/Ui/MainWindowViewModel.cs`：`ReportIssue`/`SuggestFeature`/`AskQuestion` 的硬编码 `https://github.com/Kittyfisto/Tailviewer/...` → `https://github.com/brofea/Tailviewer-NEXT/...`
- `src/Tailviewer/App.cs` 日志文案（`"Tailviewer started"`、`LogEnvironment` 的 `"Tailviewer: v..."`）→ 品牌名改为 `Tailviewer NEXT`（日志文本，非功能）
- About 页 `TailviewerControl.xaml` 用 `Strings.Tailviewer`，随 resx 自动生效。

## R2：自动更新改为 GitHub Releases API

### 现状数据流（`src/Tailviewer/BusinessLogic/AutoUpdates/AutoUpdater.cs`）

`CheckForUpdates` → `QueryNewestVersions`（下载 `kittyfisto.github.io/.../version.xml`）→ `Parse`（XML `<versions><stable|beta .../></versions>`）→ `VersionInfo{Stable,Beta}` → `Install` → `DownloadAndInstall(Stable, StableAddress)` → `SaveInstaller` → `StartInstallation`（`update "{ApplicationFolder}"` 调起下载的安装器）。

### 新数据流

1. `CheckForUpdates(bool)` 不变。
2. `QueryNewestVersions()`：
   - `WebClient` 带 `User-Agent`（GitHub API 必需，否则 403）与代理设置，`DownloadString(BuildVersionCheckUri())`。
   - `BuildVersionCheckUri()` 返回 `https://api.github.com/repos/brofea/Tailviewer-NEXT/releases/latest`。
   - 解析 JSON（`DataContractJsonSerializer`，DTO 含 `tag_name` / `prerelease` / `assets[]`）。
   - `tag_name` 去掉前导 `v`/`V` → `Version.TryParse` → `Stable`；`StableAddress` = 资产中文件名含 `setup` 且 `.exe` 结尾者（否则退化为首个 `.exe` 资产）的 `browser_download_url`。
   - `Beta`/`BetaAddress` = null（fork 发布流程只发稳定 tag，稳定版优先；beta 支持 deferred）。
3. `Parse(byte[] data, out VersionInfo)` 改为解析上述 JSON（供单测）；签名保留以最小化外部改动，语义从 XML 变为 JSON。
4. 下载/安装/拉起安装器逻辑（`Download`、`SaveInstaller`、`StartInstallation`）不变。

### 代码变更

- `AutoUpdater.cs`：删 `Server`/`VersionFile`/`DownloadServer` 常量；新增 `LatestReleaseUri` 常量；`BuildVersionCheckUri` 重写；`QueryNewestVersions` 用 `DownloadString`；新增私有 `[DataContract]` DTO 与 JSON 反序列化；`ReadVersions`/`ReadVersion`（XML 专用）删除。
- 新增/更新单测：`src/Tailviewer.Tests/BusinessLogic/AutoUpdater/AutoUpdaterTest.cs` 的 `TestParse` 改为用新 JSON fixture `TestData\version.json`；删除对 XML 结构的断言。
- `AutoUpdateViewModel.cs`、`IAutoUpdater.cs`、`VersionInfo.cs` 不变（接口形状未变）。
- 验收测试 `Tailviewer.Acceptance.Tests/BusinessLogic/AutoUpdater/AutoUpdaterTest.cs` 属 `[LocalTest]`，仍会真实请求最新 Release，改后指向 fork（本地/手动运行）。

## R3：移除 changelog 功能

### 删除文件

- `src/Tailviewer/Changelog.cs`
- `src/Tailviewer/Ui/About/ChangelogViewModel.cs`
- `src/Tailviewer/Ui/About/ChangelogControl.xaml` + `.xaml.cs`
- `src/Tailviewer/BusinessLogic/ActionCenter/Change.cs`
- `src/Tailviewer/Ui/ActionCenter/ChangeViewModel.cs`
- `src/Tailviewer/Ui/ActionCenter/ChangeTemplate.xaml`
- `src/Tailviewer.Tests/ChangelogTest.cs`
- `src/Tailviewer.Tests/Ui/Controls/About/ChangelogViewModelTest.cs`

### 编辑文件

- `src/Tailviewer/Ui/About/AboutFlyoutDataTemplate.xaml`：删除 Changelog 选项卡（`FlatTabItem Header={x:Static loc:Strings.Changelog}` 整块）
- `src/Tailviewer/App.cs`：删除 `actionCenter.Add(Change.Merge(Changelog.MostRecentPatches));`
- `src/Tailviewer/Ui/ActionCenter/ActionCenterViewModel.cs`：无参构造删除 `AddNotification(Changelog.MostRecent)`；`AddNotification` 删除 `Change` 分支（`ChangeViewModel` 已删）
- `src/Tailviewer/Ui/ActionCenter/ActionCenterControl.xaml`：MergedDictionaries 删除 `ChangeTemplate.xaml` 一行
- `src/Tailviewer/Localization/Strings.cs`：删除 `Changelog` 属性
- `src/Tailviewer/Localization/Strings.resx` + `Strings.zh-CN.resx`：删除 `<data name="Changelog">` 块
- `src/Tailviewer.Tests/Tailviewer.Tests.csproj`：删除 `ChangelogTest.cs` 与 `Ui\Controls\About\ChangelogViewModelTest.cs` 两个 `<Compile Include>` 行

### 保留（不属 changelog）

- `Tailviewer.Archiver` / `PluginRepository` 的 `SerializableChange`、`IChange`（插件描述变更，与 UI changelog 无关）
- ActionCenter 的 `Build`/`Notification`/`Bug`/`Export` 通知体系
- `Strings.Features`/`Bugfixes`/`Miscellaneous`（可能仅 changelog 使用；删除无害，但为控噪保留）

## 编码风险（关键）

`src/Tailviewer/Localization/Strings.zh-CN.resx` 是 **UTF-8 BOM + GBK 正文** 的混合编码文件（已用 GBK/936 解码验证：`Changelog`→"更新日志"，`MenuAbout`→"关于"）。`Strings.resx` 为纯 ASCII（安全）。

- 所有对 `Strings.zh-CN.resx` 的修改必须用 PowerShell + `[System.Text.Encoding]::GetEncoding(936)` 读取、写入回 GBK，**禁止** 用 `edit`/`write` 工具直接改，否则中文全部变乱码。
- `Strings.resx` 可用 UTF-8（ASCII 内容，等价）。

## 兼容性 / 回滚

- 程序集强名称身份、命名空间、`App.config` binding redirect、pack URI 均不变 → 插件与引用兼容。
- 单实例互斥由「进程名」匹配 `Tailviewer NEXT`；老 `Tailviewer.exe` 与新 `Tailviewer NEXT.exe` 会因进程名不同而互不可见（可接受的迁移点，一次性）。
- 回滚：`git checkout` 全部改动文件 + 删除新增 fixture/target；无数据迁移（AppData 目录未变）。

## 兄弟任务依赖

- `08-22-github-actions-ci-release`（规划中，未提交）：其 `release.yml` 产出 `Tailviewer-setup-<version>.exe` 与 `Tailviewer-portable-<version>.zip`，是本任务 R2 的更新来源。本任务不改该任务，只消费其 Release 产物。
- 交叉注意：该任务设计中的便携包路径 `%ProgramFiles%\Tailviewer` 与安装器默认目录 `%ProgramFiles%\Tailviewer NEXT` 会不一致；实现本任务时可在设计侧记录，但修改归该任务/后续统一处理。

## 风险 / 待验证

- GitHub API 未认证限流 60 次/小时/IP；对个人应用足够，若需更稳可后续加 token（deferred）。
- exe 文件名含空格（`Tailviewer NEXT.exe`）：`Process.GetProcessesByName("Tailviewer NEXT")`、`ProcessStartInfo`、安装器嵌入均已验证可处理空格；仍需实机 smoke test。
- `DataContractJsonSerializer` 在 net48 下可用（`System.Runtime.Serialization`）；实现时无需新增 NuGet 包。
