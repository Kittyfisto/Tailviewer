using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;

namespace Tailviewer.Test.BusinessLogic.Searches
{
	[TestFixture]
	public sealed class SearchResultsTest
	{
		[Test]
		public void TestAdd1()
		{
			var results = new SearchResults();
			results.Add(1, new LogLineMatch(5, 42));
			results.Count.Should().Be(1);
			results.MatchesByLine[1].Should().Equal(new[] {new LogLineMatch(5, 42)});
		}

		[Test]
		public void TestAdd2()
		{
			var results = new SearchResults();
			results.Add(0, new LogLineMatch(1, 2));
			results.Add(1, new LogLineMatch(5, 42));
			results.Count.Should().Be(2);
			results.MatchesByLine[0].Should().Equal(new[] { new LogLineMatch(1, 2) });
			results.MatchesByLine[1].Should().Equal(new[] { new LogLineMatch(5, 42) });
		}

		[Test]
		public void TestClear1()
		{
			var results = new SearchResults();
			new Action(results.Clear).Should().NotThrow();
			results.Count.Should().Be(0);
			results.Matches.Should().BeEmpty();
		}

		[Test]
		public void TestClear2()
		{
			var results = new SearchResults();
			results.Add(42, new LogLineMatch(1, 5));

			results.Count.Should().Be(1);
			results.Matches.Should().Equal(new[] {new LogMatch(42, new LogLineMatch(1, 5))});

			new Action(results.Clear).Should().NotThrow();
			results.Count.Should().Be(0);
			results.Matches.Should().BeEmpty();
		}

		[Test]
		public void TestClear3()
		{
			var results = new SearchResults();
			results.Add(42, new LogLineMatch(1, 5));

			results.MatchesByLine[42].Should().NotBeEmpty();

			new Action(results.Clear).Should().NotThrow();
			results.MatchesByLine[42].Should().BeEmpty();
		}
	}
}