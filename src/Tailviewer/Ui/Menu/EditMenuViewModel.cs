using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Tailviewer.Collections;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.Menu
{
	/// <summary>
	///     Represents the "Edit" main menu.
	/// </summary>
	public sealed class EditMenuViewModel
		: IMenu
	{
		private readonly ICommand _goToLineCommand;
		private readonly CompoundObservableCollection<IMenuViewModel> _items;
		private readonly int _dataSourceInsertionIndex;
		private readonly int _minCount;
		private readonly ObservableCollectionExt<IMenuViewModel> _goToItems;

		public EditMenuViewModel(ICommand goToLineCommand)
		{
			_goToLineCommand = goToLineCommand;
			_items = new CompoundObservableCollection<IMenuViewModel>(true);
			_goToItems = new ObservableCollectionExt<IMenuViewModel>();

			_items.Add(_goToItems);
			_dataSourceInsertionIndex = _items.ChildCollectionCount;
			_minCount = _items.ChildCollectionCount;
		}

		private IEnumerable<IMenuViewModel> CreateGoToItems()
		{
			return new[]
			{
				new CommandMenuViewModel(new KeyBindingCommand(_goToLineCommand)
				{
					GestureModifier = ModifierKeys.Control,
					GestureKey = Key.G
				})
				{
					Header = "Go To Line..."
				}
			};
		}

		#region Implementation of IMenu

		public IEnumerable<IMenuViewModel> Items => _items;

		public IDataSourceViewModel CurrentDataSource
		{
			set
			{
				if (_items.ChildCollectionCount > _minCount)
				{
					_items.RemoveAt(_dataSourceInsertionIndex);
				}

				_items.Insert(_dataSourceInsertionIndex, value?.EditMenuItems);

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