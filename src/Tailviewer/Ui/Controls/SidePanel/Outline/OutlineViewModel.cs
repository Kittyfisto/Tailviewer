using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using log4net;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Outline;
using Exception = System.Exception;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	public sealed class OutlineViewModel
		: AbstractSidePanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private readonly IServiceContainer _services;
		private readonly IReadOnlyList<ILogFileOutlinePlugin> _plugins;
		private readonly Dictionary<IDataSource, LogFileOutlineViewModelProxy> _viewModelsByDataSource;

		private IDataSource _currentDataSource;
		private FrameworkElement _currentContent;

		public OutlineViewModel(IServiceContainer services)
		{
			_services = services;
			_plugins = services.Retrieve<IPluginLoader>().LoadAllOfType<ILogFileOutlinePlugin>();
			Tooltip = "Show outline of the current log file";
			_viewModelsByDataSource = new Dictionary<IDataSource, LogFileOutlineViewModelProxy>();
		}

		#region Overrides of AbstractSidePanelViewModel

		public override Geometry Icon
		{
			get { return Icons.FileDocumentOutline; }
		}

		public override string Id
		{
			get { return "synopsis"; }
		}

		public IDataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				CurrentContent = GetContentFor(value);
			}
		}

		public FrameworkElement CurrentContent
		{
			get { return _currentContent; }
			set
			{
				if (Equals(value, _currentContent))
					return;

				_currentContent = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
			
		}

		#endregion

		private FrameworkElement GetContentFor(IDataSource dataSource)
		{
			if (!_viewModelsByDataSource.TryGetValue(dataSource, out var viewModel))
			{
				viewModel = TryCreateViewModelFor(dataSource);
				_viewModelsByDataSource.Add(dataSource, viewModel);
			}

			return viewModel?.TryCreateContent();
		}

		[Pure]
		private LogFileOutlineViewModelProxy TryCreateViewModelFor(IDataSource dataSource)
		{
			var fileName = Path.GetFileName(dataSource.FullFileName);
			var plugin = FindMatchingPlugin(fileName);
			if (plugin == null)
				return null;

			return new LogFileOutlineViewModelProxy(plugin, _services, dataSource.UnfilteredLogFile);
		}

		private ILogFileOutlinePlugin FindMatchingPlugin(string fileName)
		{
			var plugins = FindMatchingPlugins(fileName);
			if (plugins.Count == 0)
				return null;

			if (plugins.Count > 1)
			{
				Log.WarnFormat("There are multiple plugins which claim to provide an outline for '{0}', selecting the first one:\r\n{1}",
				               fileName,
				               string.Join("\r\n", plugins.Select(x => string.Format("    {0}", x.GetType().FullName))));
			}

			return plugins[0];
		}

		private IReadOnlyList<ILogFileOutlinePlugin> FindMatchingPlugins(string fileName)
		{
			return _plugins.Where(x => Matches(x, fileName)).ToList();
		}

		private bool Matches(ILogFileOutlinePlugin plugin, string fileName)
		{
			return plugin.SupportedFileNames.Any(x => x.IsMatch(fileName));
		}
	}
}
