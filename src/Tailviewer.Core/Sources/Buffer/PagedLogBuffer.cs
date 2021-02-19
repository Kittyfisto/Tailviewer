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
		private readonly int _maxPageCount;
		private readonly IReadOnlyList<IColumnDescriptor> _allColumns;
		private readonly IReadOnlyList<IColumnDescriptor> _cachedColumns;
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
			_maxPageCount = maxPageCount;
			_allColumns = new []{GeneralColumns.Index}.Concat(columns).Distinct().ToList(); //< This buffer only makes sense when it stores the index column, as otherwise one just doesn't know which data is present
			_cachedColumns = _allColumns.Except(new []{GeneralColumns.Index}).ToList();
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
			if (count < _sourceCount)
			{
				EvictFromOnward(count);
			}

			_sourceCount = count;
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
			return TryGetEntries(sourceIndices, destination, destinationIndex, out _);
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
		/// <param name="accessedPageBoundaries"></param>
		/// <returns>True when *all* entries could be retrieved from this buffer, false otherwise</returns>
		public bool TryGetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                          ILogBuffer destination,
		                          int destinationIndex,
		                          out IReadOnlyList<LogFileSection> accessedPageBoundaries)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex), destinationIndex, "The destination index must 0 or greater");

			if (sourceIndices is LogFileSection contiguousSection)
			{
				return TryGetEntriesContiguous(contiguousSection, destination, destinationIndex, out accessedPageBoundaries);
			}

			return TryGetEntriesSegmented(sourceIndices, destination, destinationIndex, out accessedPageBoundaries);
		}

		/// <summary>
		///    Adds data from the given source to this cache.
		/// </summary>
		/// <param name="destinationSection">The destination indices of the log entries from the given source</param>
		/// <param name="source">The source from which to copy data to this buffer</param>
		/// <param name="sourceIndex">The index into the given source from which onwards log entries may be added to this cache.</param>
		public void TryAdd(LogFileSection destinationSection, IReadOnlyLogBuffer source, int sourceIndex)
		{
			if (!CanCache(source))
				return;

			// We want to make sure to only read that data, which is actually covered by the log source.
			// This is because one thread might resize this buffer to a smaller size and then another thread
			// might try to add data which has been previously read, but is now no longer supposed to be part of the source.
			var sourceSectionEndIndex = Math.Min((int)(destinationSection.Index + destinationSection.Count), _sourceCount);
			for (LogLineIndex i = destinationSection.Index; i < sourceSectionEndIndex;)
			{
				var pageIndex = GetPageIndex(i);
				var page = GetOrCreatePageByIndex(pageIndex);

				var remainingPageCount = (pageIndex + 1) * _pageSize - i;
				var count = Math.Min(remainingPageCount, sourceSectionEndIndex- i);
				var sourceStartIndex = (i - destinationSection.Index) + sourceIndex;

				page.Add(sourceStartIndex, count, source, i);

				// We want the next copy operation to start at the beginning of the next page (it's a contiguous write after all)
				i += remainingPageCount;
			}
		}

		private bool CanCache(IReadOnlyLogBuffer source)
		{
			var sourceColumns = source.Columns;
			foreach(var cachedColumn in _cachedColumns)
			{
				if (!sourceColumns.Contains(cachedColumn))
					return false;
			}

			return true;
		}

		private bool TryGetEntriesContiguous(LogFileSection sourceSection, ILogBuffer destination, int destinationIndex, out IReadOnlyList<LogFileSection> accessedPageBoundaries)
		{
			bool fullyRead = true;
			var sourceSectionEndIndex = Math.Min((int)(sourceSection.Index + sourceSection.Count), _sourceCount);
			var numEntriesRead = 0;
			var tmpAccessedPageBoundaries = new List<LogFileSection>();

			for (LogLineIndex i = sourceSection.Index; i < sourceSectionEndIndex;)
			{
				var pageIndex = GetPageIndex(i);
				var remainingPageCount = (pageIndex + 1) * _pageSize - i;
				var count = Math.Min(remainingPageCount, sourceSectionEndIndex- i);

				var page = TryGetPage(pageIndex);
				if (page != null)
				{
					fullyRead &= page.TryRead(i, count, destination, destinationIndex + numEntriesRead, fullyRead);
					tmpAccessedPageBoundaries.Add(page.Section);
				}
				else
				{
					destination.FillDefault(destinationIndex + numEntriesRead, count);
					fullyRead = false;
					tmpAccessedPageBoundaries.Add(GetSectionForPage(pageIndex));
				}

				numEntriesRead += count;
				i += count;
			}

			if (numEntriesRead < sourceSection.Count)
			{
				var start = destinationIndex + numEntriesRead;
				destination.FillDefault(start, sourceSection.Count - numEntriesRead);
				fullyRead = false;
			}

			accessedPageBoundaries = tmpAccessedPageBoundaries;
			return fullyRead;
		}

		private bool TryGetEntriesSegmented(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, out IReadOnlyList<LogFileSection> accessedPageBoundaries)
		{
			var tmpAccessedPageBoundaries = new List<LogFileSection>();

			bool fullyRead = true;
			for (int i = 0; i < sourceIndices.Count; ++i)
			{
				var sourceIndex = sourceIndices[i];
				var pageIndex = GetPageIndex(sourceIndex);
				var page = TryGetPage(pageIndex);
				if (page != null)
				{
					fullyRead &= page.TryRead(sourceIndex, 1, destination, destinationIndex + i, fullyRead);
					tmpAccessedPageBoundaries.Add(page.Section);
				}
				else
				{
					destination.FillDefault(destinationIndex, 1);
					fullyRead = false;
					tmpAccessedPageBoundaries.Add(GetSectionForPage(pageIndex));
				}
			}

			accessedPageBoundaries = tmpAccessedPageBoundaries;
			return fullyRead;
		}

		[Pure]
		private LogFileSection GetSectionForPage(int pageIndex)
		{
			return new LogFileSection(_pageSize * pageIndex, _pageSize);
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
				page = new Page(pageIndex, _pageSize, _allColumns, _cachedColumns);
				if (_pages.Count >= _maxPageCount)
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

		private void EvictFromOnward(LogLineIndex evictionStartIndex)
		{
			// The section of log sources *actually* currently buffered
			// by pages is incredibly small compared to the section which
			// may be invalidated. Therefore it is faster to iterate over all pages
			// (which are only ever a few handful) and see if they are part of
			// the evicted region
			for (int i = 0; i < _pages.Count;)
			{
				var page = _pages[i];
				if (page.Section.Index >= evictionStartIndex)
				{
					// Fully evict the page
					_pages.RemoveAt(i);
				}
				else
				{
					if (page.Section.Index + _pageSize >= evictionStartIndex)
					{
						// Partially evict the page
						page.EvictFromOnward(evictionStartIndex);
					}

					++i;
				}
			}
		}
	}
}