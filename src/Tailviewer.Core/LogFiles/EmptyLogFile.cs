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

		public void GetColumn<T>(LogFileSection sourceSection, ILogFileColumn<T> column, T[] destination, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, ILogFileColumn<T> column, T[] destination, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(LogFileSection sourceSection, ILogEntries destination, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex)
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
