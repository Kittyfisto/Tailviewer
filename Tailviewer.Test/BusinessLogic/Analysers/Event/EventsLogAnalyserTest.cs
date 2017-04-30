using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Analysers.Event;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings.Dashboard.Analysers.Event;

namespace Tailviewer.Test.BusinessLogic.Analysers.Event
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
				new EventsAnalyserSettings()
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
			var settings = new EventsAnalyserSettings
			{
				Events =
				{
					new EventSettings
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
		public void TestOneEntry()
		{
			var settings = new EventsAnalyserSettings
			{
				Events =
				{
					new EventSettings {FilterExpression = @"Found (\d+) thing(s)"}
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
	}
}