using System;
using System.Collections.Generic;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Sources.Buffer
{
	internal sealed class Page
	{
		private readonly int _index;
		private readonly int _pageSize;
		private readonly LogBufferArray _buffer;
		private readonly IReadOnlyList<IColumnDescriptor> _copiedColumns;
		private readonly LogFileSection _section;
		private DateTime? _lastAccessTime;
		private int _numReads;

		public Page(int index, int pageSize, IReadOnlyList<IColumnDescriptor> columns, IReadOnlyList<IColumnDescriptor> copiedColumns)
		{
			_index = index;
			_pageSize = pageSize;
			_copiedColumns = copiedColumns;
			_section = new LogFileSection(index * pageSize, pageSize);
			_buffer = new LogBufferArray(pageSize, columns);
			_lastAccessTime = DateTime.MinValue;
			_numReads = 0;
		}

		public int NumReads => _numReads;

		public int Index => _index;

		public LogFileSection Section => _section;

		public void Add(int sourceIndex, int count, IReadOnlyLogBuffer source, LogLineIndex destinationIndex)
		{
			// Let us see where that data lies in this page..
			var pageDestinationIndex = destinationIndex - _section.Index;
			foreach (var column in _copiedColumns)
			{
				_buffer.CopyFrom(column, pageDestinationIndex, source, new Int32Range(sourceIndex, count));
			}

			// We neither need, nor want the source buffer to have to supply the indices - we know them ourselves
			_buffer.CopyFrom(GeneralColumns.Index, pageDestinationIndex, new LogFileSection(_section.Index + pageDestinationIndex, count), 0, count);
		}

		public bool TryRead(LogLineIndex sourceStartIndex, int count, ILogBuffer destination, int destinationIndex, bool requiresValidityCheck)
		{
			var pageSourceIndex = sourceStartIndex - _section.Index;
			var range = new Int32Range(pageSourceIndex, count);
			foreach (var column in _buffer.Columns)
			{
				destination.CopyFrom(column, destinationIndex, _buffer, range);
			}

			if (requiresValidityCheck)
			{
				if (_buffer.ContainsAnyDefault(GeneralColumns.Index, range))
					return false;
			}

			return true;
		}

		public void EvictFromOnward(LogLineIndex evictionStartIndex)
		{
			var offset = evictionStartIndex - _section.Index;
			var count = _pageSize - offset;
			_buffer.FillDefault(offset, count);
		}
	}
}