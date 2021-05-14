using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources.Filtered
{
	[TestFixture]
	public sealed class FilteredLogSourceTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(IFilesystem filesystem, ITaskScheduler taskScheduler)
		{
			return new FilteredLogSource(taskScheduler, TimeSpan.Zero, new EmptyLogSource(), new WildcardFilter("*", true),
			                           null);
		}

		#endregion
	}
}