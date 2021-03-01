using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Metrolib;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Properties;

namespace Tailviewer.Core.Sources
{
	/// <summary>
	///     A <see cref="ILogSource"/> implementation which buffers the entire contents in memory.
	/// </summary>
	/// <remarks>
	///     Should only be used for log files who's content actually fits into memory.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	public sealed class InMemoryLogSource
		: ILogSource
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly LogBufferList _logBuffer;
		private readonly PropertiesBufferList _properties;
		private readonly LogSourceListenerCollection _listeners;

		private readonly object _syncRoot;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public InMemoryLogSource()
			: this(GeneralColumns.Minimum)
		{ }

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public InMemoryLogSource(IReadOnlyDictionary<IReadOnlyPropertyDescriptor, object> properties)
			: this(GeneralColumns.Minimum, properties)
		{ }

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public InMemoryLogSource(params IColumnDescriptor[] columns)
			: this((IEnumerable < IColumnDescriptor > )columns)
		{}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="content"></param>
		public InMemoryLogSource(IReadOnlyLogBuffer content)
			: this(content.Columns)
		{
			AddRange(content);
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public InMemoryLogSource(IEnumerable<IColumnDescriptor> columns)
			: this(columns, new Dictionary<IReadOnlyPropertyDescriptor, object>())
		{ }

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="properties"></param>
		public InMemoryLogSource(IEnumerable<IColumnDescriptor> columns, IReadOnlyDictionary<IReadOnlyPropertyDescriptor, object> properties)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));

			_syncRoot = new object();
			_logBuffer = new LogBufferList(GeneralColumns.CombineWithMinimum(columns));
			_listeners = new LogSourceListenerCollection(this);

			_properties = new PropertiesBufferList(GeneralProperties.Minimum);
			_properties.SetValue(GeneralProperties.Size, Size.Zero);
			_properties.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			foreach (var pair in properties)
			{
				_properties.SetValue(pair.Key, pair.Value);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			// https://github.com/Kittyfisto/Tailviewer/issues/282
			_listeners.Clear();
			_properties.Clear();

			lock (_syncRoot)
			{
				_logBuffer.Clear();
			}
		}

		/// <inheritdoc />
		public int Count => _logBuffer.Count;

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns => _logBuffer.Columns;

		/// <inheritdoc />
		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogSourceListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		#region Properties

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _properties.Properties;

		/// <inheritdoc />
		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			object value;
			_properties.TryGetValue(property, out value);
			return value;
		}

		/// <inheritdoc />
		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			T value;
			_properties.TryGetValue(property, out value);
			return value;
		}

		/// <inheritdoc />
		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_properties.SetValue(property, value);
		}

		/// <inheritdoc />
		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_properties.SetValue(property, value);
		}

		/// <inheritdoc />
		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyDescriptor"></param>
		/// <param name="value"></param>
		public void SetValue<T>(IReadOnlyPropertyDescriptor<T> propertyDescriptor, T value)
		{
			_properties.SetValue(propertyDescriptor, value);
		}

		#endregion

		/// <inheritdoc />
		public void GetColumn<T>(LogSourceSection sourceSection, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			_logBuffer.CopyTo(column, (int)sourceSection.Index, destination, destinationIndex, sourceSection.Count);
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			_logBuffer.CopyTo(column, new Int32View(sourceIndices), destination, destinationIndex);
		}

		/// <inheritdoc />
		public void GetEntries(LogSourceSection sourceSection, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			lock (_syncRoot)
			{
				foreach (var column in destination.Columns)
					destination.CopyFrom(column, destinationIndex, _logBuffer, new Int32View(sourceSection));
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			lock (_syncRoot)
			{
				foreach (var column in destination.Columns)
					destination.CopyFrom(column, destinationIndex, _logBuffer, new Int32View(sourceIndices));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public IReadOnlyLogEntry this[int index]
		{
			get { return _logBuffer[index]; }
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
				if (originalLineIndex >= _logBuffer.Count)
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
				if (_logBuffer.Count > 0)
				{
					_logBuffer.Clear();
					SetValue(GeneralProperties.LogEntryCount, _logBuffer.Count);
					_properties.SetValue(TextProperties.MaxCharactersInLine, 0);
					_properties.SetValue(GeneralProperties.StartTimestamp, null);
					_properties.SetValue(GeneralProperties.EndTimestamp, null);
					_properties.SetValue(GeneralProperties.Size, Size.Zero);
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

				if (index > _logBuffer.Count)
				{
					Log.WarnFormat("Invalid index '{0}', Count is '{1}'", index, _logBuffer.Count);
					return;
				}

				var available = _logBuffer.Count - index;
				_logBuffer.RemoveRange((int)index, available);
				SetValue(GeneralProperties.LogEntryCount, _logBuffer.Count);
				_listeners.Remove((int)index, available);
				Touch();
			}
		}

		private void Touch()
		{
			_properties.SetValue(GeneralProperties.LastModified, DateTime.Now);
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
				UpdateTimestampProperties(timestamp);
				var logEntryIndex = GetLogEntryIndex(timestamp, out var elapsed, out var deltaTime);

				foreach (var line in lines)
				{
					var logEntry = new LogEntry
					{
						Index = _logBuffer.Count,
						OriginalIndex = _logBuffer.Count,
						LineNumber = _logBuffer.Count + 1,
						OriginalLineNumber = _logBuffer.Count + 1,
						LogEntryIndex = logEntryIndex,
						RawContent = line,
						LogLevel = level,
						Timestamp = timestamp,
						ElapsedTime = elapsed,
						DeltaTime = deltaTime
					};
					_logBuffer.Add(logEntry);
					SetValue(GeneralProperties.LogEntryCount, _logBuffer.Count);
					SetValue(TextProperties.MaxCharactersInLine, Math.Max(GetProperty(TextProperties.MaxCharactersInLine), line.Length));
				}
				Touch();
				_listeners.OnRead(_logBuffer.Count);
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
		public void Add(IReadOnlyDictionary<IColumnDescriptor, object> entry)
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
				entry.TryGetValue(GeneralColumns.Timestamp, out var timestamp);
				UpdateTimestampProperties(timestamp);
				var logEntryIndex = GetLogEntryIndex(timestamp, out var elapsed, out var deltaTime);

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

				finalLogEntry.Index = _logBuffer.Count;
				finalLogEntry.OriginalIndex = _logBuffer.Count;
				finalLogEntry.LineNumber = _logBuffer.Count + 1;
				finalLogEntry.OriginalLineNumber = _logBuffer.Count + 1;
				finalLogEntry.LogEntryIndex = logEntryIndex;
				finalLogEntry.Timestamp = timestamp;
				finalLogEntry.ElapsedTime = elapsed;
				finalLogEntry.DeltaTime = deltaTime;

				_logBuffer.Add(finalLogEntry);
				SetValue(GeneralProperties.LogEntryCount, _logBuffer.Count);
				SetValue(TextProperties.MaxCharactersInLine, Math.Max(GetProperty(TextProperties.MaxCharactersInLine), finalLogEntry.RawContent?.Length ?? 0));
				Touch();
				_listeners.OnRead(_logBuffer.Count);

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

		private LogEntryIndex GetLogEntryIndex(DateTime? timestamp, out TimeSpan? elapsed, out TimeSpan? deltaTime)
		{
			LogEntryIndex logEntryIndex;
			DateTime? lastTimestamp = null;
			if (_logBuffer.Count > 0)
			{
				var last = _logBuffer[_logBuffer.Count - 1];
				logEntryIndex = last.LogEntryIndex + 1;
				lastTimestamp = last.Timestamp;
			}
			else
			{
				logEntryIndex = 0;
			}

			elapsed = timestamp - _properties.GetValue(GeneralProperties.StartTimestamp);
			deltaTime = timestamp - lastTimestamp;
			return logEntryIndex;
		}

		private void UpdateTimestampProperties(DateTime? timestamp)
		{
			if (timestamp != null)
			{
				var startTimestamp = _properties.GetValue(GeneralProperties.StartTimestamp);
				if (startTimestamp == null)
				{
					_properties.SetValue(GeneralProperties.StartTimestamp, timestamp);
					_properties.SetValue(GeneralProperties.Duration, TimeSpan.Zero);
				}
				else
				{
					_properties.SetValue(GeneralProperties.Duration, timestamp - startTimestamp);
				}

				_properties.SetValue(GeneralProperties.EndTimestamp, timestamp);
			}
		}
	}
}
