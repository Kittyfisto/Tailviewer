# Why

You want to develop a log file format matcher plugin because:

- You want to change the way tailviewer treats a particular type of files
- You want to develop a plugin for a particular type of file that tailviewer doesn't already recognize

# Where

If you open a particular file with tailviewer (either by opening it directly or by dragging it over tailviewer),
then tailviewer will try to identify the file being opened. This is done by inspecting both the file path (including
its extension) as well as its content (if available). Tailviewer will query all available `ILogFileFormatMatcherPlugin`
implementations to identify the file. The resulting format (or `LogFileFormats.Generic` in case no match is found) will be assigned to the log file's `LogFileProperties.Format` property.

Tailviewer might call plugins multiple times on the same file in case the file's contents have changed and previously plugins did not positively
identify the file's format.

# How

