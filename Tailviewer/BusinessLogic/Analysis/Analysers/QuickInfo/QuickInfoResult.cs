using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	/// <summary>
	///     The result of the <see cref="QuickInfoAnalyser" />.
	/// </summary>
	public sealed class QuickInfoResult
		: ILogAnalysisResult
	{
		/// <summary>
		///     The list of quick infos for which a match has been found, grouped by their id.
		///     Each quick info consists of the entire log line which matched.
		/// </summary>
		/// <remarks>
		///     If the analyser didn't find a matching log line for a quick info, then the quick info will
		///     not be added to the list of values.
		/// </remarks>
		public Dictionary<Guid, QuickInfo> QuickInfos;

		public object Clone()
		{
			return new QuickInfoResult
			{
				QuickInfos = QuickInfos?.ToDictionary(p => p.Key, p => p.Value)
			};
		}
	}
}