using System;
using System.Collections.Generic;
using System.Linq;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	public sealed class QuickInfoResult
		: ILogAnalysisResult
	{
		public Dictionary<Guid, string> Values;

		public object Clone()
		{
			return new QuickInfoResult
			{
				Values = Values?.ToDictionary(p => p.Key, p => p.Value)
			};
		}
	}
}