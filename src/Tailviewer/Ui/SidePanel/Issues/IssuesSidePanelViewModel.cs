using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using log4net;
using Metrolib;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;

namespace Tailviewer.Ui.SidePanel.Issues
{
	internal sealed class IssuesSidePanelViewModel
		: AbstractSidePanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<ILogFileIssuesPlugin> _plugins;
		private readonly Dictionary<IDataSource, IssuesViewModel> _viewModels;
		private readonly IServiceContainer _services;
		private IDataSource _currentDataSource;
		private IssuesViewModel _currentIssues;
		private int _issueCount;
		private string _emptyStatement;
		private string _emptyExplanation;

		public IssuesSidePanelViewModel(IServiceContainer services)
		{
			_services = services;
			_plugins = services.Retrieve<IPluginLoader>()
			                   .LoadAllOfType<ILogFileIssuesPlugin>()
			                   .Select(x => new NoThrowLogFileIssuePlugin(x)).ToList();
			_viewModels = new Dictionary<IDataSource, IssuesViewModel>();
			UpdateEmptyReason();
		}

		#region Overrides of AbstractSidePanelViewModel

		public override Geometry Icon
		{
			get { return Icons.AlertCircleOutline; }
		}

		public override string Id
		{
			get { return "issues"; }
		}

		public IDataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				EmitPropertyChanged();

				CurrentIssues = GetOrCreateViewModel(value);
			}
		}

		public IssuesViewModel CurrentIssues
		{
			get { return _currentIssues; }
			private set
			{
				if (value == _currentIssues)
					return;

				_currentIssues = value;
				EmitPropertyChanged();
				UpdateEmptyReason();
			}
		}

		private int IssueCount
		{
			set
			{
				if (value == _issueCount)
					return;

				_issueCount = value;
				QuickInfo = $"{_issueCount} issue(s)";
			}
		}

		public override void Update()
		{
			if (_currentIssues != null && IsSelected)
				_currentIssues.Update();

			IssueCount = _currentIssues?.Count ?? 0;
		}

		#endregion

		public string EmptyStatement
		{
			get { return _emptyStatement; }
			private set
			{
				if (value == _emptyStatement)
					return;

				_emptyStatement = value;
				EmitPropertyChanged();
			}
		}

		public string EmptyExplanation
		{
			get { return _emptyExplanation; }
			private set
			{
				if (value == _emptyExplanation)
					return;

				_emptyExplanation = value;
				EmitPropertyChanged();
			}
		}

		private IssuesViewModel GetOrCreateViewModel(IDataSource dataSource)
		{
			if (dataSource == null)
				return null;

			if (!_viewModels.TryGetValue(dataSource, out var viewModel))
			{
				viewModel = CreateViewModel(dataSource);
				_viewModels.Add(dataSource, viewModel);
			}

			return viewModel;
		}

		private IssuesViewModel CreateViewModel(IDataSource dataSource)
		{
			var plugin = FindMatchingPlugin(dataSource);
			if (plugin == null)
				return null;

			return new IssuesViewModel(plugin.CreateAnalyser(_services, dataSource.UnfilteredLogSource),
			                           _services.Retrieve<INavigationService>());
		}

		private ILogFileIssuesPlugin FindMatchingPlugin(IDataSource dataSource)
		{
			IReadOnlyList<ILogFileIssuesPlugin> plugins;
			if (dataSource is IMultiDataSource multi)
			{
				var children = multi.OriginalSources ?? Enumerable.Empty<IDataSource>();
				plugins = children.SelectMany(x => FindMatchingPlugins(x.UnfilteredLogSource.GetProperty(Properties.Format))).ToList();
			}
			else
			{
				plugins = FindMatchingPlugins(dataSource.UnfilteredLogSource.GetProperty(Properties.Format));
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

		private IReadOnlyList<ILogFileIssuesPlugin> FindMatchingPlugins(ILogFileFormat format)
		{
			return _plugins.Where(x => Matches(x, format)).ToList();
		}

		private bool Matches(ILogFileIssuesPlugin plugin, ILogFileFormat format)
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

		private void UpdateEmptyReason()
		{
			if (CurrentIssues != null)
			{
				EmptyStatement = null;
				EmptyExplanation = null;
			}
			else
			{
				EmptyStatement = "No issues plugin installed";
				EmptyExplanation =
					"There is no plugin installed which is able to present a list of issues for the current data source's log format.";
			}
		}
	}
}