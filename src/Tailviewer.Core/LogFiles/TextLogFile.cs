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
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Settings;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     The bread-and-butter <see cref="ILogFile" /> implementation for Tailviewer.
	///     Responsible for scanning and reading the content of a file on disk, forwarding
	///     them to its <see cref="ILogFileListener"/>s.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class TextLogFile
		: AbstractLogFile
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private readonly IServiceContainer _serviceContainer;

		#region Data

		private readonly Encoding _encoding;
		private readonly List<LogLine> _entries;
		private readonly object _syncRoot;
		private readonly ILogFileProperties _properties;
		private int _maxCharactersPerLine;
		private readonly NoThrowLogLineTranslator _translator;

		#endregion

		#region Parsing
		
		private readonly ILogFileFormatMatcher _formatMatcher;
		private readonly ITextLogFileParserPlugin _parserPlugin;
		private ITextLogFileParser _parser;
		private ILogFileFormat _parserFormat;

		#endregion

		#region Listeners

		private readonly string _fileName;
		private readonly string _fullFilename;
		private int _numberOfLinesRead;
		private bool _lastLineHadNewline;
		private string _untrimmedLastLine;
		private long _lastPosition;
		private bool _loggedTimestampWarning;

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
		internal TextLogFile(IServiceContainer serviceContainer,
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
			_parserPlugin = serviceContainer.Retrieve<ITextLogFileParserPlugin>();
			_entries = new List<LogLine>();
			_properties = new LogFilePropertyList(LogFileProperties.Minimum);
			_properties.SetValue(LogFileProperties.Name, _fileName);
			_syncRoot = new object();


			var defaultEncoding = serviceContainer.TryRetrieve<ILogFileSettings>()?.DefaultEncoding;
			var overwrittenEncoding = serviceContainer.TryRetrieve<Encoding>();
			var encoding = overwrittenEncoding ?? defaultEncoding;
			_encoding = encoding ?? Encoding.UTF8;
			_properties.SetValue(LogFileProperties.Encoding, encoding);

			Log.DebugFormat("Log File '{0}' is interpreted using {1}", _fileName, _encoding.EncodingName);

			StartTask();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _fileName;
		}

		/// <summary>
		/// 
		/// </summary>
		public IEnumerable<LogLine> Entries => _entries;

		/// <inheritdoc />
		public override int Count => _entries.Count;

		/// <inheritdoc />
		public override int OriginalCount => Count;

		/// <inheritdoc />
		public override int MaxCharactersPerLine => _maxCharactersPerLine;

		/// <inheritdoc />
		public override IReadOnlyList<ILogFileColumnDescriptor> Columns => LogFileColumns.Minimum;

		/// <inheritdoc />
		public override IReadOnlyList<ILogFilePropertyDescriptor> Properties => _properties.Properties;

		/// <inheritdoc />
		public override object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			object value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public override T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			T value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public override void GetValues(ILogFileProperties properties)
		{
			_properties.GetValues(properties);
		}

		/// <inheritdoc />
		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions)
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
				if (Equals(column, LogFileColumns.Index) ||
				    Equals(column, LogFileColumns.OriginalIndex))
				{
					GetIndex(sourceIndices, (LogLineIndex[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.LogEntryIndex))
				{
					GetIndex(sourceIndices, (LogEntryIndex[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.LineNumber) ||
				         Equals(column, LogFileColumns.OriginalLineNumber))
				{
					GetLineNumber(sourceIndices, (int[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.LogLevel))
				{
					GetLogLevel(sourceIndices, (LevelFlags[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.Timestamp))
				{
					GetTimestamp(sourceIndices, (DateTime?[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.DeltaTime))
				{
					GetDeltaTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.ElapsedTime))
				{
					GetElapsedTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.OriginalDataSourceName))
				{
					GetDataSourceName(sourceIndices, (string[]) (object) destination, destinationIndex);
				}
				else if (Equals(column, LogFileColumns.RawContent))
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
		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			foreach (var column in destination.Columns)
			{
				destination.CopyFrom(column, destinationIndex, this, sourceIndices, queryOptions);
			}
		}

		/// <inheritdoc />
		public override double Progress
		{
			get
			{
				var fileSize = _properties.GetValue(LogFileProperties.Size);
				var position = _lastPosition;
				if (fileSize == null)
					return 1; //< We've fully read the non-existant file...

				var progress = (double) fileSize.Value.Bytes / position;
				// Since we've performed two reads, it's possible that they have inconsistent values
				// and therefore we should perform a sanity check on the resulting progress value
				// so it stays within the expected boundaries. (It's just not worth a lock)
				return MathEx.Saturate(progress);
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
					_properties.SetValue(LogFileProperties.LastModified, info.LastWriteTime);
					_properties.SetValue(LogFileProperties.Created, info.CreationTime);
					_properties.SetValue(LogFileProperties.Size, Size.FromBytes(info.Length));

					using (var stream = new FileStream(_fileName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite))
					{
						var format = _properties.GetValue(LogFileProperties.Format);
						var certainty = _properties.GetValue(LogFileProperties.FormatDetectionCertainty);
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

							_properties.SetValue(LogFileProperties.Format, format);
							_properties.SetValue(LogFileProperties.FormatDetectionCertainty, certainty);
						}

						var encoding = format?.Encoding ?? _encoding;
						_properties.SetValue(LogFileProperties.Encoding, encoding);
						using (var reader = new StreamReaderEx(stream, encoding))
						{
							// We change the error flag explicitly AFTER opening
							// the stream because that operation might throw if we're
							// not allowed to access the file (in which case a different
							// error must be set).

							_properties.SetValue(LogFileProperties.EmptyReason, ErrorFlags.None);
							if (stream.Length >= _lastPosition)
							{
								stream.Position = _lastPosition;
							}
							else
							{
								OnReset(stream, out _numberOfLinesRead, out _lastPosition);
							}

							string currentLine;
							while ((currentLine = reader.ReadLine()) != null)
							{
								token.ThrowIfCancellationRequested();

								ResetEndOfSourceReached();

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

								IReadOnlyLogEntry logEntry = new RawLogEntry(_entries.Count, trimmedLine, _fullFilename);
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
							}

							_lastPosition = stream.Position;
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
						buffer[destinationIndex + i] = LogFileColumns.Index.DefaultValue;
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
						buffer[destinationIndex + i] = LogFileColumns.LogEntryIndex.DefaultValue;
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
						: LogFileColumns.RawContent.DefaultValue;
				}
			}
		}

		private void GetElapsedTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				var startTimestamp = _properties.GetValue(LogFileProperties.StartTimestamp);

				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					var line = GetLogLine(index);
					buffer[destinationIndex + i] = line != null
						? line.Value.Timestamp - startTimestamp
						: LogFileColumns.ElapsedTime.DefaultValue;
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
						: LogFileColumns.LogLevel.DefaultValue;
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
						buffer[destinationIndex + i] = LogFileColumns.LineNumber.DefaultValue;
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
			OnReset(null, out _numberOfLinesRead, out _lastPosition);
			_properties.SetValue(LogFileProperties.Created, null);
			_properties.SetValue(LogFileProperties.Size, null);
			_properties.SetValue(LogFileProperties.Format, null);
			_properties.SetValue(LogFileProperties.FormatDetectionCertainty, Certainty.None);
			_properties.SetValue(LogFileProperties.Encoding, null);
			SetError(ErrorFlags.SourceDoesNotExist);
		}

		private void SetError(ErrorFlags error)
		{
			_properties.SetValue(LogFileProperties.EmptyReason, error);
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
			_properties.SetValue(LogFileProperties.StartTimestamp, null);
			_properties.SetValue(LogFileProperties.EndTimestamp, null);
			_properties.SetValue(LogFileProperties.Duration, null);
			_maxCharactersPerLine = 0;

			_entries.Clear();
			Listeners.Reset();
		}

		private void Add(string line, LevelFlags level, int numberOfLinesRead, DateTime? timestamp)
		{
			if (_properties.GetValue(LogFileProperties.StartTimestamp) == null)
				_properties.SetValue(LogFileProperties.StartTimestamp, timestamp);
			if (timestamp != null)
				_properties.SetValue(LogFileProperties.EndTimestamp, timestamp);

			var duration = timestamp - _properties.GetValue(LogFileProperties.StartTimestamp);
			_properties.SetValue(LogFileProperties.Duration, duration);


			lock (_syncRoot)
			{
				int lineIndex = _entries.Count;
				var logLine = new LogLine(lineIndex, lineIndex, line, level, timestamp);
				var translated = Translate(logLine);
				_entries.Add(translated);
				_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, translated.Message?.Length ?? 0);

				if (timestamp != null)
				{
					UpdateLastModifiedIfNecessary(timestamp.Value);
				}
			}

			Listeners.OnRead(numberOfLinesRead);
		}

		private void UpdateLastModifiedIfNecessary(DateTime timestamp)
		{
			var lastModified = _properties.GetValue(LogFileProperties.LastModified);
			var difference = timestamp - lastModified;
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
						lastModified,
						timestamp
					);
					_loggedTimestampWarning = true;
				}

				_properties.SetValue(LogFileProperties.LastModified, timestamp);
			}
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

		/// <summary>
		///     Simple <see cref="IReadOnlyLogEntry" /> implementation for unparsed log lines.
		/// </summary>
		sealed class RawLogEntry
			: IReadOnlyLogEntry
		{
			private static readonly IReadOnlyList<ILogFileColumnDescriptor> AllColumns = new ILogFileColumnDescriptor[]
			{
				LogFileColumns.RawContent,
				LogFileColumns.Index,
				LogFileColumns.OriginalIndex,
				LogFileColumns.LineNumber,
				LogFileColumns.OriginalLineNumber,
				LogFileColumns.LogEntryIndex
			};

			private readonly LogLineIndex _index;
			private readonly string _rawContent;
			private readonly string _originalDataSourceName;

			public RawLogEntry(LogLineIndex index,
			                   string rawContent,
			                   string originalDataSourceName)
			{
				_index = index;
				_rawContent = rawContent;
				_originalDataSourceName = originalDataSourceName;
			}

			#region Implementation of IReadOnlyLogEntry

			public string RawContent
			{
				get { return _rawContent; }
			}

			public LogLineIndex Index
			{
				get { return _index; }
			}

			public LogLineIndex OriginalIndex
			{
				get { return _index; }
			}

			public LogEntryIndex LogEntryIndex
			{
				get { return (int)_index; }
			}

			public int LineNumber
			{
				get { return _index.Value + 1; }
			}

			public int OriginalLineNumber
			{
				get { return LineNumber; }
			}

			public string OriginalDataSourceName
			{
				get { return _originalDataSourceName; }
			}

			public LogLineSourceId SourceId
			{
				get { throw new NoSuchColumnException(LogFileColumns.SourceId); }
			}

			public LevelFlags LogLevel
			{
				get { return LevelFlags.None; }
			}

			public DateTime? Timestamp
			{
				get { return null; }
			}

			public TimeSpan? ElapsedTime
			{
				get { return null; }
			}

			public TimeSpan? DeltaTime
			{
				get { return null; }
			}

			public T GetValue<T>(ILogFileColumnDescriptor<T> column)
			{
				if (!TryGetValue(column, out var value))
					throw new NoSuchColumnException(column);

				return value;
			}

			public bool TryGetValue<T>(ILogFileColumnDescriptor<T> column, out T value)
			{
				if (TryGetValue(column, out object tmp))
				{
					value = (T) tmp;
					return true;
				}

				value = default;
				return false;
			}

			public object GetValue(ILogFileColumnDescriptor column)
			{
				if (!TryGetValue(column, out var value))
					throw new NoSuchColumnException(column);

				return value;
			}

			public bool TryGetValue(ILogFileColumnDescriptor column, out object value)
			{
				if (Equals(column, LogFileColumns.RawContent))
				{
					value = RawContent;
					return true;
				}
				if (Equals(column, LogFileColumns.Index) ||
				    Equals(column, LogFileColumns.OriginalIndex))
				{
					value = Index;
					return true;
				}
				if (Equals(column, LogFileColumns.LineNumber) ||
				    Equals(column, LogFileColumns.OriginalLineNumber))
				{
					value = LineNumber;
					return true;
				}
				if (Equals(column, LogFileColumns.LogEntryIndex))
				{
					value = LogEntryIndex;
					return true;
				}
				if (Equals(column, LogFileColumns.OriginalDataSourceName))
				{
					value = OriginalDataSourceName;
					return true;
				}

				value = default;
				return false;
			}

			public IReadOnlyList<ILogFileColumnDescriptor> Columns
			{
				get { return AllColumns; }
			}

			#endregion
		}
	}
}