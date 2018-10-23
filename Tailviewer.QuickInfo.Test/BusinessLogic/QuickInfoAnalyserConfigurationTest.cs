using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.QuickInfo.BusinessLogic;
using Tailviewer.Test;

namespace Tailviewer.QuickInfo.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickInfoAnalyserConfigurationTest
	{
		[Test]
		public void TestAdd()
		{
			var config = new QuickInfoAnalyserConfiguration();
			config.QuickInfos.Should().BeEmpty();

			var id = Guid.NewGuid();
			var info = new QuickInfoConfiguration(id);
			config.Add(info);
			config.QuickInfos.Should().HaveCount(1);
			config.QuickInfos.First().Should().BeSameAs(info);
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var config = new QuickInfoAnalyserConfiguration();
			var actualConfig = config.Roundtrip();
			actualConfig.Should().NotBeNull();
			actualConfig.QuickInfos.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripOneQuickInfo()
		{
			var config = new QuickInfoAnalyserConfiguration();
			var id = Guid.NewGuid();
			config.Add(new QuickInfoConfiguration(id)
			{
				FilterValue = "ERROR: ",
				MatchType = FilterMatchType.TimeFilter
			});

			var actualConfig = config.Roundtrip(typeof(QuickInfoConfiguration));
			actualConfig.Should().NotBeNull();
			actualConfig.QuickInfos.Should().HaveCount(1);
			actualConfig.QuickInfos.First().Id.Should().Be(id);
			actualConfig.QuickInfos.First().FilterValue.Should().Be("ERROR: ");
			actualConfig.QuickInfos.First().MatchType.Should().Be(FilterMatchType.TimeFilter);
		}
	}
}
