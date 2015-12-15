using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;

namespace LogViewer
{
	public sealed class MockLogFile
		: ILogFile
	{
		private readonly List<LogEntry> _entries;

		public MockLogFile()
		{
			_entries = new List<LogEntry>
				{
					new LogEntry("This is a test", LevelFlags.Debug)
				};
		}

		public void Dispose()
		{
			
		}

		public void Wait()
		{
			
		}

		public int Count
		{
			get { return _entries.Count; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			
		}

		public void Remove(ILogFileListener listener)
		{
			
		}

		public void GetSection(LogFileSection section, LogEntry[] dest)
		{
			_entries.CopyTo(0, dest, 0, section.Count);
		}

		public LogEntry GetEntry(int index)
		{
			return _entries[index];
		}
	}
}