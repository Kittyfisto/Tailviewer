# Rename data folders, registry and install dir to Tailviewer NEXT

## Goal

将 Tailviewer NEXT 的所有数据/身份位置统一为品牌名 `Tailviewer NEXT`（含空格）：
AppData 目录、Documents 目录、注册表卸载项、安装目录，全部使用 `Tailviewer NEXT`，消除残留的裸 `Tailviewer`。

## Background / Confirmed Facts

- 上一轮 rebrand 已完成「显示名 + 可执行文件名 + 安装目录 + 注册表」→ `Tailviewer NEXT`：
  - `src/Installer/Constants.cs`：`ApplicationTitle = "Tailviewer NEXT"`；
    `DefaultApplicationFolder = %ProgramFiles%\Tailviewer NEXT`（派生自 ApplicationTitle）。
  - `src/Installer/Installer.cs`：注册表卸载项 `CreateSubKey(Constants.ApplicationTitle)` →
    `HKLM\...\Uninstall\Tailviewer NEXT`，`DisplayName = "Tailviewer NEXT"`，`Publisher = "brofea"`。
- **尚未改名**（本任务实际改动点）：
  - `src/Tailviewer/Constants.cs:11`：`private const string ApplicationFolderName = "Tailviewer";`
    → 同时决定 AppData（`AppDataLocalFolder`）与 Documents（`MyDocumentsFolder`）目录名。
  - `src/Installer/Constants.cs:22`：`InstallationLog = %LOCALAPPDATA%\Tailviewer\Installation.log`。
- 派生关系：AppData/Documents 下的 Settings、日志、Downloads/Plugins、Export、Snapshots、Analyses、Templates
  全部通过 `ApplicationFolderName` 派生，改这一处即覆盖；`InstallationLog` 另有一处字面量。
- `Core.Constants.ApplicationFolder` = 程序集所在目录（安装目录），动态获取，无硬编码。
- 不在本任务范围（测试/元数据，非用户路径）：
  - 测试隔离目录 `Path.GetTempPath()\Tailviewer\Tests`（多个测试文件）。
  - `AssemblyProduct("Tailviewer")`（Api/Core/Archiver 程序集元数据；主程序与安装器已是 NEXT）。
  - 本地化资源键名 `Strings.cs` 的 `"Tailviewer"`、测试夹具里的 `E:\Tailviewer\...` 等。

## Requirements

- R1: AppData 目录 → `%LOCALAPPDATA%\Tailviewer NEXT`（改 `ApplicationFolderName`）。
- R2: Documents 目录 → `%USERPROFILE%\Documents\Tailviewer NEXT`（同 R1 一处）。
- R3: 安装器日志 `InstallationLog` → `%LOCALAPPDATA%\Tailviewer NEXT\Installation.log`。
- R4: 注册表卸载项 → 已为 `Tailviewer NEXT`（无需改，验证即可）。
- R5: 安装目录 → 已为 `Tailviewer NEXT`（无需改，验证即可）。

## Acceptance Criteria

- [ ] AC1: `ApplicationFolderName` 为 `Tailviewer NEXT`，全新运行后 AppData 数据写入 `%LOCALAPPDATA%\Tailviewer NEXT\`。
- [ ] AC2: 全新运行后导出/快照/分析默认写 `%USERPROFILE%\Documents\Tailviewer NEXT\`。
- [ ] AC3: 安装器日志写到 `%LOCALAPPDATA%\Tailviewer NEXT\Installation.log`。
- [ ] AC4: 注册表卸载项路径与 DisplayName 均为 `Tailviewer NEXT`（验证现状，无改动）。
- [ ] AC5: 安装目录为 `%ProgramFiles%\Tailviewer NEXT`（验证现状，无改动）。
- [ ] AC6: 项目可成功编译（`TreatWarningsAsErrors` 下无新告警）。

## Key Decisions

| 决策 | 结论 |
| --- | --- |
| 旧数据迁移 | **不迁移，直接改**：忽略旧 `Tailviewer` 目录，视为新装（pre-release，无迁移负担） |
| 改动范围 | 仅 `src/Tailviewer/Constants.cs` + `src/Installer/Constants.cs` 两处字面量 |
| 注册表 / 安装目录 | 已为 `Tailviewer NEXT`，不改 |

## Out of Scope

- 测试隔离目录、程序集元数据（AssemblyProduct）、本地化资源键名的改名。
- 可执行文件/程序集/命名空间的改名（上轮已完成，保持 `Tailviewer`）。
- 旧目录（`%LOCALAPPDATA%\Tailviewer`、`Documents\Tailviewer`）的迁移或删除。
