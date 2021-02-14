using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.MultiLine
{
	[TestFixture]
	public sealed class MultiLineLogFileTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogFile CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new MultiLineLogFile(taskScheduler, new EmptyLogFile(), TimeSpan.Zero);
		}

		#endregion
	}
}