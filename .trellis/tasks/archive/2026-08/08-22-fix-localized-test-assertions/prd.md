# 修复本地化后 4 个仍硬编码英文断言的测试

## Goal

上一个本地化任务把 UI 文案迁移到 `Strings.resx`，但留下了 4 个仍硬编码英文断言的测试。在本机 `CurrentUICulture=zh-CN` 下这些断言必然失败。本任务将这些断言改为引用 `Strings.<Key>`，使测试在任意 UI 文化下（CI 的 en-US 与本地 zh-CN）都能通过。

## Requirements

- `MainWindowViewModelTest.cs` 中 3 处菜单标题断言改用 `Strings` 键：
  - `TestShowLog`：`x.Header == Strings.MenuShowLog`
  - `TestShowQuickNavigation`：`x.Header == Strings.MenuGoToDataSource`
  - `TestShowGoToLine`：`x.Header == Strings.MenuGoToLine`
- `AutoUpdateViewModelTest.cs` 中 2 处标题断言改用 `Strings` 键：
  - `InstallTitle.Should().Be(Strings.Install)`
  - `InstallTitle.Should().Be(Strings.Downloading)`
- 不允许把测试固定到 `en-US` 文化下运行；测试必须在 CI（en-US）与本地（zh-CN）两种 `CurrentUICulture` 下都通过。
- 保持既有代码风格（文件为 UTF-8 BOM、制表符缩进），仅修改断言，不改动生产代码。

## Acceptance Criteria

- [ ] 4 个失败的测试断言全部改用 `Strings.<Key>`，不再硬编码英文文案。
- [ ] 未引入任何固定 `en-US` 文化或修改 `CurrentUICulture` 的代码。
- [ ] 在本地（`zh-CN`）运行相关测试全部通过。
- [ ] 在 `en-US` 文化下运行相关测试全部通过（验证 CI 路径）。

## Notes

- 相关键已确认存在于 `Strings.cs`：`MenuShowLog`、`MenuGoToDataSource`、`MenuGoToLine`、`Install`、`Downloading`。
- 轻量任务，仅需 PRD。
