using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.QuickInfo.BusinessLogic
{
	public sealed class QuickInfoAnalyserConfiguration
		: ILogAnalyserConfiguration
	{
		public Dictionary<Guid, QuickInfoConfiguration> QuickInfos;

		public QuickInfoAnalyserConfiguration()
		{
			QuickInfos = new Dictionary<Guid, QuickInfoConfiguration>();
		}

		public object Clone()
		{
			return new QuickInfoAnalyserConfiguration
			{
				QuickInfos = QuickInfos?.ToDictionary(x => x.Key, x => x.Value?.Clone())
			};
		}

		public bool IsEquivalent(ILogAnalyserConfiguration other)
		{
			return false;
		}
	}
}