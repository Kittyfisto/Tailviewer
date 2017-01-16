using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Tailviewer.Test.BusinessLogic.Scheduling
{
	[TestFixture]
	[LocalTest("Won't work on AppVeyor anymore...")]
	public sealed class TaskSchedulerTest
	{
		[Test]
		public void TestDispose1()
		{
			var scheduler = new DefaultTaskScheduler();
			new Action(scheduler.Dispose).ShouldNotThrow();
		}

		[Test]
		public void TestStart1()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				scheduler.Start(() => "foobar").Result.Should().Be("foobar");
			}
		}

		[Test]
		public void TestStartPeriodic1()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				var task = scheduler.StartPeriodic(() => { }, TimeSpan.FromSeconds(1));
				task.Should().NotBeNull();
				scheduler.StopPeriodic(task).Should().BeTrue();
			}
		}

		[Test]
		public void TestStartPeriodic2()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				int counter = 0;
				var task = scheduler.StartPeriodic(() => ++counter, TimeSpan.Zero);

				new object().Property(x => counter).ShouldEventually().BeGreaterOrEqual(100,
				                                                                        TimeSpan.FromSeconds(5),
				                                                                        "because our periodic task should've been executed at least 100 times by now");

				scheduler.StopPeriodic(task);
			}
		}

		[Test]
		public void TestRemovePeriodic1()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				scheduler.StopPeriodic(new Mock<IPeriodicTask>().Object).Should().BeFalse();
			}
		}
	}
}