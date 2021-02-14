using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings.Bookmarks;

namespace Tailviewer.BusinessLogic.Bookmarks
{
	public sealed class BookmarkCollection
		: ILogFileListener
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly TimeSpan _maximumWaitTime;
		private readonly Dictionary<Bookmark, BookmarkSettings> _bookmarks;
		private IReadOnlyList<Bookmark> _roBookmarks;
		private readonly Dictionary<ILogFile, IDataSource> _dataSourcesByLogFile;
		private readonly object _syncRoot;
		private readonly IBookmarks _settings;

		public BookmarkCollection(IBookmarks bookmarks,
		                          TimeSpan maximumWaitTime)
		{
			_settings = bookmarks ?? throw new ArgumentNullException(nameof(bookmarks));
			_maximumWaitTime = maximumWaitTime;
			_syncRoot = new object();
			_dataSourcesByLogFile = new Dictionary<ILogFile, IDataSource>();
			_bookmarks = new Dictionary<Bookmark, BookmarkSettings>();
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

					foreach (var setting in _settings.All.Where(x => x.DataSourceId == dataSource.Id))
					{
						var bookmark = new Bookmark(dataSource, setting.Index);
						if (_bookmarks.ContainsKey(bookmark))
						{
							Log.WarnFormat("Skipping duplicate {0}!", bookmark); // bookmark.ToString() already prints "Bookmark at .... " hence the strange log statement
						}
						else
						{
							_bookmarks.Add(bookmark, setting);
						}
					}

					Update();
				}
			}
		}

		public void RemoveDataSource(IDataSource dataSource)
		{
			var logFile = dataSource.UnfilteredLogFile;
			logFile.RemoveListener(this);

			lock (_syncRoot)
			{
				if (_dataSourcesByLogFile.Remove(logFile))
				{
					Remove(_bookmarks.Keys.Where(x => x.DataSource == dataSource).ToList());
				}
			}
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			lock (_syncRoot)
			{
				if (!_dataSourcesByLogFile.ContainsKey(logFile))
					return;

				if (!section.IsReset)
					return;

				var toRemove = _bookmarks.Keys.Where(x => x.DataSource.UnfilteredLogFile == logFile).ToList();
				Remove(toRemove);
				Update();
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

				if (logLineIndex >= logFile.GetProperty(Properties.LogEntryCount))
					return null;

				Bookmark bookmark = new Bookmark(dataSource, logLineIndex);
				if (_bookmarks.ContainsKey(bookmark))
					return null;

				var settings = bookmark.CreateSetting();
				_bookmarks.Add(bookmark, settings);
				_settings.Add(settings);
				Update();

				_settings.SaveAsync();

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
				if (!_bookmarks.Remove(bookmark))
					return;

				Update();
				_settings.SaveAsync();
			}
		}

		[Pure]
		public bool Contains(IDataSource dataSource, LogLineIndex index)
		{
			lock (_syncRoot)
			{
				return _bookmarks.ContainsKey(new Bookmark(dataSource, index));
			}
		}

		private void Remove(IReadOnlyCollection<Bookmark> toList)
		{
			var bookmarksSettings = new List<BookmarkSettings>();
			foreach (var bookmark in toList)
			{
				if (_bookmarks.TryGetValue(bookmark, out var settings))
				{
					bookmarksSettings.Add(settings);
					_bookmarks.Remove(bookmark);
				}
			}
			_settings.Remove(bookmarksSettings);

			Update();
		}

		private void Update()
		{
			_roBookmarks = _bookmarks.Keys.ToList();
		}
	}
}