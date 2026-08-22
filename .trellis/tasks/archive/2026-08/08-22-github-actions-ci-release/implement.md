# 执行计划：GitHub Actions CI + Release（portable 优先）

## 前置

- 本任务改构建/发布链路，不改应用运行时代码（仅 `GlobalAssemblyInfo.cs` 版本读取点、`Installer.csproj` 嵌入来源、新增脚本与 YAML）。
- 依赖并行任务「Tailviewer → Tailviewer-NEXT」改名先行/同步；产物名以并行任务为准。
- 实现前 spike：验证 GitVersion 注入方式（见 design.md 决策 1），再定 `set-version.ps1` vs GitVersion.MsBuild。

## 实施清单（按序）

1. GitVersion 配置
   - 新增 `GitVersion.yml`（Mainline，tag `v*` → SemVer）。
   - spike 并落地注入：新增 `scripts/set-version.ps1`（或 GitVersion.MsBuild）；收敛 `GlobalAssemblyInfo.cs` 与经典 `Properties/AssemblyInfo.cs` 的重复版本属性。
   - 改应用版本显示为 3 段（`Constants.ApplicationVersion` 读 File/Info 版本，或 `Format()` 去尾 `.0`）。

2. portable staging
   - 新增 `scripts/stage-portable.ps1`：从 `bin\` 按白名单复制到 `artifacts/portable/`（含本地化卫星）；`artifacts/` 加入 `.gitignore`。
   - 白名单 = design.md「staging 文件清单」。

3. Installer.csproj 改造
   - 移除逐文件 `EmbeddedResource` 清单，改通配嵌入 `..\..\artifacts\portable\**`，`LogicalName` 保留 `InstallationFiles\` 前缀与相对目录。

4. ci.yml
   - `on`: `workflow_dispatch` + `pull_request: branches: [master]`。
   - `test` job = 现 test.yml 全部 steps；`build` job = restore → GitVersion → 按序构建（Archiver→Serilog→Installer）→ stage → 上传 installer artifact。
   - 两 job 无 `needs`（并行）。

5. release.yml
   - `on: push: tags: ['v*']`；`permissions: contents: write`。
   - checkout(fetch-depth 0) → master 守卫 → restore → GitVersion → 构建 → stage → portable zip → 构建 Installer → 重命名 → `softprops/action-gh-release` 上传两产物。
   - portable 失败即整体失败（不进入发布步骤）。

6. 删除 `.github/workflows/test.yml`。

## 校验命令

- YAML：
  `python -c "import yaml,glob; [yaml.safe_load(open(f)) for f in glob.glob('.github/workflows/*.yml')]"`
- 本地构建 + staging 冒烟：
  ```powershell
  $env:PATH = "$pwd\bin;$env:PATH"
  nuget restore Tailviewer.sln
  gitversion /output json   # 验证版本推导
  msbuild src\Tailviewer.Archiver\Tailviewer.Archiver.csproj /t:Build /p:Configuration=Release /m /v:minimal
  msbuild src\Plugins\Tailviewer.Formats.Serilog\Tailviewer.Formats.Serilog.csproj /t:Build /p:Configuration=Release /m /v:minimal
  msbuild src\Tailviewer\Tailviewer.csproj /t:Build /p:Configuration=Release /m /v:minimal
  ./scripts/stage-portable.ps1 -Version 1.2.3
  Test-Path artifacts/portable/Tailviewer-NEXT.exe
  msbuild src\Installer\Installer.csproj /t:Build /p:Configuration=Release /m /v:minimal
  ```
- 版本断言：构建后 `(Get-Item bin\Tailviewer-NEXT.exe).VersionInfo.FileVersion` 应为 `1.2.3`（3 段）。
- 无 secret：`grep -n "secrets\." .github/workflows/*.yml` 应为空。

## 检查点 / 回滚点

- 回滚：`git checkout src/Installer/Installer.csproj src/GlobalAssemblyInfo.cs .github/workflows/test.yml`；删除新增 YAML/脚本/`GitVersion.yml`、`artifacts/`。
- 完成后跑 `trellis-check` 质量门，再 Phase 3 提交。
