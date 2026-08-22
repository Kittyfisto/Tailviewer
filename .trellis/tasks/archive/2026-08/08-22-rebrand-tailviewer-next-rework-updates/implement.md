# 执行计划：Tailviewer NEXT 品牌更新 + 更新机制重构

## 前置

- 只改源码/资源/测试；不提交，不跑 `task.py start`（等用户批准规划摘要）。
- 大量 `.cs` 文件带特殊编码（部分被识别为二进制）；用 PowerShell `Get-Content`/`Set-Content` + 正确编码读写，或 `edit` 工具（对纯 UTF-8 文件）。
- 改 `Strings.zh-CN.resx` 必须走 GBK 脚本（见下方 S3）。
- 改完后跑 `trellis-check`（构建 + 测试）。

## 执行清单

### S1：主程序品牌常量（`src/Tailviewer/Constants.cs`）

1. `ApplicationTitle = "Tailviewer NEXT"`。
2. 新增 `private const string ApplicationFolderName = "Tailviewer";`。
3. `MainWindowTitle = string.Format("Tailviewer NEXT, v{0}", ...)`。
4. `AppDataLocalFolder` / `MyDocumentsFolder` 用 `ApplicationFolderName` 拼接（替换 `ApplicationTitle`）。
5. `ProjectPage`/`GithubPage`/`ReportBugPage` → `https://github.com/brofea/Tailviewer-NEXT`（/issues/new）。

### S2：安装器常量（`src/Installer/Constants.cs`）

1. `ApplicationTitle = "Tailviewer NEXT"`。
2. `MainWindowTitle = "Installing Tailviewer NEXT, v{0}"`。
3. `Publisher = "brofea"`。
4. `ApplicationExecutable` 保持 `$"{ApplicationTitle}.exe"`（自动变 `Tailviewer NEXT.exe`）。

### S3：本地化（编码敏感）

1. `Strings.resx`（UTF-8）：`<data name="Tailviewer"><value>Tailviewer</value>` → `Tailviewer NEXT`；删除 `<data name="Changelog">` 块。
2. `Strings.zh-CN.resx`（GBK）：用 PowerShell 脚本以 GBK 读取，做同样两处替换（`Tailviewer`→`Tailviewer NEXT`，删除 `Changelog` 块），GBK 写回。
3. `Strings.cs`：删除 `public static string Changelog => ...` 属性（Tailviewer 属性保留，值取自 resx）。

### S4：exe 重命名（`src/Tailviewer/Tailviewer.csproj`）

1. 追加 `AfterBuild` Target：复制 `$(TargetPath)` → `$(TargetDir)Tailviewer NEXT.exe`、`$(TargetPath).config` → `$(TargetDir)Tailviewer NEXT.exe.config`（**只复制，不删除**原 `Tailviewer.exe`，否则 Installer/测试项目的 ProjectReference 解析会 CS0006）。
2. 保持 `AssemblyName = Tailviewer`。

### S5：安装器打包（`src/Installer/Installer.csproj`）

1. `EmbeddedResource Include="..\..\bin\Tailviewer.exe"` → `..\..\bin\Tailviewer NEXT.exe`（Link 与 LogicalName 同步）。
2. `..\..\bin\Tailviewer.exe.config` → `..\..\bin\Tailviewer NEXT.exe.config`。

### S6：单实例（`src/Tailviewer/SingleApplicationHelper.cs`）

1. `FindOtherTailviewers()`：`Process.GetProcessesByName("Tailviewer")` → `Process.GetProcessesByName("Tailviewer NEXT")`；保留 `Tailviewer.vshost`。

### S7：硬编码链接与日志（`src/Tailviewer/Ui/MainWindowViewModel.cs`、`src/Tailviewer/App.cs`）

1. `ReportIssue`/`SuggestFeature`/`AskQuestion` 的 `https://github.com/Kittyfisto/Tailviewer/...` → `.../brofea/Tailviewer-NEXT/...`。
2. `App.cs` 日志文案 `"Tailviewer started"`、`"Tailviewer: v..."` → `Tailviewer NEXT`。

### S8：自动更新改为 GitHub Releases API（`src/Tailviewer/BusinessLogic/AutoUpdates/AutoUpdater.cs`）

1. 删 `Server`/`VersionFile`/`DownloadServer`；新增 `LatestReleaseUri = "https://api.github.com/repos/brofea/Tailviewer-NEXT/releases/latest"`。
2. `BuildVersionCheckUri()` 返回 `LatestReleaseUri`。
3. `QueryNewestVersions()`：`WebClient.DownloadString` + `Headers["User-Agent"]`（+ 代理、`ServicePointManager` TLS12 保留），`Parse(data, out versions)`。
4. 新增 `[DataContract]` DTO（`tag_name`、`prerelease`、`assets[]`：`name`、`browser_download_url`），`DataContractJsonSerializer` 反序列化。
5. `Parse(byte[] data, out VersionInfo)`：解析 JSON；`Stable` = `tag_name` 去 `v` 前缀后 `Version.TryParse`；`StableAddress` = 资产名含 `setup` 且 `.exe` 结尾者（退化首个 `.exe`）的 `browser_download_url`；`Beta`/`BetaAddress` = null。删除 `ReadVersions`/`ReadVersion`（XML 逻辑）。

