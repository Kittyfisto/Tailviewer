using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Sources;

namespace Tailviewer.Tests.BusinessLogic.Sources.Proxy
{
	[TestFixture]
	public sealed class LogSourceProxyTest2
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