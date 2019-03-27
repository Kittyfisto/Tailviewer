using System;

namespace Tailviewer.BusinessLogic.Analysis
{
	/// <summary>
	///     The interface for a result of a <see cref="ILogAnalyser" />.
	/// </summary>
	public interface ILogAnalysisResult
		: ISerializableType
		, ICloneable
	{
	}
}