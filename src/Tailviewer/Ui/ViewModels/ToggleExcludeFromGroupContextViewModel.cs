using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class ToggleExcludeFromGroupContextViewModel
		: IContextMenuViewModel
		, INotifyPropertyChanged
	{
		private readonly ISingleDataSourceViewModel _dataSource;
		private readonly DelegateCommand2 _command;
		private string _header;
		private bool _excludeFromGroup;

		public ToggleExcludeFromGroupContextViewModel(ISingleDataSourceViewModel dataSource)
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

		public ICommand Command
		{
			get
			{
				return _command;
			}
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
			_excludeFromGroup = !_excludeFromGroup;
			
			UpdateHeader();
		}

		private void UpdateHeader()
		{
			// The text describes what Command() does, and thus if we're currently excluding the file,
			// then executing the command will include it (and vice versa).
			if (_excludeFromGroup)
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