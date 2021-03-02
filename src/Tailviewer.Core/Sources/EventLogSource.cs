using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     This <see cref="ILogSource" /> implementation is responsible for exposing the windows event log
	///     via tailviewer's API.
	/// </summary>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="ILogSourceFactory.CreateEventLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	internal sealed class EventLogSource
		: ILogSource
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// 
		/// </summary>
		public const string FilePathPrefix = @"EventLog://";

		/// <summary>
		/// </summary>
		public const string SystemEventLogName = FilePathPrefix + @"System";

		/// <summary>
		/// 
		/// </summary>
		public const string ApplicationEventLogName = FilePathPrefix + @"Application";

		private readonly InMemoryLogSource _buffer;
		private readonly CancellationTokenSource _cancellationTokenSource;

		private EventLogReader _reader;
		private readonly IPeriodicTask _readTask;
		private readonly ITaskScheduler _scheduler;
		private readonly string _fileName;
		private int _nextLogEntryIndex;
		private bool _setStartTimestamp;
		private bool _loggedException;

		/// <summary>
		/// </summary>
		/// <param name="scheduler"></param>
		/// <param name="fileName"></param>
		internal EventLogSource(ITaskScheduler scheduler, string fileName)
		{
			_scheduler = scheduler;
			_fileName = fileName;

			_buffer = new InMemoryLogSource();
			_buffer.SetValue(Core.Properties.Created, DateTime.Now);

			_cancellationTokenSource = new CancellationTokenSource();

			_readTask = _scheduler.StartPeriodic(RunOnce, ToString());
		}

		#region Overrides of AbstractLogFile

		/// <inheritdoc />
		public bool EndOfSourceReached => false;

		/// <inheritdoc />
		public int Count => _buffer.Count;

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns => _buffer.Columns;

		/// <inheritdoc />
		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_buffer.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogSourceListener listener)
		{
			_buffer.RemoveListener(listener);
		}

		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _buffer.Properties;

		/// <inheritdoc />
		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			return _buffer.GetProperty(property);
		}

		/// <inheritdoc />
		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			return _buffer.GetProperty(property);
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			throw new NotImplementedException();
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_buffer.GetAllProperties(destination);
		}

		/// <inheritdoc />
		public void GetColumn<T>(LogSourceSection sourceSection, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			_buffer.GetColumn(sourceSection, column, destination, destinationIndex, queryOptions);
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                         IColumnDescriptor<T> column,
		                         T[] destination,
		                         int destinationIndex,
		                         LogSourceQueryOptions queryOptions)
		{
			_buffer.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
		}

		/// <inheritdoc />
		public void GetEntries(LogSourceSection sourceSection, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			_buffer.GetEntries(sourceSection, destination, destinationIndex, queryOptions);
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			_buffer.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);
		}

		/// <inheritdoc />
		public double Progress => _buffer.Progress;

		/// <inheritdoc />
		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			throw new NotImplementedException();
		}

		private TimeSpan RunOnce()
		{
			if (_reader == null)
			{
				if (!TryCreateEventLogReader())
					return TimeSpan.FromSeconds(10);
			}

			var stopwatch = Stopwatch.StartNew();
			var maximum = TimeSpan.FromSeconds(value: 1);
			EventRecord eventRecord;
			while (stopwatch.Elapsed < maximum && !_cancellationTokenSource.Token.IsCancellationRequested &&
			       (eventRecord = TryReadNextEvent()) != null)
			{
				using (eventRecord)
				{
					if (!_setStartTimestamp && eventRecord.TimeCreated != null)
					{
						_buffer.SetValue(Core.Properties.StartTimestamp, eventRecord.TimeCreated);
						_setStartTimestamp = true;
					}

					var logLevel = GetLevelFlags(eventRecord);
					var message = FormatRawMessage(eventRecord);
					var logEntryIndex = _nextLogEntryIndex++;
					var lines = message.Split('\n');
					bool first = true;
					foreach (var line in lines)
					{
						var values = new Dictionary<IColumnDescriptor, object>
						{
							{Core.Columns.RawContent, FormatLine(line, first)},
							{Core.Columns.LogLevel, logLevel},
							{Core.Columns.LogEntryIndex, new LogEntryIndex(logEntryIndex)},
							{Core.Columns.Timestamp, eventRecord.TimeCreated}
						};
						_buffer.Add(new ReadOnlyLogEntry(values));
						first = false;
					}
				}
			}

			return maximum;
		}

		private bool TryCreateEventLogReader()
		{
			try
			{
				var path = GetPath();
				var query = new EventLogQuery(path, PathType.LogName, "*");
				_reader = new EventLogReader(query);
				_loggedException = false;

				return true;
			}
			catch (Exception e)
			{
				if (!_loggedException)
				{
					Log.ErrorFormat("Unable to create event log reader: {0}", e);

					_buffer.SetValue(Core.Properties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);

					_loggedException = true;
				}
				else
				{
					Log.DebugFormat("Unable to create event log reader: {0}", e);
				}

				return false;
			}
		}

		private string GetPath()
		{
			if (!_fileName.StartsWith(FilePathPrefix))
				throw new InvalidOperationException(string.Format("Unknown event log path: {0}", _fileName));

			var path = _fileName.Substring(FilePathPrefix.Length);
			return path;
		}

		private EventRecord TryReadNextEvent()
		{
			try
			{
				return _reader.ReadEvent(TimeSpan.FromSeconds(value: 1));
			}
			catch (InvalidOperationException e)
			{
				Log.DebugFormat("Caught expected exception while trying to read event log: {0}", e);
				return null;
			}
		}

		private static string FormatLine(string line, bool first)
		{
			if (first)
				return line;

			return '\t' + line;
		}

		private string FormatRawMessage(EventRecord eventRecord)
		{
			var builder = new StringBuilder();

			builder.Append(eventRecord.TimeCreated);
			builder.Append(' ');
			builder.Append(GetLevel(eventRecord));
			builder.Append(' ');
			builder.Append(eventRecord.FormatDescription());

			return builder.ToString();
		}

		private static LevelFlags GetLevelFlags(EventRecord eventRecord)
		{
			switch (eventRecord.Level)
			{
				case 4:
					return LevelFlags.Info;

				case 3:
					return LevelFlags.Warning;

				case 2:
					return LevelFlags.Error;

				default:
					return LevelFlags.Other;
			}
		}

		private static string GetLevel(EventRecord eventRecord)
		{
			switch (eventRecord.Level)
			{
				case 4:
					return "INFO";

				case 3:
					return "WARN";

				case 2:
					return "ERROR";

				default:
					return eventRecord.LevelDisplayName;
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_scheduler.StopPeriodic(_readTask);

			_reader?.Dispose();
		}

		#endregion
	}
}