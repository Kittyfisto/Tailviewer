using System;
using System.Collections.Generic;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// TODO: Use this implementation is tests, where applicable (should reduce number of mocks...).
	/// </summary>
	public sealed class InMemoryLogFile
		: ILogFile
	{
		private readonly object _syncRoot;
		private readonly List<LogLine> _lines;
		private readonly LogFileListenerCollection _listeners;

		public InMemoryLogFile()
		{
			_syncRoot = new object();
			_lines = new List<LogLine>();
			_listeners = new LogFileListenerCollection(this);
		}

		public void Dispose()
		{
			throw new NotImplementedException();
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

		public bool EndOfSourceReached
		{
			get { throw new NotImplementedException(); }
		}

		public int Count
		{
			get { throw new NotImplementedException(); }
		}

		public int MaxCharactersPerLine
		{
			get { throw new NotImplementedException(); }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			throw new NotImplementedException();
		}

		public LogLine GetLine(int index)
		{
			throw new NotImplementedException();
		}
	}
}