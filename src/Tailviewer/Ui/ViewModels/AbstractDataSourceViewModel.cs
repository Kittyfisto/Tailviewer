using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Exporter;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels.ContextMenu;

namespace Tailviewer.Ui.ViewModels
{
	public abstract class AbstractDataSourceViewModel
		: IDataSourceViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDataSource _dataSource;
		private readonly IActionCenter _actionCenter;
		private readonly ICommand _removeCommand;
		private readonly IApplicationSettings _applicationSettings;

		private int _traceCount;
		private int _debugCount;
		private int _errorCount;
		private int _fatalCount;
		private Size? _fileSize;
		private int _infoCount;
		private bool _isGrouped;
		private bool _isVisible;
		private bool _exists;
		private int _lastSeenLogLine;
		private TimeSpan? _lastWrittenAge;
		private int _noTimestampCount;
		private int _otherCount;
		private IDataSourceViewModel _parent;
		private int _totalCount;
		private int _warningCount;
		private string _searchTerm;
		private int _currentSearchResultIndex;
		private int _searchResultCount;
		private double _progress;
		private readonly DelegateCommand2 _clearScreenCommand;
		private readonly DelegateCommand2 _showAllCommand;

		#region Find all

		private bool _showFindAll;
		private bool _isFindAllEmpty;
		private string _findAllErrorMessage;
		private IEnumerable<LogLineIndex> _selectedFindAllLogLines;

		#endregion

		private readonly ObservableCollection<IMenuViewModel> _fileMenuItems;
		private readonly ObservableCollection<IMenuViewModel> _viewMenuItems;
		private readonly ObservableCollection<IMenuViewModel> _contextMenuItems;

		protected AbstractDataSourceViewModel(IDataSource dataSource,
		                                      IActionCenter actionCenter,
		                                      IApplicationSettings applicationSettings)
		{
			_dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
			_actionCenter = actionCenter ?? throw new ArgumentNullException(nameof(actionCenter));
			_applicationSettings = applicationSettings;
			_searchTerm = dataSource.SearchTerm;

			_removeCommand = new DelegateCommand(OnRemoveDataSource);
			_currentSearchResultIndex = -1;

			_clearScreenCommand = new DelegateCommand2(ClearScreen);

			_showAllCommand = new DelegateCommand2(ShowAll)
			{
				CanBeExecuted = false
			};

			_fileMenuItems = new ObservableCollection<IMenuViewModel>
			{
				new CommandMenuViewModel(new DelegateCommand2(ExportToFile))
				{
					Header = "Export To File",
					ToolTip = "Export the current Data Source to a text file (with the current filters)"
				}
			};

			_viewMenuItems = new ObservableCollection<IMenuViewModel>
			{
				new ToggleMenuViewModel(FollowTail, newValue => { FollowTail = newValue; })
				{
					Header = "Follow Tail",
					ToolTip = "Follow/Unfollow the last log entry"
				},
				null,
				new ToggleMenuViewModel(ShowLineNumbers, newValue => { ShowLineNumbers = newValue;})
				{
					Header = "Show Line Numbers",
					ToolTip = "Show/hide line numbers",
				},
				new ToggleMenuViewModel(ShowDeltaTimes, newValue => { ShowDeltaTimes = newValue;})
				{
					Header = "Show Delta Times",
					ToolTip = "Show the amount of time elapsed since the previous entry"
				},
				new ToggleMenuViewModel(ShowElapsedTime, newValue => { ShowElapsedTime = newValue;})
				{
					Header = "Show Elapsed Times",
					ToolTip = "Show the amount of time elapsed since the first entry"
				},
				null,
				new ToggleMenuViewModel(ColorByLevel, newValue => { ColorByLevel = newValue;})
				{
					Header = "Color by Level",
					ToolTip = "Color log messages by their log level"
				},
				new ToggleMenuViewModel(HideEmptyLines, newValue => { HideEmptyLines = newValue;})
				{
					Header = "Hide Empty Lines",
					ToolTip = "Hide lines which are completely empty"
				},
				new ToggleMenuViewModel(IsSingleLine, newValue => { IsSingleLine = newValue;})
				{
					Header = "Single Line",
					ToolTip = "Treat every single line as a separate log entry"
				},
				null,
				new CommandMenuViewModel(ClearScreenCommand)
				{
					Header = "Clear Screen",
					ToolTip = "Hides all log current entries of the data source. New log entries will still be shown once they are added to the data source."
				},
				new CommandMenuViewModel(ShowAllCommand)
				{
					Header = "ShowAll",
					ToolTip = "Shows all log entries that were previously cleared again"
				},
			};

			_contextMenuItems = new ObservableCollection<IMenuViewModel>();
		}

