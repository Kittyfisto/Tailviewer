using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
		private int? _dateTimeColumn;
		private int? _dateTimeLength;
		private bool _exists;
		private DateTime? _startTimestamp;
		private int _maxCharactersPerLine;

		#endregion

		#region Listeners

		private readonly string _fileName;
		private readonly string _fullFilename;
		private readonly List<KeyValuePair<int, LevelFlags>> _levels;
		private DateTime _lastModified;
		private int _numberOfLinesRead;
		private int _nextLogEntryIndex;
		private long _lastPosition;
		private LevelFlags _previousLevel;
		private DateTime? _previousTimestamp;
		private long? _fileSize;

		#endregion

		static LogFile()
		{
			HardcodedTimestampFormats = new[]
				{
					// The format I currently use at work - should be supported by default :P
					"yyyy-MM-dd HH:mm:ss,fff",

					// Another one used by a colleague, well its actually nanoseconds but I can't find that format string
					"yyyy MMM dd HH:mm:ss.fff",

					// Various formats...
					"dd/MMM/yyyy:HH:mm:ss"
				};
		}

		public LogFile(ITaskScheduler scheduler, string fileName)
			: base(scheduler)
		{
			if (fileName == null) throw new ArgumentNullException("fileName");

			_fileName = fileName;
			_fullFilename = fileName;
			if (!Path.IsPathRooted(_fullFilename))
				_fullFilename = Path.Combine(Directory.GetCurrentDirectory(), fileName);

			_entries = new List<LogLine>();
			_syncRoot = new object();

			_previousLevel = LevelFlags.None;
			_levels = new List<KeyValuePair<int, LevelFlags>>();

			_accessQueue = new LogDataAccessQueue<LogLineIndex, LogLine>();
			_cache = new LogDataCache();

			StartTask();
		}

		public override string ToString()
		{
			return _fileName;
		}

		public IEnumerable<LogLine> Entries
		{
			get { return _entries; }
		}

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

		public override DateTime? StartTimestamp
		{
			get { return _startTimestamp; }
		}

		public override DateTime LastModified
		{
			get { return _lastModified; }
		}

		public override int Count
		{
			get { return _entries.Count; }
		}

		public override int MaxCharactersPerLine
		{
			get { return _maxCharactersPerLine; }
		}

		public override bool Exists
		{
			get { return _exists; }
		}

		public override void GetSection(LogFileSection section, LogLine[] dest)
		{
			if (section.Index < 0)
				throw new ArgumentOutOfRangeException("section.Index");
			if (section.Count < 0)
				throw new ArgumentOutOfRangeException("section.Count");
			if (dest == null)
				throw new ArgumentNullException("dest");
			if (dest.Length < section.Count)
				throw new ArgumentOutOfRangeException("section.Count");

			lock (_syncRoot)
			{
				if (section.Index + section.Count > _entries.Count)
					throw new ArgumentOutOfRangeException("section");

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
					using (var reader = new StreamReader(stream))
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

						string line;
						while ((line = reader.ReadLine()) != null)
						{
							token.ThrowIfCancellationRequested();

							ResetEndOfSourceReached();
							++_numberOfLinesRead;

							LevelFlags level = DetermineLevel(line, _levels);

							DateTime? timestamp;
							int logEntryIndex;
							if (level != LevelFlags.None)
							{
								timestamp = ParseTimestamp(line);
								logEntryIndex = _nextLogEntryIndex;
								++_nextLogEntryIndex;
							}
							else
							{
								if (_nextLogEntryIndex > 0)
								{
									logEntryIndex = _nextLogEntryIndex - 1;
								}
								else
								{
									logEntryIndex = 0;
								}

								// This line belongs to the previous line and together they form
								// (part of) a log entry. Even though only a single line mentions
								// the log level, all lines are given the same log level.
								level = _previousLevel;
								timestamp = _previousTimestamp;
							}

							Add(line, level, _numberOfLinesRead, logEntryIndex, timestamp);
							_previousLevel = level;
							_previousTimestamp = timestamp;
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

		private LevelFlags DetermineLevel(string line, List<KeyValuePair<int, LevelFlags>> levels)
		{
			var level = LevelFlags.None;
			int idx = line.IndexOf("DEBUG", StringComparison.InvariantCulture);
			if (idx != -1)
				levels.Add(new KeyValuePair<int, LevelFlags>(idx, LevelFlags.Debug));

			idx = line.IndexOf("INFO", StringComparison.InvariantCulture);
			if (idx != -1)
				levels.Add(new KeyValuePair<int, LevelFlags>(idx, LevelFlags.Info));

			idx = line.IndexOf("WARN", StringComparison.InvariantCulture);
			if (idx != -1)
				levels.Add(new KeyValuePair<int, LevelFlags>(idx, LevelFlags.Warning));

			idx = line.IndexOf("ERROR", StringComparison.InvariantCulture);
			if (idx != -1)
				levels.Add(new KeyValuePair<int, LevelFlags>(idx, LevelFlags.Error));

			idx = line.IndexOf("FATAL", StringComparison.InvariantCulture);
			if (idx != -1)
				levels.Add(new KeyValuePair<int, LevelFlags>(idx, LevelFlags.Fatal));

			int prev = int.MaxValue;
			foreach (var pair in levels)
			{
				if (pair.Key < prev)
				{
					level = pair.Value;
					prev = pair.Key;
				}
			}
			levels.Clear();

			if (prev == int.MaxValue)
			{
				level = LevelFlags.None;
			}

			return level;
		}

		private void Add(string line, LevelFlags level, int numberOfLinesRead, int numberOfLogEntriesRead, DateTime? timestamp)
		{
			if (_startTimestamp == null)
				_startTimestamp = timestamp;

			lock (_syncRoot)
			{
				int lineIndex = _entries.Count;
				var logLine = new LogLine(lineIndex, numberOfLogEntriesRead, line, level, timestamp);
				_entries.Add(logLine);
				_maxCharactersPerLine = Math.Max(_maxCharactersPerLine, line.Length);
			}

			Listeners.OnRead(numberOfLinesRead);
		}

		private DateTime? ParseTimestamp(string line)
		{
			if (_dateTimeColumn == null || _dateTimeLength == null)
			{
				DetermineDateTimePart(line,
				                      out _dateTimeColumn,
				                      out _dateTimeLength);
			}

			return ParseTimestamp(line, _dateTimeColumn, _dateTimeLength);
		}

		public static DateTime? ParseTimestamp(string line, int? dateTimeColumn, int? dateTimeLength)
		{
			if (dateTimeColumn != null && dateTimeLength != null)
			{
				int start = dateTimeColumn.Value;
				int length = dateTimeLength.Value;
				if (line.Length >= start + length)
				{
					string timestampValue = line.Substring(start, length);
					DateTime timestamp;
					if (DateTime.TryParseExact(timestampValue,
					                           HardcodedTimestampFormats,
					                           CultureInfo.InvariantCulture,
					                           DateTimeStyles.None,
					                           out timestamp))
						return timestamp;
				}
			}

			return null;
		}

		public static void DetermineDateTimePart(string line,
		                                         out int? currentColumn,
		                                         out int? currentLength)
		{
			IFormatProvider culture = CultureInfo.InvariantCulture;

			currentColumn = null;
			currentLength = null;

			for (int i = 0; i < line.Length; ++i)
			{
				for (int n = i; n <= line.Length; ++n)
				{
					DateTime dateTime;
					string dateTimeString = line.Substring(i, n - i);
					if (DateTime.TryParseExact(dateTimeString,
					                           HardcodedTimestampFormats,
					                           culture,
					                           DateTimeStyles.None,
					                           out dateTime))
					{
						int length = n - i;
						currentColumn = i;
						currentLength = length;
						return;
					}
				}
			}
		}
	}
}