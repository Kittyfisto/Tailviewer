using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	internal abstract class AbstractDataSourceViewModel
		: IDataSourceViewModel
	{
		private readonly ICommand _removeCommand;
		private readonly IDataSource _dataSource;

		private int _debugCount;
		private int _errorCount;
		private int _fatalCount;
		private Size _fileSize;
		private int _infoCount;
		private TimeSpan _lastWrittenAge;
		private int _otherCount;
		private int _totalCount;
		private int _warningCount;

		protected AbstractDataSourceViewModel(IDataSource dataSource)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");

			_dataSource = dataSource;
			_removeCommand = new DelegateCommand(OnRemoveDataSource);
			Update();
		}

		public abstract ICommand OpenInExplorerCommand { get; }

		public abstract string DisplayName { get; }

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

		public Size FileSize
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
			set { _dataSource.VisibleLogLine = value; }
		}

		public LogLineIndex SelectedLogLine
		{
			get { return _dataSource.SelectedLogLine; }
			set { _dataSource.SelectedLogLine = value; }
		}

		public TimeSpan LastWrittenAge
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

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

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

		public string StringFilter
		{
			get { return _dataSource.StringFilter; }
			set
			{
				if (value == StringFilter)
					return;

				_dataSource.StringFilter = value;
				EmitPropertyChanged();
			}
		}

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

		public IDataSource DataSource { get { return _dataSource; } }

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

		public IDataSourceViewModel Parent { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		public event Action<IDataSourceViewModel> Remove;

		public bool IsOpen
		{
			get { return _dataSource.IsOpen; }
			set
			{
				if (value == IsOpen)
					return;

				if (value)
				{
					_dataSource.LastViewed = DateTime.Now;
				}

				_dataSource.IsOpen = value;
				EmitPropertyChanged();
			}
		}

		public void Update()
		{
			OtherCount = _dataSource.OtherCount;
			DebugCount = _dataSource.DebugCount;
			InfoCount = _dataSource.InfoCount;
			WarningCount = _dataSource.WarningCount;
			ErrorCount = _dataSource.ErrorCount;
			FatalCount = _dataSource.FatalCount;
			TotalCount = _dataSource.TotalCount;
			FileSize = _dataSource.FileSize;
			LastWrittenAge = DateTime.Now - _dataSource.LastModified;
		}

		private void OnRemoveDataSource()
		{
			Action<IDataSourceViewModel> fn = Remove;
			if (fn != null)
				fn(this);
		}

		protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}