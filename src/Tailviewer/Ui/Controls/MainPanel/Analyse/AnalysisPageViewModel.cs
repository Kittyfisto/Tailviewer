using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Metrolib;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.Analysis.Layouts;
using Tailviewer.Templates.Analysis;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.Column;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.HorizontalWrap;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts.Row;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse
{
	/// <summary>
	///     Represents a single page of an analysis.
	///     A page consists of zero or more <see cref="IWidgetViewModel" />s which are
	///     presented using a <see cref="IWidgetLayoutViewModel" />.
	///     The layout of a page can be changed through <see cref="PageLayout" />.
	/// </summary>
	public sealed class AnalysisPageViewModel
		: IAnalysisPageViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly PageTemplate _template;
		private readonly IAnalysis _analysis;
		private readonly DelegateCommand _deletePageCommand;
		private readonly List<WidgetViewModelProxy> _widgets;
		private readonly Dictionary<WidgetViewModelProxy, IDataSourceAnalyser> _analysersPerWidget;
		private readonly IAnalysisStorage _analysisStorage;
		private readonly AnalysisId _id;

		private bool _canBeDeleted;
		private IWidgetLayoutViewModel _layout;
		private PageLayout _pageLayout;
		private bool _hasWidgets;

		public AnalysisPageViewModel(AnalysisId id,
		                             PageTemplate template,
		                             IAnalysis analysis,
		                             IAnalysisStorage analysisStorage,
		                             IPluginLoader pluginLoader)
		{
			_id = id;
			_template = template ?? throw new ArgumentNullException(nameof(template));
			_analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
			_analysisStorage = analysisStorage ?? throw new ArgumentNullException(nameof(analysisStorage));
			_deletePageCommand = new DelegateCommand(DeletePage, () => _canBeDeleted);
			_widgets = new List<WidgetViewModelProxy>();
			_analysersPerWidget = new Dictionary<WidgetViewModelProxy, IDataSourceAnalyser>();

			PageLayout = template.Layout?.PageLayout ?? PageLayout.WrapHorizontal;

			var widgetPlugins = LoadRelevantPlugins(pluginLoader);
			foreach (var widgetTemplate in template.Widgets)
			{
				if (TryGetAnalyser(widgetTemplate.AnalyserId, out var analyser))
				{
					if (widgetPlugins.TryGetValue(widgetTemplate.AnalyserPluginId, out var plugin))
					{
						AddExistingWidget(plugin, widgetTemplate, analyser);
					}
					else
					{
						Log.WarnFormat("Unable to find plugin widget factory '{0}', widget '{1} ({2})' will NOT be displayed",
						               widgetTemplate.AnalyserPluginId,
						               widgetTemplate.Title,
						               widgetTemplate.Id);
					}
				}
				else
				{
					Log.WarnFormat("Unable to find analyser factory '{0}', widget '{1} ({2})' will NOT be displayed",
					               widgetTemplate.AnalyserPluginId,
					               widgetTemplate.Title,
					               widgetTemplate.Id);
				}
			}
		}

		private bool TryGetAnalyser(AnalyserId id, out IDataSourceAnalyser analyser)
		{
			if (id == AnalyserId.Empty)
			{
				analyser = new NoAnalyser();
				return true;
			}

			if (_analysis.TryGetAnalyser(id, out analyser))
				return true;

			return false;
		}

		[Pure]
		private static IReadOnlyDictionary<AnalyserPluginId, IWidgetPlugin> LoadRelevantPlugins(IPluginLoader pluginLoader)
		{
			var plugins = pluginLoader.LoadAllOfType<IWidgetPlugin>();
			var pluginsById = new Dictionary<AnalyserPluginId, IWidgetPlugin>(plugins.Count);
			foreach (var plugin in plugins)
			{
				if (!pluginsById.TryGetValue(plugin.AnalyserId, out var existingPlugin))
				{
					pluginsById.Add(plugin.AnalyserId, plugin);
				}
				else
				{
					Log.WarnFormat("Ignoring plugin {0} with id {1}, there already is another plugin registered with the same id: {2}",
					               plugin.GetType(),
					               plugin.AnalyserId,
					               existingPlugin.GetType());
				}
			}

			return pluginsById;
		}

		public PageLayout PageLayout
		{
			get => _pageLayout;
			set
			{
				if (value == _pageLayout)
					return;

				_pageLayout = value;
				EmitPropertyChanged();

				switch (value)
				{
					case PageLayout.None:
						Layout = null;
						break;

					case PageLayout.WrapHorizontal:
						Layout = new HorizontalWrapWidgetLayoutViewModel(_template.Layout as HorizontalWidgetLayoutTemplate ?? new HorizontalWidgetLayoutTemplate());
						break;

					case PageLayout.Columns:
						Layout = new ColumnWidgetLayoutViewModel(_template.Layout as ColumnWidgetLayoutTemplate ?? new ColumnWidgetLayoutTemplate());
						break;

					case PageLayout.Rows:
						Layout = new RowWidgetLayoutViewModel(_template.Layout as RowWidgetLayoutTemplate ?? new RowWidgetLayoutTemplate());
						break;
				}
			}
		}

		public bool HasWidgets
		{
			get => _hasWidgets;
			private set
			{
				if (value == _hasWidgets)
					return;

				_hasWidgets = value;
				EmitPropertyChanged();
			}
		}

		public IWidgetLayoutViewModel Layout
		{
			get => _layout;
			private set
			{
				if (value == _layout)
					return;

				if (_layout != null)
				{
					_layout.RequestAdd -= LayoutOnRequestAddWidget;
				}

				_layout = value;

				if (_layout != null)
				{
					_layout.RequestAdd += LayoutOnRequestAddWidget;
				}

				EmitPropertyChanged();

				if (_layout != null)
					foreach (var widget in _widgets)
						_layout.Add(widget);

				HasWidgets = _widgets.Any();

				UpdateLayoutTemplate();
			}
		}

		private void UpdateLayoutTemplate()
		{
			var newLayoutTemplate = _layout?.Template;

			if (!Equals(_template.Layout, newLayoutTemplate))
			{
				_template.Layout = _layout?.Template;
				// Now that we've modified the template, we MUST save it...
				// as usual, we save it asynchronously because there's really no need
				// to block...
				_analysisStorage.SaveAsync(_id);
			}
		}

		private void LayoutOnRequestAddWidget(IWidgetPlugin plugin)
		{
			var viewConfiguration = CreateViewConfiguration(plugin);
			var configuration = plugin.DefaultAnalyserConfiguration?.Clone() as ILogAnalyserConfiguration;
			AddNewWidget(plugin, configuration, viewConfiguration);
		}

		private void AddNewWidget(IWidgetPlugin plugin, ILogAnalyserConfiguration configuration, IWidgetConfiguration viewConfiguration)
		{
			var analyser = CreateAnalyser(plugin.AnalyserId, configuration);

			var widgetTemplate = new WidgetTemplate
			{
				Id = WidgetId.CreateNew(),
				AnalyserId = analyser.Id,
				AnalyserPluginId = plugin.AnalyserId,
				Configuration = viewConfiguration
			};

			_template.Add(widgetTemplate);
			AddExistingWidget(plugin, widgetTemplate, analyser);

			// Now that we've modified the template, we should save the entire analysis
			// to disk once more.
			_analysisStorage.SaveAsync(_id);
		}

		/// <summary>
		/// Adds a widget (which is already part of the template) to this page.
		/// </summary>
		/// <param name="plugin"></param>
		/// <param name="widgetTemplate"></param>
		/// <param name="analyser"></param>
		private void AddExistingWidget(IWidgetPlugin plugin, IWidgetTemplate widgetTemplate, IDataSourceAnalyser analyser)
		{
			var widgetViewModel = new WidgetViewModelProxy(plugin, widgetTemplate, analyser);
			_analysersPerWidget.Add(widgetViewModel, analyser);

			Add(widgetViewModel);
		}

		private IWidgetConfiguration CreateViewConfiguration(IWidgetPlugin plugin)
		{
			try
			{
				return plugin.DefaultViewConfiguration?.Clone() as IWidgetConfiguration;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while creating view configuration for widget '{0}': {1}", plugin, e);
				return null;
			}
		}

		private IDataSourceAnalyser CreateAnalyser(AnalyserPluginId pluginId, ILogAnalyserConfiguration configuration)
		{
			try
			{
				if (pluginId != AnalyserPluginId.Empty)
				{
					return _analysis.Add(pluginId, configuration);
				}

				Log.DebugFormat("Widget '{0}' doesn't specify a log analyser, none will created", pluginId);
				return new NoAnalyser();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while creating log analyser for widget '{0}': {1}", pluginId, e);
				return new NoAnalyser(pluginId);
			}
		}

		private void Add(WidgetViewModelProxy widgetViewModel)
		{
			_widgets.Add(widgetViewModel);
			_layout?.Add(widgetViewModel);
			widgetViewModel.OnDelete += WidgetOnDelete;
			widgetViewModel.TemplateModified += WidgetTemplateModified;
			HasWidgets = _widgets.Any();
		}

		private void WidgetOnDelete(WidgetViewModelProxy widget)
		{
			Remove(widget);
		}

		private void WidgetTemplateModified(WidgetViewModelProxy obj)
		{
			// TODO: This might be a good place to prevent plugins from bringing TV to its knees by firing this event thousands of times per second
			_analysisStorage.SaveAsync(_id);
		}

		public void Remove(WidgetViewModelProxy widget)
		{
			_widgets.Remove(widget);
			_layout?.Remove(widget);
			_template.Remove(widget.Template);
			_analysisStorage.SaveAsync(_id);

			IDataSourceAnalyser analyser;
			if (_analysersPerWidget.TryGetValue(widget, out analyser))
			{
				_analysersPerWidget.Remove(widget);
				_analysis.Remove(analyser);
			}

			widget.OnDelete -= WidgetOnDelete;
			HasWidgets = _widgets.Any();
		}

		public string Name
		{
			get => _template.Title;
			set
			{
				if (value == _template.Title)
					return;

				_template.Title = value;
				EmitPropertyChanged();
			}
		}

		public ICommand DeletePageCommand => _deletePageCommand;

		public bool CanBeDeleted
		{
			get => _canBeDeleted;
			set
			{
				_canBeDeleted = value;
				_deletePageCommand.RaiseCanExecuteChanged();
			}
		}

		public PageTemplate Template => _template;

		public event PropertyChangedEventHandler PropertyChanged;

		private void DeletePage()
		{
			OnDelete?.Invoke(this);
		}

		public event Action<AnalysisPageViewModel> OnDelete;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Update()
		{
			foreach(var widget in _widgets)
			{
				widget.Update();
			}
		}
	}
}