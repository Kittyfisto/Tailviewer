using System;
using System.Collections.Generic;
using Metrolib;
using Tailviewer.Api;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Sources
{
	internal sealed class EmptyLogSource
		: ILogSource
	{
		private readonly PropertiesBufferList _properties = new PropertiesBufferList(GeneralProperties.Minimum);
		private readonly HashSet<ILogSourceListener> _listeners = new HashSet<ILogSourceListener>();

		public EmptyLogSource()
		{
			_properties.SetValue(GeneralProperties.PercentageProcessed, Percentage.HundredPercent);
			_properties.SetValue(GeneralProperties.Size, Size.Zero);
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
			get { return GeneralColumns.Minimum; }
		}

		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			if (_listeners.Add(listener))
			{
				listener.OnLogFileModified(this, LogSourceModification.Reset());
			}
		}

		public void RemoveListener(ILogSourceListener listener)
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

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_properties.SetValue(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_properties.SetValue(property, value);
		}

		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_properties.CopyAllValuesTo(destination);
		}

		public void GetColumn<T>(LogSourceSection sourceSection, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices, IColumnDescriptor<T> column, T[] destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(LogSourceSection sourceSection, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			throw new NotImplementedException();
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
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
