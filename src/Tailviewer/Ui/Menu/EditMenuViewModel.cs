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
		: AbstractMainMenuViewModel
	{
		private readonly ICommand _goToLineCommand;
		private readonly ICommand _goToDataSource;
		private readonly int _dataSourceInsertionIndex;
		private readonly int _minCount;
		private readonly ObservableCollectionExt<IMenuViewModel> _goToItems;

		public EditMenuViewModel(ICommand goToLineCommand,
		                         ICommand goToDataSource)
		{
			_goToLineCommand = goToLineCommand;
			_goToDataSource = goToDataSource;
			_goToItems = new ObservableCollectionExt<IMenuViewModel>();

			AllItems.Add(_goToItems);
			_dataSourceInsertionIndex = AllItems.ChildCollectionCount;
			_minCount = AllItems.ChildCollectionCount;
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
				},
				null,
				new CommandMenuViewModel(new KeyBindingCommand(_goToDataSource)
				{
					GestureModifier = ModifierKeys.Control,
					GestureKey = Key.T
				})
				{
					Header = "Go To Data Source..."
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