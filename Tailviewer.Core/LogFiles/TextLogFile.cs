using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Parsers;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     The bead-and-butter <see cref="ILogFile" /> implementation for Tailviewer.
	///     Responsible for scanning and reading the content of a file on disk, forwarding
	///     them to its <see cref="ILogFileListener"/>s.
	/// </summary>
	public sealed class TextLogFile
		: AbstractLogFile
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Data

		private readonly Encoding _encoding;
		private readonly List<LogLine> _entries;
		private readonly object _syncRoot;
		private ErrorFlags _error;
		private DateTime? _startTimestamp;
		private int _maxCharactersPerLine;
		private readonly NoThrowLogLineTranslator _translator;

		#endregion

		#region Timestamp parsing

		private readonly ITimestampParser _timestampParser;
		private int _numTimestampSuccess;
		private int _numSuccessiveTimestampFailures;

		#endregion

		#region Listeners

		private readonly string _fileName;
		private readonly string _fullFilename;
		private DateTime _lastModified;
		private DateTime _created;
		private int _numberOfLinesRead;
		private bool _lastLineHadNewline;
		private string _untrimmedLastLine;
		private long _lastPosition;
		private long? _fileSize;
		private bool _loggedTimestampWarning;

		#endregion

		/// <summary>
		/// Initializes this text log file.
		/// </summary>
		/// <param name="scheduler"></param>
		/// <param name="fileName"></param>
		/// <param name="timestampParser">An optional timestamp parser that is used to find timestamps in log messages. If none is specified, then <see cref="TimestampParser"/> is used</param>
		/// <param name="translator">An optional translator that is used to translate each log line in memory. If none is specified, then log lines are displayed as they are in the file on disk</param>
		/// <param name="encoding">The encoding to use to interpet the file, if none is specified, then <see cref="Encoding.UTF8"/> is used</param>
		public TextLogFile(ITaskScheduler scheduler,
		                   string fileName,
		                   ITimestampParser timestampParser = null,
		                   ILogLineTranslator translator = null,
		                   Encoding encoding = null)
			: base(scheduler)
		{
			if (fileName == null) throw new ArgumentNullException(nameof(fileName));

			_fileName = fileName;
			_fullFilename = fileName;
			if (!Path.IsPathRooted(_fullFilename))
				_fullFilename = Path.Combine(Directory.GetCurrentDirectory(), fileName);

			if (translator != null)
				_translator = new NoThrowLogLineTranslator(translator);

			_entries = new List<LogLine>();
			_syncRoot = new object();
			_encoding = encoding ?? Encoding.UTF8;

			Log.DebugFormat("Log File '{0}' is interpreted using {1}", _fileName, _encoding.EncodingName);

			if (timestampParser != null)
			{
				_timestampParser = new NoThrowTimestampParser(timestampParser);
			}
			else
			{
				_timestampParser = new TimestampParser();
			}

			StartTask();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return _fileName;
		}

		/// <inheritdoc />
		public IEnumerable<LogLine> Entries => _entries;

		/// <inheritdoc />
		public override Size Size
		{
			get
			{
				var size = _fileSize;
				if (size == null)
					return Size.Zero;

				return Size.FromBytes(size.Value);
			}
		}

		/// <inheritdoc />
		public override DateTime? StartTimestamp => _startTimestamp;

		/// <inheritdoc />
		public override DateTime LastModified => _lastModified;

		/// <inheritdoc />
		public override DateTime Created => _created;

		/// <inheritdoc />
		public override int Count => _entries.Count;

		/// <inheritdoc />
		public override int OriginalCount => Count;

		/// <inheritdoc />
		public override int MaxCharactersPerLine => _maxCharactersPerLine;

		/// <inheritdoc />
		public override ErrorFlags Error => _error;

		/// <inheritdoc />
		public override void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			if (section.Index < 0)
				throw new ArgumentOutOfRangeException("section.Index");
			if (section.Count < 0)
				throw new ArgumentOutOfRangeException("section.Count");
			if (dest == null)
				throw new ArgumentNullException(nameof(dest));
			if (dest.Length < section.Count)
				throw new ArgumentOutOfRangeException("section.Count");

			lock (_syncRoot)
			{
				if (section.Index + section.Count > _entries.Count)
					throw new ArgumentOutOfRangeException(nameof(section));

				_entries.CopyTo((int) section.Index, dest, 0, section.Count);
			}
		}

		/// <inheritdoc />
		public override LogLine GetLine(int index)
		{
			lock (_syncRoot)
			{
				return _entries[index];
			}
		}

		/// <inheritdoc />
		public override double Progress
		{
			get
			{
				var fileSize = _fileSize;
				var position = _lastPosition;
				if (fileSize == null)
					return 1; //< We've fully read the non-existant file...

				var progress = (double) fileSize.Value / position;
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
					_lastModified = info.LastWriteTime;
					_created = info.CreationTime;
					_fileSize = info.Length;

					using (var stream = new FileStream(_fileName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite))
					using (var reader = new StreamReaderEx(stream, _encoding))
					{
						// We change the error flag explicitly AFTER opening
						// the stream because that operation might throw if we're
						// not allowed to access the file (in which case a different
						// error must be set).

						_error = ErrorFlags.None;
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

							LevelFlags level = LogLine.DetermineLevelFromLine(currentLine);

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

							var timestamp = ParseTimestamp(trimmedLine);
							Add(trimmedLine, level, _numberOfLinesRead, timestamp);
						}

						_lastPosition = stream.Position;
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

		private void SetDoesNotExist()
		{
			OnReset(null, out _numberOfLinesRead, out _lastPosition);
			_fileSize = null;
			_created = DateTime.MinValue;
			SetError(ErrorFlags.SourceDoesNotExist);
		}

		private void SetError(ErrorFlags error)
		{
			_error = error;
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
			_maxCharactersPerLine = 0;

			_entries.Clear();
			Listeners.Reset();
		}

		private void Add(string line, LevelFlags level, int numberOfLinesRead, DateTime? timestamp)
		{
			if (_startTimestamp == null)
				_startTimestamp = timestamp;

			lock (_syncRoot)
			{
				int lineIndex = _entries.Count;
				var logLine = new LogLine(lineIndex, lineIndex, line, level, timestamp);
				var translated = Translate(logLine);
				_entries.Add(translated);
				_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, translated.Message?.Length ?? 0);

				if (timestamp != null)
				{
					var difference = timestamp - _lastModified;
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
								_lastModified,
								timestamp
							);
							_loggedTimestampWarning = true;
						}
						_lastModified = timestamp.Value;
					}
				}
			}

			Listeners.OnRead(numberOfLinesRead);
		}

		[Pure]
		private LogLine Translate(LogLine logLine)
		{
			if (_translator == null)
				return logLine;

			return _translator.Translate(this, logLine);
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

		private DateTime? ParseTimestamp(string line)
		{
			// If we stumble upon a file that doesn't contain a single timestamp in the first hundred log lines,
			// then we will just call it a day and never try again...
			// This obviously opens the possibility for not being able to detect valid timestamps in a file, however
			// this is outweighed by being able to read a file without memory FAST. The current algorithm to detect
			// the position and format is so slow that I can read about 1k lines of random data which is pretty bad...
			if (_numTimestampSuccess == 0 &&
			    _numSuccessiveTimestampFailures >= 100)
			{
				return null;
			}

			DateTime timestamp;
			if (_timestampParser.TryParse(line, out timestamp))
			{
				++_numTimestampSuccess;
				_numSuccessiveTimestampFailures = 0;
				return timestamp;
			}

			++_numSuccessiveTimestampFailures;
			return null;
		}
	}
}