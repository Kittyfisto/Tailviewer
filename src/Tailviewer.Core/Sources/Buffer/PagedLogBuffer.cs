using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Sources.Buffer
{
	/// <summary>
	///    Responsible for holding segments of a log source in memory.
	/// </summary>
	internal sealed class PagedLogBuffer
	{
		private readonly int _pageSize;
		private readonly IReadOnlyList<IColumnDescriptor> _columns;
		private readonly List<Page> _pages;
		private int _sourceCount;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageSize"></param>
		/// <param name="maxPageCount"></param>
		/// <param name="columns"></param>
		public PagedLogBuffer(int pageSize, int maxPageCount, params IColumnDescriptor[] columns)
			: this (pageSize, maxPageCount, (IReadOnlyList<IColumnDescriptor>)columns)
		{ }

		/// <summary>
		/// </summary>
		/// <param name="pageSize">The number of log entries to store per page</param>
		/// <param name="maxPageCount"></param>
		/// <param name="columns"></param>
		public PagedLogBuffer(int pageSize, int maxPageCount, IReadOnlyList<IColumnDescriptor> columns)
		{
			if (pageSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "The page size must be greater than 0 log entries");
			if (maxPageCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxPageCount), maxPageCount, "The maximum number of pages must be greater than 0");

			_pageSize = pageSize;
			_columns = new []{GeneralColumns.Index}.Concat(columns).Distinct().ToList(); //< This buffer only makes sense when it stores the index column, as otherwise one just doesn't know which data is present
			_pages = new List<Page>(maxPageCount);
			_sourceCount = 0;
		}

		/// <summary>
		///     Clears the buffer of any and all data.
		/// </summary>
		public void Clear()
		{
			_sourceCount = 0;
			_pages.Clear();
		}

		/// <summary>
		///     Tells the buffer to assume that the log source has the given number of log entries.
		/// </summary>
		/// <remarks>
		///     This buffer needs this information so the buffer can fill the request if a request's buffer
		///     with default data, in case the log source just doesn't provide anymore.
		/// </remarks>
		/// <param name="count"></param>
		public void ResizeTo(int count)
		{
			_sourceCount = count;
			// TODO: Find pages that covered the previous region and evict them
		}

		/// <summary>
		///     Tries to retrieve the given entries from this buffer.
		/// </summary>
		/// <remarks>
		///     This method will fill the index column of the given buffer (if it has one) with the indices of the log entries,
		///     or <see cref="LogLineIndex.Invalid"/> in case the data isn't part of the cache or it outside of the valid section
		///     of the log source.
		/// </remarks>
		/// <param name="sourceIndices"></param>
		/// <param name="destination"></param>
		/// <param name="destinationIndex"></param>
		/// <returns>True when *all* entries could be retrieved from this buffer, false otherwise</returns>
		public bool TryGetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                          ILogBuffer destination,
		                          int destinationIndex)
		{
			if (sourceIndices is LogFileSection contiguousSection)
			{
				return TryGetEntriesContiguous(contiguousSection, destination, destinationIndex);
			}

			return TryGetEntriesSegmented(sourceIndices, destination, destinationIndex);
		}

		/// <summary>
		///    Adds data from the given source to this cache.
		/// </summary>
		/// <param name="destinationIndices">The destination indices of the log entries from the given source</param>
		/// <param name="source">The source from which to copy data to this buffer</param>
		/// <param name="sourceIndex">The index into the given source from which onwards log entries may be added to this cache.</param>
		public void Add(IReadOnlyList<LogLineIndex> destinationIndices, IReadOnlyLogBuffer source, int sourceIndex)
		{
			if (destinationIndices is LogFileSection contiguousSection)
			{
				AddContiguous(contiguousSection, source, sourceIndex);
			}
			else
			{
				AddSegmented(destinationIndices, source, sourceIndex);
			}
		}

		private void AddContiguous(LogFileSection contiguousSection, IReadOnlyLogBuffer source, int sourceIndex)
		{
			// TODO: What strategy should be choose when the data being entered is bigger than the entire cache?

			// We want to make sure to only read that data, which is actually covered by the log source.
			// This is because one thread might resize this buffer to a smaller size and then another thread
			// might try to add data which has been previously read, but is now no longer supposed to be part of the source.
			var sourceSectionEndIndex = Math.Min((int)(contiguousSection.Index + contiguousSection.Count), _sourceCount);
			for (LogLineIndex i = contiguousSection.Index; i < sourceSectionEndIndex;)
			{
				var pageIndex = GetPageIndex(i);
				var page = GetOrCreatePageByIndex(pageIndex);

				var remainingPageCount = (pageIndex + 1) * _pageSize - i;
				var count = Math.Min(remainingPageCount, sourceSectionEndIndex- i);
				var sourceStartIndex = (i - contiguousSection.Index) + sourceIndex;

				page.Add(sourceStartIndex, count, source, i);

				// We want the next copy operation to start at the beginning of the next page (it's a contiguous write after all)
				i += remainingPageCount;
			}
		}

		private void AddSegmented(IReadOnlyList<LogLineIndex> destinationIndices, IReadOnlyLogBuffer source, int sourceIndex)
		{
			// TODO: What strategy should be choose when the data being entered is bigger than the entire cache?
			throw new NotImplementedException();
		}

		private bool TryGetEntriesContiguous(LogFileSection sourceSection, ILogBuffer destination, int destinationIndex)
		{
			bool fullyRead = true;
			var sourceSectionEndIndex = Math.Min((int)(sourceSection.Index + sourceSection.Count), _sourceCount);
			for (LogLineIndex i = sourceSection.Index; i < sourceSectionEndIndex; i += _pageSize)
			{
				var pageIndex = GetPageIndex(i);
				var remainingPageCount = (pageIndex + 1) * _pageSize - i;
				var count = Math.Min(remainingPageCount, sourceSectionEndIndex- i);

				var page = TryGetPage(pageIndex);
				if (page != null)
				{
					fullyRead |= page.TryRead(i, count, destination, destinationIndex);
				}
				else
				{
					destination.FillDefault(destinationIndex, count);
					fullyRead = false;
				}
			}

			if (sourceSectionEndIndex < sourceSection.Index + sourceSection.Count)
			{
				var start = (int)(sourceSection.Index + destinationIndex) - sourceSectionEndIndex;
				destination.FillDefault(start, sourceSection.Index + sourceSection.Count - sourceSectionEndIndex);
				fullyRead = false;
			}

			return fullyRead;
		}

		private bool TryGetEntriesSegmented(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex)
		{
			throw new NotImplementedException();
		}

		[Pure]
		private int GetPageIndex(LogLineIndex logLineIndex)
		{
			return (int)logLineIndex / _pageSize;
		}

		/// <summary>
		///    Returns the page which contains the given page index or creates one if it doesn't exist.
		/// </summary>
		/// <param name="pageIndex"></param>
		private Page GetOrCreatePageByIndex(int pageIndex)
		{
			var page = TryGetPage(pageIndex);
			if (page == null)
			{
				
				page = new Page(pageIndex, _pageSize, _columns);
				if (_pages.Count >= _pageSize)
				{
					// TODO: Find a better eviction strategy, maybe evict pages with the smallest read count?
					_pages.RemoveAt(0);
				}
				_pages.Add(page);
			}

			return page;
		}

		[Pure]
		private Page TryGetPage(int pageIndex)
		{
			foreach (var page in _pages)
			{
				if (page.Index == pageIndex)
					return page;
			}

			return null;
		}
	}
}