using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
#pragma warning disable CS1718

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
		public void TestValue()
		{
			new LogLineIndex(42).Value.Should().Be(42);
			new LogLineIndex(1337).Value.Should().Be(1337);
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

		[Test]
		public void TestIncrement()
		{
			var idx = new LogLineIndex(0);
			++idx;
			idx.Should().Be(new LogLineIndex(1));
		}

		[Test]
		public void TestRange()
		{
			LogLineIndex.Range(new LogLineIndex(1), new LogLineIndex(1)).Should().Equal(new LogLineIndex(1));
			LogLineIndex.Range(new LogLineIndex(1), new LogLineIndex(2)).Should().Equal(new LogLineIndex(1), new LogLineIndex(2));
			LogLineIndex.Range(new LogLineIndex(2), new LogLineIndex(1)).Should().Equal(new LogLineIndex(1), new LogLineIndex(2));
			LogLineIndex.Range(new LogLineIndex(3), new LogLineIndex(1)).Should().Equal(new LogLineIndex(1), new LogLineIndex(2), new LogLineIndex(3));
		}
	}
}