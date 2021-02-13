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
		private readonly ILogFileProperties _properties;
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
		public InMemoryLogFile(params ILogFileColumnDescriptor[] columns)
			: this((IEnumerable < ILogFileColumnDescriptor > )columns)
		{}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="content"></param>
		public InMemoryLogFile(IReadOnlyLogEntries content)
			: this(content.Columns)
		{
			AddRange(content);
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public InMemoryLogFile(IEnumerable<ILogFileColumnDescriptor> columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_syncRoot = new object();
			_logEntries = new LogEntryList(LogFileColumns.CombineWithMinimum(columns));
			_listeners = new LogFileListenerCollection(this);

			_properties = new LogFilePropertyList(LogFileProperties.Minimum);
			_properties.SetValue(LogFileProperties.Size, Size.Zero);
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}

		/// <inheritdoc />
		public bool EndOfSourceReached => true;

		/// <inheritdoc />
		public int Count => _logEntries.Count;

		/// <inheritdoc />
		public int OriginalCount => Count;

		/// <inheritdoc />
		public int MaxCharactersPerLine { get; private set; }

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumnDescriptor> Columns => _logEntries.Columns;

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

		#region Properties

		/// <inheritdoc />
		public IReadOnlyList<ILogFilePropertyDescriptor> Properties => _properties.Properties;

		/// <inheritdoc />
		public object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			object value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			T value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public void GetValues(ILogFileProperties properties)
		{
			_properties.GetValues(properties);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyDescriptor"></param>
		/// <param name="value"></param>
		public void SetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor, T value)
		{
			_properties.SetValue(propertyDescriptor, value);
		}

		#endregion

		/// <inheritdoc />
		public void GetColumn<T>(LogFileSection sourceSection, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			_logEntries.CopyTo(column, (int)sourceSection.Index, destination, destinationIndex, sourceSection.Count);
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			_logEntries.CopyTo(column, new Int32View(sourceIndices), destination, destinationIndex);
		}

		/// <inheritdoc />
		public void GetEntries(LogFileSection sourceSection, ILogEntries destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				foreach (var column in destination.Columns)
					destination.CopyFrom(column, destinationIndex, this, sourceSection);
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				foreach (var column in destination.Columns)
					destination.CopyFrom(column, destinationIndex, this, sourceIndices);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public IReadOnlyLogEntry this[int index]
		{
			get { return _logEntries[index]; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public IReadOnlyLogEntry this[LogLineIndex index]
		{
			get { return this[(int)index]; }
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
					_properties.SetValue(LogFileProperties.StartTimestamp, null);
					_properties.SetValue(LogFileProperties.EndTimestamp, null);
					_properties.SetValue(LogFileProperties.Size, Size.Zero);
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
			_properties.SetValue(LogFileProperties.LastModified, DateTime.Now);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		/// <returns>A copy of the log entry as it was entered into this log file with all columns of this file (columns not present in the given log entry will be set to their default value).</returns>
		public IReadOnlyLogEntry AddEntry(string rawContent)
		{
			var logEntry = new LogEntry
			{
				RawContent = rawContent
			};
			return Add(logEntry);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		/// <param name="level"></param>
		/// <returns>A copy of the log entry as it was entered into this log file with all columns of this file (columns not present in the given log entry will be set to their default value).</returns>
		public IReadOnlyLogEntry AddEntry(string rawContent, LevelFlags level)
		{
			var logEntry = new LogEntry
			{
				RawContent = rawContent,
				LogLevel = level
			};
			return Add(logEntry);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rawContent"></param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		/// <returns>A copy of the log entry as it was entered into this log file with all columns of this file (columns not present in the given log entry will be set to their default value).</returns>
		public IReadOnlyLogEntry AddEntry(string rawContent, LevelFlags level, DateTime? timestamp)
		{
			var logEntry = new LogEntry
			{
				RawContent = rawContent,
				LogLevel = level,
				Timestamp = timestamp
			};
			return Add(logEntry);
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

					_properties.SetValue(LogFileProperties.StartTimestamp, timestamp);
				}
				_properties.SetValue(LogFileProperties.EndTimestamp, timestamp);

				foreach (var line in lines)
				{
					var logEntry = new LogEntry
					{
						Index = _logEntries.Count,
						OriginalIndex = _logEntries.Count,
						LineNumber = _logEntries.Count + 1,
						OriginalLineNumber = _logEntries.Count + 1,
						LogEntryIndex = logEntryIndex,
						RawContent = line,
						LogLevel = level,
						Timestamp = timestamp,
						ElapsedTime = elapsed,
						DeltaTime = deltaTime
					};
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
				Add(new LogEntry());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entry"></param>
		public void Add(IReadOnlyDictionary<ILogFileColumnDescriptor, object> entry)
		{
			Add(new ReadOnlyLogEntry(entry));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entry"></param>
		/// <returns>A copy of the log entry as it was entered into this log file with all columns of this file (columns not present in the given log entry will be set to their default value).</returns>
		public IReadOnlyLogEntry Add(IReadOnlyLogEntry entry)
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
					elapsed = timestamp - _properties.GetValue(LogFileProperties.StartTimestamp);
					deltaTime = timestamp - last.Timestamp;
				}
				else
				{
					logEntryIndex = 0;
					elapsed = null;
					deltaTime = null;
				}

				if (_properties.GetValue(LogFileProperties.StartTimestamp) == null)
					_properties.SetValue(LogFileProperties.StartTimestamp, timestamp);
				if (timestamp != null)
					_properties.SetValue(LogFileProperties.EndTimestamp, timestamp);
				var duration = timestamp - _properties.GetValue(LogFileProperties.StartTimestamp);
				_properties.SetValue(LogFileProperties.Duration, duration);

				// The user supplies us with a list of properties to add, however we will
				// never allow the user to supply us things like index or line number.
				// Therefore we create a log entry which we actually want to add...
				var finalLogEntry = new LogEntry(Columns);

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

				return finalLogEntry;
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