# 技术设计：GitHub Actions CI + Release（portable 优先）

## 目标

两个工作流 + 版本注入 + portable staging 产物重构，不改变应用运行时行为，仅改构建/发布链路。

## 运行环境

| 项 | 选择 | 理由 |
| --- | --- | --- |
| runner | `windows-2022` | 预装 .NET Framework 4.8 + MSBuild，支持经典 csproj/WPF |
| 构建配置 | `Release` | 主程序/插件/Installer 的 Release OutputPath 均仓库根 `bin\`；Debug 仅测试项目用 |
| 签名 | `sig/key.snk`（开发者 key） | 零 secret；官方 key deferred |
| 版本工具 | GitVersion（`gittools/actions` setup+execute） | 从 tag 推导 SemVer，注入 3 段 |

## 架构：portable staging 唯一源

```
[构建全部运行时项目]
        │  (输出到 bin\)
        ▼
[scripts/stage-portable.ps1]  ← 组装 artifacts/portable/
        │   (运行时文件 + 卫星 + LICENSE/Uninstall/Fonts/Icons/Readme)
        ├───────────────► [tar/Compress-Archive] ──► Tailviewer-NEXT-portable-<ver>.zip
        └───────────────► [msbuild Installer.csproj 嵌入 staging] ──► Tailviewer-NEXT-setup-<ver>.exe
