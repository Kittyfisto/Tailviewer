using System;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	internal sealed class EmptyLogFile
		: ILogFile
	{
		private readonly HashSet<ILogFileListener> _listeners = new HashSet<ILogFileListener>();

		#region Implementation of IDisposable

		public void Dispose()
		{}

		#endregion

		#region Implementation of ILogFile

		public bool EndOfSourceReached
		{
			get { return true; }
		}

		public int Count
		{
			get { return 0; }
		}

		public int OriginalCount
		{
			get { return 0; }
		}

		public int MaxCharactersPerLine
		{
			get { return 0; }
		}

		public IReadOnlyList<ILogFileColumn> Columns
		{
			get { return LogFileColumns.Minimum; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			if (_listeners.Add(listener))
			{
				listener.OnLogFileModified(this, LogFileSection.Reset);
			}
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.Remove(listener);
		}

		public IReadOnlyList<ILogFilePropertyDescriptor> Properties
		{
			get
			{
				return new List<ILogFilePropertyDescriptor>();
			}
		}

		public object GetValue(ILogFilePropertyDescriptor propertyDescriptor)
		{
			throw new NotImplementedException();
		}

		public T GetValue<T>(ILogFilePropertyDescriptor<T> propertyDescriptor)
		{
			return default;
		}

		public void GetValues(ILogFileProperties properties)
		{
			throw new NotImplementedException();
		}

		public void GetColumn<T>(LogFileSection section, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> indices, ILogFileColumn<T> column, T[] buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(LogFileSection section, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> indices, ILogEntries buffer, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			//if (section.Count > 0)
			//	throw new ArgumentOutOfRangeException(nameof(section), "This log file is empty");
			throw new NotImplementedException();
		}

		public LogLine GetLine(int index)
		{
			throw new NotImplementedException();
		}

		public double Progress
		{
			get { return 1; }
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
