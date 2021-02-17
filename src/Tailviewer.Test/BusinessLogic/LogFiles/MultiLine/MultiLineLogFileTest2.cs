using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.LogFiles.MultiLine
{
	[TestFixture]
	public sealed class MultiLineLogFileTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new MultiLineLogSource(taskScheduler, new EmptyLogSource(), TimeSpan.Zero);
		}

		#endregion
	}
}