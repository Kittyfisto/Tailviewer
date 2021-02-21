﻿using System;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.Sources.Filtered
{
	[TestFixture]
	public sealed class FilteredLogSourceTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(ITaskScheduler taskScheduler)
		{
			return new FilteredLogSource(taskScheduler, TimeSpan.Zero, new EmptyLogSource(), new WildcardFilter("*", true),
			                           null);
		}

		#endregion
	}
}