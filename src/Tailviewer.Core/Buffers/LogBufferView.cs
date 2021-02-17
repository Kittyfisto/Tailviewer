using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Entries;

namespace Tailviewer.Core.Buffers
{
	/// <summary>
	///    This class represents a view onto another <see cref="ILogBuffer"/>, exposing only a subset of its original columns.
	/// </summary>
	public sealed class LogBufferView
		: ILogBuffer
	{
		private readonly ILogBuffer _inner;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;

		/// <summary>
		/// Initializes this log entry list proxy
		/// </summary>
		/// <param name="inner"></param>
		/// <param name="columns"></param>
		public LogBufferView(ILogBuffer inner, params IColumnDescriptor[] columns)
			: this(inner, (IReadOnlyList<IColumnDescriptor>)columns)
		{}

		/// <summary>
		/// Initializes this log entry list proxy
		/// </summary>
		/// <param name="inner"></param>
		/// <param name="columns"></param>
		public LogBufferView(ILogBuffer inner, IReadOnlyList<IColumnDescriptor> columns)
		{
			_inner = inner;
			_columns = columns;
		}

		#region Implementation of IEnumerable

		/// <inheritdoc />
		public IEnumerator<ILogEntry> GetEnumerator()
		{
			return new LogBufferEnumerator(this);
		}

		IEnumerator<IReadOnlyLogEntry> IEnumerable<IReadOnlyLogEntry>.GetEnumerator()
		{
			return new ReadOnlyLogBufferEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of IReadOnlyCollection<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public int Count
		{
			get { return _inner.Count; }
		}

		#endregion

		#region Implementation of IReadOnlyList<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public void CopyFrom<T>(IColumnDescriptor<T> column, int destinationIndex, T[] source, int sourceIndex, int length)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyFrom(column, destinationIndex, source, sourceIndex, length);
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column, int destinationIndex, ILogSource source, IReadOnlyList<LogLineIndex> sourceIndices, LogFileQueryOptions queryOptions)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyFrom(column, destinationIndex, source, sourceIndices, queryOptions);
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column,
		                     int destinationIndex,
		                     IReadOnlyLogBuffer source,
		                     IReadOnlyList<int> sourceIndices)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyFrom(column, destinationIndex, source, sourceIndices);
		}

		/// <inheritdoc />
		public void FillDefault(int offset, int length)
		{
			_inner.FillDefault(offset, length);
		}

		/// <inheritdoc />
		public void FillDefault(IColumnDescriptor column, int destinationIndex, int length)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.FillDefault(column, destinationIndex, length);
		}

		ILogEntry ILogBuffer.this[int index]
		{
			get
			{
				return new LogEntryView(_inner[index], _columns);
			}
		}

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index]
		{
			get
			{
				return new ReadOnlyLogEntryView(_inner[index], _columns);
			}
		}

		#endregion

		#region Implementation of IReadOnlyLogEntries

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get { return _columns; }
		}

		/// <inheritdoc />
		public bool Contains(IColumnDescriptor column)
		{
			return _columns.Contains(column);
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, int sourceIndex, T[] destination, int destinationIndex, int length)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyTo(column, sourceIndex, destination, destinationIndex, length);
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyTo(column, sourceIndices, destination, destinationIndex);
		}

		/// <inheritdoc />
		public void CopyTo<T>(IColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, IList<T> destination, int destinationIndex)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyTo(column, sourceIndices, destination, destinationIndex);
		}

		#endregion
	}
}