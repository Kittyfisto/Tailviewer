using System;
using System.Collections.Generic;
using Tailviewer.Core.Buffers;

namespace Tailviewer.Core.Sources.Buffer
{
	internal sealed class Page
	{
		private readonly int _index;
		private readonly int _pageSize;
		private readonly LogBufferArray _buffer;
		private readonly LogFileSection _section;
		private DateTime? _lastAccessTime;
		private int _numReads;

		public Page(int index, int pageSize, IReadOnlyList<IColumnDescriptor> columns)
		{
			_index = index;
			_pageSize = pageSize;
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
			foreach (var column in _buffer.Columns)
			{
				_buffer.CopyFrom(column, pageDestinationIndex, source, new Int32Range(sourceIndex, count));
			}
		}

		public bool TryRead(LogLineIndex sourceStartIndex, int count, ILogBuffer destination, int destinationIndex)
		{
			var pageSourceIndex = sourceStartIndex - _section.Index;
			foreach (var column in _buffer.Columns)
			{
				destination.CopyFrom(column, destinationIndex, _buffer, new Int32Range(pageSourceIndex, count));
			}

			// TOOD: Determine if we've read invalid data

			return true;
		}
	}
}