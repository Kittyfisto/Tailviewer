using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Ui.ViewModels
{
	public abstract class AbstractDataSourceViewModel
		: IDataSourceViewModel
	{
		private readonly IDataSource _dataSource;
		private readonly ICommand _removeCommand;

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

		protected AbstractDataSourceViewModel(IDataSource dataSource)
		{
			if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));

			_dataSource = dataSource;
			_searchTerm = dataSource.SearchTerm;

			_removeCommand = new DelegateCommand(OnRemoveDataSource);
			_currentSearchResultIndex = -1;

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

		public abstract ICommand OpenInExplorerCommand { get; }

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
			Exists = _dataSource.UnfilteredLogFile?.GetValue(LogFileProperties.EmptyReason)
			         == ErrorFlags.None;
			NoTimestampCount = _dataSource.NoTimestampCount;
			LastWrittenAge = DateTime.Now - _dataSource.LastModified;
			SearchResultCount = (_dataSource.Search?.Count) ?? 0;
			Progress = _dataSource.FilteredLogFile?.Progress ?? 1;

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
	}
}