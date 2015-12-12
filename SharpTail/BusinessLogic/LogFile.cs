using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SharpTail.BusinessLogic
{
	public sealed class LogFile
		: ILogFile
	{
		#region Reading

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ManualResetEvent _endOfSectionHandle;
		private readonly Task _readTask;
		private readonly StreamReader _reader;
		private readonly Stream _stream;

		#endregion

		#region Data

		private readonly List<string> _entries;
		private readonly object _syncRoot;
		private int? _dateTimeColumn;
		private int? _dateTimeLength;

		#endregion

		#region Listeners

		private readonly LogFileListenerCollection _listeners;
		private readonly DataSource _dataSource;

		#endregion

		private LogFile(DataSource dataSource, FileStream stream)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");
			if (stream == null) throw new ArgumentNullException("stream");

			_dataSource = dataSource;
			_dataSource.LastWritten = File.GetLastWriteTime(dataSource.FullFileName);
			_stream = stream;
			_reader = new StreamReader(stream);
			_endOfSectionHandle = new ManualResetEvent(false);

			_entries = new List<string>();
			_syncRoot = new object();

			_cancellationTokenSource = new CancellationTokenSource();
			_readTask = new Task(ReadFile,
			                     _cancellationTokenSource.Token,
			                     _cancellationTokenSource.Token);

			_listeners = new LogFileListenerCollection();
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

		public void GetSection(LogFileSection section, string[] dest)
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
			get { return Size.FromBytes(_stream.Length); }
		}

		public string GetEntry(int index)
		{
			lock (_syncRoot)
			{
				return _entries[index];
			}
		}

		public IEnumerable<string> Entries
		{
			get { return _entries; }
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_readTask.Wait();
			_stream.Dispose();
			_reader.Dispose();
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

		private void ReadFile(object parameter)
		{
			var token = (CancellationToken)parameter;
			int numberOfLinesRead = 0;

			while (!token.IsCancellationRequested)
			{
				var line = _reader.ReadLine();
				if (line == null)
				{
					_dataSource.LastWritten = File.GetLastWriteTime(_dataSource.FullFileName);
					_listeners.OnRead(numberOfLinesRead);
					_endOfSectionHandle.Set();
					token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
				}
				else
				{
					_endOfSectionHandle.Reset();
					++numberOfLinesRead;

					DetermineDateTimeFormat(line);
					Add(line, numberOfLinesRead);
				}
			}
		}

		private void Add(string line, int numberOfLinesRead)
		{
			lock (_syncRoot)
			{
				_entries.Add(line);
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

		public static LogFile FromFile(string fname)
		{
			return FromFile(new DataSource(fname));
		}

		public static LogFile FromFile(DataSource dataSource)
		{
			FileStream stream = null;
			try
			{
				stream = new FileStream(dataSource.FullFileName,
				                        FileMode.Open,
				                        FileAccess.Read,
				                        FileShare.ReadWrite);
				return new LogFile(dataSource, stream);
			}
			catch (Exception)
			{
				if (stream != null)
					stream.Dispose();

				throw;
			}
		}

		public FilteredLogFile Filter(LevelFlags levelFilter)
		{
			return Filter(null, levelFilter);
		}

		public FilteredLogFile Filter(string stringFilter)
		{
			return Filter(stringFilter, LevelFlags.All);
		}

		public FilteredLogFile Filter(string stringFilter, LevelFlags levelFilter)
		{
			var file = new FilteredLogFile(this, stringFilter, levelFilter);
			file.Start();
			return file;
		}
	}
}