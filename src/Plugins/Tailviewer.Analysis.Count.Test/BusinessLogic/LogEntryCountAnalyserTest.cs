using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.Count.BusinessLogic;
using Tailviewer.BusinessLogic;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.Settings;

namespace Tailviewer.Analysis.Count.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogEntryCountAnalyserTest
	{
		private ManualTaskScheduler _scheduler;
		private InMemoryLogFile _logFile;
		private ServiceContainer _services;

		[SetUp]
		public void Setup()
		{
			_scheduler = new ManualTaskScheduler();
			_services = new ServiceContainer();
			_services.RegisterInstance<ITaskScheduler>(_scheduler);
			_logFile = new InMemoryLogFile();
		}

		[Test]
		public void TestAnalyseEmpty()
		{
			var analyser = new LogEntryCountAnalyser(_services,
				_logFile,
				TimeSpan.Zero,
				new LogEntryCountAnalyserConfiguration());

			_scheduler.RunOnce();
			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<LogEntryCountResult>();
			((LogEntryCountResult) analyser.Result).Count.Should().Be(0);
		}

		[Test]
		public void TestAnalyseOneEntry()
		{
			_logFile.AddEntry("foobar", LevelFlags.All);
			var analyser = new LogEntryCountAnalyser(_services,
				_logFile,
				TimeSpan.Zero,
				new LogEntryCountAnalyserConfiguration());
			_scheduler.RunOnce();
			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<LogEntryCountResult>();
			((LogEntryCountResult)analyser.Result).Count.Should().Be(1);
		}

		[Test]
		public void TestAnalyseAddEntry()
		{
			var analyser = new LogEntryCountAnalyser(_services,
				_logFile,
				TimeSpan.Zero,
				new LogEntryCountAnalyserConfiguration());
			_scheduler.RunOnce();
			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<LogEntryCountResult>();
			((LogEntryCountResult)analyser.Result).Count.Should().Be(0);

			_logFile.AddEntry("foobar", LevelFlags.All);
			_scheduler.RunOnce();
			analyser.Result.Should().NotBeNull();
			analyser.Result.Should().BeOfType<LogEntryCountResult>();
			((LogEntryCountResult)analyser.Result).Count.Should().Be(1);
		}

		[Test]
		public void TestAnalyseFiltered()
		{
			_logFile.AddEntry("foobar", LevelFlags.All);

			var analyser = new LogEntryCountAnalyser(_services,
				_logFile,
				TimeSpan.Zero,
				new LogEntryCountAnalyserConfiguration
				{
					QuickFilters =
					{
						new QuickFilter
						{
							Value = "fuck this shit",
							MatchType = FilterMatchType.SubstringFilter
						}
					}
				});

			_scheduler.RunOnce();
			((LogEntryCountResult)analyser.Result).Count.Should().Be(0, "because the one entry doesn't match the configured filter");
		}

		[Test]
		public void TestDispose1()
		{
			var analyser = new LogEntryCountAnalyser(_services,
				_logFile,
				TimeSpan.Zero,
				new LogEntryCountAnalyserConfiguration());
			analyser.Dispose();

			_logFile.AddEntry("foobar", LevelFlags.All);
			((LogEntryCountResult)analyser.Result).Count.Should().Be(0, "because the analyser shouldn't have reacted to the modification after it has been disposed of");

			_scheduler.PeriodicTaskCount.Should()
				.Be(0, "because after the analyser has been disposed of, no tasks should be left running");
		}

		[Test]
		public void TestDispose2()
		{
			var analyser = new LogEntryCountAnalyser(_services,
				_logFile,
				TimeSpan.Zero,
				new LogEntryCountAnalyserConfiguration
				{
					QuickFilters =
					{
						new QuickFilter
						{
							Value = "foobar"
						}
					}
				});
			analyser.Dispose();

			_logFile.AddEntry("foobar", LevelFlags.All);
			((LogEntryCountResult)analyser.Result).Count.Should().Be(0, "because the analyser shouldn't have reacted to the modification after it has been disposed of");

			_scheduler.PeriodicTaskCount.Should()
				.Be(0, "because after the analyser has been disposed of, no tasks should be left running");
		}
	}
}