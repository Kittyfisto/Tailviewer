using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Metrolib;
using log4net;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public sealed class LogFile
		: AbstractLogFile
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string[] HardcodedTimestampFormats;

		#region Data

		private readonly LogDataCache _cache;
		private readonly LogDataAccessQueue<LogLineIndex, LogLine> _accessQueue;

		private readonly List<LogLine> _entries;
		private readonly object _syncRoot;
		private int? _dateTimeFormatIndex;
		private int? _dateTimeColumn;
		private int? _dateTimeLength;
		private bool _exists;
		private DateTime? _startTimestamp;
		private int _maxCharactersPerLine;

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

		#endregion

		static LogFile()
		{
			HardcodedTimestampFormats = new[]
				{
					// The format I currently use at work - should be supported by default :P
					"yyyy-MM-dd HH:mm:ss,fff",
					"yyyy-MM-dd HH:mm:ss",

					// Another one used by a colleague, well its actually nanoseconds but I can't find that format string
					"yyyy MMM dd HH:mm:ss.fff",
					"yyyy MMM dd HH:mm:ss",

					// Various formats...
					"dd/MMM/yyyy:HH:mm:ss",

					"HH:mm:ss"
				};
		}

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
			
			_accessQueue = new LogDataAccessQueue<LogLineIndex, LogLine>();
			_cache = new LogDataCache();

			StartTask();
		}

		public override string ToString()
		{
			return _fileName;
		}

		public IEnumerable<LogLine> Entries => _entries;

		public override Size FileSize
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
			if (_dateTimeColumn == null || _dateTimeLength == null)
			{
				DetermineDateTimePart(line,
				                      out _dateTimeFormatIndex,
				                      out _dateTimeColumn,
				                      out _dateTimeLength);
			}

			return ParseTimestamp(line, _dateTimeFormatIndex, _dateTimeColumn, _dateTimeLength);
		}

		public static DateTime? ParseTimestamp(string line, int? dateTimeFormatIndex, int? dateTimeColumn, int? dateTimeLength)
		{
			if (dateTimeFormatIndex != null && dateTimeColumn != null && dateTimeLength != null)
			{
				int start = dateTimeColumn.Value;
				int length = dateTimeLength.Value;
				if (line.Length >= start + length)
				{
					string timestampValue = line.Substring(start, length);
					string format = HardcodedTimestampFormats[dateTimeFormatIndex.Value];
					DateTime timestamp;
					if (DateTime.TryParseExact(timestampValue,
						format,
						CultureInfo.InvariantCulture,
						DateTimeStyles.None,
						out timestamp))
					{
						return timestamp;
					}
				}
			}

			return null;
		}

		public static void DetermineDateTimePart(string line,
		                                         out int? formatIndex,
		                                         out int? currentColumn,
		                                         out int? currentLength)
		{
			IFormatProvider culture = CultureInfo.InvariantCulture;

			formatIndex = null;
			currentColumn = null;
			currentLength = null;

			for (int m = 0; m < HardcodedTimestampFormats.Length; ++m)
			{
				for (int i = 0; i < line.Length; ++i)
				{
					for (int n = i; n <= line.Length; ++n)
					{
						string dateTimeString = line.Substring(i, n - i);
						DateTime dateTime;
						if (DateTime.TryParseExact(dateTimeString,
							HardcodedTimestampFormats[m],
							culture,
							DateTimeStyles.None,
							out dateTime))
						{
							int length = n - i;
							currentColumn = i;
							currentLength = length;
							formatIndex = m;
							return;
						}
					}
				}
			}
		}
	}
}