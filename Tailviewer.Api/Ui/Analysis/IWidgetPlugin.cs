using System.Windows;
using System.Windows.Controls;
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
	public interface IWidgetPlugin
		: IPlugin
	{
		/// <summary>
		///     The type of analyser that shall be used to produce the analysis result
		///     for the resulting widgets.
		/// </summary>
		LogAnalyserFactoryId AnalyserId { get; }

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
		/// <param name="template"></param>
		/// <param name="dataSourceAnalyser"></param>
		/// <returns></returns>
		IWidgetViewModel CreateViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser);

		/// <summary>
		///     Creates a new control which presents the given view model.
		/// </summary>
		/// <param name="viewModel"></param>
		/// <returns></returns>
		FrameworkElement CreateContentPresenterFor(IWidgetViewModel viewModel);
	}
}