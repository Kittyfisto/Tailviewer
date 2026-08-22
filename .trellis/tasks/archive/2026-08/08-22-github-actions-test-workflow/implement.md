# 实施计划：GitHub Actions 测试工作流

## 执行清单

- [x] 1. 创建 `.github/workflows/` 目录。
- [x] 2. 编写 `.github/workflows/test.yml`：
  - `name`：CI / Test
  - `on`：`workflow_dispatch` + `pull_request: branches: [master]`
  - `jobs.test`：`runs-on: windows-2022`
  - steps：
    1. `actions/checkout@v4`
    2. `microsoft/setup-msbuild@v2`
    3. 下载 `nuget.exe`（`https://dist.nuget.org/win-x86-commandline/latest/nuget.exe`）
    4. `nuget restore Tailviewer.sln`
    5. MSBuild 逐个构建 7 个测试项目（**Debug**，`/m`，统一输出到 `bin\`）
    6. `nuget install NUnit.ConsoleRunner -Version 3.12.0 -OutputDirectory $env:RUNNER_TEMP`
    7. `nunit3-console.exe` 逐程序集运行 `bin\*.Tests.dll`，`--result=TestResult-*.xml --labels=Before`；Serilog 程序集附加 `--where "class != 'Tailviewer.Serilog.Test.SerilogAcceptanceTest'"`
    8. `actions/upload-artifact@v4` 上传 `TestResult-*.xml`（`if: always()`）
- [x] 3. 校验 YAML 语法（PyYAML 通过）。
- [x] 4. 确认测试程序集名称与 `bin\` 输出一致（`Tailviewer.SystemTests.dll`、`Tailviewer.Serilog.Test.dll`）。
- [x] 5. 修复首次 CI 暴露的 4 个既有测试缺陷（详见 design.md 第 5 节）。

## 验证命令

- YAML 语法：`python -c "import yaml; yaml.safe_load(open('.github/workflows/test.yml', encoding='utf-8'))"`。
- 本地冒烟（已执行）：
  - `msbuild src\Tailviewer.Tests\Tailviewer.Tests.csproj /t:Build /p:Configuration=Debug`
  - `msbuild src\Tailviewer.Archiver.Tests\Tailviewer.Archiver.Tests.csproj /t:Build /p:Configuration=Debug`
  - `nunit3-console.exe bin\Tailviewer.Archiver.Tests.dll` → 70/70 通过
  - 4 个修复的测试单独运行均通过。

## 审查门槛

- `on` 块包含 `workflow_dispatch` 与指向 `master` 的 `pull_request`。
- 步骤顺序为 restore → build → test，无 `Installer` / Acceptance.Tests 参与。
- 测试失败会向上传播非零退出码。
- Serilog 验收测试被精确跳过，纯单元测试保留。

## 回滚点

- 删除 `.github/workflows/test.yml` 即可；3 个测试源文件的修复为独立提交，可单独 revert。
