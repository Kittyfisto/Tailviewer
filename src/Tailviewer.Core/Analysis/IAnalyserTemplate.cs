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
		///     The id of the <see cref="ILogAnalyserPlugin" /> which shall be used or
		///     <see cref="BusinessLogic.Analysis.AnalyserPluginId.Empty" /> if none is to be used.
		/// </summary>
		AnalyserPluginId AnalyserPluginId { get; }

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