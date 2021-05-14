using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Sources.Merged
{
	[TestFixture]
	public sealed class MergedLogSourceTest2
		: AbstractTaskSchedulerLogFileTest
	{
		#region Overrides of AbstractTaskSchedulerLogFileTest

		protected override ILogSource CreateEmpty(IFilesystem filesystem, ITaskScheduler taskScheduler)
		{
			return new MergedLogSource(taskScheduler, TimeSpan.Zero, new EmptyLogSource());
		}

		#endregion
	}
}