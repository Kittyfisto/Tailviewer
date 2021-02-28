using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Collections;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Ui.Menu
{
	/// <summary>
	///     Represents the "Edit" main menu.
	/// </summary>
	public sealed class EditMenuViewModel
		: AbstractMainMenuViewModel
	{
		private readonly ICommand _goToLineCommand;
		private readonly ICommand _goToDataSource;
		private readonly ICommand _goToPreviousDataSource;
		private readonly ICommand _goToNextDataSource;
		private readonly ILogViewMainPanelViewModel _mainPanel;
		private readonly int _dataSourceInsertionIndex;
		private readonly int _minCount;
		private readonly ObservableCollectionExt<IMenuViewModel> _goToItems;

		public EditMenuViewModel(ICommand goToLineCommand,
		                         ICommand goToDataSource,
		                         ICommand goToPreviousDataSource,
		                         ICommand goToNextDataSource,
		                         ILogViewMainPanelViewModel mainPanel)
		{
			_goToLineCommand = goToLineCommand;
			_goToDataSource = goToDataSource;
			_goToPreviousDataSource = goToPreviousDataSource;
			_goToNextDataSource = goToNextDataSource;
			_mainPanel = mainPanel;
			_goToItems = new ObservableCollectionExt<IMenuViewModel>();

			AllItems.Add(_goToItems);
			_dataSourceInsertionIndex = AllItems.ChildCollectionCount;
			_minCount = AllItems.ChildCollectionCount;
		}

		private IEnumerable<IMenuViewModel> CreateGoToItems()
		{
			// TODO: Move this closer to where it belongs: LogViewMainPanelViewModel
			return new[]
			{
				new CommandMenuViewModel(new KeyBindingCommand(_goToLineCommand)
				{
					GestureModifier = ModifierKeys.Control,
					GestureKey = Key.G
				})
				{
					Header = "Go To Line..."
				},
				null,
				new CommandMenuViewModel(new KeyBindingCommand(_goToDataSource)
				{
					GestureModifier = ModifierKeys.Control,
					GestureKey = Key.T
				})
				{
					Header = "Go To Data Source..."
				},
				new CommandMenuViewModel(new KeyBindingCommand(_goToPreviousDataSource)
				{
					GestureModifier = ModifierKeys.Control | ModifierKeys.Shift,
					GestureKey = Key.Tab
				})
				{
					Header = "Go To Previous Data Source"
				},
				new CommandMenuViewModel(new KeyBindingCommand(_goToNextDataSource)
				{
					GestureModifier = ModifierKeys.Control,
					GestureKey = Key.Tab
				})
				{
					Header = "Go To Next Data Source"
				},
				null,
				new CommandMenuViewModel(new KeyBindingCommand(_mainPanel.AddBookmarkCommand)
				{
					GestureModifier = ModifierKeys.Control,
					GestureKey = Key.B
				})
				{
					Header = "Add Bookmark",
					Icon = Icons.BookmarkPlusOutline
				},
				new CommandMenuViewModel(_mainPanel.RemoveAllBookmarkCommand)
				{
					Header = "Remove All Bookmarks",
					Icon = Icons.BookmarkRemoveOutline
				}
			};
		}

		#region Implementation of IMenu

		public override IDataSourceViewModel CurrentDataSource
		{
			set
			{
				if (AllItems.ChildCollectionCount > _minCount)
				{
					AllItems.RemoveAt(_dataSourceInsertionIndex);
				}

				AllItems.Insert(_dataSourceInsertionIndex, value?.EditMenuItems);

				if (value != null)
				{
					if (!_goToItems.Any())
					{
						_goToItems.Clear();
						foreach (var item in CreateGoToItems())
						{
							_goToItems.Add(item);
						}
					}
				}
				else
				{
					_goToItems.Clear();
				}
			}
		}

		#endregion
	}
}