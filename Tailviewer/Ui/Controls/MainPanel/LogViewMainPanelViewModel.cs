using System;
using System.ComponentModel;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class LogViewMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		public LogViewMainPanelViewModel(IActionCenter actionCenter,
			IApplicationSettings applicationSettings)
		{
			if (actionCenter == null)
				throw new ArgumentNullException(nameof(actionCenter));

			_actionCenter = actionCenter;
			_applicationSettings = applicationSettings;
			PropertyChanged += OnPropertyChanged;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(CurrentDataSource):
					OpenFile(CurrentDataSource);
					break;
			}
		}

		private void OpenFile(IDataSourceViewModel dataSource)
		{
			if (dataSource != null)
			{
				CurrentDataSource = dataSource;
				CurrentDataSourceLogView = new LogViewerViewModel(
					dataSource,
					_actionCenter,
					_applicationSettings);
			}
			else
			{
				CurrentDataSource = null;
				CurrentDataSourceLogView = null;
			}
		}

		private LogViewerViewModel _currentDataSourceLogView;
		private readonly IActionCenter _actionCenter;
		private readonly IApplicationSettings _applicationSettings;

		public LogViewerViewModel CurrentDataSourceLogView
		{
			get { return _currentDataSourceLogView; }
			set
			{
				if (_currentDataSourceLogView == value)
					return;

				_currentDataSourceLogView = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
			CurrentDataSourceLogView?.Update();
		}
	}
}