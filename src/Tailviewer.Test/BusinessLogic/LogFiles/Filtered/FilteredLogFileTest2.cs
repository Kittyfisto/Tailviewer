using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Filtered
{
	[TestFixture]
	public sealed class FilteredLogFileTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogFile CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new FilteredLogFile(taskScheduler, TimeSpan.Zero, new EmptyLogFile(), new WildcardFilter("*", true),
			                           null);
		}

		#endregion
	}
}