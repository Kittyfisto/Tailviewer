using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Metrolib;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Properties;
using Exception = System.Exception;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///     Represents a text file as a collection of log entries where every line is a single log entry.
	///     The contents of the line are stored in the <see cref="LogColumns.RawContent"/> column.
	/// </summary>
	/// <remarks>
	///     A allows (somewhat) constant time random-access to the lines of a log file without keeping the entire file in memory.
	///     The only thing kept in main memory is an index of line offsets.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class StreamingTextLogSource
		: ILogSource
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;
		private readonly Encoding _encoding;
		private readonly LogSourceListenerCollection _listeners;
		private readonly LogBufferList _index;
		private readonly PropertiesBufferList _propertiesBuffer;
		private readonly ConcurrentPropertiesList _properties;
		private readonly string _fileName;
		private readonly IPeriodicTask _fileScanTask;
		private readonly IPeriodicTask _fileReadTask;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ConcurrentQueue<IReadRequest> _pendingReadRequests;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;
		private FileFingerprint _lastFingerprint;
		private long _lastStreamPosition;

		/// <summary>
		///    Initializes this text log file.
		/// </summary>
		/// <remarks>
		///    Plugin authors are deliberately prevented from calling this constructor directly because it's signature may change
		///    over time. In order to create an instance of this type, simply call <see cref="IServiceContainer.CreateTextLogFile"/>.
		/// </remarks>
		/// <param name="taskScheduler"></param>
		/// <param name="fileName"></param>
		/// <param name="encoding"></param>
		internal StreamingTextLogSource(ITaskScheduler taskScheduler,
		                                string fileName,
		                                Encoding encoding)
		{
			_taskScheduler = taskScheduler;
			_encoding = encoding;

			_listeners = new LogSourceListenerCollection(this);

			_fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			_index = new LogBufferList(LogColumns.LineOffsetInBytes);
			_propertiesBuffer = new PropertiesBufferList();
			_propertiesBuffer.SetValue(GeneralProperties.Name, _fileName);

			_properties = new ConcurrentPropertiesList(GeneralProperties.Minimum);
			SynchronizeProperties();
			_cancellationTokenSource = new CancellationTokenSource();

			_columns = new IColumnDescriptor[] {LogColumns.Index, LogColumns.LineOffsetInBytes, LogColumns.RawContent};

			_pendingReadRequests = new ConcurrentQueue<IReadRequest>();

			_fileScanTask = _taskScheduler.StartPeriodic(() => RunFileScan(_cancellationTokenSource.Token));
			_fileReadTask = _taskScheduler.StartPeriodic(() => RunFileRead(_cancellationTokenSource.Token));
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

			_cancellationTokenSource.Cancel();
			_taskScheduler.StopPeriodic(_fileScanTask);
			_taskScheduler.StopPeriodic(_fileReadTask);

			lock (_index)
			{
				_index.Clear();
			}
		}

		public IReadOnlyList<IColumnDescriptor> Columns => _columns;

		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogSourceListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _properties.Properties;

		public bool HasPendingReadRequests => _pendingReadRequests.Count > 0;

		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			return _properties.GetValue(property);
		}

		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			return _properties.GetValue(property);
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_properties.SetValue(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_properties.SetValue(property, value);
		}

		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                         IColumnDescriptor<T> column,
		                         T[] destination,
		                         int destinationIndex,
		                         LogSourceQueryOptions queryOptions)
		{
			if (ReferenceEquals(column, LogColumns.RawContent))
			{
				var view = new SingleColumnLogBufferView<T>(column, destination, destinationIndex, sourceIndices.Count);
				ReadRawData(sourceIndices, view, 0, queryOptions);
			}
			else if (ReferenceEquals(column, LogColumns.Index))
			{
				GetIndices(sourceIndices, (LogLineIndex[])(object)destination, destinationIndex);
			}
			else if (ReferenceEquals(column, LogColumns.LineOffsetInBytes))
			{
				lock (_index)
				{
					_index.CopyTo(column, sourceIndices, destination, destinationIndex);
				}
			}
			else
			{
				throw new NoSuchColumnException(column);
			}
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                       ILogBuffer destination,
		                       int destinationIndex,
		                       LogSourceQueryOptions queryOptions)
		{
			foreach(var column in destination.Columns)
			{
				if (!_columns.Contains(column))
					throw new NoSuchColumnException(column);
			}

			ReadRawData(sourceIndices, destination, destinationIndex, queryOptions);
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			throw new NotImplementedException();
		}

		private void GetIndices(IReadOnlyList<LogLineIndex> sourceIndices, LogLineIndex[] destination, int destinationIndex)
		{
			int count;
			lock (_index)
			{
				count = _index.Count;
			}

			for (int i = 0; i < count; ++i)
			{
				destination[destinationIndex + i] = sourceIndices[i].Value;
			}
		}

		#region File Scan

		private TimeSpan RunFileScan(CancellationToken token)
		{
			bool performedWork = false;

			try
			{
				if (!File.Exists(_fileName))
				{
					SetError(ErrorFlags.SourceDoesNotExist);
				}
				else
				{
					performedWork = ReadEntireFile(token);
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

			SynchronizeProperties();

			if (performedWork)
				return TimeSpan.Zero;

			return TimeSpan.FromMilliseconds(100);
		}

		private bool ReadEntireFile(CancellationToken cancellationToken)
		{
			if (!HasFileChanged())
				return false;

			using (var stream = new FileStream(_fileName,
			                                   FileMode.Open,
			                                   FileAccess.Read,
			                                   FileShare.ReadWrite | FileShare.Delete))
			{
				AddFirstLineIfNecessary(stream);

				stream.Position = _lastStreamPosition;
				var detector = new LineOffsetDetector(stream, _encoding);
				long lineOffset;
				while ((lineOffset = detector.FindNextLineOffset()) >= 0)
				{
					if (cancellationToken.IsCancellationRequested)
						return false;

					AddLine(lineOffset);
				}
			}

			_propertiesBuffer.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			return true;
		}

		private void AddFirstLineIfNecessary(FileStream stream)
		{
			if (stream.Length <= 0)
				return;

			// As far as we're concerned, the first line starts right after the preamble, which
			// might be an offset of anything in between 0 and 4 bytes.
			var preamble = _encoding.GetPreamble();
			var firstLineOffset = 0;
			if (preamble.Length > 0)
			{
				var buffer = new byte[preamble.Length];
				var bytesRead = stream.Read(buffer, 0, buffer.Length);
				if (bytesRead >= buffer.Length && ArrayExtensions.Equals(preamble, buffer))
				{
					firstLineOffset = preamble.Length;
				}
			}

			lock (_index)
			{
				if (_index.Count > 0)
					return;

				AddLine(firstLineOffset);
			}
		}

		private void AddLine(long offsetInBytes)
		{
			int count;
			lock (_index)
			{
				_index.Add(CreateLogEntry(offsetInBytes));

				_lastStreamPosition = offsetInBytes;
				count = _index.Count;
			}
			_propertiesBuffer.SetValue(GeneralProperties.LogEntryCount, count);
			SynchronizeProperties();
			_listeners.OnRead(count);
		}

		[Pure]
		private static LogEntry CreateLogEntry(long lastLineOffset)
		{
			var logEntry = new LogEntry(LogColumns.LineOffsetInBytes);
			logEntry.SetValue(LogColumns.LineOffsetInBytes, lastLineOffset);
			return logEntry;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		///    This method may only ever be called from within <see cref="RunFileScan"/>.
		/// </remarks>
		/// <param name="stream"></param>
		private long RepositionStreamToLastLine(FileStream stream)
		{
			var currentSize = stream.Length;
			if (currentSize < _lastStreamPosition)
			{
				ResetIndex();

				return 0;
			}
			else
			{
				// Okay so we can never be sure if the existing line was appended to or if
				// a new line was written so in order to check that, we move the buffer back
				// to the *start* of the last known line and then check...
				lock (_index)
				{
					if (_index.Count > 0)
					{
						var lastLineOffset = _index[_index.Count - 1].GetValue(LogColumns.LineOffsetInBytes);
						stream.Position = lastLineOffset;
						return lastLineOffset;
					}

					return stream.Position;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		///    This method may only ever be called from within <see cref="RunFileScan"/>.
		/// </remarks>
		private void ResetIndex()
		{
			lock (_index)
			{
				_index.Clear();
			}

			_propertiesBuffer.SetValue(GeneralProperties.LogEntryCount, 0);
			SynchronizeProperties();
			_listeners.Reset();
		}

		private bool HasFileChanged()
		{
			var fingerprint = FileFingerprint.FromFile(_fileName);
			if (Equals(fingerprint, _lastFingerprint))
				return false;

			_lastFingerprint = fingerprint;
			_propertiesBuffer.SetValue(GeneralProperties.LastModified, fingerprint.LastModified);
			_propertiesBuffer.SetValue(GeneralProperties.Created, fingerprint.Created);
			_propertiesBuffer.SetValue(GeneralProperties.Size, Size.FromBytes(fingerprint.Size));
			return true;
		}

		private void SetError(ErrorFlags error)
		{
			_propertiesBuffer.SetValue(GeneralProperties.EmptyReason, error);
			_propertiesBuffer.SetValue(GeneralProperties.Created, null);
			_propertiesBuffer.SetValue(GeneralProperties.Size, null);
			_propertiesBuffer.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			_propertiesBuffer.SetValue(GeneralProperties.LastModified, null);
			_propertiesBuffer.SetValue(GeneralProperties.Created, null);
			_propertiesBuffer.SetValue(GeneralProperties.Size, null);
			_propertiesBuffer.SetValue(GeneralProperties.LogEntryCount, 0);

			ResetIndex();
		}

		private void SynchronizeProperties()
		{
			int count;
			lock (_index)
			{
				count = _index.Count;
			}

			_listeners.OnRead(count);
			_properties.CopyFrom(_propertiesBuffer);
		}

		#endregion

		#region File Read

		/// <summary>
		///    Responsible for reading data from the source file into the given <paramref name="destination"/>.
		/// </summary>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		/// <param name="queryOptions"></param>
		private void ReadRawData(IReadOnlyList<LogLineIndex> sourceIndices,
		                         ILogBuffer destination,
		                         int destinationIndex,
		                         LogSourceQueryOptions queryOptions)
		{
			using (var request = EnqueueReadRequest(sourceIndices, destination, destinationIndex))
			{
				int linesRead;
				if (!request.Wait(queryOptions.MaximumWaitTime, _cancellationTokenSource.Token))
				{
					request.Cancel();
					linesRead = 0;
				}
				else
				{
					linesRead = request.NumLinesRead;
				}

				if (linesRead < sourceIndices.Count)
				{
					destination.FillDefault(destinationIndex + linesRead, sourceIndices.Count - linesRead);
				}
			}
		}

		/// <summary>
		///    Creates a new read request for the given read operation.
		/// </summary>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		/// <returns></returns>
		private IReadRequest EnqueueReadRequest(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex)
		{
			IReadRequest request;
			if (sourceIndices is LogFileSection section)
			{
				request = new ContiguousReadRequest(section, destination, destinationIndex);
			}
			else
			{
				request = new FragmentedReadRequest(sourceIndices, destination, destinationIndex);
			}

			_pendingReadRequests.Enqueue(request);
			// TODO: If we could wake up the file read task on-demand, that would be awesome
			return request;
		}

		private TimeSpan RunFileRead(CancellationToken cancellationToken)
		{
			try
			{
				// We need to serve read requests even (or may be specially) when the file cannot be accessed
				// or else we're gonna get stuck. Therefore we try to open the stream, but if we cannot, 
				// still process all pending requests, with the caveat that we simply cancel them instead.
				using (var reader = TryOpenRead())
				{
					var requests = GetPendingReadRequests(reader);
					ServeReadRequests(reader, requests, cancellationToken);
					if (requests.Count > 0)
						return TimeSpan.Zero;
				}
			}
			catch (Exception e)
			{
				Log.Debug(e);
			}

			return TimeSpan.FromMilliseconds(1);
		}

		private StreamReader TryOpenRead()
		{
			try
			{
				if (!File.Exists(_fileName))
					return null;

				var stream = new FileStream(_fileName,
				                            FileMode.Open,
				                            FileAccess.Read,
				                            FileShare.ReadWrite | FileShare.Delete);
				var reader = new StreamReader(stream, _encoding, _encoding.GetPreamble().Length > 0);
				return reader;
			}
			catch (Exception e)
			{
				Log.Debug(e);
			}

			return null;
		}

		private void ServeReadRequests(StreamReader streamReader,
		                               List<IReadRequest> requests,
		                               CancellationToken cancellationToken)
		{
			try
			{
				for(int i = 0; i < requests.Count; ++i)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						CancelRequests(requests, i);
					}

					var request = requests[i];
					try
					{
						request.Serve(_index, streamReader);
					}
					catch (Exception e)
					{
						Log.WarnFormat("Caught exception while serving read requests: {0}", e);
					}
				}
			}
			catch (Exception e)
			{
				Log.FatalFormat("Caught exception while serving read requests: {0}", e);
			}
		}

		private void CancelRequests(List<IReadRequest> requests, int startIndex)
		{
			for (int i = startIndex; i < requests.Count; ++i)
			{
				requests[i].Cancel();
			}
		}

		/// <summary>
		///   Consumes all pending read requests and returns an equivalent, but optimized list of requests
		/// </summary>
		/// <param name="streamReader"/>
		/// <returns></returns>
		private List<IReadRequest> GetPendingReadRequests(StreamReader streamReader)
		{
			var requests = new List<IReadRequest>();
			try
			{
				while (_pendingReadRequests.TryDequeue(out var request))
				{
					try
					{
						if (streamReader != null)
						{
							requests.Add(request);
						}
						else
						{
							request.Complete(linesRead: 0);
						}
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Caught exception while optimizing read requests: {0}", e);
						requests.Add(request);
					}
				}

				// TODO: Optimize / batch requests into larger ones
				return requests;
			}
			catch (Exception e)
			{
				Log.FatalFormat("Caught exception while optimizing read requests: {0}", e);
				return requests;
			}
		}

		interface IReadRequest
			: IDisposable
		{
			bool Wait(TimeSpan maximumWaitTime, CancellationToken cancellationToken);
			void Serve(LogBufferList index, StreamReader reader);
			void Cancel();
			int NumLinesRead { get; }
			void Complete(int linesRead);
		}

		internal abstract class AbstractReadRequest
			: IReadRequest
		{
			private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			private readonly int _destinationIndex;
			private readonly TaskCompletionSource<int> _taskSource;
			private readonly DateTime _creationTime;
			private DateTime? _startTime;
			private ILogBuffer _destination;

			protected AbstractReadRequest(ILogBuffer destination, int destinationIndex)
			{
				_destination = destination;
				_destinationIndex = destinationIndex;
				_taskSource = new TaskCompletionSource<int>();
				_creationTime = DateTime.Now;
			}

			#region Implementation of IReadRequest

			public bool Wait(TimeSpan maximumWaitTime, CancellationToken cancellationToken)
			{
				var waitTime = maximumWaitTime.TotalMilliseconds;
				var timeoutInMs = waitTime > int.MaxValue
					? int.MaxValue
					: (int) waitTime;

				return _taskSource.Task.Wait(timeoutInMs, cancellationToken);
			}

			public void Serve(LogBufferList index, StreamReader reader)
			{
				try
				{
					_startTime = DateTime.Now;
					var linesRead = ReadLines(index, reader);
					Complete(linesRead);
				}
				catch (Exception e)
				{
					if (!_taskSource.TrySetException(e))
						Log.Warn("Unable to fail pending task, here's the exception intended for the task: {0}", e);
				}
			}

			public void Cancel()
			{
				var elapsed = DateTime.Now - _creationTime;
				Log.DebugFormat("Cancelling '{0}' after {1:F1}ms...", this, elapsed.TotalMilliseconds);

				// This lock is important because we must make sure that 
				lock (this)
				{
					_destination = null;
				}

				if (!_taskSource.TrySetCanceled())
					Log.Warn("Unable to cancel pending task (why though?!)");
			}

			public void Complete(int linesRead)
			{
				var elapsed = DateTime.Now - _creationTime;
				Log.DebugFormat("Completing '{0}' after {1:F1}ms: {2} lines read", this, elapsed.TotalMilliseconds, linesRead);

				if (!_taskSource.TrySetResult(linesRead))
					Log.WarnFormat("Unable to set result for pending task (why though?!)");
			}

			public int NumLinesRead
			{
				get
				{
					return _taskSource.Task.Result;
				}
			}

			#endregion

			protected void CopyDataToDestination(LogLineIndex[] sourceIndices, string[] lines, int linesRead)
			{
				// We allow users to quickly cancel read requests, for example because their
				// timeout was reached. When that's the case, then it's possible that we're already
				// trying to fulfill their request. If we were to still copy the data to the destination
				// buffer AFTER cancellation then we could get all kinds of weird race conditions and
				// most of all, break the user's expectation that the buffer must not be modified,
				// when the GetColumn / GetEntries call has returned!
				//
				// Therefore we sync the copy to the destination with the cancellation itself, so that
				// we can make sure that the above doesn't happen.
				lock (this)
				{
					if (_destination == null) 
					{
						Log.Debug("Read request cancelled in the mean time, skipping copy to destination buffer");
						return;
					}

					_destination.CopyFrom(LogColumns.RawContent, _destinationIndex, lines, 0, linesRead);
					if (_destination.Contains(LogColumns.Index))
						_destination.CopyFrom(LogColumns.Index, _destinationIndex, sourceIndices, 0, linesRead);
				}
			}

			protected abstract int ReadLines(LogBufferList index, StreamReader reader);

			#region IDisposable

			public void Dispose()
			{
				var task = _taskSource.Task;
				if (!task.IsFaulted && !task.IsCanceled && !task.IsCompleted)
				{
					Cancel();
				}
			}

			#endregion
		}

		sealed class ContiguousReadRequest
			: AbstractReadRequest
		{
			private readonly LogFileSection _sourceSection;

			public ContiguousReadRequest(LogFileSection sourceSection,
			                             ILogBuffer destination,
			                             int destinationIndex)
				: base(destination, destinationIndex)
			{
				_sourceSection = sourceSection;
			}

			#region Overrides of Object

			public override string ToString()
			{
				return $"Read {_sourceSection.Count} contiguous lines";
			}

			#endregion

			protected override int ReadLines(LogBufferList index, StreamReader reader)
			{
				var lines = new string[_sourceSection.Count];

				var firstLineOffset = GetFirstLineOffset(index);
				if (firstLineOffset < 0) //< We don't know the offset for that index
					return 0;

				var linesRead = ReadData(reader, firstLineOffset, lines);
				// TODO: Should we try to avoid this unnecessary copy?
				CopyDataToDestination(((IReadOnlyList<LogLineIndex>)_sourceSection).ToArray(), lines, linesRead);

				return linesRead;
			}

			private long GetFirstLineOffset(LogBufferList index)
			{
				// We need to find out where to reposition the file stream.
				// This is super easy for contiguous read-requests: All we have to do is
				// to find the offset of the first requested line and then read from there...
				var indices = new long[1];
				lock (index)
				{
					index.CopyTo(LogColumns.LineOffsetInBytes, new LogFileSection(_sourceSection.Index, 1), indices, 0);
				}

				return indices[0];
			}

			private int ReadData(StreamReader reader, long firstLineOffset, string[] destination)
			{
				reader.BaseStream.Position = firstLineOffset;
				int i;
				for (i = 0; i < destination.Length; ++i)
				{
					var line = reader.ReadLine();
					if (line == null)
					{
						break;
					}

					destination[i] = line;
				}

				return i;
			}
		}

		sealed class FragmentedReadRequest
			: AbstractReadRequest
		{
			private readonly LogLineIndex[] _sourceIndices;

			public FragmentedReadRequest(IReadOnlyList<LogLineIndex> sourceIndices,
			                                ILogBuffer destination,
			                                int destinationIndex)
				: base(destination, destinationIndex)
			{
				_sourceIndices = sourceIndices as LogLineIndex[] ?? sourceIndices.ToArray();
			}

			#region Overrides of Object

			public override string ToString()
			{
				return $"Read #{_sourceIndices.Length} fragmented lines";
			}

			#endregion

			protected override int ReadLines(LogBufferList index, StreamReader reader)
			{
				var lines = new string[_sourceIndices.Length];

				var lineOffsets= GetLineOffsets(index);
				var linesRead = ReadData(reader, lineOffsets, lines);
				CopyDataToDestination(_sourceIndices, lines, linesRead);
				return linesRead;
			}

			[Pure]
			private long[] GetLineOffsets(LogBufferList index)
			{
				// We need to find out where to reposition the file stream.
				// For a non-contiguous read-request, we'll have to look up
				// the offset for every requested line since it might jump all over the place.
				var indices = new long[_sourceIndices.Length];
				lock (index)
				{
					index.CopyTo(LogColumns.LineOffsetInBytes, _sourceIndices, indices, 0);
				}

				return indices;
			}

			private int ReadData(StreamReader reader, long[] lineOffsets, string[] destination)
			{
				int i;
				for (i = 0; i < destination.Length; ++i)
				{
					string line;
					var lineOffset = lineOffsets[i];
					if (lineOffset >= 0)
					{
						reader.BaseStream.Position = lineOffset;
						line = reader.ReadLine();
						if (line == null)
						{
							break;
						}
					}
					else
					{
						line = LogColumns.RawContent.DefaultValue;
					}

					destination[i] = line;
				}

				return i;
			}
		}


		#endregion
	}
}