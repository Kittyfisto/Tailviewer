#!/usr/bin/env python3
"""Generate localization artifacts for the Tailviewer UI.

Maintain the single source of truth in the STRINGS dict below, then run:

    python tools/generate_localization.py

This regenerates:
  - src/Tailviewer/Localization/Strings.resx        (English, neutral/fallback)
  - src/Tailviewer/Localization/Strings.zh-CN.resx  (Simplified Chinese satellite)
  - src/Tailviewer/Localization/Strings.cs          (strongly-typed accessor)

Each entry maps a PascalCase key to an (English, zh-CN) tuple.
"""

import io
import os
import sys
import xml.sax.saxutils as sax

XAML_ATTRS = ("Text", "Header", "Title", "Content", "ToolTip", "Watermark")

# ---------------------------------------------------------------------------
# Single source of truth: key -> (English, Simplified Chinese)
# ---------------------------------------------------------------------------
STRINGS = {
    # --- Settings: Language ---
    "Language": ("Language", "语言"),
    "LanguageRestartNotice": (
        "Changing the language requires Tailviewer NEXT to be restarted to take effect.",
        "更改语言需要重启 Tailviewer NEXT 后生效。"),
    "English": ("English", "English"),
    "ChineseSimplified": ("中文（简体）", "中文（简体）"),

    # --- Settings: Theme ---
    "ThemeGroup": ("Theme", "主题"),
    "ThemeColor": ("Theme color", "主题色"),
    "ThemeModeLight": ("Light", "浅色"),
    "ThemeModeDark": ("Dark", "深色"),
    "ThemeModeSystem": ("System", "跟随系统"),
    "RestoreDefaultColors": ("Restore default colors", "恢复默认配色"),

    # --- Menus (MainWindow.xaml) ---
    "MenuFile": ("_File", "文件(_F)"),
    "MenuEdit": ("_Edit", "编辑(_E)"),
    "MenuView": ("_View", "视图(_V)"),
    "MenuHelp": ("_Help", "帮助(_H)"),

    # --- Settings ---
    "ExportGroup": ("Export", "导出"),
    "ExportFolder": ("Export folder", "导出文件夹"),
    "LogViewerGroup": ("Log Viewer", "日志查看器"),
    "ScrollSpeed": ("Scroll speed", "滚动速度"),
    "FontSize": ("Font Size", "字体大小"),
    "TabWidth": ("Tab Width", "制表符宽度"),
    "LogLevelsGroup": ("Log Levels", "日志级别"),
    "ForegroundColor": ("Foreground Color", "前景色"),
    "BackgroundColor": ("Background Color", "背景色"),
    "LevelOther": ("Other", "其他"),
    "LevelTrace": ("Trace", "跟踪"),
    "LevelDebug": ("Debug", "调试"),
    "LevelInfo": ("Info", "信息"),
    "LevelWarning": ("Warning", "警告"),
    "LevelError": ("Error", "错误"),
    "LevelFatal": ("Fatal", "致命"),
    "DataSourcesGroup": ("Data Sources", "数据源"),
    "FolderDataSourceRecursive": (
        "When checked, newly created folder data sources will display all files in the selected folder AND its subfolders.",
        "勾选后，新建的文件夹数据源将显示所选文件夹及其子文件夹中的所有文件。"),
    "FolderDataSourceFilter": ("Folder data source filter", "文件夹数据源过滤器"),
    "FolderPatternHint1": (
        "Newly created folder data sources will use this pattern to select files which shall be displayed.",
        "新建的文件夹数据源将使用此模式来选择要显示的文件。"),
    "FolderPatternHint2": (
        "A pattern such as '*.log' (without quotes) selects files which end in .log.",
        "例如模式 '*.log'（不带引号）会选择所有以 .log 结尾的文件。"),
    "FolderPatternHint3": (
        "'*.txt;*.log' (without quotes) selects files which end in either .txt or .log.",
        "'*.txt;*.log'（不带引号）会选择所有以 .txt 或 .log 结尾的文件。"),
    "FolderPatternHint4": (
        "'*bb*' (without quotes) selects files which have bb in their file name.",
        "'*bb*'（不带引号）会选择文件名中包含 bb 的文件。"),
    "DefaultTextFileEncoding": ("Default text file encoding", "默认文本文件编码"),
    "EncodingHint1": (
        "If anything else but 'Auto detect' is selected, then Tailviewer NEXT will only use the given encoding to decode text-based log files.",
        "如果选择的是「自动检测」以外的选项，Tailviewer NEXT 将仅使用所选编码来解码基于文本的日志文件。"),
    "EncodingHint2": (
        "Changing this value requires Tailviewer NEXT to be restarted in order to take effect.",
        "更改此值需要重启 Tailviewer NEXT 后生效。"),
    "CustomFormatsGroup": ("Custom Log file Formats", "自定义日志文件格式"),
    "WindowGroup": ("Window", "窗口"),
    "AlwaysOnTop": ("Always on top", "始终置顶"),
    "UpdatesGroup": ("Updates", "更新"),
    "NotifyWhenUpdateAvailable": (
        "Notify me when an update becomes available",
        "当有新版本可用时通知我"),
    "AutoInstallNewest": (
        "Automatically install the newest versions",
        "自动安装最新版本"),
    "ProxyHint": (
        "If you're behind a proxy server, then you might need to fill out these fields in order to be automatically notified when an update becomes available.",
        "如果你在代理服务器后面，则可能需要填写以下字段，以便在有新版本可用时自动收到通知。"),
    "ProxyUsername": ("Proxy username", "代理用户名"),
    "ProxyPassword": ("Proxy password", "代理密码"),
    "ProxyServer": ("Proxy server", "代理服务器"),
    "PluginRepositoriesHint": (
        "Third party plugins will be automatically updated from the following repositories:",
        "第三方插件将从以下仓库自动更新："),

    # --- Log view / data source tree ---
    "DataSourceFilter": ("Filter data sources (Ctrl+E)", "筛选数据源 (Ctrl+E)"),
    "DataSourceNameOrPath": ("Enter data source name or path", "输入数据源名称或路径"),
    "CurrentDataSource": ("Current Data Source", "当前数据源"),
    "CloseThisFile": ("Close this file", "关闭此文件"),
    "DataSourceDescription": (
        "Identifies this data source amongst all others in this group - also displayed next to each log line",
        "用于在该组的所有数据源中标识此数据源——同时会显示在每一行日志旁边"),
    "DataSourceExcluded": (
        "This data source is excluded from the parent's data source view",
        "此数据源已从父级数据源视图中排除"),
    "RemoveMergedDataSource": ("Remove this merged data source", "移除这个合并数据源"),
    "ShowCharacterCode": (
        "Show a one or two digit character code next to each line (and in the data sources panel)",
        "在每一行（以及数据源面板中）旁边显示一到两位的字符代码"),
    "ShowFilename": ("Show the filename next to each line", "在每一行旁边显示文件名"),
    "FilterLogFile": ("Filter log file", "筛选日志文件"),
    "SearchLogFile": ("Search log file (Ctrl+F)", "搜索日志文件 (Ctrl+F)"),
    "FindAllInLogFile": ("Find all in log file (Ctrl+Shift+F)", "在日志文件中查找全部 (Ctrl+Shift+F)"),
    "FindAllResults": ("Find all results:", "查找全部结果："),
    "GoToLine": ("Go to line:", "转到行："),
    "EnterLineNumber": ("Enter line number", "输入行号"),
    "Working": ("Working", "工作中"),
    "Refreshing": ("Refreshing...", "正在刷新…"),

    # --- Side panel ---
    "Bookmarks": ("Bookmarks", "书签"),
    "QuickFilters": ("Quick Filters", "快速筛选"),
    "Issues": ("Issues", "问题"),
    "Outline": ("Outline", "大纲"),
    "Properties": ("Properties", "属性"),
    "PinUnpinPanel": ("Pin/Unpin this panel", "固定/取消固定此面板"),

    # --- Plugins ---
    "PluginDetails": ("Plugin details:", "插件详情："),
    "PluginTranslationHint": (
        "A plugin is responsible for translating the contents of this log file",
        "此日志文件的内容由某个插件负责解析"),
    "PluginTranslationTooltip": (
        "A plugin is responsible for translating the contents of this log file - click here for details",
        "此日志文件的内容由某个插件负责解析——点击此处查看详情"),
    "OpenPluginFolder": ("Open plugin folder", "打开插件文件夹"),
    "UpdateAllPlugins": ("Update all plugins", "更新所有插件"),
    "NoPluginsInstalled": ("No plugins installed!", "尚未安装任何插件！"),

    # --- Action center / notifications ---
    "ActionCenter": ("Action Center", "操作中心"),
    "NoNewMessages": ("No new messages", "没有新消息"),
    "ReportBug": ("report a bug!", "报告一个 bug！"),
    "UnexpectedError": ("An unexpected error occured :(", "发生了意外错误 :("),
    "SorryForInconvenience": (
        "We are very sorry for this. If this prevented you from doing your work, then please",
        "对此我们深表歉意。如果这妨碍了你的工作，请"),
    "GotIt": ("Got it", "知道了"),

    # --- Changelog / about ---
    "Features": ("Features:", "新功能："),
    "Bugfixes": ("Bugfixes:", "缺陷修复："),
    "Licenses": ("Licenses", "许可证"),
    "Author": ("Author:", "作者："),
    "Description": ("Description:", "描述："),
    "Version": ("Version:", "版本："),
    "Website": ("Website", "网站"),
    "WebsiteLabel": ("Website:", "网站："),
    "NameLabel": ("Name:", "名称："),
    "AIsAvailable": ("A newer version", "有更新的版本"),
    "IsAvailable": ("is available.", "可用。"),
    "NoIcon": ("No icon!", "无图标！"),
    "Installed": ("Installed", "已安装"),
    "Tailviewer": ("Tailviewer NEXT", "Tailviewer NEXT"),

    # --- Quick filter ---
    "FilterCombinationMode": (
        "Match all active quick filters (AND) or any of them (OR)",
        "匹配所有激活的快速筛选（AND）或其中任意一个（OR）"),
    "AddNewHighlighter": ("Add new highlighters (Ctrl+Shift+H)", "添加新的高亮器 (Ctrl+Shift+H)"),
    "AddNewQuickFilter": ("Add new quick filter (Ctrl+Shift+N)", "添加新的快速筛选 (Ctrl+Shift+N)"),
    "EnterHighlightedTerm": ("Enter highlighted term", "输入要高亮的词"),
    "EnterDisplayName": ("Enter display name", "输入显示名称"),
    "Highlighters": ("Highlighters", "高亮器"),
    "Filter": ("Filter", "筛选"),
    "Source": ("Source", "来源"),
    "Path": ("Path", "路径"),
    "All": ("All", "全部"),
    "From": ("From", "从"),
    "To": ("To", "到"),
    "String": ("String", "字符串"),
    "Regexp": ("Regexp", "正则表达式"),
    "Wildcard": ("Wildcard", "通配符"),
    "Character": ("Character", "字符"),
    "Filename": ("Filename", "文件名"),
    "Absolute": ("Absolute", "绝对路径"),
    "Quick": ("Quick", "快速"),
    "Miscellaneous": ("Miscellaneous", "其他"),
    "Everything": ("Everything", "全部"),
    "Recursive": ("Recursive", "递归"),
    "Exporting": ("Exporting", "正在导出"),
    "SearchDataSources": ("Search data sources:", "搜索数据源："),

    # --- Quick filter match type ---
    "ChangesAppliedAfterPen": (
        "Changes will be applied only after the pen above is pressed again.",
        "只有再次按下上方的笔形按钮后，更改才会生效。"),

    # --- Menus (C# ViewModels) ---
    "MenuNew": ("_New", "新建(_N)"),
    "MenuOpen": ("_Open", "打开(_O)"),
    "MenuOpenFolder": ("Fol_der", "文件夹(_D)"),
    "MenuClose": ("Close", "关闭"),
    "MenuCloseAll": ("Close All", "全部关闭"),
    "MenuPlugins": ("Plugins", "插件"),
    "MenuSettings": ("Settings", "设置"),
    "MenuExit": ("E_xit", "退出(_X)"),
    "MenuGoToLine": ("Go To Line...", "转到行..."),
    "MenuGoToDataSource": ("Go To Data Source...", "转到数据源..."),
    "MenuGoToPreviousDataSource": ("Go To Previous Data Source", "转到上一个数据源"),
    "MenuGoToNextDataSource": ("Go To Next Data Source", "转到下一个数据源"),
    "MenuAddBookmark": ("Add Bookmark", "添加书签"),
    "MenuRemoveAllBookmarks": ("Remove All Bookmarks", "移除所有书签"),
    "MenuReportProblem": ("Report a Problem...", "报告问题..."),
    "MenuSuggestFeature": ("Suggest a Feature...", "建议新功能..."),
    "MenuAskQuestion": ("Ask a Question...", "提问..."),
    "MenuSendFeedback": ("Send Feedback", "发送反馈"),
    "MenuCheckForUpdates": ("Check For Updates", "检查更新"),
    "MenuShowLog": ("Show Log", "显示日志"),
    "MenuAbout": ("About", "关于"),

    # --- Data source tooltips ---
    "TooltipClearScreen": ("Clear Screen", "清屏"),
    "TooltipColorByLevel": ("Color by Level", "按级别着色"),
    "TooltipColorLogMessages": ("Color log messages by their log level", "按日志级别为日志消息着色"),
    "TooltipExportDataSource": (
        "Export the current Data Source to a text file (with the current filters)",
        "将当前数据源导出为文本文件（应用当前筛选）"),
    "TooltipExportToFile": ("Export To File", "导出到文件"),
    "TooltipFollowTail": ("Follow Tail", "跟随末尾"),
    "TooltipFollowLastEntry": ("Follow/Unfollow the last log entry", "跟随/取消跟随最后一条日志"),
    "TooltipHideEmptyLines": ("Hide Empty Lines", "隐藏空行"),
    "TooltipHideCompletelyEmptyLines": ("Hide lines which are completely empty", "隐藏完全为空的行"),
    "TooltipHideAllEntries": (
        "Hides all log current entries of the data source. New log entries will still be shown once they are added to the data source.",
        "隐藏数据源当前的所有日志条目。新添加到数据源的日志条目仍会显示。"),
    "TooltipShowAll": ("Show All", "显示全部"),
    "TooltipShowDeltaTimes": ("Show Delta Times", "显示间隔时间"),
    "TooltipShowElapsedTimes": ("Show Elapsed Times", "显示经过时间"),
    "TooltipShowLineNumbers": ("Show Line Numbers", "显示行号"),
    "TooltipElapsedSinceFirst": ("Show the amount of time elapsed since the first entry", "显示自第一条条目以来经过的时间"),
    "TooltipElapsedSincePrevious": ("Show the amount of time elapsed since the previous entry", "显示自上一条目以来经过的时间"),
    "TooltipToggleLineNumbers": ("Show/hide line numbers", "显示/隐藏行号"),
    "TooltipShowClearedEntries": ("Shows all log entries that were previously cleared again", "重新显示之前被清除的所有日志条目"),
    "TooltipSingleLine": ("Single Line", "单行"),
    "TooltipTreatEachLine": ("Treat every single line as a separate log entry", "将每一行视为单独的日志条目"),

    # --- Side panel toggles ---
    "HideDataSourceList": ("Hide the list of data sources", "隐藏数据源列表"),
    "ShowDataSourceList": ("Show the list of data sources", "显示数据源列表"),
    "HideBookmarkList": ("Hide the list of bookmarks", "隐藏书签列表"),
    "ShowBookmarkList": ("Show the list of bookmarks", "显示书签列表"),
    "HideQuickFilterList": ("Hide the list of quick filters", "隐藏快速筛选列表"),
    "ShowQuickFilterList": ("Show the list of quick filters", "显示快速筛选列表"),
    "ShowOutlineOfCurrentLogFile": ("Show outline of the current log file", "显示当前日志文件的大纲"),
    "ShowPropertiesOfCurrentLogFile": ("Show properties of the current log file", "显示当前日志文件的属性"),

    # --- Empty states ---
    "NoDataSourceOpened": ("No data source opened", "尚未打开任何数据源"),
    "NoDataSourceMatchesFilter": ("No data source matches '{0}'", "没有匹配“{0}”的数据源"),
    "TryOpeningFileOrFolder": (
        "Try opening a file or folder, create a new data source from the file menu or simply drag and drop a file into this window.",
        "请尝试打开文件或文件夹、从文件菜单创建新数据源，或直接将文件拖放到此窗口中。"),
    "TryChangingFilterOrAddDataSource": (
        "Try changing the filter or add the desired data source",
        "请尝试更改筛选条件或添加所需的数据源"),
    "NoBookmarksAdded": ("No Bookmarks added", "尚未添加任何书签"),
    "TryClickingAddBookmark": ("Try clicking Add Bookmark", "请尝试点击“添加书签”"),
    "TrySelectingLogEntry": (
        "Try selecting a log entry /or line and click Add Bookmark",
        "请尝试选择一条日志/行，然后点击“添加书签”"),
    "NoIssuesPluginInstalled": ("No issues plugin installed", "尚未安装问题分析插件"),
    "NoOutlinePluginInstalled": ("No outline plugin installed", "尚未安装大纲插件"),
    "OutlineCreationFailed": ("Outline creation failed", "大纲创建失败"),
    "OutlinePresentationError": (
        "An error occurred while trying to present the outline: check the log",
        "尝试展示大纲时发生错误：请检查日志"),
    "NoOutlinePluginForFormat": (
        "There is no plugin installed which is able to present an outline for the current data source's log format.",
        "没有安装能够为当前数据源日志格式展示大纲的插件。"),
    "MultipleOutlinePlugins": (
        "There are multiple plugins which claim to provide an outline for '{0}', selecting the first one:\r\n{1}",
        "有多个插件声称能为“{0}”提供大纲，选择第一个：\r\n{1}"),
    "NoIssuesPluginForFormat": (
        "There is no plugin installed which is able to present a list of issues for the current data source's log format.",
        "没有安装能够为当前数据源日志格式展示问题列表的插件。"),
    "MultipleIssuesPlugins": (
        "There are multiple plugins which claim to provide an outline for '{0}', selecting the first one:\r\n{1}",
        "有多个插件声称能为“{0}”提供问题列表，选择第一个：\r\n{1}"),
    "PropertiesRegistryMissing": (
        "Registry is missing: No properties will be displayed...",
        "注册表缺失：不会显示任何属性..."),
    "NoDataSources": ("There are no {0}", "没有{0}"),
    "NoResultsForSearch": ("No results for search {0}", "搜索 {0} 没有结果"),

    # --- Data source context actions ---
    "OpenContainingFolder": ("Open Containing Folder", "打开所在文件夹"),
    "OpenContainingFolderHint": ("Opens Folder the current Data Source is contained in", "打开当前数据源所在的文件夹"),
    "OpenContainingFolders": ("Open Containing Folder(s)", "打开所在文件夹"),
    "OpenContainingFoldersHint": ("Opens the Folder(s) of the Child Data sources", "打开子数据源所在的文件夹"),
    "ChooseFolderToMonitor": ("Choose folder to monitor", "选择要监视的文件夹"),
    "ShowSourceName": ("Show source name", "显示来源名称"),
    "ShowSourceNameHint": (
        "Toggles between showing the full name of the data source or a character code representing the original data source",
        "在显示数据源全名与代表原始数据源的字符代码之间切换"),
    "ExcludeAll": ("Exclude all", "全部排除"),
    "IncludeAll": ("Include all", "全部包含"),
    "ExcludeFromGroup": ("Exclude from group", "从组中排除"),
    "IncludeInGroup": ("Include in group", "包含在组中"),

    # --- Monitoring / notifications ---
    "MonitoringFolder": ("Monitoring {0}", "正在监视 {0}"),
    "MonitoringFolderWithFilter": ("Monitoring {0} ({1} not matching filter)", "正在监视 {0}（{1} 个不匹配筛选）"),

    # --- Converters ---
    "ClickToFollowTail": ("Click to follow the tail of this data source", "点击以跟随此数据源的末尾"),
    "ClickToStopFollowingTail": ("Click to stop following the tail of this data source", "点击以停止跟随此数据源的末尾"),
    "ClickToToggleLevel": ("Click to {0} {1} log entries", "点击以{0}{1}日志条目"),
    "ShowWord": ("show", "显示"),
    "HideWord": ("hide", "隐藏"),
    "AllWord": ("all", "全部"),
    "Mixed": ("Mixed", "混合"),
    "None": ("None", "无"),
    "NotAvailable": ("N/A", "不可用"),
    "LogEntriesSuffix": ("{0} log entry", "{0} 条日志条目"),
    "LogEntriesAre": ("s are", "们是"),
    "LogEntryIs": (" is", "是"),
    "NotDisplayedBecause": (" not displayed in the group because ", "未显示在组中，因为"),
    "ItDoes": ("it does", "它"),
    "TheyDo": ("they do", "它们"),
    "NoTimestampReason": (" not contain a timestamp or because its timestamp was not recognized",
                          "不包含时间戳，或其时间戳无法被识别"),

    # --- Auto update ---
    "NewerVersionAvailable": ("A newer version ({0}) is available to be downloaded", "有可下载的新版本（{0}）"),
    "Downloading": ("Downloading...", "正在下载..."),
    "Install": ("Install", "安装"),
    "ShuttingDownForUpdate": ("Shutting down to perform the update", "正在关闭以执行更新"),
    "RunningLatestVersion": ("You are running the latest version!", "你正在运行最新版本！"),

    # --- Plugins ---
    "NoPluginRepositoryConfigured": ("No plugin repository configured!", "尚未配置插件仓库！"),
    "CouldNotRetrievePlugins": ("Could not retrieve plugins from repository :(", "无法从仓库获取插件 :("),
    "AllPluginsUpToDate": ("All plugins are up-to-date!", "所有插件均已是最新版本！"),
    "PluginsUpdatedRestart": ("{0} plugin(s) were updated. Please restart {1}!", "{0} 个插件已更新。请重启 {1}！"),
    "PluginUpdateError": ("An error occured while trying to update plugins: {0}", "尝试更新插件时发生错误：{0}"),
    "MyCustomFormat": ("My custom {0} format", "我的自定义 {0} 格式"),
    "PluginRepositoryRequired": (
        "In order to download/update plugins, you have to configure Tailviewer NEXT to use a plugin repository. Please note that there is no globally available plugin repository. This feature is currently intended for organizations to deploy their own private repositories.",
        "要下载/更新插件，你必须配置 Tailviewer NEXT 使用插件仓库。请注意，不存在全局可用的插件仓库。此功能目前面向需要部署自有私有仓库的组织。"),

    # --- Action center ---
    "UnknownNotification": ("Unknown notification: {0}", "未知通知：{0}"),

    # --- Settings ---
    "ChooseExportFolder": ("Choose export folder", "选择导出文件夹"),
    "AutoDetect": ("Auto detect", "自动检测"),

    # --- Log viewer empty states ---
    "DataSourceIsEmpty": ("Data source is empty", "数据源为空"),
    "ScreenWasCleared": ("The screen was cleared", "屏幕已被清除"),
    "NoNewEntriesAfterClear": (
        "No new log entries have been added to the data source since the screen was cleared. Try waiting for a little longer or show all log entries again.",
        "屏幕被清除后，数据源中没有新的日志条目。请稍等片刻，或再次显示所有日志条目。"),
    "NothingMatchesLevelFilter": ("Nothing matches level filter", "没有匹配级别筛选的日志"),
    "TryDifferentLevels": (
        "Try filtering by different levels or display everything regardless of its level again",
        "请尝试按不同级别筛选，或再次显示所有级别的日志"),
    "NothingMatchesQuickFilter": ("Nothing matches quick filter", "没有匹配快速筛选的日志"),
    "TryDifferentQuickFilterTerms": (
        "Try filtering by different terms or disable all quick filters",
        "请尝试使用不同的筛选词，或禁用所有快速筛选"),
    "NoLogEntriesWithTimestamps": ("No log entries with timestamps", "没有带时间戳的日志条目"),
    "TryMergingDataSources": (
        "Try merging different data sources, change the timestamp format of the data or create a plugin to help Tailviewer NEXT parse its timestamp",
        "请尝试合并不同的数据源、更改数据的时间戳格式，或创建插件来帮助 Tailviewer NEXT 解析时间戳"),
    "FolderDoesNotContainAnyFile": ("The folder \"{0}\" does not contain any file", "文件夹“{0}”中不包含任何文件"),
    "FolderDoesNotContainMatchingFile": ("The folder \"{0}\" does not contain any file matching \"{1}\"", "文件夹“{0}”中不包含任何匹配“{1}”的文件"),
    "NoLogEntryMatchesFilter": (
        "No log entry matches \"{0}\".\r\nTry changing your filter(s) or disable them again",
        "没有匹配“{0}”的日志条目。\r\n请尝试更改筛选条件或再次禁用它们"),

    # --- Property presenters ---
    "AutoDetectedEncoding": ("Auto Detected Encoding", "自动检测的编码"),
    "OverwrittenEncoding": ("Overwritten Encoding", "覆盖的编码"),
    "Encoding": ("Encoding", "编码"),
    "PropertyName": ("Name", "名称"),
    "Format": ("Format", "格式"),
    "Created": ("Created", "创建时间"),
    "LastModified": ("Last Modified", "最后修改"),
    "FirstTimestamp": ("First Timestamp", "首个时间戳"),
    "LastTimestamp": ("Last Timestamp", "最后时间戳"),
    "Duration": ("Duration", "时长"),
    "Size": ("Size", "大小"),
    "Processed": ("Processed", "已处理"),
    "Count": ("Count", "数量"),
    "DebugCount": ("Debug Count", "调试数量"),
    "InfoCount": ("Info Count", "信息数量"),
    "WarningCount": ("Warning Count", "警告数量"),
    "ErrorCount": ("Error Count", "错误数量"),
    "FatalCount": ("Fatal Count", "致命数量"),
    "OtherCount": ("Other Count", "其他数量"),
    "TraceCount": ("Trace Count", "跟踪数量"),

    # --- Time filters ---
    "SelectEverything": ("Select: Everything", "选择：全部"),
    "SelectTitle": ("Select: {0}", "选择：{0}"),
    "SelectRange": ("Select from {0} to {1}", "选择从 {0} 到 {1}"),
    "SelectFrom": ("Select from {0}", "选择从 {0} 开始"),
    "SelectUntil": ("Select until {0}", "选择至 {0}"),
    "SelectFromTo": ("Select from - to -", "选择 从 - 到 -"),
    "Today": ("Today", "今天"),
    "ThisWeek": ("This week", "本周"),
    "ThisMonth": ("This month", "本月"),
    "ThisYear": ("This year", "今年"),
    "Last7Days": ("Last 7 days", "最近 7 天"),
    "Last24Hours": ("Last 24 hours", "最近 24 小时"),
    "Last30Days": ("Last 30 days", "最近 30 天"),
    "Last365Days": ("Last 365 days", "最近 365 天"),

    # --- Open dialogs ---
    "SelectFolderToOpen": ("Select folder to open", "选择要打开的文件夹"),
    "SelectLogFileToOpen": ("Select log file to open", "选择要打开的日志文件"),
    "LogFileDialogFilter": (
        "Log Files (*.log)|*.log|Txt Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
        "日志文件 (*.log)|*.log|文本文件 (*.txt)|*.txt|CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*"),

    # --- Bookmarks ---
    "BookmarksCount": ("{0} bookmarks", "{0} 个书签"),
    "OneBookmark": ("1 bookmark", "1 个书签"),
    "BookmarkLineTitle": ("Line #{0}, {1}", "第 #{0} 行，{1}"),

    # --- Log view navigation ---
    "GoingToLine": ("Going to line {0}", "正在转到第 {0} 行"),
    "NavigatingTo": ("Navigating to '{0}'", "正在导航到“{0}”"),

    # --- Issues ---
    "IssuesCount": ("{0} issue(s)", "{0} 个问题"),
    "ActiveFilterCount": ("{0} active", "{0} 个生效"),
}


ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(ROOT, "src", "Tailviewer", "Localization")
NAMESPACE = "Tailviewer.Localization"
RESOURCE_NAME = "Tailviewer.Localization.Strings"

