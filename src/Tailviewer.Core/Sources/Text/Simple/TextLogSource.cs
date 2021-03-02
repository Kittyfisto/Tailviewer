using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Reads the contents of an entire file into memory, line by line and exposes it via
	///     the <see cref="ILogSource"/> interface.
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

		#region Data

		private readonly Encoding _encoding;
		private readonly LogBufferList _entries;
		private readonly object _syncRoot;

		#endregion

		#region Listeners

		private readonly string _fileName;
		private int _numberOfLinesRead;
		private bool _lastLineHadNewline;
		private string _untrimmedLastLine;
		private long _lastStreamPosition;
		private bool _loggedTimestampWarning;

		#endregion

		#region Properties
		
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

		private readonly IColumnDescriptor[] _columns;

		#endregion

		/// <summary>
		///    Initializes this text log file.
		/// </summary>
		/// <param name="taskScheduler"></param>
		/// <param name="fileName"></param>
		/// <param name="format"></param>
		/// <param name="encoding"></param>
		internal TextLogSource(ITaskScheduler taskScheduler,
		                       string fileName,
		                       ILogFileFormat format,
		                       Encoding encoding)
			: base(taskScheduler)
		{
			_fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			_encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

			_entries = new LogBufferList(GeneralColumns.RawContent);
			_columns = new IColumnDescriptor[]
			{
				GeneralColumns.Index,
				GeneralColumns.OriginalIndex,
				GeneralColumns.LogEntryIndex,
				GeneralColumns.LineNumber,
				GeneralColumns.OriginalLineNumber,
				GeneralColumns.OriginalDataSourceName,
				GeneralColumns.RawContent,
				PageBufferedLogSource.RetrievalState
			};

			_localProperties = new PropertiesBufferList(GeneralProperties.Minimum);
			_localProperties.SetValue(GeneralProperties.Name, _fileName);
			_localProperties.Add(TextProperties.LineCount);
			_localProperties.SetValue(GeneralProperties.Format, format);
			_localProperties.SetValue(TextProperties.LineCount, 0);
			_localProperties.SetValue(TextProperties.RequiresBuffer, false);

			_properties = new ConcurrentPropertiesList(GeneralProperties.Minimum);
			SynchronizePropertiesWithUser();
			_syncRoot = new object();
			_properties.SetValue(TextProperties.AutoDetectedEncoding, encoding);

			Log.DebugFormat("Log File '{0}' is interpreted using {1}", _fileName, _encoding.EncodingName);

			StartTask();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _fileName;
		}

		/// <inheritdoc />
		public override IReadOnlyList<IColumnDescriptor> Columns => _columns;

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
		public override void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
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
				if (Equals(column, GeneralColumns.Index) ||
				    Equals(column, GeneralColumns.OriginalIndex))
				{
					GetIndex(sourceIndices, (LogLineIndex[])(object)destination, destinationIndex);
				}
				else if (Equals(column, GeneralColumns.LogEntryIndex))
				{
					GetIndex(sourceIndices, (LogEntryIndex[])(object)destination, destinationIndex);
				}
				else if (Equals(column, GeneralColumns.LineNumber) ||
				         Equals(column, GeneralColumns.OriginalLineNumber))
				{
					GetLineNumber(sourceIndices, (int[])(object)destination, destinationIndex);
				}
				else if (Equals(column, GeneralColumns.OriginalDataSourceName))
				{
					GetDataSourceName(sourceIndices, (string[]) (object) destination, destinationIndex);
				}
				else if (Equals(column, GeneralColumns.RawContent))
				{
					GetRawContent(sourceIndices, (string[])(object)destination, destinationIndex);
				}
				else if (Equals(column, PageBufferedLogSource.RetrievalState))
				{
					GetRetrievalState(sourceIndices, (RetrievalState[])(object)destination, destinationIndex);
				}
				else
				{
					throw new NoSuchColumnException(column);
				}
			}
		}

		/// <inheritdoc />
		public override void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
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
					_localProperties.SetValue(GeneralProperties.LastModified, info.LastWriteTimeUtc);
					_localProperties.SetValue(GeneralProperties.Created, info.CreationTimeUtc);
					_localProperties.SetValue(GeneralProperties.Size, Size.FromBytes(fileSize));
					UpdatePercentageProcessed(_lastStreamPosition, fileSize, allow100Percent: true);
					SynchronizePropertiesWithUser();

					using (var stream = new FileStream(_fileName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite))
					{
						using (var reader = new StreamReaderEx(stream, _encoding))
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

								Add(trimmedLine,
								    _numberOfLinesRead);

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
						buffer[destinationIndex + i] = GeneralColumns.Index.DefaultValue;
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
						buffer[destinationIndex + i] = GeneralColumns.LogEntryIndex.DefaultValue;
					}
				}
			}
		}

		private void GetRawContent(IReadOnlyList<LogLineIndex> indices, string[] destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				_entries.CopyTo(GeneralColumns.RawContent, indices, destination, destinationIndex);
			}
		}

		private void GetRetrievalState(IReadOnlyList<LogLineIndex> indices, RetrievalState[] destination, int destinationIndex)
		{
			lock (_syncRoot)
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					var index = indices[i];
					destination[destinationIndex + i] = index < _entries.Count
						? RetrievalState.Retrieved
						: RetrievalState.NotInSource;
				}
			}
		}

		private void GetDataSourceName(IReadOnlyList<LogLineIndex> sourceIndices, string[] buffer, int destinationIndex)
		{
			buffer.Fill(_fileName, destinationIndex, sourceIndices.Count);
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
						buffer[destinationIndex + i] = GeneralColumns.LineNumber.DefaultValue;
					}
				}
			}
		}

		private void SetDoesNotExist()
		{
			_localProperties.SetValue(GeneralProperties.Created, null);
			_localProperties.SetValue(GeneralProperties.Size, null);
			_localProperties.SetValue(TextProperties.AutoDetectedEncoding, null);
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
			_entries.Clear();
			SynchronizePropertiesWithUser();

			Listeners.Reset();
		}

		private void Add(string line, int numberOfLinesRead)
		{
			lock (_syncRoot)
			{
				_entries.Add(new LogEntry
				{
					RawContent = line
				});
			}

			Listeners.OnRead(numberOfLinesRead);
		}

		/// <summary>
		///  TODO: Where to put this?
		/// </summary>
		/// <param name="timestamp"></param>
		/// <param name="lastModified"></param>
		/// <returns></returns>
		private bool TryGetLastBetterModified(DateTime timestamp, out DateTime lastModified)
		{
			var currentLastModified = DateTime.UtcNow;
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
			_localProperties.SetValue(TextProperties.LineCount, _entries.Count);
			_localProperties.SetValue(GeneralProperties.LogEntryCount, _entries.Count);
			_localProperties.SetValue(GeneralProperties.LogEntryCount, _entries.Count);

			// We want to update the user-facing properties in a single synchronized OP
			// so that the properties as retrieved by the user are in sync
			_properties.CopyFrom(_localProperties);
		}

		private void RemoveLast()
		{
			var index = _entries.Count - 1;
			lock (_syncRoot)
			{
				_entries.RemoveAt(index);
			}
			Listeners.Remove(index, 1);
		}
	}
}