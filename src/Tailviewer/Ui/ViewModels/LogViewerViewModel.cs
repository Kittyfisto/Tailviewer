using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Exporter;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class LogViewerViewModel
		: INotifyPropertyChanged
		  , ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDataSourceViewModel _dataSource;
		private readonly TimeSpan _maximumWaitTime;
		private readonly List<KeyValuePair<ILogFile, LogFileSection>> _pendingSections;
		private readonly IActionCenter _actionCenter;
		private readonly IApplicationSettings _applicationSettings;

		private ILogFile _logFile;
		private int _logEntryCount;
		private string _noEntriesExplanation;
		private string _noEntriesSubtext;
		private int _totalLogEntryCount;
		private ILogFileSearch _search;

		public LogViewerViewModel(IDataSourceViewModel dataSource,
			IActionCenter actionCenter,
			IApplicationSettings applicationSettings,
			TimeSpan maximumWaitTime)
		{
			if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
			if (actionCenter == null) throw new ArgumentNullException(nameof(actionCenter));
			if (applicationSettings == null) throw new ArgumentNullException(nameof(applicationSettings));

			_actionCenter = actionCenter;
			_applicationSettings = applicationSettings;
			_maximumWaitTime = maximumWaitTime;
			_dataSource = dataSource;

			_pendingSections = new List<KeyValuePair<ILogFile, LogFileSection>>();

			LogFile = _dataSource.DataSource.FilteredLogFile;
			LogFile.AddListener(this, _maximumWaitTime, 1000);
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
			return _logFile.ToString();
		}

		public ILogFile LogFile
		{
			get { return _logFile; }
			private set
			{
				if (value == _logFile)
					return;

				_logFile = value;
				EmitPropertyChanged();
			}
		}

		public ILogFileSearch Search
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

		public string NoEntriesSubtext
		{
			get { return _noEntriesSubtext; }
			private set
			{
				if (Equals(value, _noEntriesSubtext))
					return;

				_noEntriesSubtext = value;
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
			var logFile = dataSource?.FilteredLogFile;
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

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			lock (_pendingSections)
			{
				if (section == LogFileSection.Reset)
					_pendingSections.Clear();

				_pendingSections.Add(new KeyValuePair<ILogFile, LogFileSection>(_logFile, section));
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
					ILogFile file = pair.Key;
					if (file != _logFile)
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
			LogEntryCount = _logFile.GetProperty(Core.LogFiles.Properties.LogEntryCount);
			TotalLogEntryCount = _dataSource.DataSource.UnfilteredLogFile.GetProperty(Core.LogFiles.Properties.LogEntryCount);
			UpdateNoEntriesExplanation();
		}

		private void UpdateNoEntriesExplanation()
		{
			IDataSource dataSource = _dataSource.DataSource;
			var folderDataSource = dataSource as IFolderDataSource;
			ILogFile source = dataSource.UnfilteredLogFile;
			ILogFile filtered = dataSource.FilteredLogFile;

			if (filtered.GetProperty(Core.LogFiles.Properties.LogEntryCount) == 0)
			{
				IEnumerable<ILogEntryFilter> chain = dataSource.QuickFilterChain;
				var emptyReason = source.GetProperty(Core.LogFiles.Properties.EmptyReason);
				if ((emptyReason & ErrorFlags.SourceDoesNotExist) == ErrorFlags.SourceDoesNotExist)
				{
					NoEntriesExplanation = string.Format("Can't find \"{0}\"", Path.GetFileName(dataSource.FullFileName));
					NoEntriesSubtext = string.Format("It was last seen at {0}", Path.GetDirectoryName(dataSource.FullFileName));
				}
				else if ((emptyReason & ErrorFlags.SourceCannotBeAccessed) == ErrorFlags.SourceCannotBeAccessed)
				{
					NoEntriesExplanation = string.Format("Unable to access \"{0}\"", Path.GetFileName(dataSource.FullFileName));
					NoEntriesSubtext = "The file may be opened exclusively by another process or you are not authorized to view it";
				}
				else if (folderDataSource != null && folderDataSource.UnfilteredFileCount == 0)
				{
					NoEntriesExplanation = string.Format("The folder \"{0}\" does not contain any file", Path.GetFileName(dataSource.FullFileName));
					NoEntriesSubtext = dataSource.FullFileName;
				}
				else if (folderDataSource != null && folderDataSource.FilteredFileCount == 0)
				{
					NoEntriesExplanation = string.Format("The folder \"{0}\" does not contain any file matching \"{1}\"", Path.GetFileName(dataSource.FullFileName), folderDataSource.LogFileSearchPattern);
					NoEntriesSubtext = dataSource.FullFileName;
				}
				else if (source.GetProperty(Core.LogFiles.Properties.Size) == Size.Zero)
				{
					NoEntriesExplanation = "The data source is empty";
					NoEntriesSubtext = null;
				}
				else if (dataSource.LevelFilter != LevelFlags.All)
				{
					NoEntriesExplanation = "Not a single log entry matches the level selection";
					NoEntriesSubtext = null;
				}
				else if (!string.IsNullOrEmpty(dataSource.SearchTerm))
				{
					NoEntriesExplanation = "Not a single log entry matches the log file filter";
					NoEntriesSubtext = null;
				}
				else if (chain != null && chain.All(x => x != null))
				{
					NoEntriesExplanation = "Not a single log entry matches the activated quick filters";
					NoEntriesSubtext = null;
				}
				else if (source is MergedLogFile)
				{
					NoEntriesExplanation = "None of the data sources' contains identifiable timestamps: Merging them is not possible";
					NoEntriesSubtext = null;
				}
				else
				{
					NoEntriesExplanation = null;
					NoEntriesSubtext = null;
				}
			}
			else
			{
				NoEntriesExplanation = null;
				NoEntriesSubtext = null;
			}
		}
	}
}