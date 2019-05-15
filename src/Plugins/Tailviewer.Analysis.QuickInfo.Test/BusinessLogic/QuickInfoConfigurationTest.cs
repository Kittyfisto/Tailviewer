using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.QuickInfo.BusinessLogic;
using Tailviewer.Core.Settings;

namespace Tailviewer.Analysis.QuickInfo.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickInfoConfigurationTest
	{
		[Test]
		public void TestConstruction()
		{
			var config = new QuickInfoConfiguration();
			config.FilterValue.Should().BeNull();
			config.MatchType.Should().Be(FilterMatchType.RegexpFilter);
		}

		[Test]
		public void TestClone()
		{
			var id = Guid.NewGuid();
			var config = new QuickInfoConfiguration(id);
			var clone = config.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(config);
			clone.Id.Should().Be(id);
		}
	}
}