using FluentAssertions;
using NUnit.Framework;

#pragma warning disable CS1718

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogLineIndexTest
	{
		[Test]
		public void TestConstruction([Range(-5, -1)] int value)
		{
			var index = new LogLineIndex(value);
			index.IsInvalid.Should().BeTrue();
		}

		[Test]
		public void TestToString1()
		{
			LogLineIndex.Invalid.ToString().Should().Be("Invalid");
			new LogLineIndex(5).ToString().Should().Be("#5");
		}

		[Test]
		public void TestToString2([Range(-3, -1)] int value)
		{
			var index = new LogLineIndex(value);
			index.ToString().Should().Be("Invalid");
		}

		[Test]
		public void TestValue()
		{
			new LogLineIndex(42).Value.Should().Be(42);
			new LogLineIndex(1337).Value.Should().Be(1337);
		}

		[Test]
		public void TestEquality1()
		{
			LogLineIndex.Invalid.Should().Be(LogLineIndex.Invalid);
			LogLineIndex.Invalid.Equals(LogLineIndex.Invalid).Should().BeTrue();
			(LogLineIndex.Invalid == LogLineIndex.Invalid).Should().BeTrue();
			(LogLineIndex.Invalid != LogLineIndex.Invalid).Should().BeFalse();
		}

		[Test]
		public void TestEquality2()
		{
			new LogLineIndex(-1).Equals(new LogLineIndex(-2))
				.Should().BeTrue("because both indices address an invalid portion of a log file and thus should be counted to be equal");
			(new LogLineIndex(-1) == new LogLineIndex(-2))
			                    .Should().BeTrue("because both indices address an invalid portion of a log file and thus should be counted to be equal");
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