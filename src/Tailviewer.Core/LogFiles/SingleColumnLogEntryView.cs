using System.Collections;
using System.Collections.Generic;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class SingleColumnLogEntryView<T>
		: ILogEntries
	{
		private readonly IColumnDescriptor<T> _column;
		private readonly T[] _buffer;
		private readonly int _offset;
		private readonly int _count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="column"></param>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public SingleColumnLogEntryView(IColumnDescriptor<T> column, T[] buffer, int offset, int count)
		{
			_column = column;
			_buffer = buffer;
			_offset = offset;
			_count = count;
		}

		#region Implementation of IEnumerable

		IEnumerator<ILogEntry> IEnumerable<ILogEntry>.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		IEnumerator<IReadOnlyLogEntry> IEnumerable<IReadOnlyLogEntry>.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		#endregion

		#region Implementation of IReadOnlyCollection<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public int Count
		{
			get { throw new System.NotImplementedException(); }
		}

		#endregion

		#region Implementation of IReadOnlyList<out IReadOnlyLogEntry>

		/// <inheritdoc />
		public void CopyFrom<TColumn>(IColumnDescriptor<TColumn> column, int destinationIndex, TColumn[] source, int sourceIndex, int length)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column,
		                     int destinationIndex,
		                     ILogFile source,
		                     IReadOnlyList<LogLineIndex> sourceIndices,
		                     LogFileQueryOptions queryOptions)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyFrom(IColumnDescriptor column, int destinationIndex, IReadOnlyLogEntries source, IReadOnlyList<int> sourceIndices)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void FillDefault(int destinationIndex, int length)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void FillDefault(IColumnDescriptor column, int destinationIndex, int length)
		{
			throw new System.NotImplementedException();
		}

		ILogEntry ILogEntries.this[int index]
		{
			get { throw new System.NotImplementedException(); }
		}

		IReadOnlyLogEntry IReadOnlyList<IReadOnlyLogEntry>.this[int index]
		{
			get { throw new System.NotImplementedException(); }
		}

		#endregion

		#region Implementation of IReadOnlyLogEntries

		/// <inheritdoc />
		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get { throw new System.NotImplementedException(); }
		}

		/// <inheritdoc />
		public bool Contains(IColumnDescriptor column)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyTo<TColumn>(IColumnDescriptor<TColumn> column, int sourceIndex, TColumn[] destination, int destinationIndex, int length)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyTo<TColumn>(IColumnDescriptor<TColumn> column, IReadOnlyList<int> sourceIndices, TColumn[] destination, int destinationIndex)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyTo<TColumn>(IColumnDescriptor<TColumn> column, IReadOnlyList<int> sourceIndices, IList<TColumn> destination, int destinationIndex)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
