using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///    This class represents a view onto another <see cref="ILogEntries"/>, exposing only a subset of its original columns.
	/// </summary>
	public sealed class LogEntriesView
		: ILogEntries
	{
		private readonly ILogEntries _inner;
		private readonly IReadOnlyList<ILogFileColumnDescriptor> _columns;

		/// <summary>
		/// Initializes this log entry list proxy
		/// </summary>
		/// <param name="inner"></param>
		/// <param name="columns"></param>
		public LogEntriesView(ILogEntries inner, params ILogFileColumnDescriptor[] columns)
			: this(inner, (IReadOnlyList<ILogFileColumnDescriptor>)columns)
		{}

		/// <summary>
		/// Initializes this log entry list proxy
		/// </summary>
		/// <param name="inner"></param>
		/// <param name="columns"></param>
		public LogEntriesView(ILogEntries inner, IReadOnlyList<ILogFileColumnDescriptor> columns)
		{
			_inner = inner;
			_columns = columns;
		}

		#region Implementation of IEnumerable

		/// <inheritdoc />
		public IEnumerator<ILogEntry> GetEnumerator()
		{
			return new LogEntriesEnumerator(this);
		}

		IEnumerator<IReadOnlyLogEntry> IEnumerable<IReadOnlyLogEntry>.GetEnumerator()
		{
			return new ReadOnlyLogEntriesEnumerator(this);
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
		public void CopyFrom<T>(ILogFileColumnDescriptor<T> column, int destinationIndex, T[] source, int sourceIndex, int length)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyFrom(column, destinationIndex, source, sourceIndex, length);
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumnDescriptor column, int destinationIndex, ILogFile source, LogFileSection section)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyFrom(column, destinationIndex, source, section);
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumnDescriptor column, int destinationIndex, ILogFile source, IReadOnlyList<LogLineIndex> sourceIndices)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyFrom(column, destinationIndex, source, sourceIndices);
		}

		/// <inheritdoc />
		public void CopyFrom(ILogFileColumnDescriptor column,
		                     int destinationIndex,
		                     IReadOnlyLogEntries source,
		                     IReadOnlyList<int> sourceIndices)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyFrom(column, destinationIndex, source, sourceIndices);
		}

		/// <inheritdoc />
		public void FillDefault(int destinationIndex, int length)
		{
			_inner.FillDefault(destinationIndex, length);
		}

		/// <inheritdoc />
		public void FillDefault(ILogFileColumnDescriptor column, int destinationIndex, int length)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.FillDefault(column, destinationIndex, length);
		}

		ILogEntry ILogEntries.this[int index]
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
		public IReadOnlyList<ILogFileColumnDescriptor> Columns
		{
			get { return _columns; }
		}

		/// <inheritdoc />
		public bool Contains(ILogFileColumnDescriptor column)
		{
			return _columns.Contains(column);
		}

		/// <inheritdoc />
		public void CopyTo<T>(ILogFileColumnDescriptor<T> column, int sourceIndex, T[] destination, int destinationIndex, int length)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyTo(column, sourceIndex, destination, destinationIndex, length);
		}

		/// <inheritdoc />
		public void CopyTo<T>(ILogFileColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, T[] destination, int destinationIndex)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyTo(column, sourceIndices, destination, destinationIndex);
		}

		/// <inheritdoc />
		public void CopyTo<T>(ILogFileColumnDescriptor<T> column, IReadOnlyList<int> sourceIndices, IList<T> destination, int destinationIndex)
		{
			if (!_columns.Contains(column))
				throw new NoSuchColumnException(column);

			_inner.CopyTo(column, sourceIndices, destination, destinationIndex);
		}

		#endregion
	}
}