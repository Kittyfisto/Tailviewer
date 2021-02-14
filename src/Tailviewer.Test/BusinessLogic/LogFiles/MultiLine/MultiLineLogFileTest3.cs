using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.MultiLine
{
	[TestFixture]
	public sealed class MultiLineLogFileTest3
		: AbstractAggregatedLogFileTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogFile Create(ITaskScheduler taskScheduler, ILogFile source)
		{
			return new MultiLineLogFile(taskScheduler, source, TimeSpan.Zero);
		}

		#endregion
	}
}
