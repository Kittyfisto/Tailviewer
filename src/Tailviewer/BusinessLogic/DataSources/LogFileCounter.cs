using System;
using System.Collections.Generic;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Sources;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     Responsible for providing the amount of occurences of certain classes of log lines and -entries.
	/// </summary>
	internal sealed class LogFileCounter
		: ILogSourceListener
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

		private LogBufferList _buffers;
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

			_buffers = new LogBufferList(LogColumns.LogEntryIndex, LogColumns.Timestamp, LogColumns.LogLevel);
			_emptyLine = new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.LogEntryIndex, new LogEntryIndex(-1) },
				{LogColumns.Timestamp, null },
				{LogColumns.LogLevel, LevelFlags.None}
			});
		}

		public void OnLogFileModified(ILogSource logSource, LogFileSection section)
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
				AddRange(logSource, section);
			}
		}

		private void AddRange(ILogSource logSource, LogFileSection section)
		{
			var previousEntry = _buffers.Count > 0
				? (IReadOnlyLogEntry)_buffers[_buffers.Count - 1]
				: _emptyLine;

			var entries = logSource.GetEntries(section, _buffers.Columns);
			for (int i = 0; i < section.Count; ++i)
			{
				var entry = entries[i];
				IncrementCount(entry, previousEntry);
				previousEntry = entry;
			}
			_buffers.AddRange(entries);
		}

		private void RemoveRange(LogFileSection section)
		{
			var previousEntry = _buffers.Count > 0
				? (IReadOnlyLogEntry)_buffers[_buffers.Count - 1]
				: _emptyLine;

			for (int i = 0; i < section.Count; ++i)
			{
				LogLineIndex index = section.Index + i;
				IReadOnlyLogEntry entry = _buffers[(int) index];
				DecrementCount(entry, previousEntry);
				previousEntry = entry;
			}

			_buffers.RemoveRange((int) section.Index, section.Count);
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
			_buffers.Clear();
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
			_buffers = null;
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