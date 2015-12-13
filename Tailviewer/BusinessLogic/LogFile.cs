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

		private readonly List<LogEntry> _entries;
		private readonly object _syncRoot;
		private int? _dateTimeColumn;
		private int? _dateTimeLength;
		private int _fatalCount;
		private int _errorCount;
		private int _warningCount;
		private int _infoCount;
		private int _debugCount;

		#endregion

		#region Listeners

		private readonly LogFileListenerCollection _listeners;
		private readonly string _fileName;

		#endregion

		public LogFile(string fileName)
		{
			if (fileName == null) throw new ArgumentNullException("fileName");

			_fileName = fileName;
			_endOfSectionHandle = new ManualResetEvent(false);

			_entries = new List<LogEntry>();
			_syncRoot = new object();
			_cancellationTokenSource = new CancellationTokenSource();
			_readTask = new Task(ReadFile,
									_cancellationTokenSource.Token,
									_cancellationTokenSource.Token,
									TaskCreationOptions.LongRunning);
			_listeners = new LogFileListenerCollection();
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

		public void Start()
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				_readTask.Start();
			}
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void Remove(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogEntry[] dest)
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

				_entries.CopyTo(section.Index, dest, 0, section.Count);
			}
		}

		public Size FileSize
		{
			get { return Size.FromBytes(new FileInfo(_fileName).Length); }
		}

		public LogEntry GetEntry(int index)
		{
			lock (_syncRoot)
			{
				return _entries[index];
			}
		}

		public IEnumerable<LogEntry> Entries
		{
			get { return _entries; }
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
			get
			{
				return _entries.Count;
			}
		}

		public DateTime LastWritten
		{
			get { return new FileInfo(_fileName).LastWriteTime; }
		}

		private void ReadFile(object parameter)
		{
			var token = (CancellationToken)parameter;
			int numberOfLinesRead = 0;

			try
			{
				using (var stream = new FileStream(_fileName,
				                                   FileMode.Open,
				                                   FileAccess.Read,
				                                   FileShare.ReadWrite))
				using (var reader = new StreamReader(stream))
				{
					while (!token.IsCancellationRequested)
					{
						var line = reader.ReadLine();
						if (line == null)
						{
							_listeners.OnRead(numberOfLinesRead);
							_endOfSectionHandle.Set();
							token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));

							if (stream.Length < stream.Position) //< Somebody cleared the file
							{
								stream.Position = 0;
								numberOfLinesRead = 0;
								_debugCount = 0;
								_infoCount = 0;
								_warningCount = 0;
								_errorCount = 0;
								_fatalCount = 0;

								_entries.Clear();
								_listeners.OnRead(-1);
							}
						}
						else
						{
							_endOfSectionHandle.Reset();
							++numberOfLinesRead;

							DetermineDateTimeFormat(line);
							var level = DetermineLevel(line);
							Add(line, level, numberOfLinesRead);
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

		private LevelFlags DetermineLevel(string line)
		{
			var level = LevelFlags.None;
			if (line.IndexOf("debug", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				level |= LevelFlags.Debug;
				Interlocked.Increment(ref _debugCount);
			}
			if (line.IndexOf("info", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				level |= LevelFlags.Info;
				Interlocked.Increment(ref _infoCount);
			}
			if (line.IndexOf("warn", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				level |= LevelFlags.Warning;
				Interlocked.Increment(ref _warningCount);
			}
			if (line.IndexOf("error", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				level |= LevelFlags.Error;
				Interlocked.Increment(ref _errorCount);
			}
			if (line.IndexOf("fatal", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				level |= LevelFlags.Fatal;
				Interlocked.Increment(ref _fatalCount);
			}
			return level;
		}

		private void Add(string line, LevelFlags level, int numberOfLinesRead)
		{
			lock (_syncRoot)
			{
				_entries.Add(new LogEntry(line, level));
			}

			_listeners.OnRead(numberOfLinesRead);
		}

		private void DetermineDateTimeFormat(string line)
		{
			if (_dateTimeColumn == null || _dateTimeLength == null)
			{
				DetermineDateTimePart(line, out _dateTimeColumn, out _dateTimeLength);
			}
		}

		public static void DetermineDateTimePart(string line, out int? currentColumn, out int? currentLength)
		{
			currentColumn = null;
			currentLength = null;
			for (int i = 0; i < line.Length; ++i)
			{
				for (int n = i; n < line.Length; ++n)
				{
					string dateTimeString = line.Substring(i, n - i);
					DateTime dateTime;
					if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
					{
						int length = n - i;
						if (currentLength == null || length > currentLength)
						{
							currentColumn = i;
							currentLength = length;
						}
					}
				}
			}
		}

		public FilteredLogFile Filter(string stringFilter)
		{
			return Filter(stringFilter, LevelFlags.All);
		}

		public FilteredLogFile Filter(string stringFilter, LevelFlags levelFilter)
		{
			return Filter(stringFilter, levelFilter, TimeSpan.FromMilliseconds(10));
		}

		public FilteredLogFile Filter(string stringFilter, LevelFlags levelFilter, TimeSpan maximumWaitTime)
		{
			var file = new FilteredLogFile(this, stringFilter, levelFilter);
			file.Start(maximumWaitTime);
			return file;
		}
	}
}