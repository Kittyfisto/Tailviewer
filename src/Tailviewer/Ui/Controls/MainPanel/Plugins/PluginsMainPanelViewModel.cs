using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Archiver.Repository;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Plugins
{
	/// <summary>
	///     Represents the plugin page (displays the list of plugins, etc...).
	/// </summary>
	public sealed class PluginsMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly IApplicationSettings _applicationSettings;
		private readonly IDispatcher _dispatcher;
		private readonly IPluginUpdater _pluginUpdater;
		private readonly IReadOnlyList<IPluginViewModel> _installedPlugins;
		private readonly ObservableCollection<IPluginViewModel> _allPlugins;
		private readonly DelegateCommand2 _openPluginFolderCommand;
		private readonly DelegateCommand2 _updatePluginsCommand;
		private bool _showInstalledPlugins;
		private bool _showAllPlugins;

		public PluginsMainPanelViewModel(IApplicationSettings applicationSettings,
										 IDispatcher dispatcher,
										 IPluginUpdater pluginUpdater,
										 IEnumerable<IPluginDescription> plugins)
			: base(applicationSettings)
		{
			_showInstalledPlugins = true;

			_applicationSettings = applicationSettings;
			_dispatcher = dispatcher;
			_pluginUpdater = pluginUpdater;
			_installedPlugins = plugins.Select(x => new InstalledPluginViewModel(x)).ToList();
			_allPlugins = new ObservableCollection<IPluginViewModel>();
			_openPluginFolderCommand = new DelegateCommand2(OpenPluginFolder);
			_updatePluginsCommand = new DelegateCommand2(UpdatePlugins);
		}

		public bool ShowInstalledPlugins
		{
			get { return _showInstalledPlugins; }
			set
			{
				if (value == _showInstalledPlugins)
					return;

				_showInstalledPlugins = value;
				EmitPropertyChanged();

				ShowAllPlugins = !value;
			}
		}


		public bool ShowAllPlugins
		{
			get { return _showAllPlugins; }
			set
			{
				if (value == _showAllPlugins)
					return;

				_showAllPlugins = value;
				EmitPropertyChanged();

				ShowInstalledPlugins = !value;

				if (value)
				{
					QueryAllPlugins();
				}
			}
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();
		public ICommand UpdatePluginsCommand => _updatePluginsCommand;
		public IEnumerable<IPluginViewModel> Plugins => _installedPlugins;
		public IEnumerable<IPluginViewModel> AllPlugins => _allPlugins;
		public string PluginPath => Constants.PluginPath;
		public bool HasPlugins => _installedPlugins.Count > 0;

		public bool CanUpdatePlugins => HasPlugins && _applicationSettings.AutoUpdate.PluginRepositories.Any();
		public ICommand OpenPluginFolderCommand => _openPluginFolderCommand;
		private string _pluginUpdateMessage;

		public string PluginUpdateMessage
		{
			get { return _pluginUpdateMessage; }
			set
			{
				_pluginUpdateMessage = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
		}

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

		private void QueryAllPlugins()
		{
			_allPlugins.Clear();
			var repositories = _applicationSettings.AutoUpdate.PluginRepositories;
			var allPlugins = _pluginUpdater.GetAllPluginsAsync(repositories);
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
			}
			catch (Exception)
			{
				
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
	}
}