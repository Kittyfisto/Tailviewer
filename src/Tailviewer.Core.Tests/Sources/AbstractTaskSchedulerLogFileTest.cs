﻿using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources
{
	/// <summary>
	///    This class is responsible for testing <see cref="ILogSource"/> implementations which rely on a <see cref="ITaskScheduler"/>.
	/// </summary>
	public abstract class AbstractTaskSchedulerLogFileTest
	{
		private ManualTaskScheduler _taskScheduler;
		private Filesystem _filesystem;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
			_filesystem = new Filesystem(_taskScheduler);
		}

		private ILogSource CreateEmpty()
		{
			return CreateEmpty(_filesystem, _taskScheduler);
		}

		protected abstract ILogSource CreateEmpty(IFilesystem filesystem, ITaskScheduler taskScheduler);

		#region Percentage Processed

		[Test]
		public void TestPercentageProcessedEmpty()
		{
			using (var logFile = CreateEmpty())
			{
				logFile.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.Zero, "because the log file didn't have enough time to check the source");

				_taskScheduler.RunOnce();

				logFile.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because we've checked that the source doesn't exist and thus there's nothing more to process");
			}
		}

		#endregion
	}
}