### S9：移除 changelog 功能

删除文件：

- `src/Tailviewer/Changelog.cs`
- `src/Tailviewer/Ui/About/ChangelogViewModel.cs`、`ChangelogControl.xaml`、`ChangelogControl.xaml.cs`
- `src/Tailviewer/BusinessLogic/ActionCenter/Change.cs`
- `src/Tailviewer/Ui/ActionCenter/ChangeViewModel.cs`、`ChangeTemplate.xaml`
- `src/Tailviewer.Tests/ChangelogTest.cs`、`src/Tailviewer.Tests/Ui/Controls/About/ChangelogViewModelTest.cs`

编辑文件：

1. `src/Tailviewer/Ui/About/AboutFlyoutDataTemplate.xaml`：删 Changelog 选项卡块。
2. `src/Tailviewer/App.cs`：删 `actionCenter.Add(Change.Merge(Changelog.MostRecentPatches));`。
3. `src/Tailviewer/Ui/ActionCenter/ActionCenterViewModel.cs`：无参构造删 `AddNotification(Changelog.MostRecent)`；`AddNotification` 删 `Change` 分支。
4. `src/Tailviewer/Ui/ActionCenter/ActionCenterControl.xaml`：MergedDictionaries 删 `ChangeTemplate.xaml`。
5. `src/Tailviewer.Tests/Tailviewer.Tests.csproj`：删两个 `<Compile Include>`（`ChangelogTest.cs`、`Ui\Controls\About\ChangelogViewModelTest.cs`）。

### S10：更新单测

1. `src/Tailviewer.Tests/BusinessLogic/AutoUpdater/AutoUpdaterTest.cs`：`TestParse` 改为读 `TestData\version.json` 并断言 `Stable` 版本/地址；新增 `TestData\version.json` fixture（模拟 `releases/latest` JSON）。
2. 删除与 `Changelog` 相关的测试引用（S9 已删文件，确认无残留 using/引用）。

## 校验命令（PowerShell）

```powershell
# 1) 构建主程序 + 安装器（Release，与 CI 顺序一致）
$env:PATH = "$pwd\bin;$env:PATH"
nuget restore Tailviewer.sln
msbuild src\Tailviewer.Archiver\Tailviewer.Archiver.csproj /t:Build /p:Configuration=Release /m /v:minimal
msbuild src\Plugins\Tailviewer.Formats.Serilog\Tailviewer.Formats.Serilog.csproj /t:Build /p:Configuration=Release /m /v:minimal
msbuild src\Installer\Installer.csproj /t:Build /p:Configuration=Release /m /v:minimal

# 2) 产物校验
Test-Path 'bin\Tailviewer NEXT.exe'          # True
Test-Path 'bin\Tailviewer NEXT.exe.config'   # True
Test-Path 'bin\Tailviewer-setup.exe'         # True（如决定 setup 也改名则见风险）

# 3) 测试（改到的单测工程）
msbuild src\Tailviewer.Tests\Tailviewer.Tests.csproj /t:Build /p:Configuration=Debug /m /v:minimal
nunit3-console bin\Tailviewer.Tests.dll --where "test =~ /AutoUpdater/ or test =~ /MainWindowViewModel/ or test =~ /AutoUpdate/"
```

## 风险文件 / 回滚点

| 风险 | 应对 |
| --- | --- |
| `Strings.zh-CN.resx` GBK 编码被写坏 | 只用 GBK 脚本读写；改完 `Get-Content -Encoding` 校验中文；回滚 `git checkout --` |
| exe 重命名 Target 失败/重复产物 | `AfterBuild` 幂等；若 `bin\Tailviewer.exe` 残留则安装器会嵌入旧文件——Target 中显式删除旧文件并校验 |
| `DataContractJsonSerializer` 解析失败 | 单测覆盖 JSON fixture；`QueryNewestVersions` 异常已由现有 try/catch 兜底（ActionCenter 报错） |
| 删除 `Change`/`ChangeViewModel` 有残留引用 | 全局搜索 `Change` 仅在 ActionCenter 插件描述（`SerializableChange` 无关）；删后编译报错即定位 |
| setup 是否改名 | 本任务默认不改 `Tailviewer-setup.exe`（Installer `AssemblyName` 不动），仅主 exe 改名；如需 setup 改名，另行确认并同步 CI-release 任务 |

## 完成后

- 跑 `trellis-check`（质量门禁）；通过后回到 Phase 3（spec 更新 + 提交，等用户指示）。
