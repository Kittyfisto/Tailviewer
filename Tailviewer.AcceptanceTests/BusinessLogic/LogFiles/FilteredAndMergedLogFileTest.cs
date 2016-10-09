using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		[Ignore]
		public void Test()
		{
			using (var source1 = new LogFile(_scheduler, LogFileTest.File2Entries))
			using (var source2 = new LogFile(_scheduler, LogFileTest.File2Lines))
			{
				var sources = new List<ILogFile> {source1, source2};
				using (var merged = new MergedLogFile(_scheduler, sources))
				{
					var filter = new SubstringFilter("foo", true);
					using (var filtered = new FilteredLogFile(_scheduler, merged, filter))
					{
						source1.Start();
						source2.Start();
						merged.Start(TimeSpan.FromMilliseconds(10));
						filtered.Start(TimeSpan.FromMilliseconds(10));

						filtered.Property(x => x.Count).ShouldEventually().Be(1, TimeSpan.FromSeconds(5));
					}
				}
			}
		}
	}
}