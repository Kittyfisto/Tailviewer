using System.Windows.Media;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// TODO: Should this interface be part of the plugin architecture? If yes, then how exactly?
	///       Is it good enough to just inherit from <see cref="IPlugin"/>?
	///       Can we move all dependant types to the Tailviewer.Api project?
	/// </remarks>
	public interface IWidgetFactory
	{
		/// <summary>
		///     The type of analyser that shall be used to produce the analysis result
		///     for the resulting widgets.
		/// </summary>
		LogAnalyserFactoryId AnalyserId { get; }

		/// <summary>
		///     The configuration which shall be used when creating a new widget of this type.
		/// </summary>
		ILogAnalyserConfiguration DefaultConfiguration { get; }

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
		/// <param name="dataSourceAnalyser"></param>
		/// <returns></returns>
		IWidgetViewModel Create(IDataSourceAnalyser dataSourceAnalyser);
	}
}