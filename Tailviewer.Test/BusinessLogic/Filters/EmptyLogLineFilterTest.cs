using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Filters
{
	[TestFixture]
	public sealed class EmptyLogLineFilterTest
	{
		[Test]
		public void TestPassesFilter1()
		{
			var filter = new EmptyLogLineFilter();
			filter.PassesFilter(new LogLine()).Should().BeFalse("because the given logline is completely empty");
			filter.PassesFilter(new LogLine(0, "", LevelFlags.All))
				.Should()
				.BeFalse("because the given line contains only an empty message");
			filter.PassesFilter(new LogLine(0, " ", LevelFlags.All))
				.Should()
				.BeFalse("because the given line contains only spaces");
			filter.PassesFilter(new LogLine(0, " \t \r\n", LevelFlags.All))
				.Should()
				.BeFalse("because the given line contains only whitespace");
			filter.PassesFilter(new LogLine(0, " s    \t", LevelFlags.All))
				.Should()
				.BeTrue("because the given line contains a non-whitespace character");
		}
	}
}