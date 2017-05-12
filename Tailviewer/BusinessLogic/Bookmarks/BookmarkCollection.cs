using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Bookmarks
{
	internal sealed class BookmarkCollection
		: ILogFileListener
		, IDisposable
	{
		private readonly Dictionary<LogLineIndex, Bookmark> _bookmarks;
		private readonly ILogFile _logFile;
		private readonly object _syncRoot;
		private int _logLineCount;

		public BookmarkCollection(ILogFile logFile, TimeSpan maximumWaitTime)
		{
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));

			_syncRoot = new object();
			_bookmarks = new Dictionary<LogLineIndex, Bookmark>();

			_logFile = logFile;
			_logFile.AddListener(this, maximumWaitTime, 10000);
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			lock (_syncRoot)
			{
				if (section.IsReset)
				{
					_bookmarks.Clear();
				}

				_logLineCount = logFile.Count;
			}
		}

		public IReadOnlyList<Bookmark> Bookmarks
		{
			get
			{
				lock (_syncRoot)
				{
					return _bookmarks.Values.ToList();
				}
			}
		}

		public int Count
		{
			get
			{
				lock (_syncRoot)
				{
					return _bookmarks.Count;
				}
			}
		}

		public Bookmark TryAddBookmark(LogLineIndex logLineIndex)
		{
			lock (_syncRoot)
			{
				if (logLineIndex >= _logLineCount)
					return null;

				if (_bookmarks.ContainsKey(logLineIndex))
					return null;

				var bookmark = new Bookmark(logLineIndex);
				_bookmarks.Add(logLineIndex, bookmark);
				return bookmark;
			}
		}

		public void Dispose()
		{
			_logFile.RemoveListener(this);
		}

		public void RemoveBookmark(Bookmark bookmark)
		{
			if (bookmark == null)
				return;

			lock (_syncRoot)
			{
				_bookmarks.Remove(bookmark.Index);
			}
		}

		[Pure]
		public bool Contains(LogLineIndex index)
		{
			lock (_syncRoot)
			{
				return _bookmarks.ContainsKey(index);
			}
		}

		[Pure]
		public bool ContainsAll(IEnumerable<LogLineIndex> indices)
		{
			lock (_syncRoot)
			{
				foreach (var index in indices)
				{
					if (!_bookmarks.ContainsKey(index))
						return false;
				}

				return true;
			}
		}
	}
}