```

- staging 目录（如 `artifacts/portable/`，gitignore）是唯一真实产物源。
- portable zip 与 Installer 都从 staging 生成；**不执行 silentinstall**。

## 关键决策

### 1. 版本注入（GitVersion）

- 新增 `GitVersion.yml`（mode `Mainline`，从 tag `v1.2.3` → `1.2.3`）。
- 产物映射：`AssemblyVersion = AssemblySemVer (1.2.3.0)`；`FileVersion = MajorMinorPatch (1.2.3)`；`InformationalVersion = 1.2.3+<sha>`。
- 注入路径（实现前 spike 验证，二选一）：
  - (A) CI 跑 `gitversion` 输出变量 → `scripts/set-version.ps1` 在构建前把版本写入 `src/GlobalAssemblyInfo.cs`（AssemblyVersion=x.x.x.0 / FileVersion=x.x.x / InformationalVersion=x.x.x）；各经典项目 `Properties/AssemblyInfo.cs` 的重复版本属性收敛到 `GlobalAssemblyInfo.cs`。
  - (B) GitVersion.MsBuild 包注入（需验证对 classic csproj + `GenerateAssemblyInfo=false` 的兼容性）。
- 应用显示版本改为 3 段：`Constants.ApplicationVersion` 改读 `FileVersionInfo.ProductVersion`/`AssemblyInformationalVersion`，或 `Format()` 去尾 `.0`（spike 决定最小改动点）。

### 2. staging 文件清单（完整运行时集）

来源 = 现有 `Installer.csproj` 嵌入清单（主 exe/dll/config/插件 `.tvp`/archive.exe）+ 本地化卫星：

```
Tailviewer NEXT.exe, Tailviewer NEXT.exe.config, log4net.dll, Metrolib.dll, MMQ.dll,
Tailviewer.Api.dll, Tailviewer.Core.dll, archive.exe, SharpRemote.dll,
System.Extensions.dll, Ookii.Dialogs.Wpf.dll, Xceed.Wpf.Toolkit.dll,
Plugins\Tailviewer.Formats.Serilog.1.0.0.0.tvp,
zh-CN\Tailviewer.resources.dll,
cs-CZ|de|es|fr|hu|it|pt-BR|ro|ru|sv|zh-Hans\Xceed.Wpf.AvalonDock.resources.dll,
LICENSE, Uninstall.cmd, Plugins\Readme.txt, Fonts\FONTLOG.txt|OFL.txt|METADATA.pb, Icons\Tailviewer.ico
```

- 排除 `*.pdb` 及测试垃圾（`bin\` 内含大量测试产物，staging 只取上述白名单）。
- 改名任务已落地：可执行文件为 `Tailviewer NEXT.exe`（空格，Tailviewer.csproj 的 AfterBuild 复制），
  安装器 exe 仍为 `Tailviewer-setup.exe`（未改名）。发布 artifact 按 `Tailviewer-NEXT-setup-<ver>.exe` /
  `Tailviewer-NEXT-portable-<ver>.zip`（无空格）命名。

### 3. Installer.csproj 改造

- 移除逐文件 `EmbeddedResource Include="..\..\bin\..."` 清单，改为一条通配嵌入 staging：
  `<EmbeddedResource Include="..\..\artifacts\portable\**\*" Exclude="..\..\artifacts\portable\**\*.pdb">` + `LogicalName` 变换保留 `InstallationFiles\` 前缀与相对目录（`%(RecursiveDir)`）。
- 保留 Installer 自身的 WPF `<Resource>`（安装器 UI 图标）与 COM 引用；LICENSE/Uninstall.cmd/Fonts/Readme 等随 staging 一并嵌入。
- 与并行改名任务的 `AssemblyName` 改动位于 csproj 不同区域，冲突风险低。

### 4. ci.yml（test ∥ build）

- `on`: `workflow_dispatch` + `pull_request: branches: [master]`。
- `test` job：原样迁移现有 test.yml（7 测试工程 + NUnit，跳过 SerilogAcceptanceTest）。
- `build` job：nuget restore → GitVersion 注入 → 按序构建（Archiver→Serilog→Installer）→ staging → 上传 `Tailviewer-NEXT-setup.exe` artifact。
- 两 job 不写 `needs`（并行）。

### 5. release.yml（tag 触发 + master 守卫）

- `on: push: tags: ['v*']`，`permissions: contents: write`。
- 守卫：`git merge-base --is-ancestor "$env:GITHUB_SHA" origin/master`，非 0 即失败（tag 不在 master）。`branches`+`tags` 是 OR 语义，故不能写 `branches:[master]`。
- 步骤：checkout(fetch-depth 0) → 守卫 → nuget restore → GitVersion → 构建 → stage-portable → portable zip → 构建 Installer → 重命名两产物 → `softprops/action-gh-release@v2` 上传。
- **portable 步骤失败即整体失败**（步骤串行，任一步失败 job 失败，不会进入发布步骤）。
- 版本号 `$env:GITHUB_REF_NAME -replace '^[vV]',''`（GitVersion 输出一致则取其一）。

## 数据流

```
ci.yml:   checkout → restore → ┬ [test] msbuild×7(Debug) → nunit3-console → upload results
                               └ [build] GitVersion → msbuild Archiver→Serilog→Installer(Release) → upload setup.exe

release.yml: checkout(0) → is-ancestor(master) → restore → GitVersion
             → msbuild Archiver→Serilog→Tailviewer(Release)
             → stage-portable → zip(portable) → msbuild Installer(嵌入 staging)
             → rename Tailviewer-NEXT-* → gh-release 上传 {setup,portable}
```

## 风险与回滚

| 风险 | 缓解 / 回滚 |
| --- | --- |
| GitVersion 注入对混合 csproj 的兼容性 | 实现前 spike；回退到 set-version.ps1 patch 方案 |
| staging 白名单遗漏运行时文件 | 以 Installer.csproj 现嵌入清单为基准 + 卫星目录；release 上传前可加冒烟校验 |
| 与并行改名任务冲突 | 本任务只改 csproj 嵌入块（不同区域）；产物名以并行任务为准 |
| `TreatWarningsAsErrors=true` Release 构建失败 | 仓库既有约束，本地已有成功 Release 构建 |
| 回滚 | 删除新增 YAML/脚本 + `git checkout src/Installer/Installer.csproj src/GlobalAssemblyInfo.cs`；无应用源码改动 |
