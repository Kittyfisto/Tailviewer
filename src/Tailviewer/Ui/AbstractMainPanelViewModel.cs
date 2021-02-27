using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Ui
{
	public abstract class AbstractMainPanelViewModel
		: IMainPanelViewModel
	{
		private ISearchViewModel _search;
		private IFindAllViewModel _findAll;

		public event PropertyChangedEventHandler PropertyChanged;

		public ISearchViewModel Search
		{
			get { return _search; }
			protected set
			{
				if (value == _search)
					return;

				_search = value;
				EmitPropertyChanged();
			}
		}

		public IFindAllViewModel FindAll
		{
			get { return _findAll; }
			protected set
			{
				if (value == _findAll)
					return;

				_findAll = value;
				EmitPropertyChanged();
			}
		}

		public abstract void Update();

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
