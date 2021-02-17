using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.Sources;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Searches
{
	[TestFixture]
	public sealed class LogFileSearchProxyTest
	{
		private DefaultTaskScheduler _scheduler;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new DefaultTaskScheduler();
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
		}

		[Test]
		public void TestCtor1()
		{
			var source = new InMemoryLogSource();
			using (var proxy = new LogSourceSearchProxy(_scheduler, source, TimeSpan.Zero))
			{
				proxy.SearchTerm.Should().BeNull();
				proxy.Count.Should().Be(0);
			}
		}

		[Test]
		[Description("Verifies that the search delivers correct results when the file is completely available before the search is started")]
		public void TestSearch1()
		{
			var source = new InMemoryLogSource();
			source.AddEntry("Hello World!");
			source.AddEntry("Foobar");

			using (var proxy = new LogSourceSearchProxy(_scheduler, source, TimeSpan.Zero))
			{
				proxy.SearchTerm = "foobar";
				proxy.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(20)).Be(1, "because we should be able to search through the file in a few seconds");

				proxy.Matches.Should().Equal(new[]
					{
						new LogMatch(1, new LogLineMatch(0, 6))
					});
				proxy.Count.Should().Be(1);
			}
		}

		[Test]
		[Description("Verifies that the search delivers correct results when the file is modified while the search is performed")]
		public void TestSearch2()
		{
			var source = new InMemoryLogSource();
			using (var proxy = new LogSourceSearchProxy(_scheduler, source, TimeSpan.Zero))
			{
				proxy.SearchTerm = "Foobar";

				source.AddEntry("Hello World!");
				source.AddEntry("Foobar");

				proxy.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1, "because we should be able to search through the file in a few seconds");

				proxy.Matches.Should().Equal(new[]
					{
						new LogMatch(1, new LogLineMatch(0, 6))
					});
				proxy.Count.Should().Be(1);
			}
		}
	}
}