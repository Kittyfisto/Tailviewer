using System.Collections.Generic;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Represents view- and analysis configuration of an analysis.
	///     Can be used to start an active analysis.
	/// </summary>
	/// <remarks>
	///     Contains:
	///     - Pages and their layouts
	///     - Widgets per page
	///     - View configuration of every widget
	///     - Analysis configuration of every widget
	/// </remarks>
	public interface IAnalysisTemplate
		: ISerializableType
	{
		/// <summary>
		///     The pages that are part of this template.
		/// </summary>
		IEnumerable<IAnalysisPageTemplate> Pages { get; }
	}
}