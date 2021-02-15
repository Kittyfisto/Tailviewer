using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;

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

		private TextLogFile Create(string fileName)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance<ITaskScheduler>(_scheduler);
			return new TextLogFile(serviceContainer, fileName);
		}

		[Test]
		[Ignore("Test isn't finished yet")]
		public void Test()
		{
			using (var source1 = Create(TextLogFileAcceptanceTest.File2Entries))
			using (var source2 = Create(TextLogFileAcceptanceTest.File2Lines))
			{
				var sources = new List<ILogFile> {source1, source2};
				using (var merged = new MergedLogFile(_scheduler, TimeSpan.FromMilliseconds(10), sources))
				{
					var filter = new SubstringFilter("foo", true);
					using (var filtered = new FilteredLogFile(_scheduler, TimeSpan.FromMilliseconds(10), merged, null, filter))
					{
						filtered.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);
					}
				}
			}
		}
	}
}