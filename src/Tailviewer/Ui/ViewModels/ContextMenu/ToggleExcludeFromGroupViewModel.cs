using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Shapes;
using Metrolib;

namespace Tailviewer.Ui.ViewModels.ContextMenu
{
	public sealed class ToggleExcludeFromGroupViewModel
		: IMenuViewModel
		, INotifyPropertyChanged
	{
		private readonly ISingleDataSourceViewModel _dataSource;
		private readonly DelegateCommand2 _command;
		private string _header;

		public ToggleExcludeFromGroupViewModel(ISingleDataSourceViewModel dataSource)
		{
			_dataSource = dataSource;
			_command = new DelegateCommand2(ToggleFilterAll);
			UpdateHeader();
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

		public string ToolTip => null;

		public Path Icon => null;

		public ICommand Command
		{
			get
			{
				return _command;
			}
		}

		public bool IsCheckable => false;

		public bool IsChecked
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		#endregion

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

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
			{
				Header = "Include in group";
			}
			else
			{
				Header = "Exclude from group";
			}
		}
	}
}