using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Core.Filters;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.DataSources
{
	/// <summary>
	///     A data source which watches over a given folder and maintains a list
	///     of child data sources, one for each log file in that folder. This list
	///     is synchronized automatically with that folder.
	/// </summary>
	/// <remarks>
	///     Does not support manually adding / removing data sources. A user can, however,
	///     change the regular expression / wildcard filter used to select log files.
	/// </remarks>
	public sealed class FolderDataSource
		: IFolderDataSource
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private const char PatternSeparator = ';';

		private readonly Dictionary<IFileInfoAsync, SingleDataSource> _dataSources;
		private readonly MergedDataSource _mergedDataSource;
		private readonly IFilesystem _filesystem;
		private readonly ITaskScheduler _taskScheduler;
		private readonly ILogFileFactory _logFileFactory;
		private readonly DataSource _settings;
		private readonly object _syncRoot;
		private IFilesystemWatcher _watchdog;
		private Predicate<string> _filter;
		private int _unfilteredFileCount;

		public FolderDataSource(ITaskScheduler taskScheduler,
								ILogFileFactory logFileFactory,
								IFilesystem filesystem,
								DataSource settings)
			: this(taskScheduler, logFileFactory, filesystem, settings, TimeSpan.FromMilliseconds(value: 10))
		{
		}

		public FolderDataSource(ITaskScheduler taskScheduler,
								ILogFileFactory logFileFactory,
								IFilesystem filesystem,
								DataSource settings,
								TimeSpan maximumWaitTime)
		{
			_taskScheduler = taskScheduler;
			_logFileFactory = logFileFactory;
			_filesystem = filesystem;
			_settings = settings;
			_syncRoot = new object();
			_dataSources = new Dictionary<IFileInfoAsync, SingleDataSource>();
			_mergedDataSource = new MergedDataSource(taskScheduler, settings, maximumWaitTime);

			DoChange();
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_mergedDataSource.Dispose();
		}

		#endregion

		#region Implementation of IDataSource

		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _mergedDataSource.QuickFilterChain; }
			set { _mergedDataSource.QuickFilterChain = value; }
		}

		public ILogFile OriginalLogFile
		{
			get { return _mergedDataSource.OriginalLogFile; }
		}

		public ILogFile UnfilteredLogFile
		{
			get { return _mergedDataSource.UnfilteredLogFile; }
		}

		public ILogFile FilteredLogFile
		{
			get { return _mergedDataSource.FilteredLogFile; }
		}

		public ILogFileSearch Search
		{
			get { return _mergedDataSource.Search; }
		}

		public DateTime? LastModified
		{
			get { return _mergedDataSource.LastModified; }
		}

		public DateTime LastViewed
		{
			get { return _mergedDataSource.LastViewed; }
			set { _mergedDataSource.LastViewed = value; }
		}

		public string FullFileName
		{
			get { return _settings.LogFileFolderPath; }
		}

		public bool FollowTail
		{
			get { return _mergedDataSource.FollowTail; }
			set { _mergedDataSource.FollowTail = value; }
		}

		public bool ShowLineNumbers
		{
			get { return _mergedDataSource.ShowLineNumbers; }
			set { _mergedDataSource.ShowLineNumbers = value; }
		}

		public bool ShowDeltaTimes
		{
			get { return _mergedDataSource.ShowDeltaTimes; }
			set { _mergedDataSource.ShowDeltaTimes = value; }
		}

		public bool ShowElapsedTime
		{
			get { return _mergedDataSource.ShowElapsedTime; }
			set { _mergedDataSource.ShowElapsedTime = value; }
		}

		public string SearchTerm
		{
			get { return _mergedDataSource.SearchTerm; }
			set { _mergedDataSource.SearchTerm = value; }
		}

		public LevelFlags LevelFilter
		{
			get { return _mergedDataSource.LevelFilter; }
			set { _mergedDataSource.LevelFilter = value; }
		}

		public HashSet<LogLineIndex> SelectedLogLines
		{
			get { return _mergedDataSource.SelectedLogLines; }
			set { _mergedDataSource.SelectedLogLines = value; }
		}

		public LogLineIndex VisibleLogLine
		{
			get { return _mergedDataSource.VisibleLogLine; }
			set { _mergedDataSource.VisibleLogLine = value; }
		}

		public double HorizontalOffset
		{
			get { return _mergedDataSource.HorizontalOffset; }
			set { _mergedDataSource.HorizontalOffset = value; }
		}

		public DataSource Settings
		{
			get { return _mergedDataSource.Settings; }
		}

		public int TotalCount
		{
			get { return _mergedDataSource.TotalCount; }
		}

		public Size? FileSize
		{
			get { return _mergedDataSource.FileSize; }
		}

		public bool ColorByLevel
		{
			get { return _mergedDataSource.ColorByLevel; }
			set { _mergedDataSource.ColorByLevel = value; }
		}

		public bool HideEmptyLines
		{
			get { return _mergedDataSource.HideEmptyLines; }
			set { _mergedDataSource.HideEmptyLines = value; }
		}

		public bool IsSingleLine
		{
			get { return _mergedDataSource.IsSingleLine; }
			set { _mergedDataSource.IsSingleLine = value; }
		}

		public DataSourceId Id
		{
			get { return _mergedDataSource.Id; }
		}

		public DataSourceId ParentId
		{
			get { return _mergedDataSource.ParentId; }
		}

		public string CharacterCode
		{
			get { return _mergedDataSource.CharacterCode; }
			set { _mergedDataSource.CharacterCode = value; }
		}

		public int NoLevelCount
		{
			get { return _mergedDataSource.NoLevelCount; }
		}

		public int TraceCount
		{
			get { return _mergedDataSource.TraceCount; }
		}

		public int DebugCount
		{
			get { return _mergedDataSource.DebugCount; }
		}

		public int InfoCount
		{
			get { return _mergedDataSource.InfoCount; }
		}

		public int WarningCount
		{
			get { return _mergedDataSource.WarningCount; }
		}

		public int ErrorCount
		{
			get { return _mergedDataSource.ErrorCount; }
		}

		public int FatalCount
		{
			get { return _mergedDataSource.FatalCount; }
		}

		public int NoTimestampCount
		{
			get { return _mergedDataSource.NoTimestampCount; }
		}

		public void ActivateQuickFilter(QuickFilterId id)
		{
			_mergedDataSource.ActivateQuickFilter(id);
		}

		public bool DeactivateQuickFilter(QuickFilterId id)
		{
			return _mergedDataSource.DeactivateQuickFilter(id);
		}

		public bool IsQuickFilterActive(QuickFilterId id)
		{
			return _mergedDataSource.IsQuickFilterActive(id);
		}

		public void EnableAnalysis(AnalysisId id)
		{
			_mergedDataSource.EnableAnalysis(id);
		}

		public void DisableAnalysis(AnalysisId id)
		{
			_mergedDataSource.DisableAnalysis(id);
		}

		public bool IsAnalysisActive(AnalysisId id)
		{
			return _mergedDataSource.IsAnalysisActive(id);
		}

		#endregion

		#region Implementation of IMultiDataSource

		public bool IsExpanded
		{
			get { return _mergedDataSource.IsExpanded; }
			set { _mergedDataSource.IsExpanded = value; }
		}

		public DataSourceDisplayMode DisplayMode
		{
			get { return _mergedDataSource.DisplayMode; }
			set { _mergedDataSource.DisplayMode = value; }
		}

		public IReadOnlyList<IDataSource> OriginalSources
		{
			get { return _mergedDataSource.OriginalSources; }
		}

		#endregion

		#region Implementation of IFolderDataSource

		public string LogFileFolderPath
		{
			get { return _settings.LogFileFolderPath; }
		}

		public string LogFileSearchPattern
		{
			get { return _settings.LogFileSearchPattern; }
		}

		public bool Recursive
		{
			get { return _settings.Recursive; }
		}

		public int UnfilteredFileCount => _unfilteredFileCount;

		public int FilteredFileCount => OriginalSources.Count;

		public void Change(string folderPath, string searchPattern, bool recursive)
		{
			if (folderPath == LogFileFolderPath &&
				searchPattern == LogFileSearchPattern &&
				Recursive == recursive)
				return;

			_settings.LogFileFolderPath = folderPath;
			_settings.LogFileSearchPattern = searchPattern;
			_settings.Recursive = recursive;

			// TODO: Maybe we should somehow trigger a persist?

			DoChange();
		}

		private void DoChange()
		{
			_filter = CreateFilter(_settings.LogFileSearchPattern);
			_watchdog?.Dispose();
			_watchdog = _filesystem.Watchdog.StartDirectoryWatch(_settings.LogFileFolderPath,
			                                                     TimeSpan.FromMilliseconds(500),
			                                                     null,
			                                                     _settings.Recursive
				                                                     ? SearchOption.AllDirectories
				                                                     : SearchOption.TopDirectoryOnly);
			_watchdog.Changed += OnFolderChanged;
			OnFolderChanged();
		}

		#endregion

		private void OnFolderChanged()
		{
			var unfilteredFiles = _watchdog.Files;
			var files = FilterFiles(unfilteredFiles, _filter, out _unfilteredFileCount);
			var dataSources = SynchronizeDataSources(files);
			_mergedDataSource.SetDataSources(dataSources);
		}

		private IReadOnlyList<IDataSource> SynchronizeDataSources(IReadOnlyList<IFileInfoAsync> files)
		{
			var newFiles = new List<IFileInfoAsync>();
			var oldFiles = new List<IFileInfoAsync>();
			var dataSources = new List<IDataSource>();

			try
			{
				lock (_syncRoot)
				{
					foreach (var file in files)
					{
						if (!_dataSources.TryGetValue(file, out var dataSource))
						{
							var settings = new DataSource(file.FullPath)
							{
								Id = DataSourceId.CreateNew()
							};
							dataSource = new SingleDataSource(_logFileFactory,
															  _taskScheduler,
															  settings);
							_dataSources.Add(file, dataSource);
							newFiles.Add(file);

						}

						dataSources.Add(dataSource);
					}

					foreach (var file in _dataSources.Keys)
					{
						if (!files.Contains(file))
						{
							oldFiles.Add(file);
						}
					}

					foreach (var file in oldFiles)
					{
						_dataSources.TryGetValue(file, out var dataSource);
						_dataSources.Remove(file);
						dataSource?.Dispose();
					}
				}
			}
			catch (Exception)
			{
				foreach (var dataSource in dataSources)
				{
					dataSource.Dispose();
				}
				throw;
			}

			if (Log.IsDebugEnabled)
			{
				Log.DebugFormat("Adding #{0} files ({1}), removing #{2} files ({3})",
					newFiles.Count,
					string.Join(", ", newFiles.Select(x => x.FullPath)),
					oldFiles.Count,
					string.Join(", ", oldFiles.Select(x => x.FullPath)));
			}

			return dataSources;
		}

		[Pure]
		private static IReadOnlyList<IFileInfoAsync> FilterFiles(IEnumerable<IFileInfoAsync> files, Predicate<string> filter,
		                                                         out int unfilteredFileCount)
		{
			int count = 0;
			var matches = new List<IFileInfoAsync>();
			foreach (var file in files)
			{
				if (filter(file.Name))
					matches.Add(file);
				++count;
			}

			unfilteredFileCount = count;
			return matches;
		}

		[Pure]
		private static Predicate<string> CreateFilter(string searchPattern)
		{
			if (string.IsNullOrWhiteSpace(searchPattern))
				return _ => true;

			var patterns = ParseSearchPattern(searchPattern);
			var regexPattern = string.Join("|", patterns.Select(x => Regex.Escape(x).Replace("\\*", ".*?")));
			var regex = new Regex(regexPattern, RegexOptions.Compiled);
			return str => regex.IsMatch(str);
		}

		[Pure]
		private static IReadOnlyList<string> ParseSearchPattern(string value)
		{
			if (value == null)
				return new string[0];

			var values = new List<string>();
			foreach (var pattern in value.Split(PatternSeparator))
			{
				if (!string.IsNullOrWhiteSpace(pattern))
				{
					values.Add(pattern.Trim());
				}
			}

			return values;
		}
	}
}