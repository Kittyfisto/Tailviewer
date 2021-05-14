using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources.Proxy
{
	[TestFixture]
	public sealed class LogSourceProxyTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(IFilesystem filesystem, ITaskScheduler taskScheduler)
		{
			return new LogSourceProxy(taskScheduler, TimeSpan.Zero);
		}

		#endregion
	}
}