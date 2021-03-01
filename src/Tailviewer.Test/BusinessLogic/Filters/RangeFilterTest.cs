using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Filters;

namespace Tailviewer.Test.BusinessLogic.Filters
{
	[TestFixture]
	public sealed class RangeFilterTest
	{
		[Test]
		public void Test()
		{
			var filter = new RangeFilter(new LogFileSection(42, 101));
			filter.PassesFilter(CreateLine(0)).Should().BeTrue();
			filter.PassesFilter(CreateLine(41)).Should().BeTrue();
			filter.PassesFilter(CreateLine(42)).Should().BeFalse();
			filter.PassesFilter(CreateLine(142)).Should().BeFalse();
			filter.PassesFilter(CreateLine(143)).Should().BeTrue();
		}

		private static IReadOnlyLogEntry CreateLine(LogLineIndex lineIndex)
		{
			return new LogEntry(GeneralColumns.Minimum) {Index = lineIndex};
		}
	}
}
