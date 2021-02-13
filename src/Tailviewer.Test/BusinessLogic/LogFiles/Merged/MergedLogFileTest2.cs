using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Merged
{
	[TestFixture]
	public sealed class MergedLogFileTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogFile CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new MergedLogFile(taskScheduler, TimeSpan.Zero, new EmptyLogFile());
		}

		#endregion
	}
}