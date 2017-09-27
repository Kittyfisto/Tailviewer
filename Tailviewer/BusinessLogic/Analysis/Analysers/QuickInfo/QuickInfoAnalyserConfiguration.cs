using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	public sealed class QuickInfoAnalyserConfiguration
		: LogAnalyserConfiguration
	{
		public Dictionary<Guid, QuickInfoConfiguration> QuickInfos;

		public QuickInfoAnalyserConfiguration()
		{
			QuickInfos = new Dictionary<Guid, QuickInfoConfiguration>();
		}

		protected override void RestoreInternal(XmlReader reader)
		{
		}

		protected override void SaveInternal(XmlWriter writer)
		{
		}

		public override object Clone()
		{
			return new QuickInfoAnalyserConfiguration
			{
				QuickInfos = QuickInfos?.ToDictionary(x => x.Key, x => x.Value?.Clone())
			};
		}

		public override bool IsEquivalent(ILogAnalyserConfiguration other)
		{
			return false;
		}
	}
}