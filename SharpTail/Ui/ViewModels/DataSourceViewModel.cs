using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
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
		private bool _isOpen;

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

		public string FilterString
		{
			get { return _dataSource.FilterString; }
			set
			{
				if (value == FilterString)
					return;

				_dataSource.FilterString = value;
				EmitPropertyChanged();
			}
		}

		public DateTime LastOpened
		{
			get { return _dataSource.LastOpened; }
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
			_dataSource = dataSource;
			_fileName = Path.GetFileName(dataSource.FileName);
			_folder = Path.GetDirectoryName(dataSource.FileName);
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public string Folder
		{
			get { return _folder; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}