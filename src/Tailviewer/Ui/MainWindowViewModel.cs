using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Metrolib;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Settings;
using Tailviewer.Ui.About;
using Tailviewer.Ui.ActionCenter;
using Tailviewer.Ui.ContextMenu;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;
using Tailviewer.Ui.Settings;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui
{
	/// <summary>
	///    The view model governing the main window of the application.
	/// </summary>
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private readonly IServiceContainer _services;
		private readonly IApplicationSettings _applicationSettings;
		private readonly DelegateCommand _showLogCommand;

		#region ViewModels

		private readonly ActionCenterViewModel _actionCenterViewModel;
		private readonly AutoUpdateViewModel _autoUpdater;
		private readonly SettingsFlyoutViewModel _settings;

		#endregion

		#region Main Menu

		private readonly FileMenu _fileMenu;
		private IEnumerable<IMenuViewModel> _viewMenuItems;
		private readonly IEnumerable<IMenuViewModel> _helpMenuItems;

		#endregion

		#region Main Panel

		private readonly LogViewMainPanelViewModel _logViewPanel;

		#endregion

		private string _windowTitle;
		private string _windowTitleSuffix;
		private readonly DelegateCommand2 _showQuickNavigationCommand;
		private readonly DelegateCommand2 _showGoToLineCommand;
		private readonly ICommand _goToPreviousDataSourceCommand;
		private readonly ICommand _goToNextDataSourceCommand;

		private string _leftSidePanelExpanderTooltip;
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

			_services = services;
			_applicationSettings = settings;

			_settings = new SettingsFlyoutViewModel(settings, services);
			_actionCenterViewModel = new ActionCenterViewModel(services.Retrieve<IDispatcher>(), actionCenter);

			_logViewPanel = new LogViewMainPanelViewModel(services,
			                                              actionCenter,
			                                              dataSources,
			                                              quickFilters,
			                                              services.Retrieve<IHighlighters>(),
			                                              _applicationSettings);
			
			WindowTitle = _logViewPanel.WindowTitle;
			WindowTitleSuffix = _logViewPanel.WindowTitleSuffix;

			((NavigationService) services.Retrieve<INavigationService>()).LogViewer = _logViewPanel;

			_fileMenu = new FileMenu(new DelegateCommand2(AddDataSourceFromFile),
			                         new DelegateCommand2(AddDataSourceFromFolder),
			                         _logViewPanel.DataSources.RemoveCurrentDataSourceCommand,
			                         _logViewPanel.DataSources.RemoveAllDataSourcesCommand,
			                         new DelegateCommand2(ShowSettings),
			                         new DelegateCommand2(Exit));

			_logViewPanel.PropertyChanged += LogViewPanelOnPropertyChanged;

			var timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(100)
			};
			timer.Tick += TimerOnTick;
			timer.Start();
			
			_autoUpdater = new AutoUpdateViewModel(updater, settings.AutoUpdate, services.Retrieve<IDispatcher>());
			_showLogCommand = new DelegateCommand(ShowLog);
			_showGoToLineCommand = new DelegateCommand2(ShowGoToLine);
			_showQuickNavigationCommand = new DelegateCommand2(ShowQuickNavigation);
			_goToNextDataSourceCommand = new DelegateCommand2(GoToNextDataSource);
			_goToPreviousDataSourceCommand = new DelegateCommand2(GoToPreviousDataSource);

			_helpMenuItems = new[]
			{
				new CommandMenuViewModel(AutoUpdater.CheckForUpdatesCommand)
				{
					Header = "Check For Updates"
				},
				new CommandMenuViewModel(ShowLogCommand)
				{
					Header = "Show Log"
				},
				null,
				new CommandMenuViewModel(new DelegateCommand2(ShowAboutFlyout))
				{
					Header = "About"
				}
			};
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

		private void ShowQuickNavigation()
		{
			_logViewPanel.ShowQuickNavigation = true;
		}

		private void ShowSettings()
		{
			CurrentFlyout = new SettingsFlyoutViewModel(_applicationSettings, _services);
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
		public ICommand ShowLogCommand => _showLogCommand;
		public ICommand ShowQuickNavigationCommand => _showQuickNavigationCommand;
		public ICommand GoToNextDataSourceCommand => _goToNextDataSourceCommand;
		public ICommand GoToPreviousDataSourceCommand => _goToPreviousDataSourceCommand;
		public ICommand ShowGoToLineCommand => _showGoToLineCommand;
		public IEnumerable<IMenuViewModel> FileMenuItems => _fileMenu.Items;

		public IEnumerable<IMenuViewModel> ViewMenuItems
		{
			get{return _viewMenuItems;}
			private set
			{
				if (value == _viewMenuItems)
					return;

				_viewMenuItems = value;
				EmitPropertyChanged();
			}
		}
		public IEnumerable<IMenuViewModel> HelpMenuItems => _helpMenuItems;
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
					_fileMenu.CurrentDataSource = _logViewPanel.CurrentDataSource;
					ViewMenuItems = _logViewPanel.CurrentDataSource?.ViewMenuItems;
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

		public string LeftSidePanelExpanderTooltip
		{
			get { return _leftSidePanelExpanderTooltip; }
			set
			{
				if (value == _leftSidePanelExpanderTooltip)
					return;

				_leftSidePanelExpanderTooltip = value;
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

		private void ShowAboutFlyout()
		{
			CurrentFlyout = new AboutFlyoutViewModel();
		}
	}
}
