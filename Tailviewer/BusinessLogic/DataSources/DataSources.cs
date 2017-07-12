using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tailviewer.Settings;
using log4net;
using Tailviewer.BusinessLogic.Bookmarks;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class DataSources
		: IDataSources
		, IDisposable
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<IDataSource> _dataSources;
		private readonly TimeSpan _maximumWaitTime;
		private readonly IDataSourcesSettings _settings;
		private readonly object _syncRoot;
		private readonly ITaskScheduler _taskScheduler;
		private readonly BookmarkCollection _bookmarks;

		public DataSources(ITaskScheduler taskScheduler, IDataSourcesSettings settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			_taskScheduler = taskScheduler;
			_maximumWaitTime = TimeSpan.FromMilliseconds(100);
			_syncRoot = new object();
			_settings = settings;
			_bookmarks = new BookmarkCollection(_maximumWaitTime);
			_dataSources = new List<IDataSource>();
			foreach (DataSource dataSource in settings)
			{
				AddDataSource(dataSource);
			}

			foreach (IDataSource dataSource in _dataSources)
			{
				Guid parentId = dataSource.Settings.ParentId;
				if (parentId != Guid.Empty)
				{
					var parent = _dataSources.FirstOrDefault(x => x.Id == parentId) as MergedDataSource;
					if (parent != null)
					{
						parent.Add(dataSource);
					}
					else
					{
						Log.WarnFormat("DataSource '{0} ({1})' is assigned a parent '{2}' but we don't know that one",
						               dataSource.FullFileName,
						               dataSource.Id,
						               parentId);
						// We don't want the rest of the application having to deal with this.
						// Therefore we'll simply remove the parent link and treat this data
						// source as any other ungrouped one.
						dataSource.Settings.ParentId = Guid.Empty;
					}
				}
			}
		}

		public IDataSource this[int index] => _dataSources[index];

		#region Bookmarks

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="orignalLogLineIndex"></param>
		/// <returns></returns>
		public Bookmark TryAddBookmark(IDataSource dataSource, LogLineIndex orignalLogLineIndex)
		{
			return _bookmarks.TryAddBookmark(dataSource, orignalLogLineIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		public IReadOnlyList<Bookmark> Bookmarks => _bookmarks.Bookmarks;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bookmark"></param>
		public void RemoveBookmark(Bookmark bookmark)
		{
			_bookmarks.RemoveBookmark(bookmark);
		}

		#endregion

		public int Count
		{
			get
			{
				lock (_syncRoot)
				{
					return _dataSources.Count;
				}
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				foreach (IDataSource dataSource in _dataSources)
				{
					dataSource.Dispose();
				}
			}
		}

		public IEnumerator<IDataSource> GetEnumerator()
		{
			lock (_syncRoot)
			{
				return _dataSources.ToList().GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IDataSource AddDataSource(DataSource settings)
		{
			lock (_syncRoot)
			{
				AbstractDataSource dataSource;
				if (!string.IsNullOrEmpty(settings.File))
				{
					dataSource = new SingleDataSource(_taskScheduler, settings, _maximumWaitTime);
				}
				else
				{
					dataSource = new MergedDataSource(_taskScheduler, settings, _maximumWaitTime);
				}

				_dataSources.Add(dataSource);
				_bookmarks.AddDataSource(dataSource);
				return dataSource;
			}
		}

		public MergedDataSource AddGroup()
		{
			MergedDataSource dataSource;

			lock (_syncRoot)
			{
				var settings = new DataSource
					{
						Id = Guid.NewGuid()
					};
				_settings.Add(settings);
				dataSource = (MergedDataSource) AddDataSource(settings);
			}

			return dataSource;
		}

		public SingleDataSource AddDataSource(string fileName)
		{
			string fullFileName;
			string key = GetKey(fileName, out fullFileName);
			SingleDataSource dataSource;

			lock (_syncRoot)
			{
				dataSource =
					(SingleDataSource)
					_dataSources.FirstOrDefault(x => string.Equals(x.FullFileName, key, StringComparison.InvariantCultureIgnoreCase));
				if (dataSource == null)
				{
					var settings = new DataSource(fullFileName)
						{
							Id = Guid.NewGuid()
						};
					_settings.Add(settings);
					dataSource = (SingleDataSource) AddDataSource(settings);
				}
			}

			return dataSource;
		}

		private static string GetKey(string fileName, out string fullFileName)
		{
			fullFileName = Path.GetFullPath(fileName);
			string key = fullFileName.ToLower();
			return key;
		}

		public bool Remove(IDataSource dataSource)
		{
			lock (_syncRoot)
			{
				_settings.Remove(dataSource.Settings);
				if (_dataSources.Remove(dataSource))
				{
					_bookmarks.RemoveDataSource(dataSource);
					dataSource.Dispose();
					return true;
				}

				return false;
			}
		}
	}
}