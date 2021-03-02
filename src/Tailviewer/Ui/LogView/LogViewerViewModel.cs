using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using log4net;
using Metrolib;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Exporter;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.LogView
{
	public sealed class LogViewerViewModel
		: INotifyPropertyChanged
		  , ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDataSourceViewModel _dataSource;
		private readonly TimeSpan _maximumWaitTime;
		private readonly List<KeyValuePair<ILogSource, LogSourceModification>> _pendingSections;
		private readonly IActionCenter _actionCenter;
		private readonly IApplicationSettings _applicationSettings;

		private ILogSource _logSource;
		private int _logEntryCount;
		private string _noEntriesExplanation;
		private string _noEntriesAction;
		private Geometry _noEntriesIcon;
		private int _totalLogEntryCount;
		private ILogSourceSearch _search;

		public LogViewerViewModel(IDataSourceViewModel dataSource,
		                          IActionCenter actionCenter,
		                          IApplicationSettings applicationSettings,
		                          TimeSpan maximumWaitTime)
		{
			_actionCenter = actionCenter ?? throw new ArgumentNullException(nameof(actionCenter));
			_applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
			_maximumWaitTime = maximumWaitTime;
			_dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

			_pendingSections = new List<KeyValuePair<ILogSource, LogSourceModification>>();

			LogSource = _dataSource.DataSource.FilteredLogSource;
			LogSource.AddListener(this, _maximumWaitTime, 1000);
			Search = _dataSource.DataSource.Search;

			UpdateCounts();
		}

		public LogViewerViewModel(IDataSourceViewModel dataSource,
			IActionCenter actionCenter,
			IApplicationSettings applicationSettings)
			: this(dataSource, actionCenter, applicationSettings, TimeSpan.FromMilliseconds(10))
		{
		}

		public override string ToString()
		{
			return _logSource.ToString();
		}

		public ILogSource LogSource
		{
			get { return _logSource; }
			private set
			{
				if (value == _logSource)
					return;

				_logSource = value;
				EmitPropertyChanged();
			}
		}

		public ILogSourceSearch Search
		{
			get { return _search; }
			private set
			{
				if (value == _search)
					return;

				_search = value;
				EmitPropertyChanged();
			}
		}

		public Geometry NoEntriesIcon
		{
			get { return _noEntriesIcon; }
			private set
			{
				if (value == _noEntriesIcon)
					return;
				_noEntriesIcon = value;
				EmitPropertyChanged();
			}
		}

		public string NoEntriesAction
		{
			get { return _noEntriesAction; }
			private set
			{
				if (Equals(value, _noEntriesAction))
					return;

				_noEntriesAction = value;
				EmitPropertyChanged();
			}
		}

		public string NoEntriesExplanation
		{
			get { return _noEntriesExplanation; }
			private set
			{
				if (Equals(value, _noEntriesExplanation))
					return;

				_noEntriesExplanation = value;
				EmitPropertyChanged();
			}
		}

		public int LogEntryCount
		{
			get { return _logEntryCount; }
			private set
			{
				if (value == _logEntryCount)
					return;

				_logEntryCount = value;
				EmitPropertyChanged();
			}
		}

		public int TotalLogEntryCount
		{
			get { return _totalLogEntryCount; }
			private set
			{
				if (value == _totalLogEntryCount)
					return;

				_totalLogEntryCount = value;
				EmitPropertyChanged();
			}
		}

		public IDataSourceViewModel DataSource => _dataSource;

		/// <summary>
		///     The list of filters as produced by the "quick filter" panel.
		/// </summary>
		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get
			{
				return _dataSource?.QuickFilterChain;
			}
			set
			{
				if (value == QuickFilterChain)
					return;

				if (_dataSource != null)
				{
					_dataSource.QuickFilterChain = value;
				}
			}
		}

		public ICommand ExportToFileCommand => new DelegateCommand(ExportToFile);

		private void ExportToFile()
		{
			var viewModel = DataSource;
			var dataSource = viewModel?.DataSource;
			var logFile = dataSource?.FilteredLogSource;
			if (logFile == null)
			{
				Log.Warn("DataSource is null, cancelling export...");
				return;
			}

			var exportDirectory = _applicationSettings.Export.ExportFolder;
			var exporter = new LogFileToFileExporter(logFile,
				exportDirectory,
				dataSource.FullFileName
			);

			var action = new ExportAction(exporter,
				viewModel.DisplayName,
				exportDirectory);
			_actionCenter.Add(action);
		}

		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			lock (_pendingSections)
			{
				if (modification.IsReset())
					_pendingSections.Clear();

				_pendingSections.Add(new KeyValuePair<ILogSource, LogSourceModification>(_logSource, modification));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Update()
		{
			lock (_pendingSections)
			{
				foreach (var pair in _pendingSections)
				{
					ILogSource source = pair.Key;
					if (source != _logSource)
						continue; //< This message belongs to an old change and must be ignored

					LogEntryCount = 0;
					TotalLogEntryCount = 0;
				}

				_pendingSections.Clear();
			}

			UpdateCounts();
		}

		private void UpdateCounts()
		{
			LogEntryCount = _logSource.GetProperty(GeneralProperties.LogEntryCount);
			TotalLogEntryCount = _dataSource.DataSource.UnfilteredLogSource.GetProperty(GeneralProperties.LogEntryCount);
			UpdateNoEntriesExplanation();
		}

		private void UpdateNoEntriesExplanation()
		{
			IDataSource dataSource = _dataSource.DataSource;
			var folderDataSource = dataSource as IFolderDataSource;
			ILogSource source = dataSource.UnfilteredLogSource;
			ILogSource filtered = dataSource.FilteredLogSource;

			if (filtered.GetProperty(GeneralProperties.LogEntryCount) == 0)
			{
				ILogEntryFilter filter = dataSource.LogEntryFilter;
				var emptyReason = source.GetProperty(GeneralProperties.EmptyReason);
				if ((emptyReason & ErrorFlags.SourceDoesNotExist) == ErrorFlags.SourceDoesNotExist)
				{
					NoEntriesIcon = Icons.FileRemove;
					NoEntriesExplanation = "Data source does not exist";
					NoEntriesAction = $"The data source '{Path.GetFileName(dataSource.FullFileName)}' was last seen {Path.GetDirectoryName(dataSource.FullFileName)}";
				}
				else if ((emptyReason & ErrorFlags.SourceCannotBeAccessed) == ErrorFlags.SourceCannotBeAccessed)
				{
					NoEntriesIcon = Icons.FileAlert;
					NoEntriesExplanation = "Data source cannot be opened";
					NoEntriesAction = $"The file '{Path.GetFileName(dataSource.FullFileName)}' may be opened exclusively by another process or you are not authorized to view it";
				}
				else if (folderDataSource != null && folderDataSource.UnfilteredFileCount == 0)
				{
					NoEntriesIcon = null;
					NoEntriesExplanation = $"The folder \"{Path.GetFileName(dataSource.FullFileName)}\" does not contain any file";
					NoEntriesAction = dataSource.FullFileName;
				}
				else if (folderDataSource != null && folderDataSource.FilteredFileCount == 0)
				{
					NoEntriesIcon = null;
					NoEntriesExplanation = $"The folder \"{Path.GetFileName(dataSource.FullFileName)}\" does not contain any file matching \"{folderDataSource.LogFileSearchPattern}\"";
					NoEntriesAction = dataSource.FullFileName;
				}
				else if (source.GetProperty(GeneralProperties.Size) == Size.Zero)
				{
					NoEntriesIcon = Icons.File;
					NoEntriesExplanation = "Data source is empty";
					NoEntriesAction = null;
				}
				else if (dataSource.LevelFilter != LevelFlags.All)
				{
					NoEntriesIcon = Icons.FileSearch;
					NoEntriesExplanation = "Nothing matches level filter";
					NoEntriesAction = "Try filtering by different levels or display everything regardless of its level again";
				}
				else if (dataSource.ScreenCleared)
				{
					NoEntriesIcon = Icons.PlaylistRemove;
					NoEntriesExplanation = "The screen was cleared";
					NoEntriesAction = "No new log entries have been added to the data source since the screen was cleared. Try waiting for a little longer or show all log entries again.";
				}
				else if (filter != null)
				{
					NoEntriesIcon = Icons.FileSearch;
					NoEntriesExplanation = "Nothing matches quick filter";
					NoEntriesAction = filter is FilterExpression
						? $"No log entry matches \"{filter}\".\r\nTry changing your filter(s) or disable them again"
						: "Try filtering by different terms or disable all quick filters";
				}
				else if (source is MergedLogSource)
				{
					NoEntriesIcon = null;
					NoEntriesExplanation = "No log entries with timestamps";
					NoEntriesAction = "Try merging different data sources, change the timestamp format of the data or create a plugin to help Tailviewer parse its timestamp";
				}
				else
				{
					NoEntriesIcon = null;
					NoEntriesExplanation = null;
					NoEntriesAction = null;
				}
			}
			else
			{
				NoEntriesExplanation = null;
				NoEntriesAction = null;
			}
		}
	}
}