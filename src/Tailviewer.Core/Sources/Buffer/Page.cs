using System;
using System.Collections.Generic;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	internal sealed class Page
	{
		private readonly int _index;
		private readonly int _pageSize;
		private readonly LogBufferArray _buffer;
		private readonly IReadOnlyList<IColumnDescriptor> _copiedColumns;
		private readonly LogSourceSection _section;
		private DateTime? _lastAccessTime;
		private int _numReads;

		public Page(int index, int pageSize, IReadOnlyList<IColumnDescriptor> columns, IReadOnlyList<IColumnDescriptor> copiedColumns)
		{
			_index = index;
			_pageSize = pageSize;
			_copiedColumns = copiedColumns;
			_section = new LogSourceSection(index * pageSize, pageSize);
			_buffer = new LogBufferArray(pageSize, columns);
			_lastAccessTime = DateTime.MinValue;
			_numReads = 0;

			_buffer.Fill(PageBufferedLogSource.RetrievalState, RetrievalState.NotInSource, 0, _pageSize);
		}

		public int NumReads => _numReads;

		public int Index => _index;

		public LogSourceSection Section => _section;

		public void Add(int sourceIndex, int count, IReadOnlyLogBuffer source, LogLineIndex destinationIndex)
		{
			// Let us see where that data lies in this page..
			var pageDestinationIndex = destinationIndex - _section.Index;
			foreach (var column in _copiedColumns)
			{
				_buffer.CopyFrom(column, pageDestinationIndex, source, new Int32Range(sourceIndex, count));
			}

			// We neither need, nor want the source buffer to have to supply the indices - we know them ourselves
			_buffer.CopyFrom(Columns.Index, pageDestinationIndex, new LogSourceSection(_section.Index + pageDestinationIndex, count), 0, count);
			_buffer.Fill(PageBufferedLogSource.RetrievalState, RetrievalState.Retrieved, pageDestinationIndex, count);
		}

		public bool TryRead(LogLineIndex sourceStartIndex, int count, ILogBuffer destination, int destinationIndex, bool requiresValidityCheck)
		{
			++_numReads;
			_lastAccessTime = DateTime.UtcNow;

			var pageSourceIndex = sourceStartIndex - _section.Index;
			var range = new Int32Range(pageSourceIndex, count);
			foreach (var column in _buffer.Columns)
			{
				if (destination.Contains(column))
				{
					destination.CopyFrom(column, destinationIndex, _buffer, range);
				}
			}

			if (requiresValidityCheck)
			{
				if (_buffer.ContainsAnyDefault(Columns.Index, range))
					return false;
			}

			return true;
		}

		public void EvictFromOnward(LogLineIndex evictionStartIndex)
		{
			var offset = evictionStartIndex - _section.Index;
			var count = _pageSize - offset;
			_buffer.FillDefault(offset, count);
			_buffer.Fill(PageBufferedLogSource.RetrievalState, RetrievalState.NotInSource, offset, count);
		}

		public void FillTo(int sourceCount)
		{
			// We have to determine ourselves how much the page needs to be filled.
			// It's possible (nay, likely) that the source covers more than this entire page,
			// so we will have to pay attention to not fill too much
			var count = Math.Min(sourceCount - _section.Index, _pageSize);
			_buffer.Fill(PageBufferedLogSource.RetrievalState, RetrievalState.NotCached, 0, count);
		}

		public void SetSourceSize(int count)
		{
			// We need to check how much of the source has become available and especially how many entries that is
			// into our page (it might still only be a partial page):
			var countInPage = Math.Min(count - _section.Index, _pageSize);

			// Alright, so we have to fill those entries where we previously said "NotInSource" with "NotInCache", because
			// those entries just became part of the source (albeit have not been cached, hence the change in state).
			for (int i = 0; i < countInPage; ++i)
			{
				var entry = _buffer[i];
				if (entry.GetValue(PageBufferedLogSource.RetrievalState) == RetrievalState.NotInSource)
				{
					entry.SetValue(PageBufferedLogSource.RetrievalState, RetrievalState.NotCached);
				}
			}
		}
	}
}