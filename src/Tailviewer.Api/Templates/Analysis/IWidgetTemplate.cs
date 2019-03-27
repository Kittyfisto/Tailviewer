using System;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Templates.Analysis
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
			, ICloneable
	{
		/// <summary>
		///     The title of the widget.
		/// </summary>
		string Title { get; set; }

		/// <summary>
		///     The id of this widget.
		/// </summary>
		WidgetId Id { get; }

		/// <summary>
		///     The id of the analyser instance which is coupled
		///     with this widget.
		/// </summary>
		AnalyserId AnalyserId { get; }

		/// <summary>
		/// The id of the factory which should be used to instantiate a widget from this template.
		/// </summary>
		AnalyserPluginId AnalyserPluginId { get; }

		/// <summary>
		///     The configuration of the view (widget).
		/// </summary>
		IWidgetConfiguration Configuration { get; }
	}
}