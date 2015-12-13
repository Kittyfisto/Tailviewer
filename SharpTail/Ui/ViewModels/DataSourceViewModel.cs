using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SharpTail.BusinessLogic;

namespace SharpTail.Ui.ViewModels
{
	/// <summary>
	/// Represents a data source and is capable
	/// </summary>
	public sealed class DataSourceViewModel
		: INotifyPropertyChanged
	{
		private readonly DataSource _dataSource;
		private readonly string _fileName;
		private readonly string _folder;
		private readonly ICommand _removeCommand;

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		private bool _isOpen;
		private DateTime _lastWritten;

		public DateTime LastWritten
		{
			get { return _lastWritten; }
			private set
			{
				if (value == _lastWritten)
					return;

				_lastWritten = value;
				EmitPropertyChanged();
			}
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
			get { return _isOpen; }
			set
			{
				if (value == _isOpen)
					return;

				if (value)
				{
					_dataSource.LastOpened = DateTime.Now;
				}

				_isOpen = value;
				EmitPropertyChanged();
			}
		}

		public DataSourceViewModel(DataSource dataSource)
		{
			if (dataSource == null) throw new ArgumentNullException("dataSource");

			_dataSource = dataSource;
			_fileName = Path.GetFileName(dataSource.FullFileName);
			_folder = Path.GetDirectoryName(dataSource.FullFileName);
			_lastWritten = dataSource.LastWritten;
			_removeCommand = new DelegateCommand(OnRemoveDataSource);
		}

		public event Action<DataSourceViewModel> Remove;

		private void OnRemoveDataSource()
		{
			var fn = Remove;
			if (fn != null)
				fn(this);
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
			get { return _dataSource.Levels; }
			set
			{
				if (value == LevelsFilter)
					return;

				_dataSource.Levels = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void UpdateLastWritten()
		{
			LastWritten = _dataSource.LastWritten;
		}
	}
}