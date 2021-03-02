using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Filters
{
	[TestFixture]
	public sealed class SubstringFilterTest
	{
		[Test]
		public void TestMatch1()
		{
			var filter = new SubstringFilter("Foobar", true);
			var matches = new List<LogLineMatch>();
			new Action(() => filter.Match(new LogEntry(Core.Columns.Minimum){RawContent = null}, matches)).Should().NotThrow();
			matches.Should().BeEmpty();
		}

		[Test]
		public void TestMatch2()
		{
			var filter = new SubstringFilter("a", true);
			var matches = new List<LogLineMatch>();
			filter.Match(new LogEntry(Core.Columns.Minimum){RawContent = "Foobar"}, matches);
			matches.Count.Should().Be(1);
			matches[0].Index.Should().Be(4);
			matches[0].Count.Should().Be(1);
		}

		[Test]
		public void TestPassesFilter()
		{
			var filter = new SubstringFilter("a", true);
			filter.PassesFilter(new LogEntry(Core.Columns.Minimum){RawContent = null}).Should().BeFalse();
		}

		[Test]
		public void TestToString()
		{
			var filter = new SubstringFilter("a", true);
			filter.ToString().Should().Be("message.Contains(a, InvariantCultureIgnoreCase)");

			filter = new SubstringFilter("a", false);
			filter.ToString().Should().Be("message.Contains(a, InvariantCulture)");
		}
	}
}