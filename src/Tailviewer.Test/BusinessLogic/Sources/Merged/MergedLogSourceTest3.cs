﻿using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Sources.Merged;

namespace Tailviewer.Test.BusinessLogic.Sources.Merged
{
	[TestFixture]
	public sealed class MergedLogSourceTest3
		: AbstractAggregatedLogSourceTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogSource Create(ITaskScheduler taskScheduler, ILogSource source)
		{
			return new MergedLogSource(taskScheduler, TimeSpan.Zero, source);
		}

		#endregion
	}
}
