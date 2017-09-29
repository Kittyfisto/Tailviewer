using System;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Core.Analysis
{
	/// <summary>
	/// </summary>
	public interface IAnalyserTemplate
		: ISerializableType
			, ICloneable
	{
		/// <summary>
		///     The id of the factory which produced the analysier.
		/// </summary>
		LogAnalyserFactoryId FactoryId { get; }

		/// <summary>
		///     The id of the analyser instance.
		/// </summary>
		AnalyserId Id { get; }

		/// <summary>
		///     The configuration of the analyser.
		/// </summary>
		ILogAnalyserConfiguration Configuration { get; }
	}
}