using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.Menu
{
	public sealed class MainMenu
	{
		private readonly FileMenuViewModel _file;
		private readonly ViewMenuViewModel _view;
		private readonly HelpMenuViewModel _help;
		private readonly CompoundObservableCollection<IMenuViewModel> _allMenuItems;
		private readonly ProjectingObservableCollection<IMenuViewModel, KeyBindingCommand> _keyBindings;

		public MainMenu(FileMenuViewModel file,
		                ViewMenuViewModel view,
		                HelpMenuViewModel help)
		{
			_file = file;
			_view = view;
			_help = help;

			_allMenuItems = new CompoundObservableCollection<IMenuViewModel>(false, null)
			{
				file.Items, view.Items, help.Items
			};
			_keyBindings = new ProjectingObservableCollection<IMenuViewModel, KeyBindingCommand>(_allMenuItems, menuItem => menuItem?.Command as KeyBindingCommand);
		}

		public IDataSourceViewModel CurrentDataSource
		{
			set
			{
				_file.CurrentDataSource = value;
				_view.CurrentDataSource = value;
			}
		}

		public IObservableCollection<KeyBindingCommand> KeyBindings => _keyBindings;

		public FileMenuViewModel File => _file;

		public ViewMenuViewModel View => _view;

		public HelpMenuViewModel Help => _help;
	}
}
