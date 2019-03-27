using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class PageLayoutTest
	{
		[Test]
		[Description("Verifies that the constants for the layouts are never modified")]
		public void TestConstants()
		{
			const string reason = "because the enum numbers should be immutable so they can be used for serialization";

			((int)PageLayout.None).Should().Be(0, reason);
			((int)PageLayout.WrapHorizontal).Should().Be(1, reason);
			((int)PageLayout.Columns).Should().Be(2, reason);
			((int)PageLayout.Rows).Should().Be(3, reason);
		}

		[Test]
		[Description("Verifies that the constants for the layouts are never modified")]
		public void TestNames()
		{
			const string reason = "because the enum names should be immutable so they can be used for serialization";

			nameof(PageLayout.None).Should().Be("None", reason);
			nameof(PageLayout.WrapHorizontal).Should().Be("WrapHorizontal", reason);
			nameof(PageLayout.Columns).Should().Be("Columns", reason);
			nameof(PageLayout.Rows).Should().Be("Rows", reason);
		}
	}
}