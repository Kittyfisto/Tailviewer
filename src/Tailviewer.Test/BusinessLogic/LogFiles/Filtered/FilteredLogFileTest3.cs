using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Filtered
{
	[TestFixture]
	public sealed class FilteredLogFileTest3
		: AbstractAggregatedLogFileTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogFile Create(ITaskScheduler taskScheduler, ILogFile source)
		{
			return new FilteredLogFile(taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Debug));
		}

		#endregion
	}
}
