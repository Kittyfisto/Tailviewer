using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class FilteredAndMergedLogFileTest
	{
		private DefaultTaskScheduler _scheduler;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new DefaultTaskScheduler();
		}

		[TearDown]
		public void TearDown()
		{
			_scheduler.Dispose();
		}

		[Test]
		[Ignore("Test isn't finished yet")]
		public void Test()
		{
			using (var source1 = new TestLogFile(_scheduler, LogFileRealTest.File2Entries))
			using (var source2 = new TestLogFile(_scheduler, LogFileRealTest.File2Lines))
			{
				var sources = new List<ILogFile> {source1, source2};
				using (var merged = new MergedLogFile(_scheduler, TimeSpan.FromMilliseconds(10), sources))
				{
					var filter = new SubstringFilter("foo", true);
					using (var filtered = new FilteredLogFile(_scheduler, TimeSpan.FromMilliseconds(10), merged, null, filter))
					{
						filtered.Property(x => x.Count).ShouldEventually().Be(1, TimeSpan.FromSeconds(5));
					}
				}
			}
		}
	}
}