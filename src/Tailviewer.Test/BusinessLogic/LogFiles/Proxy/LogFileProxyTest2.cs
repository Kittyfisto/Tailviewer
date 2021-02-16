using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Proxy
{
	[TestFixture]
	public sealed class LogFileProxyTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new LogSourceProxy(taskScheduler, TimeSpan.Zero);
		}

		#endregion
	}
}