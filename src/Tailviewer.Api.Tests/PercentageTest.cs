using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Api.Tests
{
	[TestFixture]
	public sealed class PercentageTest
	{
		[Test]
		public void TestConstants()
		{
			Percentage.Zero.RelativeValue.Should().Be(0);
			Percentage.FiftyPercent.RelativeValue.Should().Be(0.5f);
			Percentage.HundredPercent.RelativeValue.Should().Be(1);
		}
	}
}