		public int NewLogLineCount
		{
			get
			{
				int diff = TotalCount - _lastSeenLogLine;
				return Math.Max(diff, 0);
			}
		}

		public DataSourceId Id => _dataSource.Id;

		public bool IsGrouped
		{
			get { return _isGrouped; }
			private set
			{
				if (value == _isGrouped)
					return;

				_isGrouped = value;
				EmitPropertyChanged();
			}
		}

		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				if (value == _isVisible)
					return;

				_isVisible = value;
				if (value)
				{
					LastViewed = DateTime.Now;
				}
				EmitPropertyChanged();

				UpdateLastSeenLogLine();
			}
		}

		public IPluginDescription TranslationPlugin => _dataSource.TranslationPlugin;

		public IEnumerable<IMenuViewModel> FileMenuItems => _fileMenuItems;

		public IEnumerable<IMenuViewModel> ViewMenuItems => _viewMenuItems;

		public abstract string DisplayName { get; set; }
		public abstract bool CanBeRenamed { get; }
		public abstract string DataSourceOrigin { get; }

		public bool Exists
		{
			get { return _exists; }
			private set
			{
				if (value == _exists)
					return;

				_exists = value;
				EmitPropertyChanged();
			}
		}

		public int TotalCount
		{
			get { return _totalCount; }
			protected set
			{
				if (value == _totalCount)
					return;

				_totalCount = value;
				EmitPropertyChanged();
			}
		}

		public int NoTimestampCount
		{
			get { return _noTimestampCount; }
			protected set
			{
				if (value == _noTimestampCount)
					return;

				_noTimestampCount = value;
				EmitPropertyChanged();
			}
		}

		public int OtherCount
		{
			get { return _otherCount; }
			protected set
			{
				if (value == _otherCount)
					return;

				_otherCount = value;
				EmitPropertyChanged();
			}
		}

		public int TraceCount
		{
			get { return _traceCount; }
			protected set
			{
				if (value == _traceCount)
					return;

				_traceCount = value;
				EmitPropertyChanged();
			}
		}

		public int DebugCount
		{
			get { return _debugCount; }
			protected set
			{
				if (value == _debugCount)
					return;

				_debugCount = value;
				EmitPropertyChanged();
			}
		}

		public int InfoCount
		{
			get { return _infoCount; }
			protected set
			{
				if (value == _infoCount)
					return;

				_infoCount = value;
				EmitPropertyChanged();
			}
		}

		public int WarningCount
		{
			get { return _warningCount; }
			protected set
			{
				if (value == _warningCount)
					return;

				_warningCount = value;
				EmitPropertyChanged();
			}
		}

		public int ErrorCount
		{
			get { return _errorCount; }
			protected set
			{
				if (value == _errorCount)
					return;

				_errorCount = value;
				EmitPropertyChanged();
			}
		}

		public int FatalCount
		{
			get { return _fatalCount; }
			protected set
			{
				if (value == _fatalCount)
					return;

				_fatalCount = value;
				EmitPropertyChanged();
			}
		}

		public Size? FileSize
		{
			get { return _fileSize; }
			protected set
			{
				if (value == _fileSize)
					return;

				_fileSize = value;
				EmitPropertyChanged();
			}
		}

		public LogLineIndex VisibleLogLine
		{
			get { return _dataSource.VisibleLogLine; }
			set
			{
				if (value == _dataSource.VisibleLogLine)
					return;

				_dataSource.VisibleLogLine = value;
				EmitPropertyChanged();
			}
		}

		public double HorizontalOffset
		{
			get { return _dataSource.HorizontalOffset; }
			set { _dataSource.HorizontalOffset = value; }
		}

		public HashSet<LogLineIndex> SelectedLogLines
		{
			get { return _dataSource.SelectedLogLines; }
			set
			{
				if (ReferenceEquals(value, _dataSource.SelectedLogLines))
					return;

				_dataSource.SelectedLogLines = value;
				EmitPropertyChanged();
			}
		}

		public IEnumerable<LogLineIndex> SelectedFindAllLogLines
		{
			get { return _selectedFindAllLogLines; }
			set
			{
				if (ReferenceEquals(value, _selectedFindAllLogLines))
					return;

				_selectedFindAllLogLines = value;
				EmitPropertyChanged();

				var index = value?.FirstOrDefault() ??  LogLineIndex.Invalid;
				if (index.IsValid)
				{
					var entry = _dataSource.FindAllLogSource.GetEntries(new[] {index}, new[]{GeneralColumns.OriginalIndex});
					var originalIndex = entry[0].OriginalIndex;
					if (originalIndex.IsValid)
					{
						VisibleLogLine = Math.Max(originalIndex - 5, 0);
						SelectedLogLines = new HashSet<LogLineIndex> {originalIndex};
					}
				}
			}
		}

		public TimeSpan? LastWrittenAge
		{
			get { return _lastWrittenAge; }
			private set
			{
				if (value == _lastWrittenAge)
					return;

				_lastWrittenAge = value;
				EmitPropertyChanged();
			}
		}

		public ICommand RemoveCommand => _removeCommand;

		public bool FollowTail
		{
			get { return _dataSource.FollowTail; }
			set
			{
				if (value == FollowTail)
					return;

				_dataSource.FollowTail = value;
				EmitPropertyChanged();
			}
		}

		public bool ShowLineNumbers
		{
			get { return _dataSource.ShowLineNumbers; }
			set
			{
				if (value == ShowLineNumbers)
					return;

				_dataSource.ShowLineNumbers = value;
				EmitPropertyChanged();
			}
		}

		public bool ShowDeltaTimes
		{
			get { return _dataSource.ShowDeltaTimes; }
			set
			{
				if (value == ShowDeltaTimes)
					return;

				_dataSource.ShowDeltaTimes = value;
				EmitPropertyChanged();
			}
		}

		public bool ShowElapsedTime
		{
			get { return _dataSource.ShowElapsedTime; }
			set
			{
				if (value == ShowElapsedTime)
					return;

				_dataSource.ShowElapsedTime = value;
				EmitPropertyChanged();
			}
		}

		public bool ColorByLevel
		{
			get { return _dataSource.ColorByLevel; }
			set
			{
				if (value == ColorByLevel)
					return;

				_dataSource.ColorByLevel = value;
				EmitPropertyChanged();
			}
		}

		public bool HideEmptyLines
		{
			get { return _dataSource.HideEmptyLines; }
			set
			{
				if (value == HideEmptyLines)
					return;

				_dataSource.HideEmptyLines = value;
				EmitPropertyChanged();
			}
		}

		public bool IsSingleLine
		{
			get { return _dataSource.IsSingleLine; }
			set
			{
				if (value == IsSingleLine)
					return;

				_dataSource.IsSingleLine = value;
				EmitPropertyChanged();
			}
		}

		public bool ScreenCleared
		{
			get { return _dataSource.ScreenCleared; }
		}

		public ICommand ClearScreenCommand
		{
			get { return _clearScreenCommand; }
		}

		public ICommand ShowAllCommand
		{
			get { return _showAllCommand; }
		}

		#region Searches

		public double Progress
		{
			get { return _progress; }
			set
			{
				if (value == _progress)
					return;

				_progress = value;
				EmitPropertyChanged();
			}
		}

		public string SearchTerm
		{
			get { return _searchTerm; }
			set
			{
				if (value == _searchTerm)
					return;

				_searchTerm = value;

				if (string.IsNullOrEmpty(value))
				{
					SearchResultCount = 0;
					CurrentSearchResultIndex = -1;
					_dataSource.SearchTerm = null;
				}
				else
				{
					_dataSource.SearchTerm = value;
				}

				EmitPropertyChanged();
			}
		}

		public int SearchResultCount
		{
			get { return _searchResultCount; }
			private set
			{
				if (value == _searchResultCount)
					return;

				_searchResultCount = value;
				EmitPropertyChanged();

				if (SearchResultCount > 0)
				{
					if (CurrentSearchResultIndex < 1 || CurrentSearchResultIndex >= SearchResultCount)
					{
						CurrentSearchResultIndex = 0;
					}
				}
				else
				{
					CurrentSearchResultIndex = -1;
				}
			}
		}

		public int CurrentSearchResultIndex
		{
			get { return _currentSearchResultIndex; }
			set
			{
				if (value == _currentSearchResultIndex)
					return;

				_currentSearchResultIndex = value;
				EmitPropertyChanged();
			}
		}

		#endregion

		#region Find all

		public string FindAllSearchTerm
		{
			get { return _dataSource.FindAllFilter; }
			set
			{
				if (value == _dataSource.FindAllFilter)
					return;

				_dataSource.FindAllFilter = value;
				EmitPropertyChanged();

				ShowFindAll = !string.IsNullOrEmpty(value);
			}
		}

		public bool ShowFindAll
		{
			get { return _showFindAll; }
			private set
			{
				if (value == _showFindAll)
					return;

				_showFindAll = value;
				EmitPropertyChanged();
			}
		}

		public bool IsFindAllEmpty
		{
			get { return _isFindAllEmpty; }
			private set
			{
				if (value == _isFindAllEmpty)
					return;

				_isFindAllEmpty = value;
				EmitPropertyChanged();
			}
		}

		public string FindAllErrorMessage
		{
			get { return _findAllErrorMessage; }
			private set
			{
				if (value == _findAllErrorMessage)
					return;

				_findAllErrorMessage = value;
				EmitPropertyChanged();
			}
		}

		public ICommand CloseFindAllCommand => new DelegateCommand2(CloseFindAll);

		private void CloseFindAll()
		{
			FindAllSearchTerm = null;
			ShowFindAll = false;
		}

		#endregion

		public DateTime LastViewed
		{
			get { return _dataSource.LastViewed; }
			set
			{
				if (value == LastViewed)
					return;

				_dataSource.LastViewed = value;
				EmitPropertyChanged();
			}
		}

		public IDataSource DataSource => _dataSource;

		public LevelFlags LevelsFilter
		{
			get { return _dataSource.LevelFilter; }
			set
			{
				if (value == LevelsFilter)
					return;

				_dataSource.LevelFilter = value;
				EmitPropertyChanged();
			}
		}

		public IDataSourceViewModel Parent
		{
			get { return _parent; }
			set
			{
				_dataSource.Settings.ParentId = value?.DataSource.Id ?? DataSourceId.Empty;
				_parent = value;
				IsGrouped = value != null;

				EmitPropertyChanged();
			}
		}

		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _dataSource.QuickFilterChain; }
			set { _dataSource.QuickFilterChain = value; }
		}

		public void RequestBringIntoView(LogLineIndex index)
		{
			OnRequestBringIntoView?.Invoke(this, index);
		}

		public event Action<IDataSourceViewModel, LogLineIndex> OnRequestBringIntoView;
		public event PropertyChangedEventHandler PropertyChanged;
		public event Action<IDataSourceViewModel> Remove;

		public IEnumerable<IMenuViewModel> ContextMenuItems => _contextMenuItems;

		public virtual void Update()
		{
			int newBefore = NewLogLineCount;

			OtherCount = _dataSource.NoLevelCount;
			TraceCount = _dataSource.TraceCount;
			DebugCount = _dataSource.DebugCount;
			InfoCount = _dataSource.InfoCount;
			WarningCount = _dataSource.WarningCount;
			ErrorCount = _dataSource.ErrorCount;
			FatalCount = _dataSource.FatalCount;
			TotalCount = _dataSource.TotalCount;
			FileSize = _dataSource.FileSize;
			Exists = _dataSource.UnfilteredLogSource?.GetProperty(GeneralProperties.EmptyReason)
			         == ErrorFlags.None;
			NoTimestampCount = _dataSource.NoTimestampCount;
			LastWrittenAge = DateTime.Now - _dataSource.LastModified;
			SearchResultCount = (_dataSource.Search?.Count) ?? 0;
			Progress = _dataSource.FilteredLogSource?.GetProperty(GeneralProperties.PercentageProcessed).RelativeValue ?? 1;

			if (NewLogLineCount != newBefore)
			{
				if (_dataSource.LastModified >= LastViewed && !IsVisible)
				{
					EmitPropertyChanged("NewLogLineCount");
				}
				else
				{
					_lastSeenLogLine = TotalCount;
				}
			}
		}

		protected void AddFileMenuItems(IEnumerable<IMenuViewModel> viewModels)
		{
			foreach (var item in viewModels)
				_fileMenuItems.Add(item);
		}

		protected void SetContextMenuItems(IEnumerable<IMenuViewModel> contextMenuItems)
		{
			_contextMenuItems.Clear();
			foreach(var item in contextMenuItems)
				_contextMenuItems.Add(item);
		}

		private void UpdateLastSeenLogLine()
		{
			int before = NewLogLineCount;
			if (IsVisible)
			{
				_lastSeenLogLine = TotalCount;
				if (before != NewLogLineCount)
					EmitPropertyChanged("NewLogLineCount");
			}
		}

		private void OnRemoveDataSource()
		{
			Remove?.Invoke(this);
		}

		protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ClearScreen()
		{
			_dataSource.ClearScreen();

			EmitPropertyChanged(nameof(ScreenCleared));
			_showAllCommand.CanBeExecuted = true;
		}

		private void ShowAll()
		{
			_dataSource.ShowAll();
			EmitPropertyChanged(nameof(ScreenCleared));
			_showAllCommand.CanBeExecuted = false;
		}
		private void ExportToFile()
		{
			var dataSource = DataSource;
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
			                              DisplayName,
			                              exportDirectory);
			_actionCenter.Add(action);
		}

	}
}