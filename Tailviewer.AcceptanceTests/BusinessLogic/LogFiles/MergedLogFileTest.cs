using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class MergedLogFileTest
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
		[Ignore(
			"Doesn't work anymore because the actual source file is not ordered by time strictly ascending - several lines are completely out of order"
			)]
		[Description("Verifies that the MergedLogFile represents the very same content than its single source")]
		public void Test20Mb()
		{
			using (var source = new LogFile(_scheduler, LogFileRealTest.File20Mb))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.FromMilliseconds(1), source))
			{
				source.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				merged.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				merged.Count.Should().Be(source.Count);
				merged.FileSize.Should().Be(source.FileSize);
				merged.StartTimestamp.Should().Be(source.StartTimestamp);

				LogLine[] sourceLines = source.GetSection(new LogFileSection(0, source.Count));
				LogLine[] mergedLines = merged.GetSection(new LogFileSection(0, merged.Count));
				for (int i = 0; i < source.Count; ++i)
				{
					LogLine mergedLine = mergedLines[i];
					LogLine sourceLine = sourceLines[i];
					mergedLine.Should().Be(sourceLine);
				}
			}
		}

		[Test]
		public void Test2SmallSources()
		{
			using (var source1 = new LogFile(_scheduler, LogFileRealTest.File2Entries))
			using (var source2 = new LogFile(_scheduler, LogFileRealTest.File2Lines))
			using (var multi1 = new MultiLineLogFile(_scheduler, source1, TimeSpan.Zero))
			using (var multi2 = new MultiLineLogFile(_scheduler, source2, TimeSpan.Zero))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, multi1, multi2))
			{
				source1.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();
				source2.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				multi1.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();
				multi2.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				merged.Property(x => x.Count).ShouldEventually().Be(8, TimeSpan.FromSeconds(5),
																	"Because the merged file should've been finished");
				merged.Property(x => x.FileSize).ShouldEventually().Be(source1.FileSize + source2.FileSize);
				merged.Property(x => x.StartTimestamp).ShouldEventually().Be(source2.StartTimestamp);

				LogLine[] source1Lines = multi1.GetSection(new LogFileSection(0, source1.Count));
				LogLine[] source2Lines = multi2.GetSection(new LogFileSection(0, source2.Count));
				LogLine[] mergedLines = merged.GetSection(new LogFileSection(0, merged.Count));

				mergedLines[0].Should().Be(new LogLine(0, 0, source2Lines[0]));
				mergedLines[1].Should().Be(new LogLine(1, 1, source1Lines[0]));
				mergedLines[2].Should().Be(new LogLine(2, 1, source1Lines[1]));
				mergedLines[3].Should().Be(new LogLine(3, 1, source1Lines[2]));
				mergedLines[4].Should().Be(new LogLine(4, 2, source2Lines[1]));
				mergedLines[5].Should().Be(new LogLine(5, 3, source1Lines[3]));
				mergedLines[6].Should().Be(new LogLine(6, 3, source1Lines[4]));
				mergedLines[7].Should().Be(new LogLine(7, 3, source1Lines[5]));
			}
		}

		[Test]
		public void TestLive1And2()
		{
			using (var source1 = new LogFile(_scheduler, LogFileRealTest.FileTestLive1))
			using (var source2 = new LogFile(_scheduler, LogFileRealTest.FileTestLive2))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, source1, source2))
			{
				merged.Property(x => x.Count).ShouldEventually().Be(19, TimeSpan.FromSeconds(5),
				                                                    "Because the merged file should've been finished");
				merged.Property(x => x.FileSize).ShouldEventually().Be(source1.FileSize + source2.FileSize);
				merged.Property(x => x.StartTimestamp).ShouldEventually().Be(source1.StartTimestamp);

				LogLine[] mergedLines = merged.GetSection(new LogFileSection(0, merged.Count));

				mergedLines[0].Should()
				              .Be(new LogLine(0, 0,
				                              "2016-02-17 22:57:51,449 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - Test",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 449)));
				mergedLines[1].Should()
				              .Be(new LogLine(1, 1,
				                              "2016-02-17 22:57:51,559 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - Hello",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 559)));
				mergedLines[2].Should()
				              .Be(new LogLine(2, 2,
				                              "2016-02-17 22:57:51,560 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - Hello",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 560)));
				mergedLines[3].Should()
				              .Be(new LogLine(3, 3,
				                              "2016-02-17 22:57:51,664 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - world!",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 664)));
				mergedLines[4].Should()
				              .Be(new LogLine(4, 4,
				                              "2016-02-17 22:57:51,665 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - world!",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 665)));
				mergedLines[5].Should()
				              .Be(new LogLine(5, 5,
				                              "2016-02-17 22:57:59,284 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 284)));
				mergedLines[6].Should()
				              .Be(new LogLine(6, 6,
				                              "2016-02-17 22:57:59,284 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 284)));
				mergedLines[7].Should()
				              .Be(new LogLine(7, 7,
				                              "2016-02-17 22:57:59,299 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 299)));
				mergedLines[8].Should()
				              .Be(new LogLine(8, 8,
				                              "2016-02-17 22:57:59,299 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 299)));
				mergedLines[9].Should()
				              .Be(new LogLine(9, 9,
				                              @"2016-02-17 22:57:59,302 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear1.log' doesn't have an ID yet, setting it to: b62ea0a3-c495-4f3f-b7c7-d1a0a66e361e",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 302)));
				mergedLines[10].Should()
				               .Be(new LogLine(10, 10,
				                               @"2016-02-17 22:57:59,303 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear1.log' doesn't have an ID yet, setting it to: b62ea0a3-c495-4f3f-b7c7-d1a0a66e361e",
				                               LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 303)));
				mergedLines[11].Should()
				               .Be(new LogLine(11, 11,
				                               @"2016-02-17 22:57:59,304 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear2.log' doesn't have an ID yet, setting it to: 0ff1c032-0754-405f-8193-2fa4dbfb7d07",
				                               LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 304)));
				mergedLines[12].Should()
				               .Be(new LogLine(12, 12,
				                               @"2016-02-17 22:57:59,305 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear2.log' doesn't have an ID yet, setting it to: 0ff1c032-0754-405f-8193-2fa4dbfb7d07",
				                               LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 305)));
				mergedLines[13].Should()
				               .Be(new LogLine(13, 13,
				                               "2016-02-17 22:57:59,306 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 306)));
				mergedLines[14].Should()
				               .Be(new LogLine(14, 14,
				                               "2016-02-17 22:57:59,307 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 307)));
				mergedLines[15].Should()
				               .Be(new LogLine(15, 15,
				                               "2016-02-17 22:57:59,310 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 310)));
				mergedLines[16].Should()
				               .Be(new LogLine(16, 16,
				                               "2016-02-17 22:57:59,311 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 311)));
				mergedLines[17].Should()
				               .Be(new LogLine(17, 17,
				                               @"2016-02-17 22:57:59,863 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.BusinessLogic.DataSources - DataSource 'foo (ec976867-195b-4adf-a819-a1427f0d9aac)' is assigned a parent 'f671f235-7084-4e57-b06a-d253f750fae6' but we don't know that one",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 863)));
				mergedLines[18].Should()
				               .Be(new LogLine(18, 18,
				                               @"2016-02-17 22:57:59,864 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.BusinessLogic.DataSources - DataSource 'foo (ec976867-195b-4adf-a819-a1427f0d9aac)' is assigned a parent 'f671f235-7084-4e57-b06a-d253f750fae6' but we don't know that one",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 864)));
			}
		}
	}
}