using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     This <see cref="ILogFile" /> implementation is responsible for exposing the windows event log
	///     via tailviewer's API.
	/// </summary>
	/// <remarks>
	///    Plugin authors are deliberately prevented from instantiating this type directly because it's constructor signature may change
	///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateEventLogFile"/>
	///    who's signature is guaranteed to never change.
	/// </remarks>
	internal sealed class EventLogFile
		: ILogFile
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

		private readonly InMemoryLogFile _buffer;
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
		internal EventLogFile(ITaskScheduler scheduler, string fileName)
		{
			_scheduler = scheduler;
			_fileName = fileName;

			_buffer = new InMemoryLogFile();
			_buffer.SetValue(LogFileProperties.Created, DateTime.Now);

			_cancellationTokenSource = new CancellationTokenSource();

			_readTask = _scheduler.StartPeriodic(RunOnce, ToString());
		}

		#region Overrides of AbstractLogFile

		/// <inheritdoc />
		public int OriginalCount => _buffer.OriginalCount;

		/// <inheritdoc />
		public int MaxCharactersPerLine => _buffer.MaxCharactersPerLine;

		/// <inheritdoc />
		public bool EndOfSourceReached => false;

		/// <inheritdoc />
		public int Count => _buffer.Count;

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumn> Columns => _buffer.Columns;

		/// <inheritdoc />
		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_buffer.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public void RemoveListener(ILogFileListener listener)
		{
			_buffer.RemoveListener(listener);
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFilePropertyDescriptor> Properties => _buffer.Properties;

		/// <inheritdoc />
		public object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			return _buffer.GetValue(propertyDescriptor);
		}

		/// <inheritdoc />
		public T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			return _buffer.GetValue(propertyDescriptor);
		}

		/// <inheritdoc />
		public void GetValues(ILogFileProperties properties)
		{
			_buffer.GetValues(properties);
		}

		/// <inheritdoc />
		public void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			_buffer.GetColumn(section, column, buffer, destinationIndex);
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> indices,
		                         ILogFileColumn<T> column,
		                         T[] buffer,
		                         int destinationIndex)
		{
			_buffer.GetColumn(indices, column, buffer, destinationIndex);
		}

		/// <inheritdoc />
		public void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			_buffer.GetEntries(section, buffer, destinationIndex);
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			_buffer.GetEntries(indices, buffer, destinationIndex);
		}

		/// <inheritdoc />
		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			_buffer.GetSection(section, dest);
		}

		/// <inheritdoc />
		public LogLine GetLine(int index)
		{
			return _buffer.GetLine(index);
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
						_buffer.SetValue(LogFileProperties.StartTimestamp, eventRecord.TimeCreated);
						_setStartTimestamp = true;
					}

					var logLevel = GetLevelFlags(eventRecord);
					var message = FormatRawMessage(eventRecord);
					var logEntryIndex = _nextLogEntryIndex++;
					var lines = message.Split('\n');
					bool first = true;
					foreach (var line in lines)
					{
						var values = new Dictionary<ILogFileColumn, object>
						{
							{LogFileColumns.RawContent, FormatLine(line, first)},
							{LogFileColumns.LogLevel, logLevel},
							{LogFileColumns.LogEntryIndex, new LogEntryIndex(logEntryIndex)},
							{LogFileColumns.Timestamp, eventRecord.TimeCreated}
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

					_buffer.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);

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