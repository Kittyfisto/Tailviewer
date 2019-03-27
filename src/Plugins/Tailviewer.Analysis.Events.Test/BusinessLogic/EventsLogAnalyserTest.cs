using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.Events.BusinessLogic;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogTables;

namespace Tailviewer.Analysis.Events.Test.BusinessLogic
{
	[TestFixture]
	public sealed class EventsLogAnalyserTest
	{
		[SetUp]
		public void Setup()
		{
			_scheduler = new ManualTaskScheduler();
			_source = new InMemoryLogFile();
		}

		[TearDown]
		public void Teardown()
		{
		}

		private ManualTaskScheduler _scheduler;
		private InMemoryLogFile _source;

		[Test]
		public void TestConstruction1()
		{
			using (var analyser = new EventsLogAnalyser(_scheduler,
				_source,
				TimeSpan.Zero,
				new EventsLogAnalyserConfiguration()
			))
			{
				analyser.Events.Should().NotBeNull();
				analyser.Events.Count.Should().Be(0);
				analyser.AnalysisTime.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
				analyser.UnexpectedExceptions.Should().NotBeNull();
				analyser.UnexpectedExceptions.Should().BeEmpty();
			}
		}

		[Test]
		public void TestConstruction2()
		{
			var settings = new EventsLogAnalyserConfiguration
			{
				Events =
				{
					new EventConfiguration
					{
						FilterExpression = "[."
					}
				}
			};
			new Action(() => new EventsLogAnalyser(_scheduler,
				_source,
				TimeSpan.Zero,
				settings)).ShouldNotThrow("becuase the analyser should just ignore invalid event definitions");
		}

		[Test]
		public void TestOneEntry1()
		{
			var settings = new EventsLogAnalyserConfiguration
			{
				Events =
				{
					new EventConfiguration {FilterExpression = @"Found (\d+) thing(s)"}
				}
			};
			_source.AddEntry("Found 42 things", LevelFlags.None);

			using (var analyser = new EventsLogAnalyser(_scheduler,
				_source,
				TimeSpan.Zero,
				settings
			))
			{
				_scheduler.RunOnce();
				analyser.Events.Count.Should().Be(1);
			}
		}

		[Test]
		public void TestOneEntry2()
		{
			var settings = new EventsLogAnalyserConfiguration
			{
				Events =
				{
					new EventConfiguration {FilterExpression = @"Found (\d+) thing(s)"}
				}
			};
			_source.AddEntry("Found tmp things", LevelFlags.None);

			using (var analyser = new EventsLogAnalyser(_scheduler,
				_source,
				TimeSpan.Zero,
				settings
			))
			{
				_scheduler.RunOnce();
				analyser.Events.Count.Should().Be(0);
			}
		}

		[Test]
		public void TestAddEntry1()
		{
			var settings = new EventsLogAnalyserConfiguration
			{
				Events =
				{
					new EventConfiguration {FilterExpression = @"Found (\d+) things"}
				}
			};
			using (var analyser = new EventsLogAnalyser(_scheduler,
				_source,
				TimeSpan.Zero,
				settings
			))
			{
				_scheduler.RunOnce();
				analyser.Events.Count.Should().Be(0);

				_source.AddEntry("Found 42 things", LevelFlags.None);
				_scheduler.RunOnce();
				analyser.Events.Count.Should().Be(1);
				var actualEntry = analyser.Events[0].Result;
				actualEntry.Should().Be(new LogEntry(null, "42"));
			}
		}

	}
}