# 实施计划：语义化 UI 设计基础 + 主界面重排（方向 A，已批准，已实施）

> 每个阶段独立可验证/可回滚。遵守 `design.md` §1 红线清单。语义资源给具体值、不留 TODO。
> 状态：已全部实施并验证（2026-08-22 收尾修正后复验）。

## 阶段 0：前置确认（已由主负责人批准）

- [x] 方向 A 已批准。
- [x] 范围 = MainWindow + LogViewerControl + 其直接使用的样式资源；不扩展侧栏/设置/About。
- [x] 不新增紧凑模式设置；保持 WPF/Metrolib、ViewModel/BusinessLogic、渲染/虚拟化不变。
- [x] 语义资源用具体基线值（等价复用现有颜色）。

## 阶段 1：资源字典（回滚点 R1）

1. [x] 新增 `src/Tailviewer/Themes/Semantic.xaml`：四个语义 Brush（`SurfaceBrush`/`SurfaceMutedBrush`/`TextSecondaryBrush`/`DividerBrush`），`Color` 用 `{DynamicResource}`（`Color` 键不在本地声明）。
2. [x] 经 `src/Tailviewer/Themes/Constants.xaml` merged dictionaries 合并 `Semantic.xaml`（**不改 App.cs 合并字典**）。
3. [x] 在 `src/Tailviewer/App.cs` 构造中于应用作用域发布语义 `Color` 键默认值（`Surface`/`SurfaceMuted`/`TextSecondary`/`Divider`，镜像强调色架构）。
4. [x] 验证：`dotnet build src/Tailviewer/Tailviewer.csproj` warning-free。

## 阶段 2：中性色迁移（回滚点 R2）

5. [x] `LogViewerControl.xaml`：根 `Background` → `{DynamicResource SurfaceBrush}`（根元素不能用 StaticResource，实测修正）；错误占位 `#D8D8D8` → `SurfaceMutedBrush`；`#A0A0A0`×2 → `TextSecondaryBrush`；图标 `Fill="White"` → `SurfaceBrush`；FindAll 头 `Foreground="White"` → `{DynamicResource PrimaryForegroundBrush}`；以上消费者全部 `{DynamicResource}`。
6. [x] `MainWindow.xaml`：flyout 分隔 `Fill="Gray"` → `{DynamicResource DividerBrush}`。
7. [x] 每批构建；grep 校验无 `{StaticResource Primary*}` 回归。

## 阶段 3：主界面重排 + 焦点桥接（回滚点 R3）

8. [x] `MainWindow.xaml`：Header 删除 `PartSearchBox`/`PartFindAllBox`，列数 6 → 4（logo/Menu/star/ActionCenter），标题后缀列置为 star。
9. [x] `LogViewerControl.xaml`：在现有空置 Row1（Auto）新增搜索行 Grid，放入 `controls:SearchTextBox`(x:Name=`PART_SearchBox`) 与 `controls:FilterTextBox`(x:Name=`PART_FindAllBox`)，绑定按 `design.md` §3（`Search.Term/ResultCount/CurrentResultIndex`、`FindAll.SearchTerm`、`CurrentDataSource` 折叠）。
10. [x] `LogViewerControl.cs`：新增 `public void FocusSearch()` / `public void FocusFindAll()`（`PART_SearchBox?.Focus()` 空安全）。
11. [x] `MainWindow.xaml.cs`：`FocusLogFileSearch`/`FocusLogFileSearchAll` 改为经 `FindLogViewer()`（`this.FindChildrenOfType<LogViewerControl>().FirstOrDefault()`）转发；删除对 `PartSearchBox`/`PartFindAllBox` 的引用；添加 `using Tailviewer.Ui.LogView;`（`System.Linq` 已有）。
12. [x] 验证：构建通过；主题色切换仍实时生效（强调色引用无 `{StaticResource}` 回归）。

## 阶段 4：回归与收尾

13. [x] 运行目标 UI STA 单测：`MainWindowTest`、`LogViewerControlTest`、`LogEntryListViewTest`、`TextCanvasTest` → **94/91/0/3**。
14. [x] FindAll 回归：`FilterTextBox` 与 FindAll 结果视图共享同一 `IFindAllViewModel`（代码核实，`research/bindings-findall-lifecycle.md`；`LogViewerControlTest` 通过）。
15. [x] 红线文件审计：`git diff --name-only` 无 §1 不可改文件。
16. [x] `git diff --check` 通过。

## 验证结果（实际执行）

- 构建：app + tests 项目均 0 警告 / 0 错误。
- 目标 UI STA：94 总 / 91 通过 / 0 失败 / 3 跳过。
- 全量 `Tailviewer.Tests.dll`：2365 总 / 2295 通过 / 67 失败 / 3 跳过；66 为基线失败（`git stash` 干净基线复跑一致，zh-CN 本地化 + 状态顺序依赖），1 为 `LogEntryListViewTest.TestMouseWheelDown2` 偶发 Freezable 跨线程竞态（隔离复跑 1/1 通过）。

## 验证命令

```bash
dotnet build src/Tailviewer/Tailviewer.csproj
dotnet build src/Tailviewer.Tests/Tailviewer.Tests.csproj

rg -n "StaticResource (Primary|Secondary)" src/Tailviewer          # 应为空
rg -n "StaticResource (SurfaceBrush|SurfaceMutedBrush|TextSecondaryBrush|DividerBrush)" src/Tailviewer  # 应为空

git diff --check
git diff --name-only   # 红线文件审计

# 目标 UI STA（nunit3-console 3.12.0）
nunit3-console.exe bin\Tailviewer.Tests.dll "--where=test =~ 'MainWindowTest' or test =~ 'LogViewerControlTest' or test =~ 'LogEntryListViewTest' or test =~ 'TextCanvasTest'"
```

## Review Gates

- G1（阶段 1 后）：`Semantic.xaml` 全具体值，merge 经 `Constants.xaml` 正确，`Color` 键仅在应用作用域发布。✅
- G2（阶段 2 后）：无强调色 `{StaticResource}` 回归；`Semantic.xaml` 不本地声明 `Color` 键。✅
- G3（阶段 3 后）：快捷键聚焦经桥接正常；`MainWindow.xaml.cs` 不再引用 `PartSearchBox`/`PartFindAllBox`。✅
- G4（阶段 4 后）：构建 warning-free + UI STA 单测通过 + 红线文件零改动。✅

## 回滚点

- R1：删除 `Constants.xaml` 中 `Semantic.xaml` merge + 删除 `Semantic.xaml` + 移除 `App.cs` 中语义 `Color` 发布 + 还原颜色迁移。
- R2：按文件 revert 中性色迁移。
- R3：revert 重排与桥接（恢复 Header 搜索框 + 原 code-behind）。
- 越界即回滚：任一步触碰 §1 红线文件，立即 revert 到最近回滚点并回到 Plan。
