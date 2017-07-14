using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Metrolib;
using log4net;
using Tailviewer.BusinessLogic.LogFiles.Parsers;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public sealed class LogFile
		: AbstractLogFile
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Data

		private readonly LogDataCache _cache;
		private readonly LogDataAccessQueue<LogLineIndex, LogLine> _accessQueue;

		private readonly List<LogLine> _entries;
		private readonly object _syncRoot;
		private bool _exists;
		private DateTime? _startTimestamp;
		private int _maxCharactersPerLine;

		#endregion

		#region Timestamp parsing

		private readonly TimestampParser _timestampParser;
		private int _numTimestampSuccess;
		private int _numSuccessiveTimestampFailures;


		#endregion

		#region Listeners

		private readonly string _fileName;
		private readonly string _fullFilename;
		private DateTime _lastModified;
		private int _numberOfLinesRead;
		private bool _lastLineHadNewline;
		private string _untrimmedLastLine;
		private long _lastPosition;
		private long? _fileSize;
		private bool _loggedTimestampWarning;

		#endregion

		public LogFile(ITaskScheduler scheduler, string fileName)
			: base(scheduler)
		{
			if (fileName == null) throw new ArgumentNullException(nameof(fileName));

			_fileName = fileName;
			_fullFilename = fileName;
			if (!Path.IsPathRooted(_fullFilename))
				_fullFilename = Path.Combine(Directory.GetCurrentDirectory(), fileName);

			_entries = new List<LogLine>();
			_syncRoot = new object();
			_timestampParser = new TimestampParser();

			_accessQueue = new LogDataAccessQueue<LogLineIndex, LogLine>();
			_cache = new LogDataCache();

			StartTask();
		}

		public override string ToString()
		{
			return _fileName;
		}

		public IEnumerable<LogLine> Entries => _entries;

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

		public override DateTime? StartTimestamp => _startTimestamp;

		public override DateTime LastModified => _lastModified;

		public override int Count => _entries.Count;

		public override int OriginalCount => Count;

		public override int MaxCharactersPerLine => _maxCharactersPerLine;

		public override bool Exists => _exists;

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

		public override LogLine GetLine(int index)
		{
			lock (_syncRoot)
			{
				return _entries[index];
			}
		}

		protected override TimeSpan RunOnce(CancellationToken token)
		{
			try
			{
				if (!File.Exists(_fileName))
				{
					OnReset(null, out _numberOfLinesRead, out _lastPosition);
					_exists = false;
					_fileSize = null;
					SetEndOfSourceReached();
				}
				else
				{
					using (var stream = new FileStream(_fileName,
													   FileMode.Open,
													   FileAccess.Read,
													   FileShare.ReadWrite))
					using (var reader = new StreamReaderEx(stream))
					{
						_exists = true;
						var info = new FileInfo(_fileName);
						_lastModified = info.LastWriteTime;
						_fileSize = info.Length;
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
				Log.Debug(e);
			}
			catch (DirectoryNotFoundException e)
			{
				Log.Debug(e);
			}
			catch (OperationCanceledException e)
			{
				Log.Debug(e);
			}
			catch (Exception e)
			{
				Log.Debug(e);
			}

			return TimeSpan.FromMilliseconds(100);
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
				_entries.Add(logLine);
				_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, line.Length);


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