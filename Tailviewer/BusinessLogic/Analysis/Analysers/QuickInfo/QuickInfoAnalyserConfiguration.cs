using System.Linq;
using System.Xml;

namespace Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo
{
	public sealed class QuickInfoAnalyserConfiguration
		: LogAnalyserConfiguration
	{
		public QuickInfoConfiguration[] QuickInfos;

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
				QuickInfos = QuickInfos?.Select(x => x.Clone()).ToArray()
			};
		}

		public override bool IsEquivalent(ILogAnalyserConfiguration other)
		{
			return false;
		}
	}
}