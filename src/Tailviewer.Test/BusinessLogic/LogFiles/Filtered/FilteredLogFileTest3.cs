using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Filtered
{
	[TestFixture]
	public sealed class FilteredLogFileTest3
		: AbstractAggregatedLogFileTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogSource Create(ITaskScheduler taskScheduler, ILogSource source)
		{
			return new FilteredLogSource(taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Debug));
		}

		#endregion
	}
}
