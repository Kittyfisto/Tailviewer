# Implement — 清理遗留解决方案并统一 .NET 4.8

## 执行清单（按序）

1. **删除遗留文件**
   - `git rm Tailviewer-Complete.sln Tailviewer-Complete.sln.DotSettings`
   - `git rm -r playground`
   - 确认 `git status` 中只出现上述删除项。

2. **框架升级（4.7.1 → 4.8）**
   - `src\Tailviewer\Tailviewer.csproj`：`TargetFrameworkVersion` v4.7.1 → v4.8；`BootstrapperPackage Include=".NETFramework,Version=v4.7.1"` → v4.8；`ProductName` "4.7.1" → "4.8"。
   - `src\Tailviewer\App.config`：`sku=".NETFramework,Version=v4.7.1"` → `v4.8`。
   - `src\Installer\Installer.csproj`：同上三处。
   - `src\Installer\App.config`：`sku` → `v4.8`。

3. **文档/spec 同步**
   - `README.md` 第 9 行：".NET 4.7.1" → ".NET 4.8"。
   - `.trellis/spec/build/index.md`：第 4 行移除双解决方案描述、第 13 行 4.7.1 → 4.8。
   - `.trellis/spec/index.md`：第 16 行 4.7.1 → 4.8。

4. **在 VS 2026 中编译**
   - 通过 VS MCP：确认已打开 `Tailviewer.sln`，触发 Build。
   - 首次构建会执行 NuGet restore（自动）；若 restore 失败，检查 NuGet 源，向用户确认自定义包（Metrolib/MMQ/SharpRemote/System.Threading.Extensions）的可用源。

5. **运行主程序**
   - 通过 VS MCP 启动（F5/Ctrl+F5）`Tailviewer`，确认进程启动、无启动崩溃。

## 验证命令

- 构建：VS MCP `VS_build_solution`，随后轮询 `VS_build_status` 至 `Done`，`FailedProjects` 为 0。
- 编译错误检查：`VS_errors_list`（severity=Error）应为空。
- 运行：VS MCP `VS_debugger_launch`（或 `VS_debugger_launch_without_debugging`）后 `VS_debugger_status` 为 Run 态，进程名含 Tailviewer。

## 风险点 / 回滚点

- **回滚点 1**（删除后、框架升级前）：`git restore --staged` + `git checkout` 可恢复删除项。
- **回滚点 2**（框架升级后、构建前）：若构建因框架问题失败，`git restore` 四个 csproj/config 文件。
- **NuGet 阻塞**：若 restore 失败，先不继续改代码，向用户报告缺失包与建议源。

## 提交前检查（Phase 3 收尾）

- `git status`/`git diff` 复核：仅删除项 + 4 个 csproj/config + 3 个文档文件。
- 不在本任务内提交（等待用户指示 commit）。
