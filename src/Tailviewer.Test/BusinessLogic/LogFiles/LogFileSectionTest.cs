using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileSectionTest
	{
		[Test]
		public void TestGetByIndex()
		{
			var section = new LogFileSection(42, 10);
			for (int i = 0; i < 10; ++i)
			{
				section[i].Should().Be(new LogLineIndex(42 + i));
			}
		}

		[Test]
		public void TestEnumerate([Range(0, 5)] int startIndex,
			[Range(0, 5)] int count)
		{
			var section = new LogFileSection(startIndex, count);
			var expected = Enumerable.Range(startIndex, count).Select(x => (LogLineIndex) x).ToArray();
			section.Should().Equal(expected);
		}

		[Test]
		public void TestToString()
		{
			LogFileSection.Reset.ToString().Should().Be("Reset");
			LogFileSection.Invalidate(42, 5).ToString().Should().Be("Invalidated [#42, #5]");
		}

		[Test]
		public void TestGetCount([Values(0, 1)] int count)
		{
			var section = new LogFileSection(9001, count);
			section.Count.Should().Be(count);
			((IReadOnlyList<LogLineIndex>) section).Count.Should().Be(count);
		}

		[Test]
		public void TestSplitInvalidateLowerThanThreshold([Values(0, 99, 100)] int count)
		{
			int maxCount = count + 1;
			var section = LogFileSection.Invalidate(new LogLineIndex(10), count);
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because invalidations are NEVER split up");
		}

		[Test]
		public void TestSplitInvalidateGreaterThanThreshold([Values(2, 99, 100)] int count)
		{
			int maxCount = count - 1;
			var section = LogFileSection.Invalidate(new LogLineIndex(42), count);
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because invalidations are NEVER split up");
		}

		[Test]
		public void TestSplitReset([Values(1, 99, 100)] int maxCount)
		{
			var section = LogFileSection.Reset;
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because resets are NEVER split up");
		}

		[Test]
		public void TestSplitAppendOnePart([Values(0, 1, 99, 100)] int count)
		{
			int maxCount = count + 1;
			var section = new LogFileSection(new LogLineIndex(0), count);
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because we append less than the maximum number of lines");
		}

		[Test]
		public void TestSplitAppendTwoParts([Values(0, 1, 99, 100)] int count)
		{
			int maxCount = count + 1;
			var section = new LogFileSection(new LogLineIndex(0), count);
			section.Split(maxCount).Should().Equal(new[]
			{
				section
			}, "because we append less than the maximum number of lines");
		}

		[Test]
		public void TestSplitAppendThreeParts()
		{
			var section = new LogFileSection(new LogLineIndex(101), 99);
			section.Split(33).Should().Equal(new[]
			{
				new LogFileSection(101, 33), 
				new LogFileSection(134, 33), 
				new LogFileSection(167, 33)
			}, "because we append less than the maximum number of lines");
		}

		[Test]
		public void TestSplitAppendThreePartsPartial()
		{
			var section = new LogFileSection(new LogLineIndex(101), 67);
			section.Split(33).Should().Equal(new[]
			{
				new LogFileSection(101, 33), 
				new LogFileSection(134, 33), 
				new LogFileSection(167, 1)
			}, "because we append less than the maximum number of lines");
		}
	}
}