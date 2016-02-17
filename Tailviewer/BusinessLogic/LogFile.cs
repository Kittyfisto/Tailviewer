using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic
{
	internal sealed class LogFile
		: ILogFile
	{
		#region Reading

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ManualResetEvent _endOfSectionHandle;
		private readonly Task _readTask;

		#endregion

		#region Data

		private readonly List<LogLine> _entries;
		private readonly object _syncRoot;
		private int? _dateTimeColumn;
		private int? _dateTimeLength;
		private int _debugCount;
		private int _errorCount;
		private int _fatalCount;
		private int _infoCount;
		private int _otherCount;
		private DateTime? _startTimestamp;
		private int _warningCount;

		#endregion

		#region Listeners

		private readonly string _fileName;
		private readonly LogFileListenerCollection _listeners;
		private DateTime _lastModified;

		#endregion

		private static readonly string[] HardcodedTimestampFormats;

		static LogFile()
		{
			HardcodedTimestampFormats = new[]
				{
					// The format I currently use at work - should be supported by default :P
					"yyyy-MM-dd HH:mm:ss,fff"
				};
		}

		public LogFile(string fileName)
		{
			if (fileName == null) throw new ArgumentNullException("fileName");

			_fileName = fileName;
			_endOfSectionHandle = new ManualResetEvent(false);

			_entries = new List<LogLine>();
			_syncRoot = new object();
			_cancellationTokenSource = new CancellationTokenSource();
			_readTask = new Task(ReadFile,
			                     _cancellationTokenSource.Token,
			                     _cancellationTokenSource.Token,
			                     TaskCreationOptions.LongRunning);
			_listeners = new LogFileListenerCollection(this);
		}

		public IEnumerable<LogLine> Entries
		{
			get { return _entries; }
		}

		public int DebugCount
		{
			get { return _debugCount; }
		}

		public int InfoCount
		{
			get { return _infoCount; }
		}

		public int WarningCount
		{
			get { return _warningCount; }
		}

		public int ErrorCount
		{
			get { return _errorCount; }
		}

		public int FatalCount
		{
			get { return _fatalCount; }
		}

		public Size FileSize
		{
			get
			{
				if (!File.Exists(_fileName))
					return Size.Zero;

				try
				{
					return Size.FromBytes(new FileInfo(_fileName).Length);
				}
				catch (Exception)
				{
					return Size.Zero;
				}
			}
		}

		public DateTime? StartTimestamp
		{
			get { return _startTimestamp; }
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
		}

		public int OtherCount
		{
			get { return _otherCount; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void Remove(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
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

		public LogLine GetLine(int index)
		{
			lock (_syncRoot)
			{
				return _entries[index];
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_readTask.Wait();
		}

		/// <summary>
		///     Blocks until the entire contents of the file has been read into memory.
		/// </summary>
		public void Wait()
		{
			while (true)
			{
				if (_endOfSectionHandle.WaitOne(TimeSpan.FromMilliseconds(100)))
					break;

				if (_readTask.IsFaulted)
					throw _readTask.Exception;
			}
		}

		public int Count
		{
			get { return _entries.Count; }
		}

		public void Start()
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				_readTask.Start();
			}
		}

		private void ReadFile(object parameter)
		{
			var token = (CancellationToken) parameter;
			int numberOfLinesRead = 0;
			int nextLogEntryIndex = 0;
			bool reachedEof = false;
			var previousLevel = LevelFlags.None;
			DateTime? previousTimestamp = null;

			try
			{
				var levels = new List<KeyValuePair<int, LevelFlags>>();
				_lastModified = File.GetLastWriteTime(_fileName);

				using (var stream = new FileStream(_fileName,
				                                   FileMode.Open,
				                                   FileAccess.Read,
				                                   FileShare.ReadWrite))
				using (var reader = new StreamReader(stream))
				{
					while (!token.IsCancellationRequested)
					{
						string line = reader.ReadLine();
						if (line == null)
						{
							reachedEof = true;
							_listeners.OnRead(numberOfLinesRead);
							_endOfSectionHandle.Set();
							token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));

							if (stream.Length < stream.Position) //< Somebody cleared the file
							{
								stream.Position = 0;
								numberOfLinesRead = 0;
								_otherCount = 0;
								_debugCount = 0;
								_infoCount = 0;
								_warningCount = 0;
								_errorCount = 0;
								_fatalCount = 0;
								_startTimestamp = null;

								_entries.Clear();
								_listeners.OnRead(-1);
							}
						}
						else
						{
							if (reachedEof)
								_lastModified = DateTime.Now;

							reachedEof = false;

							_endOfSectionHandle.Reset();
							++numberOfLinesRead;

							LevelFlags level = DetermineLevel(line, levels);

							DateTime? timestamp;
							int logEntryIndex;
							if (level != LevelFlags.None)
							{
								timestamp = ParseTimestamp(line);
								logEntryIndex = nextLogEntryIndex;
								++nextLogEntryIndex;
							}
							else
							{
								if (nextLogEntryIndex > 0)
								{
									logEntryIndex = nextLogEntryIndex - 1;
								}
								else
								{
									logEntryIndex = 0;
								}

								// This line belongs to the previous line and together they form
								// (part of) a log entry. Even though only a single line mentions
								// the log level, all lines are given the same log level.
								level = previousLevel;
								timestamp = previousTimestamp;
							}


							Add(line, level, numberOfLinesRead, logEntryIndex, timestamp);
							previousLevel = level;
							previousTimestamp = timestamp;
						}
					}
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (Exception)
			{
			}
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

			switch (level)
			{
				case LevelFlags.Debug:
					Interlocked.Increment(ref _debugCount);
					break;
				case LevelFlags.Info:
					Interlocked.Increment(ref _infoCount);
					break;
				case LevelFlags.Warning:
					Interlocked.Increment(ref _warningCount);
					break;
				case LevelFlags.Error:
					Interlocked.Increment(ref _errorCount);
					break;
				case LevelFlags.Fatal:
					Interlocked.Increment(ref _fatalCount);
					break;
				default:
					Interlocked.Increment(ref _otherCount);
					break;
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
				_entries.Add(new LogLine(lineIndex, numberOfLogEntriesRead, line, level, timestamp));
			}

			_listeners.OnRead(numberOfLinesRead);
		}

		private DateTime? ParseTimestamp(string line)
		{
			if (_dateTimeColumn == null || _dateTimeLength == null)
			{
				DetermineDateTimePart(line,
				                      out _dateTimeColumn,
				                      out _dateTimeLength);
			}

			if (_dateTimeColumn != null && _dateTimeLength != null)
			{
				int start = _dateTimeColumn.Value;
				int length = _dateTimeLength.Value;
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
				for (int n = i + 1; n < line.Length; ++n)
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