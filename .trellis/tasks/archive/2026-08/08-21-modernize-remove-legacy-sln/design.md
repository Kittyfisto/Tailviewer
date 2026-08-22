# Design — 清理遗留解决方案并统一 .NET 4.8

## 目标架构

单一解决方案 `Tailviewer.sln`，所有项目（主程序、Installer、Api/Core/Archiver 库、PluginRepository 系列、Serilog 插件、各类测试、LogLevelPlugin 示例）目标框架统一为 .NET Framework 4.8，旧式 csproj + packages.config 保持不变。

## 边界与改动面

### 1. 删除遗留内容

| 路径 | 说明 |
|------|------|
| `Tailviewer-Complete.sln` | 历史遗留解决方案，引用已删除项目与 playground |
| `Tailviewer-Complete.sln.DotSettings` | 遗留解决方案的 ReSharper 设置 |
| `playground\`（整目录） | 10 个实验项目 + `Live\` 残留（`FileLogger.txt`、`SQLiteLogger.db`），均未被 `Tailviewer.sln` 引用 |

不删除：`Tailviewer.sln.DotSettings`（主解决方案设置）、`docs\`、`examples\`（被主 sln 引用）、`scripts\`、`tools\`、`Keys\`、`sig\`。

### 2. 目标框架统一（v4.7.1 → v4.8）

| 文件 | 修改点 |
|------|--------|
| `src\Tailviewer\Tailviewer.csproj` | `<TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>` → `v4.8`；`BootstrapperPackage Include=".NETFramework,Version=v4.7.1"` 及 `ProductName` → `v4.8` |
| `src\Tailviewer\App.config` | `<supportedRuntime ... sku=".NETFramework,Version=v4.7.1" />` → `v4.8` |
| `src\Installer\Installer.csproj` | 同上（TargetFrameworkVersion 与 BootstrapperPackage） |
| `src\Installer\App.config` | 同上（supportedRuntime sku） |

不修改：各项目 `packages.config` 中的 `targetFramework="net471"`（与 `net48` 兼容，NuGet 可正常解析），以及 `PluginPacker.cs`/`PluginPackerTest.cs` 中的 "4.7.1" 行为注释。

### 3. 文档与 spec 同步

| 文件 | 修改点 |
|------|--------|
| `README.md` | 第 9 行 ".NET 4.7.1" → "4.8"，更新下载链接 |
| `.trellis/spec/build/index.md` | 第 4、13 行：移除对两个解决方案的描述、目标框架改为 4.8 |
| `.trellis/spec/index.md` | 第 16 行：`src/Tailviewer` 目标框架改为 4.8 |

## 数据流与契约

- 构建契约不变：各项目仍通过旧式 `ProjectReference` + 强命名 `sig\key.snk` 签名；`OutputPath` 统一为 `..\..\bin\`。
- NuGet 契约不变：packages.config 驱动，restore 到仓库根 `packages\`（gitignored）。

## 兼容性说明

- .NET Framework 4.8 向下兼容 4.7.1；升级主程序/Installer 不影响已引用它的项目。
- `packages.config` 的 `net471` 目标与 `net48` 项目兼容，无需改动。

## 回滚

- 全部改动为文件删除 + 少量 XML 文本替换，通过 git 即可整体回滚（`git checkout` 删除项、`git restore` 修改项）。删除的 `playground\` 与 `Tailviewer-Complete.sln` 均为 git 跟踪文件，可随时恢复。
