using System;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.QuickInfo.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickInfoTest
	{
		[Test]
		public void TestEquals()
		{
			var value = new QuickInfo.BusinessLogic.QuickInfo("foobar", DateTime.MinValue);
			var sameValue = new QuickInfo.BusinessLogic.QuickInfo("foobar", DateTime.MinValue);
			var otherValue = new QuickInfo.BusinessLogic.QuickInfo("FOOBAR", DateTime.MinValue);

			value.Should().Be(sameValue);
			value.Should().NotBe(otherValue);
		}

		[Test]
		public void TestOperatorEquals()
		{
			var value = new QuickInfo.BusinessLogic.QuickInfo("foobar", DateTime.MinValue);
			var sameValue = new QuickInfo.BusinessLogic.QuickInfo("foobar", DateTime.MinValue);
			var otherValue = new QuickInfo.BusinessLogic.QuickInfo("FOOBAR", DateTime.MinValue);

			(value == sameValue).Should().BeTrue();
			(value != sameValue).Should().BeFalse();

			(value == otherValue).Should().BeFalse();
			(value != otherValue).Should().BeTrue();
		}
	}
}
