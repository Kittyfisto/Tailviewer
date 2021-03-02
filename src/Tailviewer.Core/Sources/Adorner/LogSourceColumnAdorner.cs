using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     This class is responsible for calculating the values for certain columns based on the data of an underlying
	///     <see cref="ILogSource" />.
	/// </summary>
	/// <remarks>
	///     This class calculates the values of the adorned columns on the fly for only the region specified.
	/// </remarks>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class LogSourceColumnAdorner
		: ILogSource
	{
		private static readonly IReadOnlyList<IColumnDescriptor> MaxAdornedColumns;

		private readonly ProxyLogListenerCollection _listeners;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;
		private ILogSource _source;

		static LogSourceColumnAdorner()
		{
			MaxAdornedColumns = new IColumnDescriptor[]
			{
				Core.Columns.Index,
				Core.Columns.OriginalIndex,
				Core.Columns.LogEntryIndex,
				Core.Columns.LineNumber,
				Core.Columns.OriginalLineNumber,
				Core.Columns.ElapsedTime,
				Core.Columns.DeltaTime,
			};
		}

		public LogSourceColumnAdorner(ILogSource source)
			: this(source, MaxAdornedColumns)
		{ }

		public LogSourceColumnAdorner(ILogSource source, IReadOnlyList<IColumnDescriptor> adornedColumns)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_columns = source.Columns.Concat(adornedColumns).Distinct().ToList();
			_listeners = new ProxyLogListenerCollection(source, this);
		}

		public IReadOnlyList<IColumnDescriptor> Columns => _columns;

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

			if (Equals(column, Core.Columns.Index) || Equals(column, Core.Columns.OriginalIndex))
			{
				GetIndex(sourceIndices, (LogLineIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, Core.Columns.LogEntryIndex))
			{
				GetLogEntryIndex(sourceIndices, (LogEntryIndex[])(object)destination, destinationIndex);
			}
			else if (Equals(column, Core.Columns.LineNumber))
			{
				GetLineNumber(sourceIndices, (int[])(object)destination, destinationIndex);
			}
			else if (Equals(column, Core.Columns.OriginalLineNumber))
			{
				GetLineNumber(sourceIndices, (int[])(object)destination, destinationIndex);
			}
			else if (IsAdorned(column, Core.Columns.ElapsedTime))
			{
				GetElapsedTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex, queryOptions);
			}
			else if (IsAdorned(column, Core.Columns.DeltaTime))
			{
				GetDeltaTime(sourceIndices, (TimeSpan?[])(object)destination, destinationIndex, queryOptions);
			}
			else
			{
				_source.GetColumn(sourceIndices, column, destination, destinationIndex, queryOptions);
			}
		}

		[Pure]
		private bool IsAdorned(IColumnDescriptor actualColumn, IColumnDescriptor adornedColumn)
		{
			if (Equals(actualColumn, adornedColumn) && _columns.Contains(adornedColumn))
				return true;

			return false;
		}

		private void GetIndex(IReadOnlyList<LogLineIndex> sourceIndices, LogLineIndex[] destination, int destinationIndex)
		{
			var count = _source?.GetProperty(Core.Properties.LogEntryCount) ?? 0;
			for (int i = 0; i < sourceIndices.Count; ++i)
			{
				var sourceIndex = sourceIndices[i].Value;
				if (sourceIndex >= 0 && sourceIndex < count)
				{
					destination[destinationIndex + i] = sourceIndex;
				}
				else
				{
					destination[destinationIndex + i] = Core.Columns.Index.DefaultValue;
				}
			}
		}

		private void GetLogEntryIndex(IReadOnlyList<LogLineIndex> sourceIndices, LogEntryIndex[] destination, int destinationIndex)
		{
			var count = _source?.GetProperty(Core.Properties.LogEntryCount) ?? 0;
			for (int i = 0; i < sourceIndices.Count; ++i)
			{
				var sourceIndex = sourceIndices[i].Value;
				if (sourceIndex >= 0 && sourceIndex < count)
				{
					destination[destinationIndex + i] = sourceIndex;
				}
				else
				{
					destination[destinationIndex + i] = Core.Columns.LogEntryIndex.DefaultValue;
				}
			}
		}

		private void GetLineNumber(IReadOnlyList<LogLineIndex> indices, int[] destination, int destinationIndex)
		{
			var count = _source?.GetProperty(Core.Properties.LogEntryCount) ?? 0;
			for (int i = 0; i < indices.Count; ++i)
			{
				var index = indices[i];
				if (index >= 0 && index < count)
				{
					var lineNumber = (int) (index + 1);
					destination[destinationIndex + i] = lineNumber;
				}
				else
				{
					destination[destinationIndex + i] = Core.Columns.LineNumber.DefaultValue;
				}
			}
		}

		private void GetElapsedTime(IReadOnlyList<LogLineIndex> indices, TimeSpan?[] buffer, int destinationIndex,
		                            LogSourceQueryOptions queryOptions)
		{
			var startTimestamp = _source.GetProperty(Core.Properties.StartTimestamp);
			if (startTimestamp != null)
			{
				var timestamps = _source.GetColumn(indices, Core.Columns.Timestamp, queryOptions);
				for (int i = 0; i < indices.Count; ++i)
				{
					var timestamp = timestamps[i];
					buffer[destinationIndex + i] = timestamp != null
						? timestamp.Value - startTimestamp
						: Core.Columns.ElapsedTime.DefaultValue;
				}
			}
			else
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					buffer[destinationIndex + i] = Core.Columns.ElapsedTime.DefaultValue;
				}
			}
		}

		private void GetDeltaTime(IReadOnlyList<LogLineIndex> indices,
		                          TimeSpan?[] destination,
		                          int destinationIndex,
		                          LogSourceQueryOptions queryOptions)
		{
			// The easiest way to serve random access to this column is to simply retrieve
			// the timestamp for every requested index as well as for the preceding index.
			var actualIndices = new LogLineIndex[indices.Count * 2];
			for(int i = 0; i < indices.Count; ++i)
			{
				var index = indices[i];
				actualIndices[i * 2 + 0] = index - 1;
				actualIndices[i * 2 + 1] = index;
			}

			var timestamps = _source.GetColumn(actualIndices, Core.Columns.Timestamp, queryOptions);
			for (int i = 0; i < indices.Count; ++i)
			{
				var previousTimestamp = timestamps[i * 2 + 0];
				var currentTimestamp = timestamps[i * 2 + 1];
				destination[destinationIndex + i] = currentTimestamp - previousTimestamp;
			}
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                       ILogBuffer destination,
		                       int destinationIndex,
		                       LogSourceQueryOptions queryOptions)
		{
			var source = _source;
			if (source == null)
			{
				destination.FillDefault(destinationIndex, sourceIndices.Count);
				return;
			}

			source.GetEntries(sourceIndices, destination.Except(MaxAdornedColumns), destinationIndex, queryOptions);

			var augmentedColumns = FindAugmentedColumns(destination);
			if (augmentedColumns.Count == 0)
				return;

			if (destinationIndex != 0)
				throw new NotImplementedException();

			foreach (var column in augmentedColumns)
			{
				destination.CopyFrom(column, this, sourceIndices, queryOptions);
			}
		}

		[Pure]
		private static IReadOnlyList<IColumnDescriptor> FindAugmentedColumns(ILogBuffer destination)
		{
			var augmented = new List<IColumnDescriptor>(MaxAdornedColumns.Count);
			foreach (var column in MaxAdornedColumns)
			{
				if (destination.Contains(column))
					augmented.Add(column);
			}

			return augmented;
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