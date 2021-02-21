using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text;
using Tailviewer.Core;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Merged;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Core.Sources.Text.Simple;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class FilteredAndMergedLogSourceTest
	{
		private DefaultTaskScheduler _taskScheduler;

		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		private TextLogSource Create(string fileName)
		{
			return new TextLogSource(_taskScheduler, fileName, LogFileFormats.GenericText, Encoding.Default);
		}

		[Test]
		[Ignore("Test isn't finished yet")]
		public void Test()
		{
			using (var source1 = Create(AbstractTextLogSourceAcceptanceTest.File2Entries))
			using (var source2 = Create(AbstractTextLogSourceAcceptanceTest.File2Lines))
			{
				var sources = new List<ILogSource> {source1, source2};
				using (var merged = new MergedLogSource(_taskScheduler, TimeSpan.FromMilliseconds(10), sources))
				{
					var filter = new SubstringFilter("foo", true);
					using (var filtered = new FilteredLogSource(_taskScheduler, TimeSpan.FromMilliseconds(10), merged, null, filter))
					{
						filtered.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);
					}
				}
			}
		}
	}
}