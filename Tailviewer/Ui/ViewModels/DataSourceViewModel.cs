using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     Represents a data source and is capable
	/// </summary>
	internal sealed class DataSourceViewModel
		: INotifyPropertyChanged
	{
		private readonly DataSource _dataSource;
		private readonly string _fileName;
		private readonly string _folder;
		private readonly ICommand _openInExplorerCommand;
		private readonly ICommand _removeCommand;
		private int _debugCount;
		private int _errorCount;
		private int _fatalCount;
		private Size _fileSize;
		private int _infoCount;
		private TimeSpan _lastWrittenAge;
		private int _otherCount;
		private int _totalCount;
		private int _warningCount;

		public DataSourceViewModel(DataSource dataSource)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");

			_dataSource = dataSource;
			_fileName = Path.GetFileName(dataSource.FullFileName);
			_folder = Path.GetDirectoryName(dataSource.FullFileName);
			_removeCommand = new DelegateCommand(OnRemoveDataSource);
			_openInExplorerCommand = new DelegateCommand(OpenInExplorer);
			Update();
		}

		public ICommand OpenInExplorerCommand
		{
			get { return _openInExplorerCommand; }
		}

		public int TotalCount
		{
			get { return _totalCount; }
			set
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
			set
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
			set
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
			set
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
			set
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
			set
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
			set
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
			set
			{
				if (value == _fileSize)
					return;

				_fileSize = value;
				EmitPropertyChanged();
			}
		}

		public LogEntryIndex VisibleLogEntry
		{
			get { return _dataSource.VisibleLogEntry; }
			set { _dataSource.VisibleLogEntry = value; }
		}

		public LogEntryIndex SelectedLogEntry
		{
			get { return _dataSource.SelectedLogEntry; }
			set { _dataSource.SelectedLogEntry = value; }
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

		public DateTime LastOpened
		{
			get { return _dataSource.LastOpened; }
			set
			{
				if (value == LastOpened)
					return;

				_dataSource.LastOpened = value;
				EmitPropertyChanged();
			}
		}

		public bool IsOpen
		{
			get { return _dataSource.IsOpen; }
			set
			{
				if (value == IsOpen)
					return;

				if (value)
				{
					_dataSource.LastOpened = DateTime.Now;
				}

				_dataSource.IsOpen = value;
				EmitPropertyChanged();
			}
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public string Folder
		{
			get { return _folder; }
		}

		public string FullName
		{
			get { return _dataSource.FullFileName; }
		}

		public DataSource DataSource
		{
			get { return _dataSource; }
		}

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

		public event PropertyChangedEventHandler PropertyChanged;

		private void OpenInExplorer()
		{
			var argument = string.Format(@"/select, {0}", _dataSource.FullFileName);
			Process.Start("explorer.exe", argument);
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
			LastWrittenAge = DateTime.Now - _dataSource.LastWritten;
		}

		public event Action<DataSourceViewModel> Remove;

		private void OnRemoveDataSource()
		{
			Action<DataSourceViewModel> fn = Remove;
			if (fn != null)
				fn(this);
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}