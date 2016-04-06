using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogViewer
{
	public sealed class MockLogFile
		: ILogFile
	{
		private readonly List<LogLine> _entries;

		public MockLogFile()
		{
			_entries = new List<LogLine>
				{
					new LogLine(0, 0, "This is a test", LevelFlags.Debug)
				};
		}

		public void Dispose()
		{
			
		}

		public void Wait()
		{
			
		}

		public DateTime? StartTimestamp
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LastModified
		{
			get { throw new NotImplementedException(); }
		}

		public Size FileSize
		{
			get { throw new NotImplementedException(); }
		}

		public bool Exists
		{
			get { throw new NotImplementedException(); }
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

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			_entries.CopyTo(0, dest, 0, section.Count);
		}

		public LogLine GetLine(int index)
		{
			throw new NotImplementedException();
		}

		public LogLine GetEntry(int index)
		{
			return _entries[index];
		}
	}
}