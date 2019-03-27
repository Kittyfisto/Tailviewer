using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class DataSourceAnalyserPluginIdTest
	{
		[Test]
		public void TestEqualsEmptyId()
		{
			new DataSourceAnalyserPluginId().Should().Be(DataSourceAnalyserPluginId.Empty);
			new DataSourceAnalyserPluginId(null).Should().Be(DataSourceAnalyserPluginId.Empty);
			new DataSourceAnalyserPluginId("").Should().Be(DataSourceAnalyserPluginId.Empty);
		}

		[Test]
		public void TestEqualsDifferentIds()
		{
			new DataSourceAnalyserPluginId("Foo").Should().NotBe(new DataSourceAnalyserPluginId("Bar"));
		}

		[Test]
		public void TestGetHashCodeEmptyId()
		{
			new DataSourceAnalyserPluginId().GetHashCode().Should().Be(DataSourceAnalyserPluginId.Empty.GetHashCode());
			new DataSourceAnalyserPluginId(null).GetHashCode().Should().Be(DataSourceAnalyserPluginId.Empty.GetHashCode());
			new DataSourceAnalyserPluginId("").GetHashCode().Should().Be(DataSourceAnalyserPluginId.Empty.GetHashCode());
		}
	}
}