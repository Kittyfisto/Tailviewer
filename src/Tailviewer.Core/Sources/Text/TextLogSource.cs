using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Settings;
using Tailviewer.Plugins;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///     The bread-and-butter <see cref="ILogSource" /> implementation for Tailviewer.
	///     Responsible for scanning and reading the content of a file on disk, forwarding
	///     them to its <see cref="ILogSourceListener"/>s.
	/// </summary>
	/// <remarks>
	///     TODO: Delete once the new one is finished.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class TextLogSource
		: AbstractLogSource
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private readonly IServiceContainer _serviceContainer;

		#region Data

		private readonly Encoding _encoding;
		private readonly List<LogLine> _entries;
		private readonly object _syncRoot;
		private readonly NoThrowLogLineTranslator _translator;

		#endregion

		#region Parsing
		
		private readonly ILogFileFormatMatcher _formatMatcher;
		private readonly ILogEntryParserPlugin _parserPlugin;
		private ILogEntryParser _parser;
		private ILogFileFormat _parserFormat;

		#endregion

		#region Listeners

		private readonly string _fileName;
		private readonly string _fullFilename;
		private int _numberOfLinesRead;
		private bool _lastLineHadNewline;
		private string _untrimmedLastLine;
		private long _lastStreamPosition;
		private bool _loggedTimestampWarning;

		#endregion

		#region Properties

		private DateTime? _startTimestamp;
		private DateTime? _endTimestamp;
		private DateTime? _lastModified;
		private TimeSpan? _duration;
		private int _maxCharactersInLine;

		/// <summary>
		///    This object exists to hold all properties we want to eventually forward to the user.
		///    It is to be copied to <see cref="_properties"/> in *one* operation when we want the user to
		///    see its values.
		/// </summary>
		/// <remarks>
		///    This object should only be modified from the Task which executes RunOnce() and not from any other
		///    thread or there be demons.
		/// </remarks>
		private readonly PropertiesBufferList _localProperties;

		/// <summary>
		///    These are the properties the user gets to see. They are only updated when we deem it necessary.
		/// </summary>
		private readonly ConcurrentPropertiesList _properties;

		#endregion

		/// <summary>
		///    Initializes this text log file.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateTextLogFile"/>.
		/// </remarks>
		/// <param name="serviceContainer"></param>
		/// <param name="fileName"></param>
		internal TextLogSource(IServiceContainer serviceContainer,
		                     string fileName)
			: base(serviceContainer.TryRetrieve<ITaskScheduler>())
		{
			_serviceContainer = serviceContainer;
			_fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			_fullFilename = fileName;
			if (!Path.IsPathRooted(_fullFilename))
				_fullFilename = Path.Combine(Directory.GetCurrentDirectory(), fileName);

			var translator = serviceContainer.TryRetrieve<ILogLineTranslator>();
			if (translator != null)
				_translator = new NoThrowLogLineTranslator(translator);

			_formatMatcher = serviceContainer.Retrieve<ILogFileFormatMatcher>();
			_parserPlugin = serviceContainer.Retrieve<ILogEntryParserPlugin>();
			_entries = new List<LogLine>();

			_localProperties = new PropertiesBufferList(GeneralProperties.Minimum);
			_localProperties.SetValue(GeneralProperties.Name, _fileName);
			_localProperties.Add(TextProperties.LineCount);
			_localProperties.Add(TextProperties.MaxCharactersInLine);
			_localProperties.SetValue(TextProperties.LineCount, 0);
			_localProperties.SetValue(TextProperties.MaxCharactersInLine, 0);

			_properties = new ConcurrentPropertiesList(GeneralProperties.Minimum);
			SynchronizePropertiesWithUser();
			_syncRoot = new object();

			var defaultEncoding = serviceContainer.TryRetrieve<ILogFileSettings>()?.DefaultEncoding;
			var overwrittenEncoding = serviceContainer.TryRetrieve<Encoding>();
			var encoding = overwrittenEncoding ?? defaultEncoding;
			_encoding = encoding ?? Encoding.UTF8;
			_properties.SetValue(GeneralProperties.Encoding, encoding);

			Log.DebugFormat("Log File '{0}' is interpreted using {1}", _fileName, _encoding.EncodingName);

			StartTask();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _fileName;
		}

		/// <inheritdoc />
		public override IReadOnlyList<IColumnDescriptor> Columns => LogColumns.Minimum;

		/// <inheritdoc />
		public override IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { return _properties.Properties; }
		}

		/// <inheritdoc />
		public override object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		/// <inheritdoc />
		public override T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		public override void SetProperty(IPropertyDescriptor property, object value)
		{
			throw new NotImplementedException();
		}

		public override void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		/// <inheritdoc />
		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceIndices.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			lock (_syncRoot)
			{
				if (Equals(column, LogColumns.Index) ||
				    Equals(column, LogColumns.OriginalIndex))
				{
					GetIndex(sourceIndices, (LogLineIndex[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.LogEntryIndex))
				{
					GetIndex(sourceIndices, (LogEntryIndex[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.LineNumber) ||
				         Equals(column, LogColumns.OriginalLineNumber))
				{
					GetLineNumber(sourceIndices, (int[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.LogLevel))
				{
					GetLogLevel(sourceIndices, (LevelFlags[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.Timestamp))
				{
					GetTimestamp(sourceIndices, (DateTime?[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.DeltaTime))
				{
					GetDeltaTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.ElapsedTime))
				{
					GetElapsedTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.OriginalDataSourceName))
				{
					GetDataSourceName(sourceIndices, (string[]) (object) destination, destinationIndex);
				}
				else if (Equals(column, LogColumns.RawContent))
				{
					GetRawContent(sourceIndices, (string[])(object)destination, destinationIndex);
				}
				else
				{
					throw new NoSuchColumnException(column);
				}
			}
		}

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			foreach (var column in destination.Columns)
			{
				destination.CopyFrom(column, destinationIndex, this, sourceIndices, queryOptions);
			}
		}

		/// <inheritdoc />
		protected override TimeSpan RunOnce(CancellationToken token)
		{
			bool read = false;

			try
			{
				if (!File.Exists(_fileName))
				{
					SetDoesNotExist();
				}
				else
				{
					var info = new FileInfo(_fileName);
					var fileSize = info.Length;
					_localProperties.SetValue(GeneralProperties.LastModified, info.LastWriteTime);
					_localProperties.SetValue(GeneralProperties.Created, info.CreationTime);
					_localProperties.SetValue(GeneralProperties.Size, Size.FromBytes(fileSize));
					UpdatePercentageProcessed(_lastStreamPosition, fileSize, allow100Percent: true);
					SynchronizePropertiesWithUser();

					using (var stream = new FileStream(_fileName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite))
					{
						var format = _localProperties.GetValue(GeneralProperties.Format);
						var certainty = _localProperties.GetValue(GeneralProperties.FormatDetectionCertainty);
						if (format == null || certainty != Certainty.Sure)
						{
							format = TryFindFormat(stream, out certainty);

							if (format != null)
							{
								// We only need to create a new parser when we don't have one already or we've changed out minds
								if (_parser == null || !Equals(format, _parserFormat))
								{
									_parser = _parserPlugin.CreateParser(_serviceContainer, format);
									_parserFormat = format;
								}
							}

							_localProperties.SetValue(GeneralProperties.Format, format);
							_localProperties.SetValue(GeneralProperties.FormatDetectionCertainty, certainty);
						}

						var encoding = format?.Encoding ?? _encoding;
						_localProperties.SetValue(GeneralProperties.Encoding, encoding);
						using (var reader = new StreamReaderEx(stream, encoding))
						{
							// We change the error flag explicitly AFTER opening
							// the stream because that operation might throw if we're
							// not allowed to access the file (in which case a different
							// error must be set).

							_localProperties.SetValue(GeneralProperties.EmptyReason, ErrorFlags.None);
							if (stream.Length >= _lastStreamPosition)
							{
								stream.Position = _lastStreamPosition;
							}
							else
							{
								OnReset(stream, out _numberOfLinesRead, out _lastStreamPosition);
							}

							int numProcessed = 0;
							string currentLine;
							while ((currentLine = reader.ReadLine()) != null)
							{
								token.ThrowIfCancellationRequested();

								bool lastLineHadNewline = _lastLineHadNewline;
								var trimmedLine = currentLine.TrimNewlineEnd(out _lastLineHadNewline);
								var entryCount = _entries.Count;
								if (entryCount > 0 && !lastLineHadNewline)
								{
									// We need to remove the last line and create a new line
									// that consists of the entire content.
									RemoveLast();
									trimmedLine = _untrimmedLastLine + trimmedLine;
									_untrimmedLastLine = _untrimmedLastLine + currentLine;
								}
								else
								{
									_untrimmedLastLine = currentLine;
									++_numberOfLinesRead;
									read = true;
								}

								IReadOnlyLogEntry logEntry = new RawTextLogEntry(_entries.Count, trimmedLine, _fullFilename);
								if (_parser != null)
								{
									var parsedLogEntry = _parser.Parse(logEntry);
									if (parsedLogEntry != null)
										logEntry = parsedLogEntry;
								}

								Add(logEntry.RawContent,
								    logEntry.LogLevel,
								    _numberOfLinesRead,
								    logEntry.Timestamp);

								if (++numProcessed % 1000 == 0)
								{
									// Here's the deal: Since we're processing the file in chunks, we advance the underlying
									// stream faster than we're actually consuming lines. This means that it's quite likely
									// that at the end of the file, we have moved the stream to the end, but have not quite
									// yet processed the underlying buffer from StreamReaderEx. The percentage processed
									// should be accurate enough so that if it is at 100%, then no more log entries are added.
									// We can only guarantee that when we have processed all lines and therefore we reserve
									// setting the percentage to 100% ONLY when we can read no more lines
									// (See the SetEndOfSourceReached() call below, outside the loop).
									UpdatePercentageProcessed(stream.Position, fileSize, allow100Percent: false);

									SynchronizePropertiesWithUser();
								}
							}

							_lastStreamPosition = stream.Position;
							_localProperties.SetValue(TextProperties.LineCount, _entries.Count);
							_localProperties.SetValue(GeneralProperties.LogEntryCount, _entries.Count);
						}
					}

					Listeners.OnRead(_numberOfLinesRead);
					SetEndOfSourceReached();
				}
			}
			catch (FileNotFoundException e)
			{
				SetError(ErrorFlags.SourceDoesNotExist);
				Log.Debug(e);
			}
			catch (DirectoryNotFoundException e)
			{
				SetError(ErrorFlags.SourceDoesNotExist);
				Log.Debug(e);
			}
			catch (OperationCanceledException e)
			{
				Log.Debug(e);
			}
			catch (UnauthorizedAccessException e)
			{
				SetError(ErrorFlags.SourceCannotBeAccessed);
				Log.Debug(e);
			}
			catch (IOException e)
			{
				SetError(ErrorFlags.SourceCannotBeAccessed);
				Log.Debug(e);
			}
			catch (Exception e)
			{
				Log.Debug(e);
			}

			if (read)
				return TimeSpan.Zero;

			return TimeSpan.FromMilliseconds(100);
		}

		/// <summary>
		/// </summary>
		private void SetEndOfSourceReached()
		{
			// Now this line is very important:
			// Most tests expect that listeners have been notified
			// of all pending changes when the source enters the
			// "EndOfSourceReached" state. This would be true, if not
			// for listeners specifying a timespan that should elapse between
			// calls to OnLogFileModified. The listener collection has
			// been notified, but the individual listeners may not be, because
			// neither the maximum line count, nor the maximum timespan has elapsed.
			// Therefore we flush the collection to ensure that ALL listeners have been notified
			// of ALL changes (even if they didn't want them yet) before we enter the
			// EndOfSourceReached state.
			Listeners.Flush();
			_localProperties.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			SynchronizePropertiesWithUser();
		}

		#region Overrides of AbstractLogFile

		protected override void DisposeAdditional()
		{
			lock (_syncRoot)
			{
				_entries.Clear();
				_entries.Capacity = 0;

				_localProperties.Clear();
				_properties.Clear();
			}

			base.DisposeAdditional();
		}

		#endregion

		private void UpdatePercentageProcessed(long streamPosition, long fileSize, bool allow100Percent)
		{
			var processed = Percentage.Of(streamPosition, fileSize).Clamped();
			if (processed >= Percentage.FromPercent(99) && !allow100Percent)
				processed = Percentage.FromPercent(99);
			_localProperties.SetValue(GeneralProperties.PercentageProcessed, processed);
		}

		private ILogFileFormat TryFindFormat(FileStream stream, out Certainty certainty)
		{
			var pos = stream.Position;

			const int maxHeaderLength = 512;
			var length = Math.Min(maxHeaderLength, stream.Length - pos);
			var header = new byte[length];
			stream.Read(header, 0, header.Length);
			certainty = length >= maxHeaderLength
				? Certainty.Sure
				: Certainty.Uncertain;

			_formatMatcher.TryMatchFormat(_fullFilename, header, out var format);
			if (format != null)
				return format;

			return LogFileFormats.GenericText;
		}

		private void GetTimestamp(IReadOnlyList<LogLineIndex> indices, DateTime?[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					buffer[destinationIndex + i] = GetLogLine(index)?.Timestamp;
				}
			}
		}

		private void GetDeltaTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					var value = GetLogLine(index)?.Timestamp;
					var previousValue = GetLogLine(index - 1)?.Timestamp;
					buffer[destinationIndex + i] = value - previousValue;
				}
			}
		}

		private void GetIndex(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _entries.Count)
					{
						buffer[destinationIndex + i] = index;
					}
					else
					{
						buffer[destinationIndex + i] = LogColumns.Index.DefaultValue;
					}
				}
			}
		}

		private void GetIndex(IReadOnlyList<LogLineIndex> indices, LogEntryIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _entries.Count)
					{
						buffer[destinationIndex + i] = new LogEntryIndex((int)index);
					}
					else
					{
						buffer[destinationIndex + i] = LogColumns.LogEntryIndex.DefaultValue;
					}
				}
			}
		}

		private void GetRawContent(IReadOnlyList<LogLineIndex> indices, string[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					var line = GetLogLine(index);
					buffer[destinationIndex + i] = line != null
						? line.Value.Message
						: LogColumns.RawContent.DefaultValue;
				}
			}
		}

		private void GetElapsedTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				var startTimestamp = _startTimestamp;
				if (startTimestamp != null)
				{
					for (int i = 0; i < indices.Count; ++i)
					{
						var index = indices[i];
						var line = GetLogLine(index);
						buffer[destinationIndex + i] = line != null
							? line.Value.Timestamp - startTimestamp
							: LogColumns.ElapsedTime.DefaultValue;
					}
				}
				else
				{
					for (int i = 0; i < indices.Count; ++i)
					{
						buffer[destinationIndex + i] = LogColumns.ElapsedTime.DefaultValue;
					}
				}

				
			}
		}

		private void GetDataSourceName(IReadOnlyList<LogLineIndex> sourceIndices, string[] buffer, int destinationIndex)
		{
			for (int i = 0; i < sourceIndices.Count; ++i)
			{
				buffer[destinationIndex + i] = _fileName;
			}
		}

		private void GetLogLevel(IReadOnlyList<LogLineIndex> sourceIndices, LevelFlags[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < sourceIndices.Count; ++i)
				{
					var index = sourceIndices[i];
					var line = GetLogLine(index);
					buffer[destinationIndex + i] = line != null
						? line.Value.Level
						: LogColumns.LogLevel.DefaultValue;
				}
			}
		}

		private void GetLineNumber(IReadOnlyList<LogLineIndex> indices, int[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _entries.Count)
					{
						var lineNumber = (int) (index + 1);
						buffer[destinationIndex + i] = lineNumber;
					}
					else
					{
						buffer[destinationIndex + i] = LogColumns.LineNumber.DefaultValue;
					}
				}
			}
		}

		private LogLine? GetLogLine(LogLineIndex index)
		{
			if (index >= 0 && index < _entries.Count)
			{
				return _entries[(int) index];
			}

			return null;
		}

		private void SetDoesNotExist()
		{
			_localProperties.SetValue(GeneralProperties.Created, null);
			_localProperties.SetValue(GeneralProperties.Size, null);
			_localProperties.SetValue(GeneralProperties.Format, null);
			_localProperties.SetValue(GeneralProperties.FormatDetectionCertainty, Certainty.None);
			_localProperties.SetValue(GeneralProperties.Encoding, null);
			_localProperties.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			OnReset(null, out _numberOfLinesRead, out _lastStreamPosition);
			SetError(ErrorFlags.SourceDoesNotExist);
		}

		private void SetError(ErrorFlags error)
		{
			_localProperties.SetValue(GeneralProperties.EmptyReason, error);
			SynchronizePropertiesWithUser();
			SetEndOfSourceReached();
		}

		private void OnReset(FileStream stream,
		                     out int numberOfLinesRead,
		                     out long lastPosition)
		{
			lastPosition = 0;
			if (stream != null)
				stream.Position = 0;

			numberOfLinesRead = 0;
			_startTimestamp = null;
			_endTimestamp = null;
			_duration = null;
			_maxCharactersInLine = 0;
			_entries.Clear();
			SynchronizePropertiesWithUser();

			Listeners.Reset();
		}

		private void Add(string line, LevelFlags level, int numberOfLinesRead, DateTime? timestamp)
		{
			lock (_syncRoot)
			{
				int lineIndex = _entries.Count;
				var logLine = new LogLine(lineIndex, lineIndex, line, level, timestamp);
				var translated = Translate(logLine);
				_entries.Add(translated);

				// For huge log files, updating properties is somewhat expensive.
				// We'll update our fields all the time, but only write back our properties
				// when it's necessary.
				_maxCharactersInLine = Math.Max(_maxCharactersInLine, translated.Message?.Length ?? 0);
				if (timestamp != null)
				{
					if (_startTimestamp == null)
						_startTimestamp = timestamp;
					_endTimestamp = timestamp;
					_duration = _endTimestamp - _startTimestamp;
					if (TryGetLastBetterModified(timestamp.Value, out var newLastModified))
						_lastModified = newLastModified;
				}

			}

			Listeners.OnRead(numberOfLinesRead);
		}

		private bool TryGetLastBetterModified(DateTime timestamp, out DateTime lastModified)
		{
			var currentLastModified = _lastModified;
			var difference = timestamp - currentLastModified;
			if (difference >= TimeSpan.FromSeconds(10))
			{
				// I've had this issue occur on one system and I can't really explain it.
				// For some reason, new FileInfo(...).LastWriteTime will not give correct
				// results when we can see that the bloody file is being written to as we speak.
				// As a work around, we'll just use the timestamp of the log file as lastModifed
				// if that happens to be newer.
				//
				// This might be related to files being consumed from a network share. Anyways,
				// this quick fix should improve user feedback...
				if (!_loggedTimestampWarning)
				{
					Log.InfoFormat(
						"FileInfo.LastWriteTime results in a time stamp that is less than the parsed timestamp (LastWriteTime={0}, Parsed={1}), using parsed instead",
						currentLastModified,
						timestamp
					);
					_loggedTimestampWarning = true;
				}

				lastModified = timestamp;
				return true;
			}

			lastModified = default;
			return false;
		}

		private void SynchronizePropertiesWithUser()
		{
			_localProperties.SetValue(TextProperties.MaxCharactersInLine, _maxCharactersInLine);
			_localProperties.SetValue(TextProperties.LineCount, _entries.Count);
			_localProperties.SetValue(GeneralProperties.LogEntryCount, _entries.Count);
			_localProperties.SetValue(GeneralProperties.StartTimestamp, _startTimestamp);
			_localProperties.SetValue(GeneralProperties.EndTimestamp, _endTimestamp);
			_localProperties.SetValue(GeneralProperties.Duration, _duration);
			_localProperties.SetValue(GeneralProperties.LogEntryCount, _entries.Count);

			// We want to update the user-facing properties in a single synchronized OP
			// so that the properties as retrieved by the user are in sync
			_properties.CopyFrom(_localProperties);
		}

		[Pure]
		private LogLine Translate(LogLine logLine)
		{
			if (_translator == null)
				return logLine;

			var translated = _translator.Translate(this, logLine);

			// With the introduction of LevelFlags.Other, we need to ensure that "old" plugins continue to work
			// as expected (Now, Other shall be used where previously None had to be).
			if (translated.Level == LevelFlags.None)
				translated.Level = LevelFlags.Other;

			return translated;
		}

		private void RemoveLast()
		{
			var index = _entries.Count - 1;
			lock (_syncRoot)
			{
				_entries.RemoveAt(index);
			}
			Listeners.Invalidate(index, 1);
		}
	}
}