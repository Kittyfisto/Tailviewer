using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Proxy
{
	[TestFixture]
	public sealed class LogFileProxyTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogFile CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new LogFileProxy(taskScheduler, TimeSpan.Zero);
		}

		#endregion
	}
}