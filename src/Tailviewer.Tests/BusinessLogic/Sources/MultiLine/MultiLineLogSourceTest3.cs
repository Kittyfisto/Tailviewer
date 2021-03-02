using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Sources;

namespace Tailviewer.Tests.BusinessLogic.Sources.MultiLine
{
	[TestFixture]
	public sealed class MultiLineLogSourceTest3
		: AbstractAggregatedLogSourceTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogSource Create(ITaskScheduler taskScheduler, ILogSource source)
		{
			return new MultiLineLogSource(taskScheduler, source, TimeSpan.Zero);
		}

		#endregion
	}
}
