using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Filters;

namespace Tailviewer.Test.BusinessLogic.Filters
{
	[TestFixture]
	public sealed class OrFilterTest
	{
		[Test]
		public void TestSingleLine1()
		{
			var filter = new OrFilter(new[] {new SubstringFilter("foo", true)});
			var line = new LogEntry(LogColumns.RawContent){RawContent = "foobar"};
			filter.PassesFilter(line).Should().BeTrue();
			var matches = filter.Match(line);
			matches.Should().NotBeNull();
			matches.Should().HaveCount(1);
		}

		[Test]
		public void TestSingleLine2()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var line = new LogEntry(LogColumns.RawContent){RawContent = "fobar"};
			filter.PassesFilter(line).Should().BeFalse();
			var matches = filter.Match(line);
			matches.Should().NotBeNull();
			matches.Should().BeEmpty();
		}

		[Test]
		public void TestSingleLine3()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var line = new LogEntry(LogColumns.RawContent){RawContent = "bar"};
			filter.PassesFilter(line).Should().BeFalse();
			var matches = filter.Match(line);
			matches.Should().NotBeNull();
			matches.Should().BeEmpty();
		}

		[Test]
		public void TestSingleLine4()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogEntry(LogColumns.RawContent){RawContent = "foo"}).Should().BeTrue();
		}

		[Test]
		public void TestSingleLine5()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogEntry(LogColumns.RawContent){RawContent = "bar"}).Should().BeTrue();
		}

		[Test]
		public void TestSingleLine6()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogEntry(LogColumns.RawContent){RawContent = "foobar"}).Should().BeTrue();
		}

		[Test]
		public void TestSingleLine7()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogEntry(LogColumns.RawContent){RawContent = "FOOBAR"}).Should().BeFalse();
		}

		[Test]
		public void TestMultiLine1()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var lines = new[]
			{
				new LogEntry(LogColumns.RawContent){RawContent = "bar"},
				new LogEntry(LogColumns.RawContent){RawContent = "foo"}
			};

			filter.PassesFilter(lines).Should().BeTrue("because it should be enough to have a hit on one line of a multi line entry");
		}

		[Test]
		public void TestMultiLine2()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var lines = new[]
			{
				new LogEntry(LogColumns.RawContent){RawContent = "fo"},
				new LogEntry(LogColumns.RawContent){RawContent = "obar"}
			};
			
			filter.PassesFilter(lines).Should().BeFalse("because substring filters shouldn't be matched across lines");
		}

		[Test]
		public void TestToString()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			filter.ToString().Should().Be("message.Contains(foo, InvariantCultureIgnoreCase)");

			filter = new OrFilter(new ILogEntryFilter[] { new SubstringFilter("foo", true), new LevelFilter(LevelFlags.Info) });
			filter.ToString().Should().Be("message.Contains(foo, InvariantCultureIgnoreCase) || level == Info");
		}
	}
}