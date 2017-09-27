using System;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Test.BusinessLogic.Analysis.Analysers.QuickInfo
{
	[TestFixture]
	public sealed class QuickInfoTest
	{
		[Test]
		public void TestEquals()
		{
			var value = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("foobar", DateTime.MinValue);
			var sameValue = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("foobar", DateTime.MinValue);
			var otherValue = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("FOOBAR", DateTime.MinValue);

			value.Should().Be(sameValue);
			value.Should().NotBe(otherValue);
		}

		[Test]
		public void TestOperatorEquals()
		{
			var value = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("foobar", DateTime.MinValue);
			var sameValue = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("foobar", DateTime.MinValue);
			var otherValue = new Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo.QuickInfo("FOOBAR", DateTime.MinValue);

			(value == sameValue).Should().BeTrue();
			(value != sameValue).Should().BeFalse();

			(value == otherValue).Should().BeFalse();
			(value != otherValue).Should().BeTrue();
		}
	}
}
