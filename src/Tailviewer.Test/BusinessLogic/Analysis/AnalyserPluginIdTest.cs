using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class AnalyserPluginIdTest
	{
		[Test]
		public void TestEqualsEmptyId()
		{
			new AnalyserPluginId().Should().Be(AnalyserPluginId.Empty);
			new AnalyserPluginId(null).Should().Be(AnalyserPluginId.Empty);
			new AnalyserPluginId("").Should().Be(AnalyserPluginId.Empty);
		}

		[Test]
		public void TestEqualsDifferentIds()
		{
			new AnalyserPluginId("Foo").Should().NotBe(new AnalyserPluginId("Bar"));
		}

		[Test]
		public void TestGetHashCodeEmptyId()
		{
			new AnalyserPluginId().GetHashCode().Should().Be(AnalyserPluginId.Empty.GetHashCode());
			new AnalyserPluginId(null).GetHashCode().Should().Be(AnalyserPluginId.Empty.GetHashCode());
			new AnalyserPluginId("").GetHashCode().Should().Be(AnalyserPluginId.Empty.GetHashCode());
		}
	}
}
