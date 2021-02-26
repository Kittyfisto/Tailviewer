using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.ContextMenu
{
	public sealed class ToggleExcludeFromGroupViewModel
		: IMenuViewModel
			, INotifyPropertyChanged
	{
		private readonly DelegateCommand2 _command;
		private readonly ISingleDataSourceViewModel _dataSource;
		private string _header;

		public ToggleExcludeFromGroupViewModel(ISingleDataSourceViewModel dataSource)
		{
			_dataSource = dataSource;
			_command = new DelegateCommand2(ToggleFilterAll);
			UpdateHeader();
		}

		private void ToggleFilterAll()
		{
			_dataSource.ExcludeFromParent = !_dataSource.ExcludeFromParent;

			UpdateHeader();
		}

		public void UpdateHeader()
		{
			// The text describes what Command() does, and thus if we're currently excluding the file,
			// then executing the command will include it (and vice versa).
			if (_dataSource.ExcludeFromParent)
				Header = "Include in group";
			else
				Header = "Exclude from group";
		}

		#region Implementation of IContextMenuViewModel

		public string Header
		{
			get { return _header; }
			private set
			{
				if (value == _header)
					return;

				_header = value;
				EmitPropertyChanged();
			}
		}

		public string ToolTip
		{
			get { return null; }
		}

		public Geometry Icon
		{
			get { return null; }
		}

		public ICommand Command
		{
			get { return _command; }
		}

		public bool IsCheckable
		{
			get { return false; }
		}

		public bool IsChecked { get; set; }

		public IEnumerable<IMenuViewModel> Children
		{
			get { return null; }
		}

		#endregion

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}