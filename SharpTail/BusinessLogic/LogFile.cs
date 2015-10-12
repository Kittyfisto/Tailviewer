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
		private readonly ManualResetEvent _readEntireFileEvent;
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

		#endregion

		private LogFile(FileStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_stream = stream;
			_reader = new StreamReader(stream);
			_readEntireFileEvent = new ManualResetEvent(false);

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
			_readEntireFileEvent.WaitOne();
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
			int currentLineIndex = 0;

			while (!token.IsCancellationRequested)
			{
				var line = _reader.ReadLine();
				if (line == null)
				{
					_listeners.OnLineRead(currentLineIndex);
					_readEntireFileEvent.Set();
					token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
				}
				else
				{
					_readEntireFileEvent.Reset();
					++currentLineIndex;

					DetermineDateTimeFormat(line);
					Add(line, currentLineIndex);
				}
			}
		}

		private void Add(string line, int currentLineIndex)
		{
			lock (_syncRoot)
			{
				_entries.Add(line);
			}

			_listeners.OnLineRead(currentLineIndex);
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
			FileStream stream = null;
			try
			{
				stream = new FileStream(fname,
				                        FileMode.Open,
				                        FileAccess.Read,
				                        FileShare.ReadWrite);
				return new LogFile(stream);
			}
			catch (Exception)
			{
				if (stream != null)
					stream.Dispose();

				throw;
			}
		}

		public FilteredLogFile Filter(string filterString)
		{
			var file = new FilteredLogFile(this, filterString);
			file.Start();
			return file;
		}
	}
}