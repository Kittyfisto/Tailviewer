using Tailviewer.Collections;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.Menu
{
	/// <summary>
	///     Represents the main menu of Tailviewer.
	/// </summary>
	/// <remarks>
	///     Responsible for updating the available menu items depending on what the user is currently doing.
	/// </remarks>
	public sealed class MainMenu
	{
		private readonly EditMenuViewModel _edit;
		private readonly FileMenuViewModel _file;
		private readonly HelpMenuViewModel _help;
		private readonly ProjectingObservableCollection<IMenuViewModel, KeyBindingCommand> _keyBindings;
		private readonly ViewMenuViewModel _view;

		public MainMenu(FileMenuViewModel file,
		                EditMenuViewModel edit,
		                ViewMenuViewModel view,
		                HelpMenuViewModel help)
		{
			_file = file;
			_edit = edit;
			_view = view;
			_help = help;

			var allMenuItems = new CompoundObservableCollection<IMenuViewModel>(hasSeparator: false)
			{
				file.Items, _edit.Items, view.Items, help.Items
			};
			_keyBindings =
				new ProjectingObservableCollection<IMenuViewModel, KeyBindingCommand>(allMenuItems,
					menuItem => menuItem?.Command as KeyBindingCommand);
		}

		public IDataSourceViewModel CurrentDataSource
		{
			set
			{
				_file.CurrentDataSource = value;
				_edit.CurrentDataSource = value;
				_view.CurrentDataSource = value;
			}
		}

		/// <summary>
		///     A collection of all key bindings the user may currently press.
		/// </summary>
		public IObservableCollection<KeyBindingCommand> KeyBindings
		{
			get { return _keyBindings; }
		}

		public FileMenuViewModel File
		{
			get { return _file; }
		}

		public EditMenuViewModel Edit
		{
			get { return _edit; }
		}

		public ViewMenuViewModel View
		{
			get { return _view; }
		}

		public HelpMenuViewModel Help
		{
			get { return _help; }
		}
	}
}