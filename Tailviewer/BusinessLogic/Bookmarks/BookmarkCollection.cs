using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Bookmarks
{
	internal sealed class BookmarkCollection
		: ILogFileListener
		, IDisposable
	{
		private readonly TimeSpan _maximumWaitTime;
		private readonly HashSet<Bookmark> _bookmarks;
		private IReadOnlyList<Bookmark> _roBookmarks;
		private readonly Dictionary<ILogFile, IDataSource> _dataSourcesByLogFile;
		private readonly object _syncRoot;

		public BookmarkCollection(TimeSpan maximumWaitTime)
		{
			_maximumWaitTime = maximumWaitTime;
			_syncRoot = new object();
			_dataSourcesByLogFile = new Dictionary<ILogFile, IDataSource>();
			_bookmarks = new HashSet<Bookmark>();
			Update();
		}

		public void AddDataSource(IDataSource dataSource)
		{
			lock (_syncRoot)
			{
				var logFile = dataSource.UnfilteredLogFile;
				if (!_dataSourcesByLogFile.ContainsKey(logFile))
				{
					_dataSourcesByLogFile.Add(logFile, dataSource);
					logFile.AddListener(this, _maximumWaitTime, 10000);
				}
			}
		}

		public void RemoveDataSource(IDataSource dataSource)
		{
			lock (_syncRoot)
			{
				var logFile = dataSource.UnfilteredLogFile;
				if (_dataSourcesByLogFile.Remove(logFile))
				{
					logFile.RemoveListener(this);
					_bookmarks.RemoveWhere(x => x.DataSource == dataSource);
				}
			}
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			lock (_syncRoot)
			{
				if (!_dataSourcesByLogFile.ContainsKey(logFile))
					return;

				if (section.IsReset)
				{
					_bookmarks.RemoveWhere(x => x.DataSource.UnfilteredLogFile == logFile);
					Update();
				}
			}
		}

		public IReadOnlyList<Bookmark> Bookmarks => _roBookmarks;

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

		public Bookmark TryAddBookmark(IDataSource dataSource, LogLineIndex logLineIndex)
		{
			lock (_syncRoot)
			{
				var logFile = dataSource?.UnfilteredLogFile;
				if (logFile == null)
					return null;

				if (!_dataSourcesByLogFile.ContainsKey(logFile))
					return null;

				if (logLineIndex >= logFile.Count)
					return null;

				var bookmark = new Bookmark(dataSource, logLineIndex);
				if (!_bookmarks.Add(bookmark))
					return null;

				Update();
				return bookmark;
			}
		}

		public void Dispose()
		{
			foreach (var dataSource in _dataSourcesByLogFile.Values.ToList())
			{
				RemoveDataSource(dataSource);
			}
		}

		public void RemoveBookmark(Bookmark bookmark)
		{
			if (bookmark == null)
				return;

			lock (_syncRoot)
			{
				if (_bookmarks.Remove(bookmark))
				{
					Update();
				}
			}
		}

		[Pure]
		public bool Contains(IDataSource dataSource, LogLineIndex index)
		{
			lock (_syncRoot)
			{
				return _bookmarks.Contains(new Bookmark(dataSource, index));
			}
		}

		private void Update()
		{
			_roBookmarks = _bookmarks.ToList();
		}
	}
}