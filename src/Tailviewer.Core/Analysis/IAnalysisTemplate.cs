using System;
using System.Collections.Generic;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	///     Represents analysis configuration of an analysis.
	///     Can be used to start an active analysis.
	/// </summary>
	/// <remarks>
	///     Contains:
	///     - Analysis configuration of every analyser
	/// </remarks>
	public interface IAnalysisTemplate
		: ISerializableType
		, ICloneable
	{
		/// <summary>
		/// The analysers which are part of this template.
		/// </summary>
		IEnumerable<IAnalyserTemplate> Analysers { get; }
	}
}