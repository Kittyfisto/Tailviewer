using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.Analysis.Analysers;
using Tailviewer.BusinessLogic.Analysis.Analysers.Count;
using Tailviewer.Core.Settings;

namespace Tailviewer.Test.BusinessLogic.Analysis.Analysers.Count
{
	[TestFixture]
	public sealed class LogEntryCountAnalyserConfigurationTest
	{
		[Test]
		public void TestIsEquivalent1()
		{
			var config = new LogEntryCountAnalyserConfiguration();
			config.IsEquivalent(null).Should().BeFalse();
			config.IsEquivalent(new Mock<ILogAnalyserConfiguration>().Object).Should().BeFalse();
		}

		[Test]
		public void TestIsEquivalent2()
		{
			var config1 = new LogEntryCountAnalyserConfiguration();
			var config2 = new LogEntryCountAnalyserConfiguration();
			config1.IsEquivalent(config2).Should().BeTrue();
			config2.IsEquivalent(config1).Should().BeTrue();
		}

		[Test]
		public void TestIsEquivalent3()
		{
			var config1 = new LogEntryCountAnalyserConfiguration();
			var config2 = new LogEntryCountAnalyserConfiguration
			{
				QuickFilters =
				{
					new QuickFilter
					{
						Value = "foo"
					}
				}
			};
			config1.IsEquivalent(config2).Should().BeFalse();
			config2.IsEquivalent(config1).Should().BeFalse();
		}
	}
}