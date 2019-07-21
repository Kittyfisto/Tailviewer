using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using log4net;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Outline;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	public sealed class OutlineSidePanelViewModel
		: AbstractSidePanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private readonly IServiceContainer _services;
		private readonly IReadOnlyList<ILogFileOutlinePlugin> _plugins;
		private readonly Dictionary<IDataSource, IInternalLogFileOutlineViewModel> _viewModelsByDataSource;

		private IDataSource _currentDataSource;
		private FrameworkElement _currentContent;
		private IInternalLogFileOutlineViewModel _currentViewModel;

		public OutlineSidePanelViewModel(IServiceContainer services)
		{
			_services = services;
			_plugins = services.Retrieve<IPluginLoader>().LoadAllOfType<ILogFileOutlinePlugin>();
			Tooltip = "Show outline of the current log file";
			_viewModelsByDataSource = new Dictionary<IDataSource, IInternalLogFileOutlineViewModel>();
		}

		#region Overrides of AbstractSidePanelViewModel

		public override Geometry Icon
		{
			get { return Icons.FileDocumentOutline; }
		}

		public override string Id
		{
			get { return "outline"; }
		}

		public IDataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				CurrentViewModel = GetOrCreateViewModel(value);
			}
		}

		private IInternalLogFileOutlineViewModel CurrentViewModel
		{
			get { return _currentViewModel; }
			set
			{
				if (Equals(value, _currentViewModel))
					return;

				_currentViewModel = value;
				EmitPropertyChanged();

				CurrentContent = value?.TryCreateContent();
			}
		}

		public FrameworkElement CurrentContent
		{
			get { return _currentContent; }
			private set
			{
				if (Equals(value, _currentContent))
					return;

				_currentContent = value;
				EmitPropertyChanged();
			}
		}

		public override void Update()
		{
			if (IsSelected)
				CurrentViewModel?.Update();
		}

		#endregion

		private IInternalLogFileOutlineViewModel GetOrCreateViewModel(IDataSource dataSource)
		{
			if (!_viewModelsByDataSource.TryGetValue(dataSource, out var viewModel))
			{
				viewModel = TryCreateViewModelFor(dataSource);
				_viewModelsByDataSource.Add(dataSource, viewModel);
			}

			return viewModel;
		}

		[Pure]
		private IInternalLogFileOutlineViewModel TryCreateViewModelFor(IDataSource dataSource)
		{
			var logFile = dataSource.UnfilteredLogFile;

			var plugin = FindMatchingPlugin(dataSource);
			if (plugin == null)
				return new DefaultLogFileOutlineViewModel(logFile);

			return new LogFileOutlineViewModelProxy(plugin, _services, logFile);
		}

		private ILogFileOutlinePlugin FindMatchingPlugin(IDataSource dataSource)
		{
			IReadOnlyList<ILogFileOutlinePlugin> plugins;
			if (dataSource is IMultiDataSource multi)
			{
				var children = multi.OriginalSources ?? Enumerable.Empty<IDataSource>();
				plugins = children.SelectMany(x => FindMatchingPlugins(x.UnfilteredLogFile.GetValue(LogFileProperties.Format))).ToList();
			}
			else
			{
				plugins = FindMatchingPlugins(dataSource.UnfilteredLogFile.GetValue(LogFileProperties.Format));
			}

			if (plugins.Count == 0)
				return null;

			if (plugins.Count > 1)
			{
				Log.WarnFormat("There are multiple plugins which claim to provide an outline for '{0}', selecting the first one:\r\n{1}",
				               dataSource.FullFileName,
				               string.Join("\r\n", plugins.Select(x => string.Format("    {0}", x.GetType().FullName))));
			}

			return plugins[0];
		}

		private IReadOnlyList<ILogFileOutlinePlugin> FindMatchingPlugins(ILogFileFormat format)
		{
			return _plugins.Where(x => Matches(x, format)).ToList();
		}

		private bool Matches(ILogFileOutlinePlugin plugin, ILogFileFormat format)
		{
			try
			{
				return plugin.SupportedFormats.Any(x => Equals(x, format));
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return false;
			}
		}
	}
}
