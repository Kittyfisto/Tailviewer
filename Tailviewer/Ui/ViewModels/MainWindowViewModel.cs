using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.ActionCenter;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.Controls.MainPanel.About;
using Tailviewer.Ui.Controls.MainPanel.Analyse;
using Tailviewer.Ui.Controls.MainPanel.Plugins;
using Tailviewer.Ui.Controls.MainPanel.Settings;
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
		private readonly SettingsMainPanelViewModel _settings;

		#endregion

		#region Main Panel

		private readonly AnalyseMainPanelEntry _analyseEntry;
		private readonly LogViewMainPanelEntry _rawEntry;
		private readonly IMainPanelEntry _pluginsEntry;
		private readonly IMainPanelEntry _settingsEntry;
		private readonly IMainPanelEntry _aboutEntry;
		private readonly AnalyseMainPanelViewModel _analysePanel;
		private readonly LogViewMainPanelViewModel _logViewPanel;
		private readonly IMainPanelEntry[] _topEntries;
		private IMainPanelEntry _selectedTopEntry;
		private readonly IMainPanelEntry[] _bottomEntries;
		private IMainPanelEntry _selectedBottomEntry;
		private IMainPanelViewModel _selectedMainPanel;

		#endregion

		private string _windowTitle;
		private string _windowTitleSuffix;
		private readonly IEnumerable<IPluginDescription> _plugins;


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

			_plugins = plugins;
			_settings = new SettingsMainPanelViewModel(settings);
			_actionCenterViewModel = new ActionCenterViewModel(dispatcher, actionCenter);

			_analysePanel = new AnalyseMainPanelViewModel(_applicationSettings);
			_analysePanel.PropertyChanged += AnalysePanelOnPropertyChanged;

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
			_showLogCommand = new DelegateCommand(ShowLog);

			_analyseEntry = new AnalyseMainPanelEntry();
			_rawEntry = new LogViewMainPanelEntry();
			_topEntries = new IMainPanelEntry[]
			{
				_analyseEntry,
				_rawEntry
			};

			_settingsEntry = new SettingsMainPanelEntry();
			_pluginsEntry = new PluginsMainPanelEntry();
			_aboutEntry = new AboutMainPanelEntry();
			_bottomEntries = new[]
			{
				_settingsEntry,
				_pluginsEntry,
				_aboutEntry
			};

			var selectedTopEntry = _topEntries.FirstOrDefault(x => x.Id == _applicationSettings.MainWindow.SelectedMainPanel);
			var selectedBottomEntry = _bottomEntries.FirstOrDefault(x => x.Id == _applicationSettings.MainWindow.SelectedMainPanel);

			if (selectedTopEntry != null)
			{
				SelectedTopEntry = selectedTopEntry;
			}
			else if (selectedBottomEntry != null)
			{
				SelectedBottomEntry = selectedBottomEntry;
			}
			else
			{
				SelectedTopEntry = _rawEntry;
			}
		}

		private void AnalysePanelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(AnalyseMainPanelViewModel.WindowTitle):
					if (SelectedMainPanel == _analysePanel)
						WindowTitle = _analysePanel.WindowTitle;
					break;

				case nameof(AnalyseMainPanelViewModel.WindowTitleSuffix):
					if (SelectedMainPanel == _analysePanel)
						WindowTitleSuffix = _analysePanel.WindowTitleSuffix;
					break;
			}
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
				case nameof(LogViewMainPanelViewModel.WindowTitle):
					if (SelectedMainPanel == _logViewPanel)
						WindowTitle = _logViewPanel.WindowTitle;
					break;

				case nameof(LogViewMainPanelViewModel.WindowTitleSuffix):
					if (SelectedMainPanel == _logViewPanel)
						WindowTitleSuffix = _logViewPanel.WindowTitleSuffix;
					break;
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

		public IEnumerable<IMainPanelEntry> TopEntries => _topEntries;
		public IEnumerable<IMainPanelEntry> BottomEntries => _bottomEntries;

		public IMainPanelEntry SelectedTopEntry
		{
			get { return _selectedTopEntry; }
			set
			{
				if (value == _selectedTopEntry)
					return;

				_selectedTopEntry = value;
				EmitPropertyChanged();

				if (value == _analyseEntry)
				{
					SelectedMainPanel = _analysePanel;
					WindowTitle = _analysePanel.WindowTitle;
					WindowTitleSuffix = _analysePanel.WindowTitleSuffix;
				}
				else if (value == _rawEntry)
				{
					SelectedMainPanel = _logViewPanel;
					WindowTitle = _logViewPanel.WindowTitle;
					WindowTitleSuffix = _logViewPanel.WindowTitleSuffix;
				}

				if (value != null)
				{
					_applicationSettings.MainWindow.SelectedMainPanel = value.Id;
					SelectedBottomEntry = null;
				}
			}
		}

		public IMainPanelEntry SelectedBottomEntry
		{
			get { return _selectedBottomEntry; }
			set
			{
				if (value == _selectedBottomEntry)
					return;

				_selectedBottomEntry = value;
				EmitPropertyChanged();

				if (value == _settingsEntry)
				{
					SelectedMainPanel = _settings;
					WindowTitle = Constants.MainWindowTitle;
					WindowTitleSuffix = "Settings";
				}
				else if (value == _pluginsEntry)
				{
					SelectedMainPanel = new PluginsMainPanelViewModel(_applicationSettings, _plugins);
					WindowTitle = Constants.MainWindowTitle;
					WindowTitleSuffix = "Plugins";
				}
				else if (value == _aboutEntry)
				{
					SelectedMainPanel = new AboutMainPanelViewModel(_applicationSettings);
					WindowTitle = Constants.MainWindowTitle;
					WindowTitleSuffix = "About";
				}

				if (value != null)
				{
					_applicationSettings.MainWindow.SelectedMainPanel = value.Id;
					SelectedTopEntry = null;
				}
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