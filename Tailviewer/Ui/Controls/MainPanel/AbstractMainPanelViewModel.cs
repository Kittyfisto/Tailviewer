using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public abstract class AbstractMainPanelViewModel
		: IMainPanelViewModel
	{
		private IDataSourceViewModel _currentDataSource;
		private IEnumerable<ILogEntryFilter> _quickFilterChain;

		public event PropertyChangedEventHandler PropertyChanged;

		public IDataSourceViewModel CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				EmitPropertyChanged();

				if (_currentDataSource != null)
					_currentDataSource.QuickFilterChain = _quickFilterChain;
			}
		}

		public IEnumerable<ILogEntryFilter> QuickFilterChain
		{
			get { return _quickFilterChain; }
			set
			{
				if (value == _quickFilterChain)
					return;

				_quickFilterChain = value;
				EmitPropertyChanged();

				if (_currentDataSource != null)
					_currentDataSource.QuickFilterChain = value;
			}
		}

		public abstract void Update();

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
