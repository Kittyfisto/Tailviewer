using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tailviewer.Settings;
using log4net;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources.Custom;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Plugins;
using Tailviewer.Settings.Bookmarks;
using Tailviewer.Settings.CustomFormats;

namespace Tailviewer.BusinessLogic.DataSources
{
	public sealed class DataSources
		: IDataSources
		, IDisposable
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogSourceFactoryEx _logSourceFactory;
		private readonly ITaskScheduler _taskScheduler;
		private readonly List<IDataSource> _dataSources;
		private readonly HashSet<DataSourceId> _dataSourceIds;
		private readonly TimeSpan _maximumWaitTime;
		private readonly IDataSourcesSettings _settings;
		private readonly object _syncRoot;
		private readonly BookmarkCollection _bookmarks;
		private readonly IFilesystem _filesystem;

		public DataSources(ILogSourceFactoryEx logSourceFactory,
		                   ITaskScheduler taskScheduler,
						   IFilesystem filesystem,
		                   IDataSourcesSettings settings,
		                   IBookmarks bookmarks)
		{
			_logSourceFactory = logSourceFactory ?? throw new ArgumentNullException(nameof(logSourceFactory));
			_taskScheduler = taskScheduler;
			_filesystem = filesystem ?? throw new ArgumentNullException(nameof(filesystem));
			_maximumWaitTime = TimeSpan.FromMilliseconds(100);
			_syncRoot = new object();
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_bookmarks = new BookmarkCollection(bookmarks, _maximumWaitTime);
			_dataSources = new List<IDataSource>();
			_dataSourceIds = new HashSet<DataSourceId>();
			foreach (DataSource dataSource in settings)
			{
				AddDataSource(dataSource);
			}

			foreach (IDataSource dataSource in _dataSources)
			{
				DataSourceId parentId = dataSource.Settings.ParentId;
				if (parentId != DataSourceId.Empty)
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
						dataSource.Settings.ParentId = DataSourceId.Empty;
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

		public void ClearBookmarks()
		{
			_bookmarks.Clear();
		}

		public IReadOnlyList<ICustomDataSourcePlugin> CustomDataSources
		{
			get { return _logSourceFactory.CustomDataSources; }
		}

		public bool Contains(DataSourceId id)
		{
			return _dataSourceIds.Contains(id);
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

		public IReadOnlyList<IDataSource> Sources
		{
			get
			{
				lock (_syncRoot)
				{
					return _dataSources.ToList();
				}
			}
		}

		private IDataSource AddDataSource(DataSource settings)
		{
			lock (_syncRoot)
			{
				IDataSource dataSource;
				if (!string.IsNullOrEmpty(settings.LogFileFolderPath))
				{
					dataSource = new FolderDataSource(_taskScheduler, _logSourceFactory, _filesystem, settings);
				}
				else if (!string.IsNullOrEmpty(settings.File))
				{
					dataSource = new FileDataSource(_logSourceFactory, _taskScheduler, settings, _maximumWaitTime);
				}
				else if (settings.CustomDataSourceConfiguration != null)
				{
					dataSource = new CustomDataSource(_logSourceFactory, _taskScheduler, settings, _maximumWaitTime);
				}
				else
				{
					if (settings.DisplayName == null)
						settings.DisplayName = "Merged Data Source";

					dataSource = new MergedDataSource(_taskScheduler, settings, _maximumWaitTime);
				}

				_dataSources.Add(dataSource);
				_dataSourceIds.Add(dataSource.Id);
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
						Id = DataSourceId.CreateNew(),
						DisplayName = "Merged Data Source"
				};
				_settings.Add(settings);
				dataSource = (MergedDataSource) AddDataSource(settings);
			}

			return dataSource;
		}

		public CustomDataSource AddCustom(CustomDataSourceId id)
		{
			CustomDataSource dataSource;

			var plugin = _logSourceFactory.CustomDataSources.First(x => x.Id == id);

			lock (_syncRoot)
			{
				var settings = new DataSource
				{
					Id = DataSourceId.CreateNew(),
					DisplayName = plugin.DisplayName,
					CustomDataSourceId = plugin.Id,
					CustomDataSourceConfiguration = plugin.CreateConfiguration(null)
				};
				_settings.Add(settings);
				dataSource = (CustomDataSource) AddDataSource(settings);
			}

			return dataSource;
		}

		public FileDataSource AddFile(string fileName)
		{
			string key = GetKey(fileName, out var fullFileName);
			FileDataSource dataSource;

			lock (_syncRoot)
			{
				dataSource =
					(FileDataSource)
					_dataSources.FirstOrDefault(x => string.Equals(x.FullFileName, key, StringComparison.InvariantCultureIgnoreCase));
				if (dataSource == null)
				{
					var settings = new DataSource(fullFileName)
						{
							Id = DataSourceId.CreateNew()
						};
					_settings.Add(settings);
					dataSource = (FileDataSource) AddDataSource(settings);
				}
			}

			return dataSource;
		}



		public IFolderDataSource AddFolder(string folderPath)
		{
			string key = GetKey(folderPath, out var fullFolderPath);
			FolderDataSource dataSource;

			lock (_syncRoot)
			{
				dataSource =
					(FolderDataSource)
					_dataSources.FirstOrDefault(x => string.Equals(x.FullFileName, key, StringComparison.InvariantCultureIgnoreCase));
				if (dataSource == null)
				{
					var settings = new DataSource
					{
						Id = DataSourceId.CreateNew(),
						LogFileFolderPath = fullFolderPath,
						LogFileSearchPattern = _settings.FolderDataSourcePattern,
						Recursive = _settings.FolderDataSourceRecursive
					};
					_settings.Add(settings);
					dataSource = (FolderDataSource) AddDataSource(settings);
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
			bool removed;
			lock (_syncRoot)
			{
				removed = RemoveNoLock(dataSource);
			}

			GC.Collect(3, GCCollectionMode.Forced);
			return removed;
		}

		public void Clear()
		{
			lock (_syncRoot)
			{
				_settings.Clear();
				_dataSources.Clear();
			}

			GC.Collect(3, GCCollectionMode.Forced);
		}

		private bool RemoveNoLock(IDataSource dataSource)
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