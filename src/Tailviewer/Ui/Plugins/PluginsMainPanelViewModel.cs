using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Archiver.Repository;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Settings;
using Tailviewer.Ui.SidePanel;

namespace Tailviewer.Ui.Plugins
{
	/// <summary>
	///     Represents the plugin page (displays the list of plugins, etc...).
	/// </summary>
	public sealed class PluginsMainPanelViewModel
		: IFlyoutViewModel
	{
		private readonly IApplicationSettings _applicationSettings;
		private readonly IDispatcher _dispatcher;
		private readonly IPluginUpdater _pluginUpdater;
		private readonly IReadOnlyList<IPluginViewModel> _installedPlugins;
		private readonly ObservableCollection<IPluginViewModel> _allPlugins;
		private readonly DelegateCommand2 _openPluginFolderCommand;
		private readonly DelegateCommand2 _updatePluginsCommand;
		private readonly DelegateCommand2 _refreshPluginsCommand;
		private bool _isRefreshingPlugins;
		private string _showAllPluginsError;
		private string _showAllPluginsErrorDescription;
		private string _pluginUpdateMessage;

		public PluginsMainPanelViewModel(IApplicationSettings applicationSettings,
										 IDispatcher dispatcher,
										 IPluginUpdater pluginUpdater,
										 IEnumerable<IPluginDescription> plugins)
		{
			_applicationSettings = applicationSettings;
			_dispatcher = dispatcher;
			_pluginUpdater = pluginUpdater;
			_installedPlugins = plugins.Select(x => new InstalledPluginViewModel(x)).ToList();
			_allPlugins = new ObservableCollection<IPluginViewModel>();
			_openPluginFolderCommand = new DelegateCommand2(OpenPluginFolder);
			_updatePluginsCommand = new DelegateCommand2(UpdatePlugins);
			_refreshPluginsCommand = new DelegateCommand2(RefreshPlugins);

			if (!HasPluginRepositoryConfigured)
			{
				ShowAllPluginsError = "No plugin repository configured!";
				ShowAllPluginsErrorDescription = "In order to download/update plugins, you have to configure tailviewer to use a plugin repository. Please note that there is no globally available plugin repository. This feature is currently intended for organizations to deploy their own private repositories.";
			}
			else
			{
				ShowAllPluginsError = null;
				ShowAllPluginsErrorDescription = null;
				RefreshPlugins();
			}
		}

		public IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();
		public ICommand UpdatePluginsCommand => _updatePluginsCommand;
		public ICommand RefreshPluginsCommand => _refreshPluginsCommand;
		public IEnumerable<IPluginViewModel> Plugins => _installedPlugins;
		public IEnumerable<IPluginViewModel> AllPlugins => _allPlugins;
		public string PluginPath => Constants.PluginPath;
		public bool HasPlugins => _installedPlugins.Count > 0;

		public bool HasPluginRepositoryConfigured => HasPlugins && _applicationSettings.AutoUpdate.PluginRepositories.Any();
		public ICommand OpenPluginFolderCommand => _openPluginFolderCommand;

		public string PluginUpdateMessage
		{
			get { return _pluginUpdateMessage; }
			set
			{
				_pluginUpdateMessage = value;
				EmitPropertyChanged();
			}
		}

		public bool IsRefreshingPlugins
		{
			get { return _isRefreshingPlugins; }
			private set
			{
				if (value == _isRefreshingPlugins)
					return;

				_isRefreshingPlugins = value;
				EmitPropertyChanged();
			}
		}


		public string ShowAllPluginsError
		{
			get { return _showAllPluginsError; }
			private set
			{
				if (value == _showAllPluginsError)
					return;

				_showAllPluginsError = value;
				EmitPropertyChanged();
			}
		}


		public string ShowAllPluginsErrorDescription
		{
			get { return _showAllPluginsErrorDescription; }
			private set
			{
				if (value == _showAllPluginsErrorDescription)
					return;

				_showAllPluginsErrorDescription = value;
				EmitPropertyChanged();
			}
		}

		public void Update()
		{
		}

		public string Name => "Plugins";

		private void OpenPluginFolder()
		{
			string argument = string.Format(@"{0}", Constants.PluginPath);
			Process.Start("explorer.exe", argument);
		}

		private void UpdatePlugins()
		{
			try
			{
				_updatePluginsCommand.CanBeExecuted = false;

				var repositories = _applicationSettings.AutoUpdate.PluginRepositories;
				var numUpdatedPlugins = _pluginUpdater.UpdatePluginsAsync(repositories);
				numUpdatedPlugins.ContinueWith(task => _dispatcher.BeginInvoke(() => OnPluginsUpdated(task)));
			}
			catch(Exception)
			{
				_updatePluginsCommand.CanBeExecuted = true;
				throw;
			}
		}

		private void OnPluginsUpdated(Task<int> task)
		{
			try
			{
				var numPluginsUpdated = task.Result;
				if (numPluginsUpdated == 0)
				{
					PluginUpdateMessage = "All plugins are up-to-date!";
				}
				else
				{
					PluginUpdateMessage = $"{numPluginsUpdated} plugin(s) were updated. Please restart {Constants.ApplicationTitle}!";
				}
			}
			catch (AggregateException e)
			{
				PluginUpdateMessage = $"An error occured while trying to update plugins: {e.InnerException?.Message}";
			}
			catch (Exception e)
			{
				PluginUpdateMessage = $"An error occured while trying to update plugins: {e.Message}";
			}
			finally
			{
				_updatePluginsCommand.CanBeExecuted = true;
			}
		}

		private void RefreshPlugins()
		{
			_allPlugins.Clear();
			var repositories = _applicationSettings.AutoUpdate.PluginRepositories;
			var allPlugins = _pluginUpdater.GetAllPluginsAsync(repositories);

			// Note: Ordering is important here!
			IsRefreshingPlugins = true;
			ShowAllPluginsError = null;
			ShowAllPluginsErrorDescription = null;
			allPlugins.ContinueWith(task => _dispatcher.BeginInvoke(() => OnPluginsQueried(task)));
		}

		private void OnPluginsQueried(Task<IReadOnlyList<PublishedPluginDescription>> task)
		{
			try
			{
				var plugins = task.Result;
				_allPlugins.Clear();
				foreach (var plugin in plugins)
				{
					_allPlugins.Add(new AvailablePluginViewModel(plugin, () => DownloadPlugin(plugin.Identifier)));
				}

				ShowAllPluginsError = null;
				ShowAllPluginsErrorDescription = null;
			}
			catch (Exception e)
			{
				ShowAllPluginsError = "Could not retrieve plugins from repository :(";

				var description = e is AggregateException a ? a.InnerException.Message : e.Message;
				ShowAllPluginsErrorDescription = description;
			}
			finally
			{
				IsRefreshingPlugins = false;
			}
		}

		private void DownloadPlugin(PluginIdentifier plugin)
		{
			var repositories = _applicationSettings.AutoUpdate.PluginRepositories;
			var task = _pluginUpdater.DownloadPluginAsync(repositories, plugin);
			task.ContinueWith(t => _dispatcher.BeginInvoke(() => OnPluginDownloaded(t)));
		}

		private void OnPluginDownloaded(Task task)
		{
			var exception = task.Exception;
		}

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}