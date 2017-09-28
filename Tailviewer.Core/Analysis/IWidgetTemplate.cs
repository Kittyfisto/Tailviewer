using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The template for a widget:
	///     - widget "view" configuration
	///     - analysis factory id (defines which analyser was used)
	///     - analyser id (defines the actual analyser instance)
	///     - analysis configuration
	/// </summary>
	public interface IWidgetTemplate
		: ISerializableType
	{
		/// <summary>
		///     The title of the widget.
		/// </summary>
		string Title { get; }

		/// <summary>
		///     The configuration of the view (widget).
		/// </summary>
		IWidgetConfiguration ViewConfiguration { get; }

		/// <summary>
		///     The configuration of the analyser.
		/// </summary>
		ILogAnalyserConfiguration AnalysisConfiguration { get; }

	}
}
