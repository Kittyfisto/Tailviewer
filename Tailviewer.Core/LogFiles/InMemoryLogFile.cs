using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     A <see cref="ILogFile"/> implementation which buffers the entire contents in memory.
	/// </summary>
	/// <remarks>
	///     Should only be used for log files which's content actually fits into memory.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogFileView))]
	public sealed class InMemoryLogFile
		: ILogFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly LogEntryList _logEntries;
		private readonly LogFileListenerCollection _listeners;

		private readonly object _syncRoot;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public InMemoryLogFile()
			: this(LogFileColumns.Minimum)
		{ }

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public InMemoryLogFile(params ILogFileColumn[] columns)
			: this((IEnumerable < ILogFileColumn > )columns)
		{}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public InMemoryLogFile(IEnumerable<ILogFileColumn> columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_syncRoot = new object();
			_logEntries = new LogEntryList(LogFileColumns.CombineWithMinimum(columns));
			_listeners = new LogFileListenerCollection(this);
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="content"></param>
		public InMemoryLogFile(IReadOnlyLogEntries content)
			: this(content.Columns)
		{
			AddRange(content);
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}

		/// <inheritdoc />
		public DateTime? StartTimestamp { get; private set; }

		/// <inheritdoc />
		public DateTime LastModified { get; private set; }

		/// <inheritdoc />
		public DateTime Created => DateTime.MinValue;

		/// <inheritdoc />
		public Size Size { get; set; }

		/// <inheritdoc />
		public ErrorFlags Error => ErrorFlags.None;

		/// <inheritdoc />
		public bool EndOfSourceReached => true;

		/// <inheritdoc />
		public int Count => _logEntries.Count;

		/// <inheritdoc />
		public int OriginalCount => Count;

		/// <inheritdoc />
		public int MaxCharactersPerLine { get; private set; }

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _logEntries.Columns;

		/// <inheritdoc />
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		/// <inheritdoc />
		public void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			_logEntries.CopyTo(column, (int)section.Index, buffer, destinationIndex, section.Count);
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			_logEntries.CopyTo(column, new Int32View(indices), buffer, destinationIndex);
		}

		/// <inheritdoc />
		public void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				foreach (var column in buffer.Columns)
					buffer.CopyFrom(column, destinationIndex, this, section);
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				foreach (var column in buffer.Columns)
					buffer.CopyFrom(column, destinationIndex, this, indices);
			}
		}

		/// <inheritdoc />
		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < section.Count; ++i)
				{
					var line = CreateLogLine(_logEntries[(int)(section.Index + i)]);
					dest[i] = line;
				}
			}
		}

		private LogLine CreateLogLine(IReadOnlyLogEntry logEntry)
		{
			return new LogLine((int)logEntry.Index,
							   (int)logEntry.OriginalIndex,
							   (int)logEntry.LogEntryIndex,
							   logEntry.RawContent,
							   logEntry.LogLevel,
							   logEntry.Timestamp);
		}

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			lock (_syncRoot)
			{
				if (originalLineIndex >= _logEntries.Count)
				{
					return LogLineIndex.Invalid;
				}

				return originalLineIndex;
			}
		}

		/// <inheritdoc />
		public LogLine GetLine(int index)
		{
			lock (_syncRoot)
			{
				return CreateLogLine(_logEntries[index]);
			}
		}

		/// <inheritdoc />
		public double Progress => 1;

		/// <summary>
		///     Removes all log lines.
		/// </summary>
		public void Clear()
		{
			lock (_syncRoot)
			{
				if (_logEntries.Count > 0)
				{
					_logEntries.Clear();
					MaxCharactersPerLine = 0;
					StartTimestamp = null;
					Touch();

					_listeners.Reset();
				}
			}
		}

		/// <summary>
		///     Removes everything from the given index onwards until the end.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveFrom(LogLineIndex index)
		{
			lock (_syncRoot)
			{
				if (index < 0)
				{
					Log.WarnFormat("Invalid index '{0}'", index);
					return;
				}

				if (index > _logEntries.Count)
				{
					Log.WarnFormat("Invalid index '{0}', Count is '{1}'", index, _logEntries.Count);
					return;
				}

				var available = _logEntries.Count - index;
				_logEntries.RemoveRange((int)index, available);
				_listeners.Invalidate((int)index, available);
				Touch();
			}
		}

		private void Touch()
		{
			LastModified = DateTime.Now;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		public void AddEntry(string rawContent)
		{
			var logEntry = new LogEntry2();
			logEntry.Add(LogFileColumns.RawContent, rawContent);
			Add(logEntry);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		/// <param name="level"></param>
		public void AddEntry(string rawContent, LevelFlags level)
		{
			var logEntry = new LogEntry2();
			logEntry.Add(LogFileColumns.RawContent, rawContent);
			logEntry.Add(LogFileColumns.LogLevel, level);
			Add(logEntry);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		public void AddEntry(string rawContent, LevelFlags level, DateTime? timestamp)
		{
			var logEntry = new LogEntry2();
			logEntry.Add(LogFileColumns.RawContent, rawContent);
			logEntry.Add(LogFileColumns.LogLevel, level);
			logEntry.Add(LogFileColumns.Timestamp, timestamp);
			Add(logEntry);
		}

		/// <summary>
		///     Adds a multi line log entry to this log file.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		/// <param name="lines"></param>
		public void AddMultilineEntry(LevelFlags level, DateTime? timestamp, params string[] lines)
		{
			lock (_syncRoot)
			{
				LogEntryIndex logEntryIndex;
				TimeSpan? elapsed, deltaTime;
				if (_logEntries.Count > 0)
				{
					var first = _logEntries[0];
					var last = _logEntries[_logEntries.Count - 1];

					logEntryIndex = last.LogEntryIndex + 1;
					elapsed = timestamp - first.Timestamp;
					deltaTime = timestamp - last.Timestamp;
				}
				else
				{
					logEntryIndex = 0;
					elapsed = null;
					deltaTime = null;

					StartTimestamp = timestamp;
				}

				foreach (var line in lines)
				{
					var logEntry = new LogEntry2();
					logEntry.Add(LogFileColumns.Index, _logEntries.Count);
					logEntry.Add(LogFileColumns.OriginalIndex, _logEntries.Count);
					logEntry.Add(LogFileColumns.LineNumber, _logEntries.Count + 1);
					logEntry.Add(LogFileColumns.OriginalLineNumber, _logEntries.Count + 1);
					logEntry.Add(LogFileColumns.LogEntryIndex, logEntryIndex);
					logEntry.Add(LogFileColumns.RawContent, line);
					logEntry.Add(LogFileColumns.LogLevel, level);
					logEntry.Add(LogFileColumns.Timestamp, timestamp);
					logEntry.Add(LogFileColumns.ElapsedTime, elapsed);
					logEntry.Add(LogFileColumns.DeltaTime, deltaTime);
					_logEntries.Add(logEntry);
					MaxCharactersPerLine = Math.Max(MaxCharactersPerLine, line.Length);
				}
				Touch();
				_listeners.OnRead(_logEntries.Count);
			}
		}

		/// <summary>
		///     Adds <paramref name="count" /> amount of empty lines to this log file.
		/// </summary>
		/// <param name="count"></param>
		public void AddEmptyEntries(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				Add(new LogEntry2());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entry"></param>
		public void Add(IReadOnlyLogEntry entry)
		{
			lock (_syncRoot)
			{
				DateTime? timestamp;
				entry.TryGetValue(LogFileColumns.Timestamp, out timestamp);
				LogEntryIndex logEntryIndex;
				TimeSpan? elapsed, deltaTime;
				if (_logEntries.Count > 0)
				{
					var last = _logEntries[_logEntries.Count - 1];

					logEntryIndex = last.LogEntryIndex + 1;
					elapsed = timestamp - StartTimestamp;
					deltaTime = timestamp - last.Timestamp;
				}
				else
				{
					logEntryIndex = 0;
					elapsed = null;
					deltaTime = null;
				}

				if (StartTimestamp == null)
					StartTimestamp = timestamp;

				// The user supplies us with a list of properties to add, however we will
				// never allow the user to supply us things like index or line number.
				// Therefore we create a log entry which we actually want to add...
				var finalLogEntry = new LogEntry2(Columns);

				foreach (var column in Columns)
				{
					object value;
					if (entry.TryGetValue(column, out value))
					{
						finalLogEntry.SetValue(column, value);
					}
				}

				finalLogEntry.Index = _logEntries.Count;
				finalLogEntry.OriginalIndex = _logEntries.Count;
				finalLogEntry.LineNumber = _logEntries.Count + 1;
				finalLogEntry.OriginalLineNumber = _logEntries.Count + 1;
				finalLogEntry.LogEntryIndex = logEntryIndex;
				finalLogEntry.Timestamp = timestamp;
				finalLogEntry.ElapsedTime = elapsed;
				finalLogEntry.DeltaTime = deltaTime;

				_logEntries.Add(finalLogEntry);
				MaxCharactersPerLine = Math.Max(MaxCharactersPerLine, finalLogEntry.RawContent?.Length ?? 0);
				Touch();
				_listeners.OnRead(_logEntries.Count);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entries"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void AddRange(IEnumerable<IReadOnlyLogEntry> entries)
		{
			foreach (var entry in entries)
			{
				Add(entry);
			}
		}
	}
}