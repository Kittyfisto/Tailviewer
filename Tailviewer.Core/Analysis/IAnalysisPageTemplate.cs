using System;
using System.Collections.Generic;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     The template for an entire page of an analysis:
	///     Maintains the page's layout and all widgets placed on that page.
	/// </summary>
	public interface IAnalysisPageTemplate
		: ISerializableType
		, ICloneable
	{
		/// <summary>
		///     The widgets that are placed on this page.
		/// </summary>
		IEnumerable<IWidgetTemplate> Widgets { get; }

		/// <summary>
		///     The layout controlling how these widgets are positioned.
		/// </summary>
		IWidgetLayoutTemplate Layout { get; set; }
	}
}