# 研究：深色调色板与对比度 / 强调色交互

调查日期：2026-08-23

## 推荐深色调色板（VS Code 类中性灰，可调）

| 键 | 深色值 | 用途 |
|---|---|---|
| `Surface` | `#1E1E1E` | 主背景 |
| `SurfaceMuted` | `#2D2D30` | 弱表面/占位 |
| `TextPrimary`（新） | `#DCDCDC` | 默认前景 |
| `TextSecondary` | `#9E9E9E` | 次要前景 |
| `Divider` | `#3F3F46` | 分隔/边框 |
| `TitleBar` | `#252526` | 标题栏/底部信息条 |
| `OverlayBackground` | `#99000000` | flyout 遮罩（黑 60%） |
| 禁用前景 | `#6B6B6B` | `DisabledForegroundBrush` |
| 未聚焦选中（C#） | `#3F3F46` | `SelectedUnfocusedBackgroundBrush` |
| 交替行（C#） | `#252526` | `AlternatingBackgroundBrush` |

## WCAG 相对亮度对比（对 `Surface #1E1E1E`）

| 前景 | 对比度 | 达标 |
|---|---|---|
| `#DCDCDC` | ≈ 15.3:1 | AA / AAA |
| `#9E9E9E` | ≈ 6.5:1 | AA |
| `#FFFFFF`（PrimaryForeground 落在强调色底） | 取决于强调色 | 见下 |

## 强调色交互分析

- 默认强调色 `#0047AB`（深蓝）。`PrimaryForegroundBrush` = 白，落在 `PrimaryBrush`（`#0047AB`）底上：相对亮度 `#0047AB`≈0.026，白≈1.0 → 对比 ≈ (1.0+0.05)/(0.026+0.05) ≈ 13.8:1，AA/AAA 达标。
- `MainWindow` 背景与 `LogViewerControl` 工具栏是 `SecondaryBrush`（= 强调色），文字 `PrimaryForegroundBrush`（白）。**若用户把强调色调成很浅的颜色，白字对比不足**——这是既有行为，非本任务引入；验收时记录为已知限制，不扩大范围。
- 选中（选中背景=强调色 + 白前景）在深/浅两态一致，无需改动。
- 高亮 `#FFFF4D`+黑前景、高亮选中 `#FF9632`+黑前景，深/浅两态均对比充足，保持不变。

## 强调色在深色表面上的建议（供未来，不在本任务）

- 若后续想让强调色在深色下更醒目，可在 `ThemePalette`/`SemanticPalette` 增加"深色下自动 Lighten 强调色"的派生规则；本任务**不做**，保持强调色语义与 `ThemePaletteTest` 基线不变。

## 结论

- 深色可读性主要由 `TextPrimary`/`TextSecondary` 对 `Surface` 保证，基线值对比达标。
- 强调色交互不新增风险（既有白字-on-强调色行为延续）；仅记录浅色强调色的已知限制。
