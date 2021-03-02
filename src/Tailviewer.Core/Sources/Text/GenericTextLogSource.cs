using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Sources.Text
{
	/// <summary>
	///     A simple accessor which provides access to log entries produced by a <see cref="ILogEntryParser" />.
	///     Parsing happens on demand when corresponding properties are requested.
	/// </summary>
	internal sealed class GenericTextLogSource
		: ILogSource
	{
		private readonly ProxyLogListenerCollection _listeners;
		private readonly ILogEntryParser _parser;
		private readonly IReadOnlyList<IColumnDescriptor> _parsedColumns;
		private readonly IReadOnlyList<IColumnDescriptor> _allColumns;
		private readonly IReadOnlyLogEntry _nothingParsed;
		private ILogSource _source;

		public GenericTextLogSource(ILogSource source,
		                            ILogEntryParser parser)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_parser = parser;
			_parsedColumns = _parser.Columns.ToList();
			_allColumns = _source.Columns.Concat(_parsedColumns).Distinct().ToList();
			_listeners = new ProxyLogListenerCollection(source, this);
			_nothingParsed = new ReadOnlyLogEntry(_parsedColumns);
		}

		public IReadOnlyList<IColumnDescriptor> Columns => _allColumns;

		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogSourceListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties
		{
			get { return _source?.Properties ?? new IReadOnlyPropertyDescriptor[0]; }
		}

		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			var source = _source;
			if (source != null)
				return source.GetProperty(property);

			return property.DefaultValue;
		}

		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			var source = _source;
			if (source != null)
				return source.GetProperty(property);

			return property.DefaultValue;
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_source?.SetProperty(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source?.SetProperty(property, value);
		}

		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_source?.GetAllProperties(destination);
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                         IColumnDescriptor<T> column,
		                         T[] destination,
		                         int destinationIndex,
		                         LogSourceQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));
			if (column == null)
				throw new ArgumentNullException(nameof(column));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			if (destinationIndex + sourceIndices.Count > destination.Length)
				throw new ArgumentException("The given buffer must have an equal or greater length than destinationIndex+length");

			GetEntries(sourceIndices,
			           new SingleColumnLogBufferView<T>(column, destination, destinationIndex, sourceIndices.Count),
			           0, queryOptions);
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                       ILogBuffer destination,
		                       int destinationIndex,
		                       LogSourceQueryOptions queryOptions)
		{
			var source = _source;
			if (source != null)
			{
				var columnsToCopy = new IColumnDescriptor[] {GeneralColumns.Index, GeneralColumns.RawContent};
				var tmp = new LogBufferArray(sourceIndices.Count, columnsToCopy);
				source.GetEntries(sourceIndices, tmp, 0, queryOptions);

				foreach (var column in columnsToCopy)
				{
					if (destination.Contains(column))
					{
						destination.CopyFrom(column, destinationIndex, tmp, new Int32Range(0, sourceIndices.Count));
					}
				}

				for (var i = 0; i < sourceIndices.Count; ++i)
				{
					var parsedLogEntry = _parser.Parse(tmp[i]);
					if (parsedLogEntry != null)
						destination[destinationIndex + i].CopyFrom(parsedLogEntry);
					else
						destination[destinationIndex + i].CopyFrom(_nothingParsed);
				}
			}
			else
			{
				destination.FillDefault(destinationIndex, sourceIndices.Count);
			}
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return _source?.GetLogLineIndexOfOriginalLineIndex(originalLineIndex) ?? LogLineIndex.Invalid;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_source?.Dispose();
			_source = null;
		}

		#endregion
	}
}