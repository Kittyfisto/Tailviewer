using System;
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
		, ICloneable
	{
		/// <summary>
		///     The pages that are part of this template.
		/// </summary>
		IEnumerable<IPageTemplate> Pages { get; }

		/// <summary>
		/// The analysers which are part of this template.
		/// </summary>
		IEnumerable<IAnalyserTemplate> Analysers { get; }
	}
}