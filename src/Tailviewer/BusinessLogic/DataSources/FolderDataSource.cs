using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
		private readonly MergedDataSource _dataSource;
		private readonly IFilesystem _filesystem;
		private readonly ILogFileFactory _logFileFactory;
		private readonly DataSource _settings;

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
			_logFileFactory = logFileFactory;
			_filesystem = filesystem;
			_settings = settings;
			_dataSource = new MergedDataSource(taskScheduler, settings, maximumWaitTime);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_dataSource.Dispose();
		}

		#endregion

		#region Implementation of IDataSource

		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _dataSource.QuickFilterChain; }
			set { _dataSource.QuickFilterChain = value; }
		}

		public ILogFile OriginalLogFile
		{
			get { return _dataSource.OriginalLogFile; }
		}

		public ILogFile UnfilteredLogFile
		{
			get { return _dataSource.UnfilteredLogFile; }
		}

		public ILogFile FilteredLogFile
		{
			get { return _dataSource.FilteredLogFile; }
		}

		public ILogFileSearch Search
		{
			get { return _dataSource.Search; }
		}

		public DateTime? LastModified
		{
			get { return _dataSource.LastModified; }
		}

		public DateTime LastViewed
		{
			get { return _dataSource.LastViewed; }
			set { _dataSource.LastViewed = value; }
		}

		public string FullFileName
		{
			get { return _dataSource.FullFileName; }
		}

		public bool FollowTail
		{
			get { return _dataSource.FollowTail; }
			set { _dataSource.FollowTail = value; }
		}

		public bool ShowLineNumbers
		{
			get { return _dataSource.ShowLineNumbers; }
			set { _dataSource.ShowLineNumbers = value; }
		}

		public bool ShowDeltaTimes
		{
			get { return _dataSource.ShowDeltaTimes; }
			set { _dataSource.ShowDeltaTimes = value; }
		}

		public bool ShowElapsedTime
		{
			get { return _dataSource.ShowElapsedTime; }
			set { _dataSource.ShowElapsedTime = value; }
		}

		public string SearchTerm
		{
			get { return _dataSource.SearchTerm; }
			set { _dataSource.SearchTerm = value; }
		}

		public LevelFlags LevelFilter
		{
			get { return _dataSource.LevelFilter; }
			set { _dataSource.LevelFilter = value; }
		}

		public HashSet<LogLineIndex> SelectedLogLines
		{
			get { return _dataSource.SelectedLogLines; }
			set { _dataSource.SelectedLogLines = value; }
		}

		public LogLineIndex VisibleLogLine
		{
			get { return _dataSource.VisibleLogLine; }
			set { _dataSource.VisibleLogLine = value; }
		}

		public double HorizontalOffset
		{
			get { return _dataSource.HorizontalOffset; }
			set { _dataSource.HorizontalOffset = value; }
		}

		public DataSource Settings
		{
			get { return _dataSource.Settings; }
		}

		public int TotalCount
		{
			get { return _dataSource.TotalCount; }
		}

		public Size? FileSize
		{
			get { return _dataSource.FileSize; }
		}

		public bool ColorByLevel
		{
			get { return _dataSource.ColorByLevel; }
			set { _dataSource.ColorByLevel = value; }
		}

		public bool HideEmptyLines
		{
			get { return _dataSource.HideEmptyLines; }
			set { _dataSource.HideEmptyLines = value; }
		}

		public bool IsSingleLine
		{
			get { return _dataSource.IsSingleLine; }
			set { _dataSource.IsSingleLine = value; }
		}

		public DataSourceId Id
		{
			get { return _dataSource.Id; }
		}

		public DataSourceId ParentId
		{
			get { return _dataSource.ParentId; }
		}

		public string CharacterCode
		{
			get { return _dataSource.CharacterCode; }
			set { _dataSource.CharacterCode = value; }
		}

		public int NoLevelCount
		{
			get { return _dataSource.NoLevelCount; }
		}

		public int TraceCount
		{
			get { return _dataSource.TraceCount; }
		}

		public int DebugCount
		{
			get { return _dataSource.DebugCount; }
		}

		public int InfoCount
		{
			get { return _dataSource.InfoCount; }
		}

		public int WarningCount
		{
			get { return _dataSource.WarningCount; }
		}

		public int ErrorCount
		{
			get { return _dataSource.ErrorCount; }
		}

		public int FatalCount
		{
			get { return _dataSource.FatalCount; }
		}

		public int NoTimestampCount
		{
			get { return _dataSource.NoTimestampCount; }
		}

		public void ActivateQuickFilter(QuickFilterId id)
		{
			_dataSource.ActivateQuickFilter(id);
		}

		public bool DeactivateQuickFilter(QuickFilterId id)
		{
			return _dataSource.DeactivateQuickFilter(id);
		}

		public bool IsQuickFilterActive(QuickFilterId id)
		{
			return _dataSource.IsQuickFilterActive(id);
		}

		public void EnableAnalysis(AnalysisId id)
		{
			_dataSource.EnableAnalysis(id);
		}

		public void DisableAnalysis(AnalysisId id)
		{
			_dataSource.DisableAnalysis(id);
		}

		public bool IsAnalysisActive(AnalysisId id)
		{
			return _dataSource.IsAnalysisActive(id);
		}

		#endregion

		#region Implementation of IMultiDataSource

		public bool IsExpanded
		{
			get { return _dataSource.IsExpanded; }
			set { _dataSource.IsExpanded = value; }
		}

		public DataSourceDisplayMode DisplayMode
		{
			get { return _dataSource.DisplayMode; }
			set { _dataSource.DisplayMode = value; }
		}

		public IReadOnlyList<IDataSource> OriginalSources
		{
			get { return _dataSource.OriginalSources; }
		}

		#endregion

		#region Implementation of IFolderDataSource

		public string LogFileFolderPath
		{
			get { return _settings.LogFileFolderPath; }
		}

		public string LogFileRegex
		{
			get { return _settings.LogFileRegex; }
		}

		public bool Recursive
		{
			get { return _settings.Recursive; }
		}

		public void Change(string folderPath, string logFileRegex, bool recursive)
		{
			if (folderPath == LogFileFolderPath &&
			    logFileRegex == LogFileRegex &&
			    Recursive == recursive)
				return;

			_settings.LogFileFolderPath = folderPath;
			_settings.LogFileRegex = logFileRegex;
			_settings.Recursive = recursive;

			// TODO: Maybe we should somehow trigger a persist?
			// TODO: Create / update watchdog
		}

		#endregion
	}
}