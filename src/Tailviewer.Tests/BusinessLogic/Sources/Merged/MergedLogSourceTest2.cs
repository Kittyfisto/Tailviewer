using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Tests.BusinessLogic.Sources.Merged
{
	[TestFixture]
	public sealed class MergedLogSourceTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new MergedLogSource(taskScheduler, TimeSpan.Zero, new EmptyLogSource());
		}

		#endregion
	}
}