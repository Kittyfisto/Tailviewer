﻿using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.Sources.Filtered
{
	[TestFixture]
	public sealed class FilteredLogSourceTest3
		: AbstractAggregatedLogSourceTest
	{
		#region Overrides of AbstractAggregatedLogFileTest

		protected override ILogSource Create(ITaskScheduler taskScheduler, ILogSource source)
		{
			return new FilteredLogSource(taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Debug));
		}

		#endregion
	}
}