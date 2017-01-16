using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Filters
{
	[TestFixture]
	public sealed class NoFilterTest
	{
		[Test]
		public void TestPassesFilter1()
		{
			var filter = new NoFilter();
			filter.PassesFilter(new LogLine()).Should().BeTrue();
			filter.PassesFilter(null).Should().BeTrue();
			filter.PassesFilter(new LogLine[0]).Should().BeTrue();
			filter.PassesFilter(new[] {new LogLine()}).Should().BeTrue();
		}
	}
}