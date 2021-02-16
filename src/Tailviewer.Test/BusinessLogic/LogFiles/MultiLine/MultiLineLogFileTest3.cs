using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.LogFiles.MultiLine
{
	[TestFixture]
	public sealed class MultiLineLogFileTest3
		: AbstractAggregatedLogFileTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogSource Create(ITaskScheduler taskScheduler, ILogSource source)
		{
			return new MultiLineLogSource(taskScheduler, source, TimeSpan.Zero);
		}

		#endregion
	}
}
