using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Core.Plugins;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.ActionCenter;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.Controls.Plugins;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		#region Dispatching

		private readonly DispatcherTimer _timer;

		#endregion

		private readonly IApplicationSettings _applicationSettings;
		private readonly DelegateCommand _showLogCommand;

		#region ViewModels

		private readonly ActionCenterViewModel _actionCenterViewModel;
		private readonly AutoUpdateViewModel _autoUpdater;
		private readonly SettingsViewModel _settings;
		private readonly PluginsViewModel _plugins;

		#endregion

		#region Main Panel

		private readonly AnalyseMainPanelEntry _analyseEntry;
		private readonly LogViewMainPanelEntry _rawEntry;
		private readonly IMainPanelEntry[] _entries;
		private readonly AnalyseMainPanelViewModel _analysePanel;
		private readonly LogViewMainPanelViewModel _logViewPanel;
		private IMainPanelEntry _selectedEntry;

		#endregion
		
		private string _windowTitle;
		private string _windowTitleSuffix;
		private IMainPanelViewModel _selectedMainPanel;

		public MainWindowViewModel(IApplicationSettings settings,
		                           DataSources dataSources,
		                           QuickFilters quickFilters,
		                           IActionCenter actionCenter,
		                           IAutoUpdater updater,
		                           IDispatcher dispatcher,
		                           IEnumerable<IPluginDescription> plugins)
		{
			if (dataSources == null) throw new ArgumentNullException(nameof(dataSources));
			if (quickFilters == null) throw new ArgumentNullException(nameof(quickFilters));
			if (updater == null) throw new ArgumentNullException(nameof(updater));
			if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

			_applicationSettings = settings;

			_settings = new SettingsViewModel(settings);
			_plugins = new PluginsViewModel(plugins);
			_actionCenterViewModel = new ActionCenterViewModel(dispatcher, actionCenter);

			_analysePanel = new AnalyseMainPanelViewModel(_applicationSettings);
			_logViewPanel = new LogViewMainPanelViewModel(actionCenter,
				dataSources,
				quickFilters,
				_applicationSettings);
			_logViewPanel.PropertyChanged += LogViewPanelOnPropertyChanged;

			_timer = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(100)
				};
			_timer.Tick += TimerOnTick;
			_timer.Start();

			_autoUpdater = new AutoUpdateViewModel(updater, settings.AutoUpdate, dispatcher);

			var dataSource = _logViewPanel.CurrentDataSource;
			OnDataSourceChanged(dataSource);

			_showLogCommand = new DelegateCommand(ShowLog);

			_analyseEntry = new AnalyseMainPanelEntry();
			_rawEntry = new LogViewMainPanelEntry();
			_entries = new IMainPanelEntry[]
			{
				//_analyseEntry,
				_rawEntry
			};
			SelectedEntry = _entries.FirstOrDefault(x => x.Id == _applicationSettings.MainWindow.SelectedMainPanel)
			                ?? _rawEntry;
		}

		private void ShowLog()
		{
			_logViewPanel.OpenFile(Constants.ApplicationLogFile);
		}

		public LogViewMainPanelViewModel LogViewPanel => _logViewPanel;
		public ICommand ShowLogCommand => _showLogCommand;

		private void LogViewPanelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(LogViewMainPanelViewModel.CurrentDataSource):
					var dataSource = _logViewPanel.CurrentDataSource;
					OnDataSourceChanged(dataSource);
					break;
			}
		}

		private void OnDataSourceChanged(IDataSourceViewModel dataSource)
		{
			if (dataSource != null)
			{
				WindowTitle = string.Format("{0} - {1}", Constants.MainWindowTitle, dataSource.DisplayName);
				WindowTitleSuffix = dataSource.DataSourceOrigin;
			}
			else
			{
				WindowTitle = Constants.MainWindowTitle;
				WindowTitleSuffix = null;
			}
		}

		public ActionCenterViewModel ActionCenter => _actionCenterViewModel;

		public AutoUpdateViewModel AutoUpdater => _autoUpdater;

		public string WindowTitle
		{
			get { return _windowTitle; }
			private set
			{
				if (value == _windowTitle)
					return;

				_windowTitle = value;
				EmitPropertyChanged();
			}
		}

		public string WindowTitleSuffix
		{
			get { return _windowTitleSuffix; }
			private set
			{
				if (value == _windowTitleSuffix)
					return;

				_windowTitleSuffix = value;
				EmitPropertyChanged();
			}
		}

		public SettingsViewModel Settings => _settings;

		public PluginsViewModel Plugins => _plugins;

		#region Main Panel

		public IMainPanelViewModel SelectedMainPanel
		{
			get { return _selectedMainPanel; }
			private set
			{
				if (value == _selectedMainPanel)
					return;

				_selectedMainPanel = value;
				EmitPropertyChanged();
			}
		}

		public IEnumerable<IMainPanelEntry> Entries => _entries;

		public IMainPanelEntry SelectedEntry
		{
			get { return _selectedEntry; }
			set
			{
				if (value == _selectedEntry)
					return;

				_selectedEntry = value;
				EmitPropertyChanged();

				if (value == _analyseEntry)
				{
					SelectedMainPanel = _analysePanel;
				}
				else if (value == _rawEntry)
				{
					SelectedMainPanel = _logViewPanel;
				}

				_applicationSettings.MainWindow.SelectedMainPanel = _selectedEntry?.Id;
			}
		}

		#endregion
		
		public event PropertyChangedEventHandler PropertyChanged;

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			_selectedMainPanel?.Update();
			_actionCenterViewModel.Update();
		}

		public void OpenFiles(string[] files)
		{
			foreach (string file in files)
			{
				OpenFile(file);
			}
		}

		public IDataSourceViewModel OpenFile(string file)
		{
			IDataSourceViewModel dataSource = _logViewPanel.OpenFile(file);
			return dataSource;
		}
		
		public bool CanBeDragged(IDataSourceViewModel source)
		{
			var panel = _selectedMainPanel as LogViewMainPanelViewModel;
			if (panel != null)
				return panel.CanBeDragged(source);

			return false;
		}

		public bool CanBeDropped(IDataSourceViewModel source,
		                         IDataSourceViewModel dest,
		                         DataSourceDropType dropType,
		                         out IDataSourceViewModel finalDest)
		{
			var panel = _selectedMainPanel as LogViewMainPanelViewModel;
			if (panel != null)
			{
				return panel.CanBeDropped(source, dest, dropType, out finalDest);
			}

			finalDest = null;
			return false;
		}

		public void OnDropped(IDataSourceViewModel source,
		                      IDataSourceViewModel dest,
		                      DataSourceDropType dropType)
		{
			var panel = _selectedMainPanel as LogViewMainPanelViewModel;
			panel?.OnDropped(source, dest, dropType);
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}