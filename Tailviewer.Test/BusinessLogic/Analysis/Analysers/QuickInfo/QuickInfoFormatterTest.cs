using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo;
using Tailviewer.Core.Settings;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.QuickInfo;

namespace Tailviewer.Test.BusinessLogic.Analysis.Analysers.QuickInfo
{
	[TestFixture]
	public sealed class QuickInfoFormatterTest
	{
		[Test]
		public void TestRegexp1()
		{
			var config = new QuickInfoConfiguration
			{
				FilterValue = "v\\d.\\d.\\d.\\d",
				MatchType = FilterMatchType.RegexpFilter
			};
			var formatter = new QuickInfoFormatter(config);
			var result = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("Product version: v1.2.4.8");
			formatter.Format(result, "{0}")
				.Should().Be("v1.2.4.8");
		}

		[Test]
		public void TestRegexp2()
		{
			var config = new QuickInfoConfiguration
			{
				FilterValue = "v(\\d+).(\\d+).\\d.\\d",
				MatchType = FilterMatchType.RegexpFilter
			};
			var formatter = new QuickInfoFormatter(config);
			var result = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("Product version: v3.12.4.8");
			formatter.Format(result, "Minor {1}, Major {2}")
				.Should().Be("Minor 3, Major 12");
		}
	}
}
