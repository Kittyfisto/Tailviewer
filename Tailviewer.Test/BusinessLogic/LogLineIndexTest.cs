using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogLineIndexTest
	{
		[Test]
		public void TestToString()
		{
			LogLineIndex.Invalid.ToString().Should().Be("Invalid");
			new LogLineIndex(5).ToString().Should().Be("#5");
		}

		[Test]
		public void TestEquality()
		{
			LogLineIndex.Invalid.Should().Be(LogLineIndex.Invalid);
			LogLineIndex.Invalid.Equals(LogLineIndex.Invalid).Should().BeTrue();
			(LogLineIndex.Invalid == LogLineIndex.Invalid).Should().BeTrue();
			(LogLineIndex.Invalid != LogLineIndex.Invalid).Should().BeFalse();
		}

		[Test]
		public void TestLessThan()
		{
			(new LogLineIndex(0) < new LogLineIndex(1)).Should().BeTrue();
			(new LogLineIndex(1) < new LogLineIndex(1)).Should().BeFalse();
			(new LogLineIndex(2) < new LogLineIndex(1)).Should().BeFalse();
		}

		[Test]
		public void TestLessThanorEquals()
		{
			(new LogLineIndex(0) <= new LogLineIndex(1)).Should().BeTrue();
			(new LogLineIndex(1) <= new LogLineIndex(1)).Should().BeTrue();
			(new LogLineIndex(2) <= new LogLineIndex(1)).Should().BeFalse();
		}
	}
}