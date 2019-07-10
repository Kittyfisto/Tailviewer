using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Archiver.Plugins.Description;
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
		private readonly IReadOnlyList<PluginViewModel> _plugins;
		private readonly DelegateCommand2 _openPluginFolderCommand;
		private readonly DelegateCommand2 _updatePluginsCommand;

		public PluginsMainPanelViewModel(IApplicationSettings applicationSettings,
										 IDispatcher dispatcher,
										 IPluginUpdater pluginUpdater,
										 IEnumerable<IPluginDescription> plugins)
			: base(applicationSettings)
		{
			_applicationSettings = applicationSettings;
			_dispatcher = dispatcher;
			_pluginUpdater = pluginUpdater;
			_plugins = plugins.Select(x => new PluginViewModel(x)).ToList();
			_openPluginFolderCommand = new DelegateCommand2(OpenPluginFolder);
			_updatePluginsCommand = new DelegateCommand2(UpdatePlugins);
		}

		public override IEnumerable<ISidePanelViewModel> SidePanels => Enumerable.Empty<ISidePanelViewModel>();
		public ICommand UpdatePluginsCommand => _updatePluginsCommand;
		public IEnumerable<PluginViewModel> Plugins => _plugins;
		public string PluginPath => Constants.PluginPath;
		public bool HasPlugins => _plugins.Count > 0;

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
	}
}