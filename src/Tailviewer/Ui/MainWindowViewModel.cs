using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Metrolib;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Collections;
using Tailviewer.Settings;
using Tailviewer.Ui.About;
using Tailviewer.Ui.ActionCenter;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;
using Tailviewer.Ui.Menu;
using Tailviewer.Ui.Plugins;
using Tailviewer.Ui.Settings;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui
{
	/// <summary>
	///    The view model governing the main window of the application.
	/// </summary>
	public sealed class MainWindowViewModel
		: IMainWindowViewModel
	{
		#region ViewModels

		private readonly ActionCenterViewModel _actionCenterViewModel;
		private readonly AutoUpdateViewModel _autoUpdater;
		private readonly SettingsFlyoutViewModel _settings;
		private readonly PluginsMainPanelViewModel _plugins;

		#endregion

		#region Main Menu

		
		private readonly MainMenu _mainMenu;

		#endregion

		#region Main Panel

		private readonly LogViewMainPanelViewModel _logViewPanel;

		#endregion

		private string _windowTitle;
		private string _windowTitleSuffix;

		private IFlyoutViewModel _currentFlyout;

		public MainWindowViewModel(IServiceContainer services,
		                           IApplicationSettings settings,
		                           DataSources dataSources,
		                           QuickFilters quickFilters,
		                           IActionCenter actionCenter,
		                           IAutoUpdater updater)
		{
			if (dataSources == null) throw new ArgumentNullException(nameof(dataSources));
			if (quickFilters == null) throw new ArgumentNullException(nameof(quickFilters));
			if (updater == null) throw new ArgumentNullException(nameof(updater));

			var services1 = services;
			var applicationSettings = settings;

			_plugins = new PluginsMainPanelViewModel(applicationSettings,
			                                         services1.Retrieve<IDispatcher>(),
			                                         services1.Retrieve<IPluginUpdater>(),
			                                         services1.Retrieve<IPluginLoader>().Plugins);
			_settings = new SettingsFlyoutViewModel(settings, services);
			_actionCenterViewModel = new ActionCenterViewModel(services.Retrieve<IDispatcher>(), actionCenter);

			_logViewPanel = new LogViewMainPanelViewModel(services,
			                                              actionCenter,
			                                              dataSources,
			                                              quickFilters,
			                                              services.Retrieve<IHighlighters>(),
			                                              applicationSettings);
			WindowTitle = _logViewPanel.WindowTitle;
			WindowTitleSuffix = _logViewPanel.WindowTitleSuffix;

			((NavigationService) services.Retrieve<INavigationService>()).LogViewer = _logViewPanel;

			_logViewPanel.PropertyChanged += LogViewPanelOnPropertyChanged;

			var timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(100)
			};
			timer.Tick += TimerOnTick;
			timer.Start();
			
			_autoUpdater = new AutoUpdateViewModel(updater, settings.AutoUpdate, services.Retrieve<IDispatcher>());

			var fileMenuViewModel = new FileMenuViewModel(new DelegateCommand2(AddDataSourceFromFile),
			                                              new DelegateCommand2(AddDataSourceFromFolder),
			                                              _logViewPanel.DataSources.RemoveCurrentDataSourceCommand,
			                                              _logViewPanel.DataSources.RemoveAllDataSourcesCommand,
			                                              new DelegateCommand2(ShowPlugins),
			                                              new DelegateCommand2(ShowSettings),
			                                              new DelegateCommand2(Exit));
			var editMenu = new EditMenuViewModel(new DelegateCommand2(ShowGoToLine),
			                                     new DelegateCommand2(ShowGoToDataSource),
			                                     new DelegateCommand2(GoToNextDataSource),
			                                     new DelegateCommand2(GoToPreviousDataSource),
			                                     _logViewPanel);
			var viewMenu = new ViewMenuViewModel();
			var helpMenu = new HelpMenuViewModel(new DelegateCommand2(ReportIssue),
			                                     new DelegateCommand2(SuggestFeature),
			                                     new DelegateCommand2(AskQuestion),
			                                     AutoUpdater.CheckForUpdatesCommand,
			                                     new DelegateCommand(ShowLog),
			                                     new DelegateCommand2(ShowAboutFlyout));
			_mainMenu = new MainMenu(fileMenuViewModel,
			                         editMenu,
			                         viewMenu,
			                         helpMenu);
			_mainMenu.CurrentDataSource = _logViewPanel.CurrentDataSource;
		}

		public IFlyoutViewModel CurrentFlyout
		{
			get { return _currentFlyout;}
			set
			{
				if (value == _currentFlyout)
					return;

				_currentFlyout = value;
				EmitPropertyChanged();
			}
		}

		private void AddDataSourceFromFile()
		{
			var dlg = new OpenFileDialog
			{
				Title = "Select log file to open",
				DefaultExt = ".log",
				Filter = "Log Files (*.log)|*.log|Txt Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
				Multiselect = true
			};

			if (dlg.ShowDialog() == true)
			{
				string[] selectedFiles = dlg.FileNames;
				foreach (string fileName in selectedFiles)
				{
					_logViewPanel.GetOrAddFile(fileName);
				}
			}
		}

		private void AddDataSourceFromFolder()
		{
			var dlg = new VistaFolderBrowserDialog
			{
				Description = "Select folder to open",
				UseDescriptionForTitle = true
			};

			if (dlg.ShowDialog() == true)
			{
				var folder = dlg.SelectedPath;
				_logViewPanel.GetOrAddFolder(folder);
			}
		}

		private void GoToNextDataSource()
		{
			_logViewPanel.GoToNextDataSource();
		}

		private void GoToPreviousDataSource()
		{
			_logViewPanel.GoToPreviousDataSource();
		}

		private void ShowGoToLine()
		{
			_logViewPanel.GoToLine.Show = true;
		}

		private void ShowGoToDataSource()
		{
			_logViewPanel.ShowQuickNavigation = true;
		}

		private void ShowPlugins()
		{
			CurrentFlyout = _plugins;
		}

		private void ShowSettings()
		{
			CurrentFlyout = _settings;
		}

		private void ShowLog()
		{
			_logViewPanel.GetOrAddPath(Constants.ApplicationLogFile);
		}

		private void Exit()
		{
			System.Windows.Application.Current.Shutdown();
		}

		public LogViewMainPanelViewModel LogViewPanel => _logViewPanel;

		public MainMenu MainMenu => _mainMenu;

		private void LogViewPanelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(LogViewMainPanelViewModel.WindowTitle):
					WindowTitle = _logViewPanel.WindowTitle;
					break;

				case nameof(LogViewMainPanelViewModel.WindowTitleSuffix):
					WindowTitleSuffix = _logViewPanel.WindowTitleSuffix;
					break;

				case nameof(LogViewMainPanelViewModel.CurrentDataSource):
					_mainMenu.CurrentDataSource = _logViewPanel.CurrentDataSource;
					
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
		
		public event PropertyChangedEventHandler PropertyChanged;

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			_logViewPanel?.Update();
			_actionCenterViewModel.Update();
		}

		public void AddFilesOrDirectories(string[] paths)
		{
			foreach (string path in paths)
			{
				AddFileOrDirectory(path);
			}
		}

		public IDataSourceViewModel AddFileOrDirectory(string file)
		{
			IDataSourceViewModel dataSource = _logViewPanel.GetOrAddPath(file);
			return dataSource;
		}

		public void OnDropped(IDataSourceViewModel source,
		                      IDataSourceViewModel dest,
		                      DataSourceDropType dropType)
		{
			_logViewPanel?.OnDropped(source, dest, dropType);
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ReportIssue()
		{
			Process.Start("https://github.com/Kittyfisto/Tailviewer/issues/new");
		}

		private void SuggestFeature()
		{
			Process.Start("https://github.com/Kittyfisto/Tailviewer/discussions/new");
		}

		private void AskQuestion()
		{
			Process.Start("https://github.com/Kittyfisto/Tailviewer/discussions/new");
		}

		private void ShowAboutFlyout()
		{
			CurrentFlyout = new AboutFlyoutViewModel();
		}

		#region Implementation of IMainWindowViewModel

		public IObservableCollection<KeyBindingCommand> KeyBindings => _mainMenu.KeyBindings;

		#endregion
	}
}
