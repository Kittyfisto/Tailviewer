using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Tailviewer.Collections;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.Menu
{
	public abstract class AbstractMainMenuViewModel
		: IMenu
	{
		private readonly CompoundObservableCollection<IMenuViewModel> _items;
		private bool _hasItems;

		public AbstractMainMenuViewModel()
		{
			_items = new CompoundObservableCollection<IMenuViewModel>(true);
			_items.CollectionChanged += ItemsOnCollectionChanged;
		}

		protected CompoundObservableCollection<IMenuViewModel> AllItems => _items;

		#region Implementation of IMenu

		public IEnumerable<IMenuViewModel> Items
		{
			get
			{
				return _items;
			}
		}

		public bool HasItems
		{
			get
			{
				return _hasItems;
			}
			set
			{
				if (value == _hasItems)
					return;
				_hasItems = value;
				EmitPropertyChanged();
			}
		}

		public abstract IDataSourceViewModel CurrentDataSource
		{
			set;
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			HasItems = _items.Any();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}