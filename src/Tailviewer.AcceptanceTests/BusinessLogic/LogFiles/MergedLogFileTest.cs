using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Test;

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
			using (var source = new TextLogFile(_scheduler, TextLogFileAcceptanceTest.File20Mb))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.FromMilliseconds(1), source))
			{
				source.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				merged.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				merged.Count.Should().Be(source.Count);
				merged.GetValue(LogFileProperties.Size).Should().Be(source.GetValue(LogFileProperties.Size));
				merged.GetValue(LogFileProperties.StartTimestamp).Should().Be(source.GetValue(LogFileProperties.StartTimestamp));

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
			using (var source0 = new TextLogFile(_scheduler, TextLogFileAcceptanceTest.File2Entries))
			using (var source1 = new TextLogFile(_scheduler, TextLogFileAcceptanceTest.File2Lines))
			using (var multi0 = new MultiLineLogFile(_scheduler, source0, TimeSpan.Zero))
			using (var multi1 = new MultiLineLogFile(_scheduler, source1, TimeSpan.Zero))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, multi0, multi1))
			{
				source0.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();
				source1.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				multi0.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();
				multi1.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				merged.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(8, "Because the merged file should've been finished");
				merged.Property(x => x.GetValue(LogFileProperties.Size)).ShouldEventually().Be(source0.GetValue(LogFileProperties.Size) + source1.GetValue(LogFileProperties.Size));
				merged.Property(x => x.GetValue(LogFileProperties.StartTimestamp)).ShouldEventually().Be(source1.GetValue(LogFileProperties.StartTimestamp));

				LogLine[] source0Lines = multi0.GetSection(new LogFileSection(0, source0.Count));
				LogLine[] source1Lines = multi1.GetSection(new LogFileSection(0, source1.Count));
				LogLine[] mergedLines = merged.GetSection(new LogFileSection(0, merged.Count));

				mergedLines[0].Should().Be(new LogLine(0, 0, new LogLineSourceId(1), source1Lines[0]));
				mergedLines[1].Should().Be(new LogLine(1, 1, new LogLineSourceId(0), source0Lines[0]));
				mergedLines[2].Should().Be(new LogLine(2, 1, new LogLineSourceId(0), source0Lines[1]));
				mergedLines[3].Should().Be(new LogLine(3, 1, new LogLineSourceId(0), source0Lines[2]));
				mergedLines[4].Should().Be(new LogLine(4, 2, new LogLineSourceId(1), source1Lines[1]));
				mergedLines[5].Should().Be(new LogLine(5, 3, new LogLineSourceId(0), source0Lines[3]));
				mergedLines[6].Should().Be(new LogLine(6, 3, new LogLineSourceId(0), source0Lines[4]));
				mergedLines[7].Should().Be(new LogLine(7, 3, new LogLineSourceId(0), source0Lines[5]));
			}
		}

		[Test]
		public void TestLive1And2()
		{
			using (var source0 = new TextLogFile(_scheduler, TextLogFileAcceptanceTest.FileTestLive1))
			using (var source1 = new TextLogFile(_scheduler, TextLogFileAcceptanceTest.FileTestLive2))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, source0, source1))
			{
				merged.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(19, "Because the merged file should've been finished");
				merged.Property(x => x.GetValue(LogFileProperties.Size)).ShouldEventually().Be(source0.GetValue(LogFileProperties.Size) + source1.GetValue(LogFileProperties.Size));
				merged.Property(x => x.GetValue(LogFileProperties.StartTimestamp)).ShouldEventually().Be(source0.GetValue(LogFileProperties.StartTimestamp));

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
				              .Be(new LogLine(2, 2, 2, new LogLineSourceId(1),
				                              "2016-02-17 22:57:51,560 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - Hello",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 560)));
				mergedLines[3].Should()
				              .Be(new LogLine(3, 3,
				                              "2016-02-17 22:57:51,664 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - world!",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 664)));
				mergedLines[4].Should()
				              .Be(new LogLine(4, 4, 4, new LogLineSourceId(1),
				                              "2016-02-17 22:57:51,665 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - world!",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 51, 665)));
				mergedLines[5].Should()
				              .Be(new LogLine(5, 5,
				                              "2016-02-17 22:57:59,284 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 284)));
				mergedLines[6].Should()
				              .Be(new LogLine(6, 6, 6, new LogLineSourceId(1),
				                              "2016-02-17 22:57:59,285 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 285)));
				mergedLines[7].Should()
				              .Be(new LogLine(7, 7,
				                              "2016-02-17 22:57:59,298 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 298)));
				mergedLines[8].Should()
				              .Be(new LogLine(8, 8, 8, new LogLineSourceId(1),
				                              "2016-02-17 22:57:59,299 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                              LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 299)));
				mergedLines[9].Should()
				              .Be(new LogLine(9, 9,
				                              @"2016-02-17 22:57:59,302 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear1.log' doesn't have an ID yet, setting it to: b62ea0a3-c495-4f3f-b7c7-d1a0a66e361e",
				                              LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 302)));
				mergedLines[10].Should()
				               .Be(new LogLine(10, 10, 10, new LogLineSourceId(1),
				                               @"2016-02-17 22:57:59,303 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear1.log' doesn't have an ID yet, setting it to: b62ea0a3-c495-4f3f-b7c7-d1a0a66e361e",
				                               LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 303)));
				mergedLines[11].Should()
				               .Be(new LogLine(11, 11,
				                               @"2016-02-17 22:57:59,304 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear2.log' doesn't have an ID yet, setting it to: 0ff1c032-0754-405f-8193-2fa4dbfb7d07",
				                               LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 304)));
				mergedLines[12].Should()
				               .Be(new LogLine(12, 12, 12, new LogLineSourceId(1),
				                               @"2016-02-17 22:57:59,305 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear2.log' doesn't have an ID yet, setting it to: 0ff1c032-0754-405f-8193-2fa4dbfb7d07",
				                               LevelFlags.Info, new DateTime(2016, 2, 17, 22, 57, 59, 305)));
				mergedLines[13].Should()
				               .Be(new LogLine(13, 13,
				                               "2016-02-17 22:57:59,306 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 306)));
				mergedLines[14].Should()
				               .Be(new LogLine(14, 14, 14, new LogLineSourceId(1),
				                               "2016-02-17 22:57:59,307 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 307)));
				mergedLines[15].Should()
				               .Be(new LogLine(15, 15, 15, new LogLineSourceId(1),
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
				               .Be(new LogLine(18, 18, 18, new LogLineSourceId(1),
				                               @"2016-02-17 22:57:59,864 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.BusinessLogic.DataSources - DataSource 'foo (ec976867-195b-4adf-a819-a1427f0d9aac)' is assigned a parent 'f671f235-7084-4e57-b06a-d253f750fae6' but we don't know that one",
				                               LevelFlags.Warning, new DateTime(2016, 2, 17, 22, 57, 59, 864)));
			}
		}

		[Test]
		[Repeat(10)]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/182")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/183")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/185")]
		[Description("Verifies that TextLogFile, MultiLineLogFile and MergedLogFile work in conjunction to produce a merged log file of two source files")]
		public void TestMultilineNoLogLevels()
		{
			using (var source0 = new TextLogFile(_scheduler, TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var source1 = new TextLogFile(_scheduler, TextLogFileAcceptanceTest.MultilineNoLogLevel2, new CustomTimestampParser()))
			using (var multi0 = new MultiLineLogFile(_scheduler, source0, TimeSpan.Zero))
			using (var multi1 = new MultiLineLogFile(_scheduler, source1, TimeSpan.Zero))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, multi0, multi1))
			{
				merged.Property(x => x.EndOfSourceReached).ShouldAfter(TimeSpan.FromSeconds(10)).BeTrue();
				var entries = merged.GetEntries(new LogFileSection(0, 11),
				                                LogFileColumns.Timestamp,
				                                LogFileColumns.LogEntryIndex,
				                                LogFileColumns.LineNumber,
				                                LogFileColumns.RawContent,
				                                LogFileColumns.OriginalDataSourceName);

				var line = entries[0];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 176));
				line.RawContent.Should().Be("18/03/2019 14:09:54:176    1 Information BTPVM3372 05:30:00 6060");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(0));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log2.txt");

				line = entries[1];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.RawContent.Should()
				    .Be("2019-03-18 14:09:54:177 1 00:00:00:0000000 Information Initialize Globals");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(1));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log1.txt");

				line = entries[2];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.RawContent.Should().Be("Started BTPVM3372 05:30:00 6060");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(1));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log1.txt");

				line = entries[3];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 178));
				line.RawContent.Should().Be("18/03/2019 14:09:54:178    1 Information   Loading preferences Started");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(2));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log2.txt");

				line = entries[4];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 178));
				line.RawContent.Should().Be("BTPVM3372 05:30:00 6060");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(2));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log2.txt");

				line = entries[5];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.RawContent.Should().Be("2019-03-18 14:09:54:313 1 00:00:00:0000000 Information   Loading");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(3));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log1.txt");

				line = entries[6];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.RawContent.Should().Be("preferences Started BTPVM3372 05:30:00 6060");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(3));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log1.txt");

				line = entries[7];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 550));
				line.RawContent.Should()
				    .Be("18/03/2019 14:09:54:550    1 Information    RMClientURL: BTPVM3372 05:30:00");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(4));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log2.txt");

				line = entries[8];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 550));
				line.RawContent.Should().Be("6060");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(4));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log2.txt");

				line = entries[9];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.RawContent.Should().Be("2019-03-18 14:09:54:551 1 00:00:00:0000000 Information    RMClientURL:");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(5));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log1.txt");

				line = entries[10];
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.RawContent.Should().Be("BTPVM3372 05:30:00 6060");
				line.LogEntryIndex.Should().Be(new LogEntryIndex(5));
				line.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be(@"TestData\Multiline\Log1.txt");
			}
		}
	}
}