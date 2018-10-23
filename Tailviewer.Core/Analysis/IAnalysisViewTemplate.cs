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
		///     The name of this template, usually defined by a meat bag.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		///     The pages that are part of this template.
		/// </summary>
		IEnumerable<IPageTemplate> Pages { get; }
	}
}