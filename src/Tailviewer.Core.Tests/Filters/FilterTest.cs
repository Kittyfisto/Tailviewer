using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Filters
{
	[TestFixture]
	public sealed class FilterTest
	{
		[Test]
		public void TestCreateDefaultIsAnd()
		{
			var filter = Filter.Create(new ILogEntryFilter[]
			{
				new SubstringFilter("foo", true),
				new SubstringFilter("bar", true)
			});

			filter.Should().BeOfType<AndFilter>();
		}

		[Test]
		public void TestCreateAnd()
		{
			var filter = Filter.Create(new ILogEntryFilter[]
			{
				new SubstringFilter("foo", true),
				new SubstringFilter("bar", true)
			}, FilterCombinationMode.And);

			filter.Should().BeOfType<AndFilter>();
			filter.PassesFilter(Entry("foo bar")).Should().BeTrue();
			filter.PassesFilter(Entry("foo")).Should().BeFalse();
		}

		[Test]
		public void TestCreateOr()
		{
			var filter = Filter.Create(new ILogEntryFilter[]
			{
				new SubstringFilter("foo", true),
				new SubstringFilter("bar", true)
			}, FilterCombinationMode.Or);

			filter.Should().BeOfType<OrFilter>();
			filter.PassesFilter(Entry("foo")).Should().BeTrue();
			filter.PassesFilter(Entry("bar")).Should().BeTrue();
			filter.PassesFilter(Entry("baz")).Should().BeFalse();
		}

		[Test]
		public void TestCreateSingleFilter()
		{
			var filter = Filter.Create(new ILogEntryFilter[] {new SubstringFilter("foo", true)}, FilterCombinationMode.Or);
			filter.Should().BeOfType<SubstringFilter>();
		}

		[Test]
		public void TestCreateEmpty()
		{
			Filter.Create(new ILogEntryFilter[0], FilterCombinationMode.And).Should().BeNull();
			Filter.Create(new ILogEntryFilter[0], FilterCombinationMode.Or).Should().BeNull();
		}

		[Test]
		public void TestCreateWithLevelAndTimeAlwaysAnd()
		{
			var quickFilters = new ILogEntryFilter[]
			{
				new SubstringFilter("foo", true),
				new SubstringFilter("bar", true)
			};
			var timeFilter = new SubstringFilter("time", true);

			var filter = Filter.Create(LevelFlags.Info, timeFilter, quickFilters, FilterCombinationMode.Or);

			filter.PassesFilter(Entry("time foo", LevelFlags.Info)).Should().BeTrue();
			filter.PassesFilter(Entry("time foo", LevelFlags.Debug)).Should().BeFalse("because the level filter must always be AND-ed");
			filter.PassesFilter(Entry("foo", LevelFlags.Info)).Should().BeFalse("because the time filter must always be AND-ed");
		}

		private static IReadOnlyLogEntry Entry(string rawContent, LevelFlags level = LevelFlags.Info)
		{
			return new LogEntry(Core.Columns.RawContent, Core.Columns.LogLevel)
			{
				RawContent = rawContent,
				LogLevel = level
			};
		}
	}
}