RESX_HEADER = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
"""


def write_resx(path, lang_index):
    with io.open(path, "w", encoding="utf-8-sig", newline="") as f:
        f.write(RESX_HEADER)
        for key in sorted(STRINGS):
            value = STRINGS[key][lang_index]
            f.write('  <data name="%s" xml:space="preserve">\r\n' % key)
            f.write("    <value>%s</value>\r\n" % sax.escape(value))
            f.write("  </data>\r\n")
        f.write("</root>\r\n")


def write_strings_cs(path):
    lines = []
    lines.append("using System.Resources;")
    lines.append("")
    lines.append("namespace %s" % NAMESPACE)
    lines.append("{")
    lines.append("\t/// <summary>")
    lines.append("\t/// Strongly-typed accessor for the UI strings in Strings.resx.")
    lines.append("\t/// Generated by tools/generate_localization.py - do not edit by hand.")
    lines.append("\t/// </summary>")
    lines.append("\tpublic static class Strings")
    lines.append("\t{")
    lines.append(
        '\t\tprivate static readonly ResourceManager _rm = new ResourceManager("%s", typeof(Strings).Assembly);'
        % RESOURCE_NAME)
    lines.append("")
    for key in sorted(STRINGS):
        lines.append(
            '\t\tpublic static string %s => _rm.GetString("%s") ?? "%s";'
            % (key, key, key))
    lines.append("\t}")
    lines.append("}")
    with io.open(path, "w", encoding="utf-8-sig", newline="") as f:
        for line in lines:
            f.write(line + "\r\n")


def apply_cs():
    ui_dir = os.path.join(ROOT, "src", "Tailviewer", "Ui")
    by_en = {}
    for key, (en, _zh) in STRINGS.items():
        if not en or '"' in en or "\r" in en or "\n" in en:
            continue
        by_en.setdefault(en, []).append(key)
    changed_files = 0
    replaced_total = 0
    for root, _dirs, files in os.walk(ui_dir):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(root, fn)
            with io.open(path, encoding="utf-8-sig") as f:
                content = f.read()
            orig = content
            for en in sorted(by_en, key=lambda e: -len(e)):
                key = by_en[en][0]
                for attr in ("Header", "ToolTip"):
                    needle = '%s = "%s"' % (attr, en)
                    repl = '%s = Strings.%s' % (attr, key)
                    if needle in content:
                        content = content.replace(needle, repl)
            if content != orig:
                if "using Tailviewer.Localization;" not in content:
                    marker = "namespace Tailviewer"
                    idx = content.find(marker)
                    if idx != -1:
                        content = content[:idx] + "using Tailviewer.Localization;\r\n\r\n" + content[idx:]
                with io.open(path, "w", encoding="utf-8-sig", newline="") as f:
                    f.write(content)
                changed_files += 1
                replaced_total += content.count("Strings.")
    print("Applied C# localization to %d files (%d static refs total)" % (changed_files, replaced_total))


CS_EDITS = [
    # --- Menu (expression-bodied Header) ---
    ("src/Tailviewer/Ui/Menu/ExcludeAllInGroupViewModel.cs",
     'get { return "Exclude all"; }',
     'get { return Strings.ExcludeAll; }'),
    ("src/Tailviewer/Ui/Menu/IncludeAllInGroupViewModel.cs",
     'get { return "Include all"; }',
     'get { return Strings.IncludeAll; }'),

    # --- Data sources side panel ---
    ("src/Tailviewer/Ui/SidePanel/DataSources/DataSourcesViewModel.cs",
     '"Hide the list of data sources"', 'Strings.HideDataSourceList'),
    ("src/Tailviewer/Ui/SidePanel/DataSources/DataSourcesViewModel.cs",
     '"Show the list of data sources"', 'Strings.ShowDataSourceList'),

    # --- Bookmarks side panel ---
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksViewModel.cs",
     '"Hide the list of bookmarks"', 'Strings.HideBookmarkList'),
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksViewModel.cs",
     '"Show the list of bookmarks"', 'Strings.ShowBookmarkList'),
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksViewModel.cs",
     'return "1 bookmark";', 'return Strings.OneBookmark;'),
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksViewModel.cs",
     'return string.Format("{0} bookmarks", count);',
     'return string.Format(Strings.BookmarksCount, count);'),
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksViewModel.cs",
     'EmptyStatement = "No Bookmarks added";', 'EmptyStatement = Strings.NoBookmarksAdded;'),
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksViewModel.cs",
     '? "Try clicking Add Bookmark"', '? Strings.TryClickingAddBookmark'),
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarksViewModel.cs",
     ': "Try selecting a log entry /or line and click Add Bookmark";',
     ': Strings.TrySelectingLogEntry;'),

    # --- Quick filters side panel ---
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/QuickFiltersSidePanelViewModel.cs",
     '"Hide the list of quick filters"', 'Strings.HideQuickFilterList'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/QuickFiltersSidePanelViewModel.cs",
     '"Show the list of quick filters"', 'Strings.ShowQuickFilterList'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/QuickFiltersSidePanelViewModel.cs",
     'string.Format("{0} active", count)', 'string.Format(Strings.ActiveFilterCount, count)'),

    # --- Outline side panel ---
    ("src/Tailviewer/Ui/SidePanel/Outline/OutlineSidePanelViewModel.cs",
     'Tooltip = "Show outline of the current log file";',
     'Tooltip = Strings.ShowOutlineOfCurrentLogFile;'),
    ("src/Tailviewer/Ui/SidePanel/Outline/OutlineSidePanelViewModel.cs",
     'EmptyStatement = "Outline creation failed";',
     'EmptyStatement = Strings.OutlineCreationFailed;'),
    ("src/Tailviewer/Ui/SidePanel/Outline/OutlineSidePanelViewModel.cs",
     'EmptyExplanation = "An error occurred while trying to present the outline: check the log";',
     'EmptyExplanation = Strings.OutlinePresentationError;'),
    ("src/Tailviewer/Ui/SidePanel/Outline/OutlineSidePanelViewModel.cs",
     'EmptyStatement = "No outline plugin installed";',
     'EmptyStatement = Strings.NoOutlinePluginInstalled;'),
    ("src/Tailviewer/Ui/SidePanel/Outline/OutlineSidePanelViewModel.cs",
     '"There is no plugin installed which is able to present an outline for the current data source\'s log format."',
     'Strings.NoOutlinePluginForFormat'),

    # --- Properties side panel ---
    ("src/Tailviewer/Ui/SidePanel/Property/PropertiesSidePanelViewModel.cs",
     'Tooltip = "Show properties of the current log file";',
     'Tooltip = Strings.ShowPropertiesOfCurrentLogFile;'),

    # --- Issues side panel ---
    ("src/Tailviewer/Ui/SidePanel/Issues/IssuesSidePanelViewModel.cs",
     'QuickInfo = $"{_issueCount} issue(s)";',
     'QuickInfo = string.Format(Strings.IssuesCount, _issueCount);'),
    ("src/Tailviewer/Ui/SidePanel/Issues/IssuesSidePanelViewModel.cs",
     'EmptyStatement = "No issues plugin installed";',
     'EmptyStatement = Strings.NoIssuesPluginInstalled;'),
    ("src/Tailviewer/Ui/SidePanel/Issues/IssuesSidePanelViewModel.cs",
     '"There is no plugin installed which is able to present a list of issues for the current data source\'s log format."',
     'Strings.NoIssuesPluginForFormat'),

    # --- Data sources control (empty state) ---
    ("src/Tailviewer/Ui/DataSourceTree/DataSourcesControl.cs",
     'NoDataSourcesReason = $"No data source matches \'{StringFilter}\'";',
     'NoDataSourcesReason = string.Format(Strings.NoDataSourceMatchesFilter, StringFilter);'),
    ("src/Tailviewer/Ui/DataSourceTree/DataSourcesControl.cs",
     'NoDataSourcesActions = "Try changing the filter or add the desired data source";',
     'NoDataSourcesActions = Strings.TryChangingFilterOrAddDataSource;'),
    ("src/Tailviewer/Ui/DataSourceTree/DataSourcesControl.cs",
     'NoDataSourcesReason = "No data source opened";',
     'NoDataSourcesReason = Strings.NoDataSourceOpened;'),
    ("src/Tailviewer/Ui/DataSourceTree/DataSourcesControl.cs",
     'NoDataSourcesActions = "Try opening a file or folder, create a new data source from the file menu or simply drag and drop a file into this window.";',
     'NoDataSourcesActions = Strings.TryOpeningFileOrFolder;'),

    # --- Log viewer empty states ---
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesExplanation = $"The folder \\"{Path.GetFileName(dataSource.FullFileName)}\\" does not contain any file";',
     'NoEntriesExplanation = string.Format(Strings.FolderDoesNotContainAnyFile, Path.GetFileName(dataSource.FullFileName));'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesExplanation = $"The folder \\"{Path.GetFileName(dataSource.FullFileName)}\\" does not contain any file matching \\"{folderDataSource.LogFileSearchPattern}\\"";',
     'NoEntriesExplanation = string.Format(Strings.FolderDoesNotContainMatchingFile, Path.GetFileName(dataSource.FullFileName), folderDataSource.LogFileSearchPattern);'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesExplanation = "Data source is empty";',
     'NoEntriesExplanation = Strings.DataSourceIsEmpty;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesExplanation = "Nothing matches level filter";',
     'NoEntriesExplanation = Strings.NothingMatchesLevelFilter;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesAction = "Try filtering by different levels or display everything regardless of its level again";',
     'NoEntriesAction = Strings.TryDifferentLevels;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesExplanation = "The screen was cleared";',
     'NoEntriesExplanation = Strings.ScreenWasCleared;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesAction = "No new log entries have been added to the data source since the screen was cleared. Try waiting for a little longer or show all log entries again.";',
     'NoEntriesAction = Strings.NoNewEntriesAfterClear;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesExplanation = "Nothing matches quick filter";',
     'NoEntriesExplanation = Strings.NothingMatchesQuickFilter;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     '? $"No log entry matches \\"{filter}\\".\\r\\nTry changing your filter(s) or disable them again"',
     '? string.Format(Strings.NoLogEntryMatchesFilter, filter)'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     ': "Try filtering by different terms or disable all quick filters";',
     ': Strings.TryDifferentQuickFilterTerms;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesExplanation = "No log entries with timestamps";',
     'NoEntriesExplanation = Strings.NoLogEntriesWithTimestamps;'),
    ("src/Tailviewer/Ui/LogView/LogViewerViewModel.cs",
     'NoEntriesAction = "Try merging different data sources, change the timestamp format of the data or create a plugin to help Tailviewer NEXT parse its timestamp";',
     'NoEntriesAction = Strings.TryMergingDataSources;'),

    # --- Main window (open dialogs) ---
    ("src/Tailviewer/Ui/MainWindowViewModel.cs",
     'Title = "Select log file to open",', 'Title = Strings.SelectLogFileToOpen,'),
    ("src/Tailviewer/Ui/MainWindowViewModel.cs",
     'Filter = "Log Files (*.log)|*.log|Txt Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",',
     'Filter = Strings.LogFileDialogFilter,'),
    ("src/Tailviewer/Ui/MainWindowViewModel.cs",
     'Description = "Select folder to open",', 'Description = Strings.SelectFolderToOpen,'),

    # --- Settings ---
    ("src/Tailviewer/Ui/Settings/SettingsFlyoutViewModel.cs",
     'new EncodingViewModel(null, "Auto detect"),',
     'new EncodingViewModel(null, Strings.AutoDetect),'),
    ("src/Tailviewer/Ui/Settings/SettingsFlyoutViewModel.cs",
     'public string Name => "Settings";', 'public string Name => Strings.MenuSettings;'),
    ("src/Tailviewer/Ui/Settings/SettingsFlyoutViewModel.cs",
     'Description = "Choose export folder",', 'Description = Strings.ChooseExportFolder,'),

    # --- Plugins ---
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'ShowAllPluginsError = "No plugin repository configured!";',
     'ShowAllPluginsError = Strings.NoPluginRepositoryConfigured;'),
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'ShowAllPluginsErrorDescription = "In order to download/update plugins, you have to configure Tailviewer NEXT to use a plugin repository. Please note that there is no globally available plugin repository. This feature is currently intended for organizations to deploy their own private repositories.";',
     'ShowAllPluginsErrorDescription = Strings.PluginRepositoryRequired;'),
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'PluginUpdateMessage = "All plugins are up-to-date!";',
     'PluginUpdateMessage = Strings.AllPluginsUpToDate;'),
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'PluginUpdateMessage = $"{numPluginsUpdated} plugin(s) were updated. Please restart {Constants.ApplicationTitle}!";',
     'PluginUpdateMessage = string.Format(Strings.PluginsUpdatedRestart, numPluginsUpdated, Constants.ApplicationTitle);'),
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'PluginUpdateMessage = $"An error occured while trying to update plugins: {e.InnerException?.Message}";',
     'PluginUpdateMessage = string.Format(Strings.PluginUpdateError, e.InnerException?.Message);'),
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'PluginUpdateMessage = $"An error occured while trying to update plugins: {e.Message}";',
     'PluginUpdateMessage = string.Format(Strings.PluginUpdateError, e.Message);'),
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'ShowAllPluginsError = "Could not retrieve plugins from repository :(";',
     'ShowAllPluginsError = Strings.CouldNotRetrievePlugins;'),
    ("src/Tailviewer/Ui/Plugins/PluginsMainPanelViewModel.cs",
     'public string Name => "Plugins";', 'public string Name => Strings.MenuPlugins;'),

    # --- Custom format ---
    ("src/Tailviewer/Ui/Settings/CustomFormats/CustomFormatPluginViewModel.cs",
     '$"My custom {_name} format"',
     'string.Format(Strings.MyCustomFormat, _name)'),

    # --- Bookmark title ---
    ("src/Tailviewer/Ui/SidePanel/Bookmarks/BookmarkViewModel.cs",
     'Name = string.Format("Line #{0}, {1}", Bookmark.Index.Value + 1, DataSourceName(bookmark.DataSource));',
     'Name = string.Format(Strings.BookmarkLineTitle, Bookmark.Index.Value + 1, DataSourceName(bookmark.DataSource));'),

    # --- Property presenters ---
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.LogEntryCount, "Count" },', '{ Core.Properties.LogEntryCount, Strings.Count },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.TraceLogEntryCount, "Trace Count" },', '{ Core.Properties.TraceLogEntryCount, Strings.TraceCount },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.DebugLogEntryCount, "Debug Count" },', '{ Core.Properties.DebugLogEntryCount, Strings.DebugCount },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.InfoLogEntryCount, "Info Count" },', '{ Core.Properties.InfoLogEntryCount, Strings.InfoCount },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.WarningLogEntryCount, "Warning Count" },', '{ Core.Properties.WarningLogEntryCount, Strings.WarningCount },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.ErrorLogEntryCount, "Error Count" },', '{ Core.Properties.ErrorLogEntryCount, Strings.ErrorCount },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.FatalLogEntryCount, "Fatal Count" },', '{ Core.Properties.FatalLogEntryCount, Strings.FatalCount },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.OtherLogEntryCount, "Other Count" },', '{ Core.Properties.OtherLogEntryCount, Strings.OtherCount },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.Name, "Name" },', '{ Core.Properties.Name, Strings.PropertyName },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.StartTimestamp, "First Timestamp" },', '{ Core.Properties.StartTimestamp, Strings.FirstTimestamp },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.EndTimestamp, "Last Timestamp" },', '{ Core.Properties.EndTimestamp, Strings.LastTimestamp },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.Duration, "Duration" },', '{ Core.Properties.Duration, Strings.Duration },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.LastModified, "Last Modified" },', '{ Core.Properties.LastModified, Strings.LastModified },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.Created, "Created" },', '{ Core.Properties.Created, Strings.Created },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.Size, "Size" },', '{ Core.Properties.Size, Strings.Size },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.PercentageProcessed, "Processed" },', '{ Core.Properties.PercentageProcessed, Strings.Processed },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ Core.Properties.Format, "Format" },', '{ Core.Properties.Format, Strings.Format },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ TextProperties.AutoDetectedEncoding, "Auto Detected Encoding" },', '{ TextProperties.AutoDetectedEncoding, Strings.AutoDetectedEncoding },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ TextProperties.OverwrittenEncoding, "Overwritten Encoding" },', '{ TextProperties.OverwrittenEncoding, Strings.OverwrittenEncoding },'),
    ("src/Tailviewer/Ui/SidePanel/Property/PropertyPresenterRegistry.cs",
     '{ TextProperties.Encoding, "Encoding" }', '{ TextProperties.Encoding, Strings.Encoding }'),

    # --- Special time ranges ---
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.Today, "Today"},', '{SpecialDateTimeInterval.Today, Strings.Today},'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.ThisWeek, "This week"},', '{SpecialDateTimeInterval.ThisWeek, Strings.ThisWeek},'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.ThisMonth, "This month"},', '{SpecialDateTimeInterval.ThisMonth, Strings.ThisMonth},'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.ThisYear, "This year"},', '{SpecialDateTimeInterval.ThisYear, Strings.ThisYear},'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.Last24Hours, "Last 24 hours"},', '{SpecialDateTimeInterval.Last24Hours, Strings.Last24Hours},'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.Last7Days, "Last 7 days"},', '{SpecialDateTimeInterval.Last7Days, Strings.Last7Days},'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.Last30Days, "Last 30 days"},', '{SpecialDateTimeInterval.Last30Days, Strings.Last30Days},'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/SpecialTimeRangeViewModel.cs",
     '{SpecialDateTimeInterval.Last365Days, "Last 365 days"}', '{SpecialDateTimeInterval.Last365Days, Strings.Last365Days}'),

    # --- Time filters description ---
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/TimeFiltersViewModel.cs",
     'return "Select: Everything";', 'return Strings.SelectEverything;'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/TimeFiltersViewModel.cs",
     'return string.Format("Select: {0}", _selectedSpecialInterval?.Title);',
     'return string.Format(Strings.SelectTitle, _selectedSpecialInterval?.Title);'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/TimeFiltersViewModel.cs",
     'return string.Format("Select from {0} to {1}", Minimum, Maximum);',
     'return string.Format(Strings.SelectRange, Minimum, Maximum);'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/TimeFiltersViewModel.cs",
     'return string.Format("Select from {0}", Minimum);',
     'return string.Format(Strings.SelectFrom, Minimum);'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/TimeFiltersViewModel.cs",
     'return string.Format("Select until {0}", Maximum);',
     'return string.Format(Strings.SelectUntil, Maximum);'),
    ("src/Tailviewer/Ui/SidePanel/QuickFilters/TimeFilter/TimeFiltersViewModel.cs",
     'return "Select from - to -";', 'return Strings.SelectFromTo;'),

    # --- Auto update ---
    ("src/Tailviewer/Ui/AutoUpdateViewModel.cs",
     '_installTitle = "Install";', '_installTitle = Strings.Install;'),
    ("src/Tailviewer/Ui/AutoUpdateViewModel.cs",
     'InstallTitle = "Downloading...";', 'InstallTitle = Strings.Downloading;'),

    # --- Converters ---
    ("src/Tailviewer/Ui/Converters/LevelToggleTooltipConverter.cs",
     'var levelName = Level == LevelFlags.All ? "all" : Level.ToString();',
     'var levelName = Level == LevelFlags.All ? Strings.AllWord : Level.ToString();'),
    ("src/Tailviewer/Ui/Converters/LevelToggleTooltipConverter.cs",
     'return string.Format("Click to {0} {1} log entries", isChecked ? "hide" : "show", levelName);',
     'return string.Format(Strings.ClickToToggleLevel, isChecked ? Strings.HideWord : Strings.ShowWord, levelName);'),
    ("src/Tailviewer/Ui/Converters/FollowTailTooltipConverter.cs",
     'return "Click to stop following the tail of this data source";',
     'return Strings.ClickToStopFollowingTail;'),
    ("src/Tailviewer/Ui/Converters/FollowTailTooltipConverter.cs",
     'return "Click to follow the tail of this data source";',
     'return Strings.ClickToFollowTail;'),
    ("src/Tailviewer/Ui/Converters/ShowAllToContentConverter.cs",
     'return "All";', 'return Strings.All;'),
    ("src/Tailviewer/Ui/Converters/ShowAllToContentConverter.cs",
     'return "None";', 'return Strings.None;'),
    ("src/Tailviewer/Ui/Converters/ShowAllToContentConverter.cs",
     'return "Mixed";', 'return Strings.Mixed;'),
    ("src/Tailviewer/Ui/Converters/NullToNotAvailableConverter.cs",
     'return "N/A";', 'return Strings.NotAvailable;'),
    ("src/Tailviewer/Ui/Converters/SkippedDueToTimestampConverter.cs",
     'builder.AppendFormat("{0} log entry", count);',
     'builder.AppendFormat(Strings.LogEntriesSuffix, count);'),
    ("src/Tailviewer/Ui/Converters/SkippedDueToTimestampConverter.cs",
     'builder.Append(plural ? "s are" : " is");',
     'builder.Append(plural ? Strings.LogEntriesAre : Strings.LogEntryIs);'),
    ("src/Tailviewer/Ui/Converters/SkippedDueToTimestampConverter.cs",
     'builder.Append(" not displayed in the group because ");',
     'builder.Append(Strings.NotDisplayedBecause);'),
    ("src/Tailviewer/Ui/Converters/SkippedDueToTimestampConverter.cs",
     'builder.AppendFormat(plural ? "they do" : "it does");',
     'builder.AppendFormat(plural ? Strings.TheyDo : Strings.ItDoes);'),
    ("src/Tailviewer/Ui/Converters/SkippedDueToTimestampConverter.cs",
     'builder.Append(" not contain a timestamp or because its timestamp was not recognized");',
     'builder.Append(Strings.NoTimestampReason);'),

    # --- Installed plugin ---
    ("src/Tailviewer/Ui/Plugins/InstalledPluginViewModel.cs",
     'public string Author => _description.Author ?? "N/A";',
     'public string Author => _description.Author ?? Strings.NotAvailable;'),
]


def apply_cs_edits():
    ui_root = os.path.join(ROOT, "src", "Tailviewer")
    changed = set()
    missing = []
    for rel, old, new in CS_EDITS:
        path = os.path.join(ROOT, rel)
        with io.open(path, encoding="utf-8-sig") as f:
            content = f.read()
        if old not in content:
            missing.append((rel, old))
            continue
        content = content.replace(old, new)
        if "using Tailviewer.Localization;" not in content:
            idx = content.find("namespace Tailviewer")
            if idx != -1:
                content = content[:idx] + "using Tailviewer.Localization;\r\n\r\n" + content[idx:]
        with io.open(path, "w", encoding="utf-8-sig", newline="") as f:
            f.write(content)
        changed.add(rel)
    print("Applied %d C# edits across %d files" % (sum(1 for _ in changed), len(changed)))
    for rel, old in missing:
        print("  MISSING in %s: %r" % (rel, old))


def apply_xaml():
    ui_dir = os.path.join(ROOT, "src", "Tailviewer", "Ui")
    xaml_ns = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\""
    loc_ns = ("xmlns:loc=\"clr-namespace:%s\"" % NAMESPACE)
    changed_files = 0
    replaced_total = 0
    for root, _dirs, files in os.walk(ui_dir):
        for fn in files:
            if not fn.endswith(".xaml"):
                continue
            path = os.path.join(root, fn)
            with io.open(path, encoding="utf-8-sig") as f:
                content = f.read()
            orig = content
            for key in sorted(STRINGS, key=lambda k: -len(k)):
                en, _zh = STRINGS[key]
                if not en:
                    continue
                for attr in XAML_ATTRS:
                    needle = '%s="%s"' % (attr, en)
                    repl = '%s="{x:Static loc:Strings.%s}"' % (attr, key)
                    if needle in content:
                        content = content.replace(needle, repl)
            if content != orig:
                if "xmlns:loc=" not in content and xaml_ns in content:
                    content = content.replace(xaml_ns, xaml_ns + " " + loc_ns, 1)
                with io.open(path, "w", encoding="utf-8-sig", newline="") as f:
                    f.write(content)
                changed_files += 1
                replaced_total += content.count("x:Static loc:Strings.")
    print("Applied XAML localization to %d files (%d static refs total)" % (changed_files, replaced_total))


def main():
    if not os.path.isdir(OUT_DIR):
        print("error: %s not found" % OUT_DIR, file=sys.stderr)
        return 1
    write_resx(os.path.join(OUT_DIR, "Strings.resx"), 0)
    write_resx(os.path.join(OUT_DIR, "Strings.zh-CN.resx"), 1)
    write_strings_cs(os.path.join(OUT_DIR, "Strings.cs"))
    print("Generated %d strings in %s" % (len(STRINGS), OUT_DIR))
    if "--apply-xaml" in sys.argv:
        apply_xaml()
    if "--apply-cs" in sys.argv:
        apply_cs()
    if "--apply-cs-edits" in sys.argv:
        apply_cs_edits()
    return 0


if __name__ == "__main__":
    sys.exit(main())
