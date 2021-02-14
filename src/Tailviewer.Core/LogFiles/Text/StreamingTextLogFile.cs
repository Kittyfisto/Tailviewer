using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.IO;
using Tailviewer.Core.Settings;

namespace Tailviewer.Core.LogFiles.Text
{
	/// <summary>
	///     A n<see cref="ILogFile" /> implementation which allows (somewhat) constant time random-access to the lines of a log file without keeping the entire file in memory.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogFileView))]
	internal sealed class StreamingTextLogFile
		: ILogFile
		, ITextFileListener
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IServiceContainer _serviceContainer;
		private readonly LogFileListenerCollection _listeners;

		#region Reading

		private readonly ITextFileReader _reader;

		#endregion

		#region Data

		private int _lineCount;
		private readonly LogEntryCache _cache;
		private readonly LogEntryList _index;
		private readonly object _syncRoot;
		private readonly ILogFileProperties _properties;
		private int _maxCharactersPerLine;

		#endregion

		#region Parsing

		private readonly ITextLogFileParserPlugin _parserPlugin;
		private ITextLogFileParser _parser;
		private ILogFileFormat _parserFormat;

		#endregion

		#region Listeners

		private readonly string _fileName;
		private readonly string _fullFilename;
		private bool _loggedTimestampWarning;
		private bool _endOfSourceReached;

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
		internal StreamingTextLogFile(IServiceContainer serviceContainer,
		                           string fileName)
		{
			_serviceContainer = serviceContainer;

			_listeners = new LogFileListenerCollection(this);

			var scheduler = serviceContainer.Retrieve<IIoScheduler>();
			var defaultEncoding = serviceContainer.TryRetrieve<ILogFileSettings>()?.DefaultEncoding;
			var overwrittenEncoding = serviceContainer.TryRetrieve<Encoding>();
			var encoding = overwrittenEncoding ?? defaultEncoding ?? Encoding.UTF8;
			var formatMatcher = serviceContainer.Retrieve<ILogFileFormatMatcher>();

			_fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			_fullFilename = fileName;
			if (!Path.IsPathRooted(_fullFilename))
				_fullFilename = Path.Combine(Directory.GetCurrentDirectory(), fileName);

			_parserPlugin = serviceContainer.Retrieve<ITextLogFileParserPlugin>();
			_cache = new LogEntryCache(LogFileColumns.LogLevel, LogFileColumns.RawContent);
			_index = new LogEntryList(LogFileColumns.Timestamp);
			_properties = new LogFilePropertyList(LogFileProperties.Minimum);
			_properties.SetValue(LogFileProperties.Name, _fileName);
			_syncRoot = new object();

			_reader = scheduler.OpenReadText(_fullFilename, this, encoding, formatMatcher);
			_reader.Start();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _fileName;
		}

		public void Dispose()
		{
			// https://github.com/Kittyfisto/Tailviewer/issues/282
			_listeners.Clear();
		}

		public bool EndOfSourceReached => _endOfSourceReached;

		/// <inheritdoc />
		public int Count => _lineCount;

		/// <inheritdoc />
		public int OriginalCount => Count;

		/// <inheritdoc />
		public int MaxCharactersPerLine => _maxCharactersPerLine;

		/// <inheritdoc />
		public IReadOnlyList<ILogFileColumnDescriptor> Columns => LogFileColumns.Minimum;

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		/// <inheritdoc />
		public IReadOnlyList<ILogFilePropertyDescriptor> Properties => _properties.Properties;

		/// <inheritdoc />
		public object GetProperty(ILogFilePropertyDescriptor propertyDescriptor)
		{
			object value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public T GetProperty<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			T value;
			_properties.TryGetValue(propertyDescriptor, out value);
			return value;
		}

