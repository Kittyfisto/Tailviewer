using System.Collections.Generic;
using Tailviewer.Collections;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.Menu
{
	public sealed class ViewMenuViewModel
		: IMenu
	{
		private readonly CompoundObservableCollection<IMenuViewModel> _items;

		public ViewMenuViewModel()
		{
			_items = new CompoundObservableCollection<IMenuViewModel>(false);
		}

		#region Implementation of IMenu

		public IEnumerable<IMenuViewModel> Items
		{
			get
			{
				return _items;
			}
		}

		public IDataSourceViewModel CurrentDataSource
		{
			set
			{
				if (_items.ChildCollectionCount > 0)
				{
					_items.RemoveAt(0);
				}

				_items.Insert(0, value?.ViewMenuItems);
			}
		}

		#endregion
	}
}
