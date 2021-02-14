using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	/// <summary>
	///    This class is responsible for testing <see cref="ILogFile"/> implementations which rely on a <see cref="ITaskScheduler"/>.
	/// </summary>
	public abstract class AbstractTaskSchedulerLogFileTest
	{
		private ManualTaskScheduler _taskScheduler;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
		}

		private ILogFile CreateEmpty()
		{
			return CreateEmpty(_taskScheduler);
		}

		protected abstract ILogFile CreateEmpty(ITaskScheduler taskScheduler);

		#region Percentage Processed

		[Test]
		public void TestPercentageProcessedEmpty()
		{
			using (var logFile = CreateEmpty())
			{
				logFile.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.Zero, "because the log file didn't have enough time to check the source");

				_taskScheduler.RunOnce();

				logFile.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because we've checked that the source doesn't exist and thus there's nothing more to process");
			}
		}

		#endregion
	}
}