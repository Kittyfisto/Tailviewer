using FluentAssertions;
using NUnit.Framework;

#pragma warning disable CS1718

namespace Tailviewer.Api.Tests
{
	[TestFixture]
	public sealed class LogEntryIndexTest
	{
		[Test]
		public void TestToString()
		{
			LogEntryIndex.Invalid.ToString().Should().Be("Invalid");
			new LogEntryIndex(5).ToString().Should().Be("#5");
		}

		[Test]
		public void TestValue()
		{
			new LogEntryIndex(42).Value.Should().Be(42);
			new LogEntryIndex(1337).Value.Should().Be(1337);
		}

		[Test]
		public void TestIsInvalid()
		{
			new LogEntryIndex(0).IsInvalid.Should().BeFalse();
			new LogEntryIndex(-1).IsInvalid.Should().BeTrue();
			new LogEntryIndex(1).IsInvalid.Should().BeFalse();
		}

		[Test]
		public void TestEquality()
		{
			LogEntryIndex.Invalid.Should().Be(LogEntryIndex.Invalid);
			LogEntryIndex.Invalid.Equals(LogEntryIndex.Invalid).Should().BeTrue();
			(LogEntryIndex.Invalid == LogEntryIndex.Invalid).Should().BeTrue();
			(LogEntryIndex.Invalid != LogEntryIndex.Invalid).Should().BeFalse();
		}

		[Test]
		public void TestLessThan()
		{
			(new LogEntryIndex(0) < new LogEntryIndex(1)).Should().BeTrue();
			(new LogEntryIndex(1) < new LogEntryIndex(1)).Should().BeFalse();
			(new LogEntryIndex(2) < new LogEntryIndex(1)).Should().BeFalse();
		}

		[Test]
		public void TestLessThanorEquals()
		{
			(new LogEntryIndex(0) <= new LogEntryIndex(1)).Should().BeTrue();
			(new LogEntryIndex(1) <= new LogEntryIndex(1)).Should().BeTrue();
			(new LogEntryIndex(2) <= new LogEntryIndex(1)).Should().BeFalse();
		}

		[Test]
		public void TestIncrement()
		{
			var idx = new LogEntryIndex(0);
			++idx;
			idx.Should().Be(new LogEntryIndex(1));
		}
	}
}