using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	internal sealed class EmptyLogFile
		: ILogFile
	{
		private readonly LogFilePropertyList _properties = new LogFilePropertyList(LogFileProperties.Minimum);
		private readonly HashSet<ILogFileListener> _listeners = new HashSet<ILogFileListener>();

		public EmptyLogFile()
		{
			_properties.SetValue(LogFileProperties.PercentageProcessed, Percentage.HundredPercent);
			_properties.SetValue(LogFileProperties.Size, Size.Zero);
		}

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

		public IReadOnlyList<IColumnDescriptor> Columns
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

		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get
			{
				return _properties.Properties;
			}
		}

		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			_properties.TryGetValue(property, out var value);
			return value;
		}

		public void SetProperty(ILogFilePropertyDescriptor property, object value)
		{
			_properties.SetValue(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_properties.SetValue(property, value);
		}

		public void GetAllProperties(ILogFileProperties destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		public void GetColumn<T>(LogFileSection sourceSection, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(LogFileSection sourceSection, ILogEntries destination, int destinationIndex, LogFileQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogEntries destination, int destinationIndex, LogFileQueryOptions queryOptions)
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
