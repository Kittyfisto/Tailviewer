using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Sources.Merged;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Merged
{
	[TestFixture]
	public sealed class MergedLogFileTest3
		: AbstractAggregatedLogFileTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogSource Create(ITaskScheduler taskScheduler, ILogSource source)
		{
			return new MergedLogSource(taskScheduler, TimeSpan.Zero, source);
		}

		#endregion
	}
}
