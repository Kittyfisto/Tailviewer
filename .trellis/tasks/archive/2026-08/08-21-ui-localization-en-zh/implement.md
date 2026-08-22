# Implement — UI 本地化（en 基线 + zh-CN + 重启生效）

## 执行清单（按序）

### 阶段 1：框架搭建（可独立验证）

1. 新建 `src/Tailviewer/Properties/Strings.resx`（英文中性资源，初始少量 key）。
2. 新建 `src/Tailviewer/Properties/Strings.zh-CN.resx`（卫星资源，初始少量 key）。
3. 生成 `src/Tailviewer/Properties/Strings.cs` 强类型静态类（脚本从 `Strings.resx` 生成属性）。
4. 在 `Tailviewer.csproj` 显式声明两个 resx 为 `EmbeddedResource`（确保文化后缀 resx 作为卫星资源）。
5. 新建 `src/Tailviewer/Settings/UISettings.cs`（`Language` + `Save`/`Restore`/`Clone`）。
6. 接线 `ApplicationSettings`（新增 `Ui` 属性，接入 `Save()`/`Restore()`/`Clone()`）。
7. `App.cs` 启动时应用 `CurrentUICulture`（`settings.Restore` 后、构建 UI 前）。
8. `SettingsFlyoutViewModel` + `SettingsControl.xaml` 新增语言下拉框（含重启提示）。
9. 构建验证阶段 1：编译通过 + 运行，确认设置里出现语言选择。

### 阶段 2：提取字符串（主体工作）

10. 提取全部 XAML 硬编码文本（50 文件，~115 处）→ `Strings.<Key>`，key 同步补入两个 resx。
11. 提取全部 C# ViewModel/code-behind 字符串（88 文件，数百处）→ `Strings.<Key>`。
12. 逐文件核对：无遗漏硬编码 UI 文本；`Language.cs` 的 `Pluralize`/`Count` 保持不动。

### 阶段 3：翻译与收尾

13. 依据英文 `Strings.resx` 生成完整 `Strings.zh-CN.resx`（AI 机翻占位）。
14. 全量构建 + 运行验证（切中文重启 / 切回英文）。
15. 更新 spec（`.trellis/spec/`，若涉及 UI/构建约定）。

## 验证命令

- 构建：`& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Tailviewer.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal`
- VS MCP：`VS_build_solution` → `VS_build_status` 至 `FailedProjects=0`；`VS_errors_list`（Error）= 0。
- 运行验证：`VS_debugger_launch_without_debugging`，确认进程启动；改语言为 zh-CN 后重启，确认界面中文；切回 en 重启确认英文。

## 风险点 / 回滚点

- **回滚点 1**（框架搭建后、提取前）：`git restore` 新增文件即可回滚，不影响现有功能。
- **resx 卫星资源未生效**（zh-CN 仍显示英文）：检查 csproj 中 resx 的 `ManifestResourceName` 与
  文化后缀处理；确保 `Strings.zh-CN.resx` 被 `GenerateResource` 标记 `WithCulture=true`。
- **`x:Static` 与复合字符串**：XAML 中带 `<Run>` 的多行说明需拆分为多个 key，避免单个 key 含
  换行/格式符导致的显示问题。
- **编译错误**：替换大量字符串易引入拼写错误，分文件提交/构建，及时定位。

## 提交前检查（Phase 3 收尾）

- `git status`/`git diff` 复核：新增 resx/cs + 修改的 XAML/cs 文件，无夹带无关改动。
- 构建 0 失败；中英文切换均验证通过。
- 不在本任务内提交（等待用户指示 commit）。
