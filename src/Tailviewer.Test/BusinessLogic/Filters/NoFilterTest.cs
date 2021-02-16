﻿using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Filters;

namespace Tailviewer.Test.BusinessLogic.Filters
{
	[TestFixture]
	public sealed class NoFilterTest
	{
		[Test]
		public void TestPassesFilter1()
		{
			var filter = new NoFilter();
			filter.PassesFilter(new LogEntry()).Should().BeTrue();
			filter.PassesFilter((IEnumerable<IReadOnlyLogEntry>) null).Should().BeTrue();
			filter.PassesFilter(new LogEntry[0]).Should().BeTrue();
			filter.PassesFilter(new[] {new LogEntry()}).Should().BeTrue();
		}
	}
}