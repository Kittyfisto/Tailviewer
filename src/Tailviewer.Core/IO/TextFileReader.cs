using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Core.IO
{
	/// <summary>
	///    Responsible for reading a text file sequentially while building a byte-offset => line index structure, which
	///    is used to serve out-of-order read requests.
	/// </summary>
	internal sealed class TextFileReader
		: ITextFileReader
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;
		private readonly string _fileName;
		private readonly ITextFileListener _listener;
		private readonly ILogFileFormatMatcher _formatMatcher;
		private readonly List<long> _lineOffsets;
		private readonly List<string> _listenerOnReadBuffer;
		private readonly ConcurrentQueue<IReadRequest> _readRequests;
		private readonly ConcurrentLogFilePropertyCollection _properties;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly CancellationToken _cancellationToken;
		private readonly Encoding _defaultEncoding;
		private IPeriodicTask _readTask;
		private long _lastPosition;
		private int _numberOfLinesRead;
		private string _untrimmedLastLine;
		private bool _lastLineHadNewline;

		public TextFileReader(ITaskScheduler taskScheduler,
		                      ILogFileFormatMatcher formatMatcher,
		                      string fileName,
		                      Encoding defaultEncoding,
		                      ITextFileListener listener)
		{
			_taskScheduler = taskScheduler;
			_formatMatcher = formatMatcher;
			_fileName = fileName;
			_defaultEncoding = defaultEncoding ?? throw new ArgumentNullException(nameof(defaultEncoding));
			_properties = new ConcurrentLogFilePropertyCollection(Properties.Minimum);
			_properties.SetValue(Properties.Encoding, defaultEncoding);
			_properties.SetValue(Properties.FormatDetectionCertainty, Certainty.None);
			_listener = new NoThrowTextFileListener(listener);
			_lineOffsets = new List<long>();
			_listenerOnReadBuffer = new List<string>();
			_readRequests = new ConcurrentQueue<IReadRequest>();
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			Log.DebugFormat("Log File '{0}' is interpreted using {1}", _fileName, defaultEncoding.EncodingName);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
		}

		#endregion

		#region Implementation of IFileReader

		public void Start()
		{
			if (_readTask != null)
				return;

			Reset();
			_readTask = _taskScheduler.StartPeriodic(RunOnce, ToString());
		}

		public Task<int> ReadAsync(LogFileSection section, string[] buffer, int index)
		{
			var taskSource = new TaskCompletionSource<int>();
			_readRequests.Enqueue(new ContiguousReadRequest(section, taskSource, buffer));
			return taskSource.Task;
		}

		public Task<int> ReadAsync(IReadOnlyList<LogLineIndex> section, string[] buffer, int index)
		{
			var taskSource = new TaskCompletionSource<int>();
			_readRequests.Enqueue(new NonContiguousReadRequest(section, taskSource, buffer));
			return taskSource.Task;
		}

		public int Read(IReadOnlyList<LogLineIndex> section, string[] buffer, int index)
		{
			throw new NotImplementedException();
		}

		public int Read(LogFileSection section, string[] buffer, int index)
		{
			var task = ReadAsync(section, buffer, index);
			task.Wait(_cancellationToken);
			return task.Result;
		}

		#endregion

		private TimeSpan RunOnce()
		{
			bool readAnything = false;

			try
			{
				if (!File.Exists(_fileName))
				{
					SetDoesNotExist();
				}
				else
				{
					var info = new FileInfo(_fileName);
					_properties.SetValue(Properties.LastModified, info.LastWriteTime);
					_properties.SetValue(Properties.Created, info.CreationTime);
					_properties.SetValue(Properties.Size, Size.FromBytes(info.Length));

					using (var stream = new FileStream(_fileName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite))
					{
						var encoding = DetermineEncoding(stream);
						using (var reader = new StreamReaderEx(stream, encoding))
						{
							// TODO: Even though we may block this method until we have read the file once, we should interleave them a little bit
							//       The reason being is that we are certainly getting more read requests as we build the index structure and reading
							//       the entire file first before serving any request is questionable.
							readAnything = ReadFileSequentially(reader, stream);
							readAnything |= ServeReadRequests(reader, stream);
						}
					}

					OnRead();
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

			if (readAnything)
				return TimeSpan.Zero;

			return TimeSpan.FromMilliseconds(100);
		}

		/// <summary>
		///    (Continues) reading the file sequentially.
		/// </summary>
		/// <remarks>
		///    Notifies the listener whenever changes were detected.
		///    Restarts the process in case the file was deleted / shrank in size compared to the previous run.
		/// </remarks>
		/// <param name="reader"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		private bool ReadFileSequentially(StreamReaderEx reader, FileStream stream)
		{
			bool readAnything = false;

			// We change the error flag explicitly AFTER opening
			// the stream because that operation might throw if we're
			// not allowed to access the file (in which case a different
			// error must be set).

			_properties.SetValue(Properties.EmptyReason, ErrorFlags.None);
			if (stream.Length >= _lastPosition)
			{
				stream.Position = _lastPosition;
			}
			else
			{
				OnReset(stream, out _numberOfLinesRead, out _lastPosition);
			}

			string currentLine;
			var startOfLine = stream.Position;
			while ((currentLine = reader.ReadLine()) != null)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				ResetEndOfSourceReached();

				bool lastLineHadNewline = _lastLineHadNewline;
				var trimmedLine = currentLine.TrimNewlineEnd(out _lastLineHadNewline);
				var entryCount = _lineOffsets.Count;
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
					readAnything = true;
				}

				Add(startOfLine, trimmedLine);
				startOfLine = stream.Position;
			}

			_lastPosition = stream.Position;
			return readAnything;
		}

		/// <summary>
		///    Serves pending read requests.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		private bool ServeReadRequests(StreamReaderEx reader, FileStream stream)
		{
			bool readAnything = false;
			while (_readRequests.TryDequeue(out var request))
			{
				try
				{
					ServeReadRequest(reader, stream, request);
					readAnything = true;
				}
				catch (Exception e)
				{
					request.TaskSource.TrySetException(e);
					Log.Debug(e);
				}
			}

			return readAnything;
		}

		private void ServeReadRequest(StreamReaderEx reader, FileStream stream, IReadRequest request)
		{
			if (request is ContiguousReadRequest contiguous)
			{
				ServeContiguousReadRequest(reader, stream, contiguous);
			}
			else if (request is NonContiguousReadRequest nonContiguous)
			{
				ServeNonContiguousReadRequest(reader, stream, nonContiguous);
			}
		}

		private void ServeContiguousReadRequest(StreamReaderEx reader, FileStream stream, ContiguousReadRequest request)
		{
			var startIndex = (int) request.Section.Index;
			stream.Position = _lineOffsets[startIndex];
			int linesRead = 0;
			for (linesRead = 0; linesRead < request.Section.Count; ++linesRead)
			{
				var line = reader.ReadLine();
				if (line == null)
					break;

				request.Buffer[linesRead] = line;
			}

			request.TaskSource.TrySetResult(linesRead);
		}

		private void ServeNonContiguousReadRequest(StreamReaderEx reader, FileStream stream, NonContiguousReadRequest request)
		{
			int linesRead = 0;
			foreach (var lineIndex in request.Section)
			{
				stream.Position = _lineOffsets[lineIndex.Value];
				var line = reader.ReadLine();
				if (line == null)
					break;

				request.Buffer[linesRead] = line;
				++linesRead;
			}

			request.TaskSource.TrySetResult(linesRead);
		}

		private void Add(long lineOffset, string trimmedLine)
		{
			_lineOffsets.Add(lineOffset);
			_listenerOnReadBuffer.Add(trimmedLine);

			if (_listenerOnReadBuffer.Count > 1000)
			{
				OnRead();
			}
		}

		private void UpdateProgress()
		{
			_properties.SetValue(Properties.PercentageProcessed, CalculateProgress());
		}

		[Pure]
		private Percentage CalculateProgress()
		{
			var fileSize = _properties.GetValue(Properties.Size);
			var position = _lastPosition;
			if (fileSize == null)
				return Percentage.HundredPercent; //< We've fully read the non-existant file...

			var progress = (double) fileSize.Value.Bytes / position;
			// Since we've performed two reads, it's possible that they have inconsistent values
			// and therefore we should perform a sanity check on the resulting progress value
			// so it stays within the expected boundaries. (It's just not worth a lock)
			return Percentage.FromPercent((float)MathEx.Saturate(progress) * 100);
		}

		/// <summary>
		/// Determines the encoding of the 
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private Encoding DetermineEncoding(FileStream stream)
		{
			var format = _properties.GetValue(Properties.Format);
			var certainty = _properties.GetValue(Properties.FormatDetectionCertainty);
			if (format == null || certainty != Certainty.Sure)
			{
				format = TryFindFormatOf(stream, out certainty);
				_properties.SetValue(Properties.Format, format);
				_properties.SetValue(Properties.FormatDetectionCertainty, certainty);

				if (format != null)
				{
					var encoding = format.Encoding ?? _defaultEncoding;
					var previousEncoding = _properties.GetValue(Properties.Encoding);
					if (!Equals(encoding, previousEncoding))
					{
						Log.DebugFormat("Log File '{0}' is now interpreted using {1} (previous: {2})", _fileName, encoding.EncodingName, previousEncoding.EncodingName);
						_properties.SetValue(Properties.Encoding, encoding);
					}
				}
			}

			return _properties.GetValue(Properties.Encoding);
		}

		[Pure]
		private ILogFileFormat TryFindFormatOf(FileStream stream, out Certainty certainty)
		{
			var pos = stream.Position;

			const int maxHeaderLength = 512;
			var length = Math.Min(maxHeaderLength, stream.Length - pos);
			var header = new byte[length];
			stream.Read(header, 0, header.Length);
			certainty = length >= maxHeaderLength
				? Certainty.Sure
				: Certainty.Uncertain;

			_formatMatcher.TryMatchFormat(_fileName, header, out var format);
			if (format != null)
				return format;

			return LogFileFormats.GenericText;
		}

		private void RemoveLast()
		{
			var index = _lineOffsets.Count - 1;
			_lineOffsets.RemoveAt(index);
		}

		private void OnRead()
		{
			UpdateProgress();
			var section = new LogFileSection(_numberOfLinesRead - _listenerOnReadBuffer.Count, _listenerOnReadBuffer.Count);
			_listener.OnRead(_properties, section, _listenerOnReadBuffer);
			_listenerOnReadBuffer.Clear();
		}

		private void OnReset(FileStream stream,
		                     out int numberOfLinesRead,
		                     out long lastPosition)
		{
			lastPosition = 0;
			if (stream != null)
				stream.Position = 0;

			numberOfLinesRead = 0;
			Reset();
		}

		private void Reset()
		{
			_properties.SetValue(Properties.StartTimestamp, null);
			_properties.SetValue(Properties.EndTimestamp, null);
			_properties.SetValue(Properties.Duration, null);

			_lineOffsets.Clear();
			_listener.OnReset(_properties);
		}

		private void SetError(ErrorFlags error)
		{
			_properties.SetValue(Properties.EmptyReason, error);
			SetEndOfSourceReached();
		}

		private void ResetEndOfSourceReached()
		{
			//_properties.SetValue(LogFileProperties.PercentageProcessed, ...);
		}

		private void SetEndOfSourceReached()
		{
			_properties.SetValue(Properties.PercentageProcessed, Percentage.HundredPercent);
			_listener.OnEndOfSourceReached(_properties);
		}

		private void SetDoesNotExist()
		{
			_properties.Reset();
			_properties.SetValue(Properties.EmptyReason, ErrorFlags.SourceDoesNotExist);
			_properties.SetValue(Properties.PercentageProcessed, Percentage.HundredPercent);
			_listener.OnReset(_properties);
		}

		interface IReadRequest
		{
			TaskCompletionSource<int> TaskSource { get; }
			string[] Buffer { get; }
		}

		sealed class ContiguousReadRequest
			: IReadRequest
		{
			public readonly LogFileSection Section;
			public TaskCompletionSource<int> TaskSource { get; }
			public string[] Buffer { get; }

			public ContiguousReadRequest(LogFileSection section,
			                             TaskCompletionSource<int> taskSourceSource,
			                             string[] buffer)
			{
				Section = section;
				TaskSource = taskSourceSource;
				Buffer = buffer;
			}
		}

		sealed class NonContiguousReadRequest
			: IReadRequest
		{
			public readonly IReadOnlyList<LogLineIndex> Section;
			public TaskCompletionSource<int> TaskSource { get; }
			public string[] Buffer { get; }

			public NonContiguousReadRequest(IReadOnlyList<LogLineIndex> section,
			                                TaskCompletionSource<int> taskSourceSource,
			                                string[] buffer)
			{
				Section = section;
				TaskSource = taskSourceSource;
				Buffer = buffer;
			}
		}
	}
}
