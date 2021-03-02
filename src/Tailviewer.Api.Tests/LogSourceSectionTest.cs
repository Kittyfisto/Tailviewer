using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.Api.Tests
{
	[TestFixture]
	public sealed class LogSourceSectionTest
	{
		[Test]
		public void TestGetByIndex()
		{
			var section = new LogSourceSection(42, 10);
			for (int i = 0; i < 10; ++i)
			{
				section[i].Should().Be(new LogLineIndex(42 + i));
			}
		}

		[Test]
		public void TestEnumerate([Range(0, 5)] int startIndex,
			[Range(0, 5)] int count)
		{
			var section = new LogSourceSection(startIndex, count);
			var expected = Enumerable.Range(startIndex, count).Select(x => (LogLineIndex) x).ToArray();
			section.Should().Equal(expected);
		}

		[Test]
		public void TestGetCount([Values(0, 1)] int count)
		{
			var section = new LogSourceSection(9001, count);
			section.Count.Should().Be(count);
			((IReadOnlyList<LogLineIndex>) section).Count.Should().Be(count);
		}

		[Test]
		public void TestSplitInvalidateLowerThanThreshold([Values(0, 99, 100)] int count)
		{
			int maxCount = count + 1;
			var section = LogSourceModification.Removed(new LogLineIndex(10), count);
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because invalidations are NEVER split up");
		}

		[Test]
		public void TestSplitInvalidateGreaterThanThreshold([Values(2, 99, 100)] int count)
		{
			int maxCount = count - 1;
			var section = LogSourceModification.Removed(new LogLineIndex(42), count);
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because invalidations are NEVER split up");
		}

		[Test]
		public void TestSplitReset([Values(1, 99, 100)] int maxCount)
		{
			var section = LogSourceModification.Reset();
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because resets are NEVER split up");
		}
	}
}