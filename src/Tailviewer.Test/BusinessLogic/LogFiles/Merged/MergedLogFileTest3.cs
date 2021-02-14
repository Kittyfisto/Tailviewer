using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Merged
{
	[TestFixture]
	public sealed class MergedLogFileTest3
		: AbstractAggregatedLogFileTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogFile Create(ITaskScheduler taskScheduler, ILogFile source)
		{
			return new MergedLogFile(taskScheduler, TimeSpan.Zero, source);
		}

		#endregion
	}
}
