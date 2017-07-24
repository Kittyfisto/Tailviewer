using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
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
			var line = new LogLine(0, 0, "foobar", LevelFlags.All);
			filter.PassesFilter(line).Should().BeTrue();
			var matches = filter.Match(line);
			matches.Should().NotBeNull();
			matches.Should().HaveCount(1);
		}

		[Test]
		public void TestSingleLine2()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var line = new LogLine(0, 0, "fobar", LevelFlags.All);
			filter.PassesFilter(line).Should().BeFalse();
			var matches = filter.Match(line);
			matches.Should().NotBeNull();
			matches.Should().BeEmpty();
		}

		[Test]
		public void TestSingleLine3()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var line = new LogLine(0, 0, "bar", LevelFlags.All);
			filter.PassesFilter(line).Should().BeFalse();
			var matches = filter.Match(line);
			matches.Should().NotBeNull();
			matches.Should().BeEmpty();
		}

		[Test]
		public void TestSingleLine4()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogLine(0, 0, "foo", LevelFlags.All)).Should().BeTrue();
		}

		[Test]
		public void TestSingleLine5()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogLine(0, 0, "bar", LevelFlags.All)).Should().BeTrue();
		}

		[Test]
		public void TestSingleLine6()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogLine(0, 0, "foobar", LevelFlags.All)).Should().BeTrue();
		}

		[Test]
		public void TestSingleLine7()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", false), new SubstringFilter("bar", false) });
			filter.PassesFilter(new LogLine(0, 0, "FOOBAR", LevelFlags.All)).Should().BeFalse();
		}

		[Test]
		public void TestMultiLine1()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var lines = new[]
			{
				new LogLine(0, 0, "bar", LevelFlags.All),
				new LogLine(0, 0, "foo", LevelFlags.All)
			};

			filter.PassesFilter(lines).Should().BeTrue("because it should be enough to have a hit on one line of a multi line entry");
		}

		[Test]
		public void TestMultiLine2()
		{
			var filter = new OrFilter(new[] { new SubstringFilter("foo", true) });
			var lines = new[]
			{
				new LogLine(0, 0, "fo", LevelFlags.All),
				new LogLine(0, 0, "obar", LevelFlags.All)
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