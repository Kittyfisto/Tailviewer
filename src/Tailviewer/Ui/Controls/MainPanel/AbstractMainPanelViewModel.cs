using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.ViewModels;
using Tailviewer.Ui.ViewModels.ContextMenu;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public abstract class AbstractMainPanelViewModel
		: IMainPanelViewModel
	{
		private readonly IApplicationSettings _applicationSettings;
		private ISidePanelViewModel _selectedSidePanel;
		private IEnumerable<IMenuViewModel> _fileMenuItems;
		private IEnumerable<IMenuViewModel> _viewMenuItems;
		private ISearchViewModel _search;

		protected AbstractMainPanelViewModel(IApplicationSettings applicationSettings)
		{
			_applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public IEnumerable<IMenuViewModel> FileMenuItems
		{
			get {return _fileMenuItems;}
			protected set
			{
				if (Equals(value, _fileMenuItems))
					return;

				_fileMenuItems = value;
				EmitPropertyChanged();
			}
		}

		public IEnumerable<IMenuViewModel> ViewMenuItems
		{
			get {return _viewMenuItems;}
			protected set
			{
				if (Equals(value, _viewMenuItems))
					return;

				_viewMenuItems = value;
				EmitPropertyChanged();
			}
		}

		public ISearchViewModel Search
		{
			get { return _search; }
			set
			{
				if (value == _search)
					return;
				_search = value;
				EmitPropertyChanged();
			}
		}

		public abstract IEnumerable<ISidePanelViewModel> SidePanels { get; }

		public ISidePanelViewModel SelectedSidePanel
		{
			get { return _selectedSidePanel; }
			set
			{
				if (value == _selectedSidePanel)
					return;

				_selectedSidePanel = value;
				EmitPropertyChanged();

				_applicationSettings.MainWindow.SelectedSidePanel = value?.Id;
				_applicationSettings.SaveAsync();
			}
		}

		public abstract void Update();

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
