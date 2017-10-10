using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Represents view-configuration of an analysis.
	/// </summary>
	/// <remarks>
	///     Contains:
	///     - Pages and their layouts
	///     - Widgets per page
	///     - View configuration of every widget
	/// </remarks>
	public interface IAnalysisViewTemplate
		: ISerializableType
			, ICloneable
	{
		/// <summary>
		///     The pages that are part of this template.
		/// </summary>
		IEnumerable<IPageTemplate> Pages { get; }
	}
}