		/// <inheritdoc />
		public void GetAllProperties(ILogFileProperties destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		/// <inheritdoc />
		public void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumnDescriptor<T> column, T[] buffer, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + indices.Count > buffer.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");


			if (Equals(column, LogFileColumns.Index) ||
			    Equals(column, LogFileColumns.OriginalIndex))
			{
				GetIndex(indices, (LogLineIndex[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LogEntryIndex))
			{
				GetIndex(indices, (LogEntryIndex[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.LineNumber) ||
			         Equals(column, LogFileColumns.OriginalLineNumber))
			{
				GetLineNumber(indices, (int[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.OriginalDataSourceName))
			{
				GetDataSourceName(indices, (string[])(object)buffer, destinationIndex);
			}
			else if (Equals(column, LogFileColumns.Timestamp))
			{
				GetTimestamp(indices, (DateTime?[]) (object) buffer, destinationIndex);
			}
			else
			{
				lock (_syncRoot)
				{
					if (_cache.TryCopyTo(indices, column, buffer, destinationIndex))
						return;
				}

				ReadEntries(indices);

				lock (_syncRoot)
				{
					if (!_cache.TryCopyTo(indices, column, buffer, destinationIndex))
					{
						Log.WarnFormat("Unable to satisfy read request after buffer was filled");
					}
				}
			}
		}

		/// <inheritdoc />
		public void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			lock (_syncRoot)
			{
				if (_cache.TryCopyTo(indices, buffer, destinationIndex))
					return;
			}

			ReadEntries(indices);

			lock (_syncRoot)
			{
				if (!_cache.TryCopyTo(indices, buffer, destinationIndex))
				{
					Log.WarnFormat("Unable to satisfy read request after buffer was filled");
				}
			}
		}

		/// <inheritdoc />
		public double Progress
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return originalLineIndex;
		}

		#region Random Access to computed values

		private void GetIndex(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _lineCount)
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
					if (index >= 0 && index < _lineCount)
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

		private void GetDataSourceName(IReadOnlyList<LogLineIndex> indices, string[] buffer, int destinationIndex)
		{
			for (int i = 0; i < indices.Count; ++i)
			{
				buffer[destinationIndex + i] = _fileName;
			}
		}

		private void GetLineNumber(IReadOnlyList<LogLineIndex> indices, int[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					if (index >= 0 && index < _lineCount)
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

		#endregion

		private void GetTimestamp(IReadOnlyList<LogLineIndex> indices, DateTime?[] buffer, int destinationIndex)
		{
			lock (_syncRoot)
			{
				_index.CopyTo(LogFileColumns.Timestamp, indices, buffer, destinationIndex);
			}
		}

		private void ReadEntries(IReadOnlyList<LogLineIndex> indices)
		{
			// TODO: How can we avoid those two buffer allocations which will happen over and over?
			var lines = _reader.Read(indices);

			lock (_syncRoot)
			{
				foreach (var line in lines)
				{
					IReadOnlyLogEntry logEntry = new RawTextLogEntry(_lineCount, line, _fullFilename);
					// Parsers expect the file to be primarily fed sequentially so they can do their magic.
					if (_parser != null)
					{
						logEntry = _parser?.Parse(logEntry);
						_cache.Add(logEntry);
					}
				}
			}
		}

		#region Sequential file scan

		public void OnReset(ILogFileProperties properties)
		{
			UpdateProperties(properties);
			Reset();
		}

		public void OnEndOfSourceReached(ILogFileProperties properties)
		{
			UpdateProperties(properties);

			// Now this line is very important:
			// Most tests expect that listeners have been notified
			// of all pending changes when the source enters the
			// "EndOfSourceReached" state. This would be true, if not
			// for listeners specifying a timespan that should ellapse between
			// calls to OnLogFileModified. The listener collection has
			// been notified, but the individual listeners may not be, because
			// neither the maximum line count, nor the maximum timespan has ellapsed.
			// Therefore we flush the collection to ensure that ALL listeners have been notified
			// of ALL changes (even if they didn't want them yet) before we enter the
			// EndOfSourceReached state.
			_listeners.Flush();
			_endOfSourceReached = true;
		}

		public void OnRead(ILogFileProperties properties, LogFileSection readSection, IReadOnlyList<string> lines)
		{
			UpdateProperties(properties);

			var intersection = new LogFileSection(0, _lineCount).Intersect(readSection);
			if (intersection != null)
			{
				Remove(intersection.Value);
			}

			foreach (var line in lines)
			{
				IReadOnlyLogEntry logEntry = new RawTextLogEntry(_lineCount, line, _fullFilename);
				// Parsers expect the file to be primarily fed sequentially so they can do their magic.
				if (_parser != null)
				{
					logEntry = _parser?.Parse(logEntry);
				}

				Add(logEntry);
			}

			_listeners.OnRead(_lineCount);
		}

		private void UpdateProperties(ILogFileProperties properties)
		{
			var previousFormat = _properties.GetValue(LogFileProperties.Format);
			_properties.CopyFrom(properties);
			var format = _properties.GetValue(LogFileProperties.Format);

			if (!Equals(format, previousFormat))
			{
				// We only need to create a new parser when we don't have one already or we've changed out minds
				if (_parser == null || !Equals(format, _parserFormat))
				{
					_parser = _parserPlugin.CreateParser(_serviceContainer, format);
					_parserFormat = format;
					// TODO: IF we have already interpreted parts of the file, then we should restart from the beginning because now we might get vastly different results.
				}
			}
		}

		private void Add(IReadOnlyLogEntry logEntry)
		{
			var timestamp = logEntry.Timestamp;
			if (_properties.GetValue(LogFileProperties.StartTimestamp) == null)
				_properties.SetValue(LogFileProperties.StartTimestamp, timestamp);
			if (timestamp != null)
				_properties.SetValue(LogFileProperties.EndTimestamp, timestamp);

			var duration = timestamp - _properties.GetValue(LogFileProperties.StartTimestamp);
			_properties.SetValue(LogFileProperties.Duration, duration);

			if (timestamp != null)
			{
				UpdateLastModifiedIfNecessary(timestamp.Value);
			}

			lock (_syncRoot)
			{
				_cache.Add(logEntry);
				_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, logEntry.RawContent?.Length ?? 0);
				++_lineCount;
			}
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

		private void Remove(LogFileSection intersection)
		{
			lock (_syncRoot)
			{
				_cache.RemoveRange((int) intersection.Index, intersection.Count);
				_lineCount -= intersection.Count;
			}

			_listeners.Invalidate((int) intersection.Index, intersection.Count);
		}

		private void Reset()
		{
			lock (_syncRoot)
			{
				_maxCharactersPerLine = 0;
				_cache.Clear();
				_lineCount = 0;
			}

			_listeners.Reset();
		}

		#endregion

	}
}