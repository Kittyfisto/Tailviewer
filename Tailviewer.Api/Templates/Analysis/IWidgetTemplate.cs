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
		///     The configuration of the view (widget).
		/// </summary>
		IWidgetConfiguration ViewConfiguration { get; }

	}
}
