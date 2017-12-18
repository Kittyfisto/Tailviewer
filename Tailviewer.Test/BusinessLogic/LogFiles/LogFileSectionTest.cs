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
	}
}