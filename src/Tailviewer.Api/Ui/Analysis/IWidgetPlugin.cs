using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Ui.Analysis
{
	/// <summary>
	///     The interface for a plugin to create widgets:
	///     Is responsible for providing view models as well as a template to display them.
	/// </summary>
	/// <remarks>
	///     A widget usually works in conjunction with an analyser: This can be either a <see cref="ILogAnalyser"/>
	///     or <see cref="IDataSourceAnalyser"/>.
	/// </remarks>
	/// <remarks>
	///    Version 1
	///    Initial definition
	/// </remarks>
	/// <remarks>
	///    Version 2
	///    Breaking changes:
	///    - Changed CreateViewModel() and CreateContentPresenterFor() signature to accept an <see cref="IServiceContainer"/>
	/// </remarks>
	[PluginInterfaceVersion(2)]
	public interface IWidgetPlugin
		: IPlugin
	{
		/// <summary>
		///     The id of the <see cref="ILogAnalyserPlugin"/> OR <see cref="IDataSourceAnalyserPlugin"/> which shall be used to produce analysis results for this widget.
		/// </summary>
		AnalyserPluginId AnalyserId { get; }

		/// <summary>
		///     The configuration which shall be used when creating a new widget of this type.
		/// </summary>
		ILogAnalyserConfiguration DefaultAnalyserConfiguration { get; }

		/// <summary>
		///     The configuration wich shall be used if no other view configuration is present.
		/// </summary>
		IWidgetConfiguration DefaultViewConfiguration { get; }

		/// <summary>
		///     The name of this widget - will be displayed to the user.
		///     Should not be longer than two or three words.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The description of this widget - will be displayed to the user.
		///     Should be no longer than one or two sentences.
		/// </summary>
		string Description { get; }

		/// <summary>
		///     The geometry to an icon that will be displayed next to the widget.
		/// </summary>
		Geometry Icon { get; }

		/// <summary>
		///     Creates a new view model to represent the given dataSourceAnalyser.
		///     It is the responsibility of the view model to update the dataSourceAnalyser's
		///     <see cref="IDataSourceAnalyser.Configuration" /> if the user made changes to
		///     the configuration as well as to present results from <see cref="IDataSourceAnalyser.Result" />.
		/// </summary>
		/// <param name="services">A service container which may be used to construct types required by this plugin and to inject custom parameters if necessary</param>
		/// <param name="template"></param>
		/// <param name="dataSourceAnalyser"></param>
		/// <returns></returns>
		IWidgetViewModel CreateViewModel(IServiceContainer services, IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser);

		/// <summary>
		///     Creates a new control which presents the given view model.
		/// </summary>
		/// <param name="services">A service container which may be used to construct types required by this plugin and to inject custom parameters if necessary</param>
		/// <param name="viewModel"></param>
		/// <returns></returns>
		FrameworkElement CreateContentPresenterFor(IServiceContainer services, IWidgetViewModel viewModel);
	}
}