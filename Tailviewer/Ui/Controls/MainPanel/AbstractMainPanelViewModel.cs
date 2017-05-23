using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public abstract class AbstractMainPanelViewModel
		: IMainPanelViewModel
	{
		private readonly IApplicationSettings _applicationSettings;
		private ISidePanelViewModel _selectedSidePanel;

		protected AbstractMainPanelViewModel(IApplicationSettings applicationSettings)
		{
			if (applicationSettings == null)
				throw new ArgumentNullException(nameof(applicationSettings));

			_applicationSettings = applicationSettings;
		}

		public event PropertyChangedEventHandler PropertyChanged;

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
