using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
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

		private static LogLine CreateLine(LogLineIndex lineIndex)
		{
			return new LogLine(lineIndex, 0, "", LevelFlags.None, null);
		}
	}
}
