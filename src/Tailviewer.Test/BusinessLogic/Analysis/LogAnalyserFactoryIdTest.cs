using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class LogAnalyserFactoryIdTest
	{
		[Test]
		public void TestEqualsEmptyId()
		{
			new LogAnalyserFactoryId().Should().Be(LogAnalyserFactoryId.Empty);
			new LogAnalyserFactoryId(null).Should().Be(LogAnalyserFactoryId.Empty);
			new LogAnalyserFactoryId("").Should().Be(LogAnalyserFactoryId.Empty);
		}

		[Test]
		public void TestEqualsDifferentIds()
		{
			new LogAnalyserFactoryId("Foo").Should().NotBe(new LogAnalyserFactoryId("Bar"));
		}

		[Test]
		public void TestGetHashCodeEmptyId()
		{
			new LogAnalyserFactoryId().GetHashCode().Should().Be(LogAnalyserFactoryId.Empty.GetHashCode());
			new LogAnalyserFactoryId(null).GetHashCode().Should().Be(LogAnalyserFactoryId.Empty.GetHashCode());
			new LogAnalyserFactoryId("").GetHashCode().Should().Be(LogAnalyserFactoryId.Empty.GetHashCode());
		}
	}
}
