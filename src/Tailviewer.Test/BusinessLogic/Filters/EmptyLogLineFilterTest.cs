using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Filters
{
	[TestFixture]
	public sealed class EmptyLogLineFilterTest
	{
		[Test]
		public void TestPassesFilter1()
		{
			var filter = new EmptyLogLineFilter();
			filter.PassesFilter(new LogEntry(LogFileColumns.Minimum)).Should().BeFalse("because the given logline is completely empty");
			filter.PassesFilter(new LogEntry(LogFileColumns.Minimum){Index=0, RawContent ="", LogLevel = LevelFlags.All})
				.Should()
				.BeFalse("because the given line contains only an empty message");
			filter.PassesFilter(new LogEntry(LogFileColumns.Minimum){Index=0, RawContent = " ", LogLevel = LevelFlags.All})
				.Should()
				.BeFalse("because the given line contains only spaces");
			filter.PassesFilter(new LogEntry(LogFileColumns.Minimum){Index=0, RawContent = " \t \r\n", LogLevel = LevelFlags.All})
				.Should()
				.BeFalse("because the given line contains only whitespace");
			filter.PassesFilter(new LogEntry(LogFileColumns.Minimum){Index=0, RawContent = " s    \t", LogLevel = LevelFlags.All})
				.Should()
				.BeTrue("because the given line contains a non-whitespace character");
		}
	}
}