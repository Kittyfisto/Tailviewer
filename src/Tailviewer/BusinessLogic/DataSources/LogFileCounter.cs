using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     Responsible for providing the amount of occurences of certain classes of log lines and -entries.
	/// </summary>
	internal sealed class LogFileCounter
		: ILogFileListener
		, IDisposable
	{
		#region Counts

		public readonly Counter Trace;
		public readonly Counter Debugs;
		public readonly Counter Errors;
		public readonly Counter Fatals;
		public readonly Counter Infos;
		public readonly Counter NoLevel;
		public readonly Counter NoTimestamp;
		public readonly Counter Total;
		public readonly Counter Warnings;

		#endregion

		private LogEntryList _entries;
		private readonly ReadOnlyLogEntry _emptyLine;

		public LogFileCounter()
		{
			Fatals = new Counter();
			Errors = new Counter();
			Warnings = new Counter();
			Infos = new Counter();
			Debugs = new Counter();
			Trace = new Counter();
			NoLevel = new Counter();
			NoTimestamp = new Counter();
			Total = new Counter();

			_entries = new LogEntryList(LogFileColumns.LogEntryIndex, LogFileColumns.Timestamp, LogFileColumns.LogLevel);
			_emptyLine = new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
			{
				{LogFileColumns.LogEntryIndex, new LogEntryIndex(-1) },
				{LogFileColumns.Timestamp, null },
				{LogFileColumns.LogLevel, LevelFlags.None}
			});
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			if (section.IsReset)
			{
				Clear();
			}
			else if (section.IsInvalidate)
			{
				RemoveRange(section);
			}
			else
			{
				AddRange(logFile, section);
			}
		}

		private void AddRange(ILogFile logFile, LogFileSection section)
		{
			var previousEntry = _entries.Count > 0
				? (IReadOnlyLogEntry)_entries[_entries.Count - 1]
				: _emptyLine;

			var entries = logFile.GetEntries(section, _entries.Columns);
			for (int i = 0; i < section.Count; ++i)
			{
				var entry = entries[i];
				IncrementCount(entry, previousEntry);
				previousEntry = entry;
			}
			_entries.AddRange(entries);
		}

		private void RemoveRange(LogFileSection section)
		{
			var previousEntry = _entries.Count > 0
				? (IReadOnlyLogEntry)_entries[_entries.Count - 1]
				: _emptyLine;

			for (int i = 0; i < section.Count; ++i)
			{
				LogLineIndex index = section.Index + i;
				IReadOnlyLogEntry entry = _entries[(int) index];
				DecrementCount(entry, previousEntry);
				previousEntry = entry;
			}

			_entries.RemoveRange((int) section.Index, section.Count);
		}

		private void IncrementCount(IReadOnlyLogEntry currentLogLine, IReadOnlyLogEntry previousLogLine)
		{
			if (currentLogLine.LogEntryIndex != previousLogLine.LogEntryIndex)
			{
				switch (currentLogLine.LogLevel)
				{
					case LevelFlags.Fatal:
						++Fatals.LogEntryCount;
						break;
					case LevelFlags.Error:
						++Errors.LogEntryCount;
						break;
					case LevelFlags.Warning:
						++Warnings.LogEntryCount;
						break;
					case LevelFlags.Info:
						++Infos.LogEntryCount;
						break;
					case LevelFlags.Debug:
						++Debugs.LogEntryCount;
						break;
					case LevelFlags.Trace:
						++Trace.LogEntryCount;
						break;
					default:
						++NoLevel.LogEntryCount;
						break;
				}

				if (currentLogLine.Timestamp == null)
				{
					++NoTimestamp.LogEntryCount;
				}

				++Total.LogEntryCount;
			}

			switch (currentLogLine.LogLevel)
			{
				case LevelFlags.Fatal:
					++Fatals.LogLineCount;
					break;
				case LevelFlags.Error:
					++Errors.LogLineCount;
					break;
				case LevelFlags.Warning:
					++Warnings.LogLineCount;
					break;
				case LevelFlags.Info:
					++Infos.LogLineCount;
					break;
				case LevelFlags.Debug:
					++Debugs.LogLineCount;
					break;
				case LevelFlags.Trace:
					++Trace.LogLineCount;
					break;
				default:
					++NoLevel.LogLineCount;
					break;
			}

			if (currentLogLine.Timestamp == null)
			{
				++NoTimestamp.LogLineCount;
			}

			++Total.LogLineCount;
		}

		private void DecrementCount(IReadOnlyLogEntry currentLogLine, IReadOnlyLogEntry previousLogLine)
		{
			if (currentLogLine.LogEntryIndex != previousLogLine.LogEntryIndex)
			{
				switch (currentLogLine.LogLevel)
				{
					case LevelFlags.Fatal:
						--Fatals.LogEntryCount;
						break;
					case LevelFlags.Error:
						--Errors.LogEntryCount;
						break;
					case LevelFlags.Warning:
						--Warnings.LogEntryCount;
						break;
					case LevelFlags.Info:
						--Infos.LogEntryCount;
						break;
					case LevelFlags.Debug:
						--Debugs.LogEntryCount;
						break;
					case LevelFlags.Trace:
						--Trace.LogEntryCount;
						break;
					default:
						--NoLevel.LogEntryCount;
						break;
				}

				if (currentLogLine.Timestamp == null)
				{
					--NoTimestamp.LogEntryCount;
				}

				--Total.LogEntryCount;
			}

			switch (currentLogLine.LogLevel)
			{
				case LevelFlags.Fatal:
					--Fatals.LogLineCount;
					break;
				case LevelFlags.Error:
					--Errors.LogLineCount;
					break;
				case LevelFlags.Warning:
					--Warnings.LogLineCount;
					break;
				case LevelFlags.Info:
					--Infos.LogLineCount;
					break;
				case LevelFlags.Debug:
					--Debugs.LogLineCount;
					break;
				case LevelFlags.Trace:
					--Trace.LogLineCount;
					break;
				default:
					--NoLevel.LogLineCount;
					break;
			}

			if (currentLogLine.Timestamp == null)
			{
				--NoTimestamp.LogLineCount;
			}

			--Total.LogLineCount;
		}

		private void Clear()
		{
			_entries.Clear();
			Fatals.Reset();
			Errors.Reset();
			Warnings.Reset();
			Infos.Reset();
			Debugs.Reset();
			Trace.Reset();
			NoLevel.Reset();
			NoTimestamp.Reset();
			Total.Reset();
		}

		public void Dispose()
		{
			Clear();

			// https://github.com/Kittyfisto/Tailviewer/issues/282
			_entries = null;
		}

		internal sealed class Counter
			: ICount
		{
			public int LogLineCount { get; set; }
			public int LogEntryCount { get; set; }

			public void Reset()
			{
				LogLineCount = 0;
				LogEntryCount = 0;
			}
		}
	}
}