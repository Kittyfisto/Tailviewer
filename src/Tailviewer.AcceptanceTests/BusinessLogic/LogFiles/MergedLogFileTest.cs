using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;
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

		private TextLogFile Create(string fileName,
		                           ITimestampParser timestampParser = null)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance<ITaskScheduler>(_scheduler);
			if (timestampParser != null)
				serviceContainer.RegisterInstance<ITimestampParser>(timestampParser);
			serviceContainer.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			serviceContainer.RegisterInstance<ITextLogFileParserPlugin>(new SimpleTextLogFileParserPlugin());
			return new TextLogFile(serviceContainer, fileName);
		}

		[Test]
		[Description("Verifies that the MergedLogFile represents the very same content than its single source")]
		public void Test20Mb()
		{
			using (var source = Create(TextLogFileAcceptanceTest.File20Mb))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.FromMilliseconds(1), source))
			{
				source.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				merged.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(source.GetValue(LogFileProperties.LogEntryCount));
				merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(165342);
				merged.GetValue(LogFileProperties.Size).Should().Be(source.GetValue(LogFileProperties.Size));
				merged.GetValue(LogFileProperties.StartTimestamp).Should().Be(source.GetValue(LogFileProperties.StartTimestamp));

				var sourceEntries = source.GetEntries(new LogFileSection(0, source.GetValue(LogFileProperties.LogEntryCount)));
				var mergedEntries = merged.GetEntries(new LogFileSection(0, merged.GetValue(LogFileProperties.LogEntryCount)));
				for (int i = 0; i < source.GetValue(LogFileProperties.LogEntryCount); ++i)
				{
					var mergedEntry = mergedEntries[i];
					var sourceEntry = sourceEntries[i];

					mergedEntry.Index.Should().Be(sourceEntry.Index);
					mergedEntry.LogEntryIndex.Should().Be(sourceEntry.LogEntryIndex);
					mergedEntry.LineNumber.Should().Be(sourceEntry.LineNumber);
					mergedEntry.RawContent.Should().Be(sourceEntry.RawContent);
					mergedEntry.DeltaTime.Should().Be(sourceEntry.DeltaTime);
					if (i > 0)
						mergedEntry.DeltaTime.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
				}
			}
		}

		[Test]
		public void Test2SmallSources()
		{
			using (var source0 = Create(TextLogFileAcceptanceTest.File2Entries))
			using (var source1 = Create(TextLogFileAcceptanceTest.File2Lines))
			using (var multi0 = new MultiLineLogFile(_scheduler, source0, TimeSpan.Zero))
			using (var multi1 = new MultiLineLogFile(_scheduler, source1, TimeSpan.Zero))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, multi0, multi1))
			{
				source0.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				source1.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				multi0.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				multi1.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				merged.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(8, "Because the merged file should've been finished");
				merged.Property(x => x.GetValue(LogFileProperties.Size)).ShouldEventually().Be(source0.GetValue(LogFileProperties.Size) + source1.GetValue(LogFileProperties.Size));
				merged.Property(x => x.GetValue(LogFileProperties.StartTimestamp)).ShouldEventually().Be(source1.GetValue(LogFileProperties.StartTimestamp));

				var source0Lines = multi0.GetEntries(new LogFileSection(0, source0.GetValue(LogFileProperties.LogEntryCount)));
				var source1Lines = multi1.GetEntries(new LogFileSection(0, source1.GetValue(LogFileProperties.LogEntryCount)));
				var mergedLines = merged.GetEntries(new LogFileSection(0, merged.GetValue(LogFileProperties.LogEntryCount)));

				mergedLines[0].Index.Should().Be(0);
				mergedLines[0].LogEntryIndex.Should().Be(0);
				mergedLines[0].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1));
				mergedLines[0].RawContent.Should().Be(source1Lines[0].RawContent);
				
				mergedLines[1].Index.Should().Be(1);
				mergedLines[1].LogEntryIndex.Should().Be(1);
				mergedLines[1].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
				mergedLines[1].RawContent.Should().Be(source0Lines[0].RawContent);
				
				mergedLines[2].Index.Should().Be(2);
				mergedLines[2].LogEntryIndex.Should().Be(1);
				mergedLines[2].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
				mergedLines[2].RawContent.Should().Be(source0Lines[1].RawContent);
				
				mergedLines[3].Index.Should().Be(3);
				mergedLines[3].LogEntryIndex.Should().Be(1);
				mergedLines[3].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
				mergedLines[3].RawContent.Should().Be(source0Lines[2].RawContent);
				
				mergedLines[4].Index.Should().Be(4);
				mergedLines[4].LogEntryIndex.Should().Be(2);
				mergedLines[4].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1));
				mergedLines[4].RawContent.Should().Be(source1Lines[1].RawContent);
				
				mergedLines[5].Index.Should().Be(5);
				mergedLines[5].LogEntryIndex.Should().Be(3);
				mergedLines[5].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
				mergedLines[5].RawContent.Should().Be(source0Lines[3].RawContent);
				
				mergedLines[6].Index.Should().Be(6);
				mergedLines[6].LogEntryIndex.Should().Be(3);
				mergedLines[6].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
				mergedLines[6].RawContent.Should().Be(source0Lines[4].RawContent);
				
				mergedLines[7].Index.Should().Be(7);
				mergedLines[7].LogEntryIndex.Should().Be(3);
				mergedLines[7].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
				mergedLines[7].RawContent.Should().Be(source0Lines[5].RawContent);
			}
		}

		[Test]
		public void TestLive1And2()
		{
			using (var source0 = Create(TextLogFileAcceptanceTest.FileTestLive1))
			using (var source1 = Create(TextLogFileAcceptanceTest.FileTestLive2))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, source0, source1))
			{
				merged.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(19, "Because the merged file should've been finished");
				merged.Property(x => x.GetValue(LogFileProperties.Size)).ShouldEventually().Be(source0.GetValue(LogFileProperties.Size) + source1.GetValue(LogFileProperties.Size));
				merged.Property(x => x.GetValue(LogFileProperties.StartTimestamp)).ShouldEventually().Be(source0.GetValue(LogFileProperties.StartTimestamp));

				var entries = merged.GetEntries(new LogFileSection(0, merged.GetValue(LogFileProperties.LogEntryCount)));

				entries[0].Index.Should().Be(0);
				entries[0].LineNumber.Should().Be(1);
				entries[0].LogEntryIndex.Should().Be(0);
				entries[0].OriginalIndex.Should().Be(0);
				entries[0].OriginalLineNumber.Should().Be(1);
				entries[0].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[0].SourceId.Should().Be(new LogLineSourceId(0));
				entries[0].RawContent.Should().Be("2016-02-17 22:57:51,449 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - Test");
				entries[0].LogLevel.Should().Be(LevelFlags.Info);
				entries[0].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 51, 449));
				entries[0].ElapsedTime.Should().Be(TimeSpan.Zero);
				entries[0].DeltaTime.Should().Be(null);

				entries[1].Index.Should().Be(1);
				entries[1].LineNumber.Should().Be(2);
				entries[1].LogEntryIndex.Should().Be(1);
				entries[1].OriginalIndex.Should().Be(1);
				entries[1].OriginalLineNumber.Should().Be(2);
				entries[1].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[1].SourceId.Should().Be(new LogLineSourceId(0));
				entries[1].RawContent.Should().Be("2016-02-17 22:57:51,559 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - Hello");
				entries[1].LogLevel.Should().Be(LevelFlags.Info);
				entries[1].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 51, 559));
				entries[1].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(110));
				entries[1].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(110));

				entries[2].Index.Should().Be(2);
				entries[2].LineNumber.Should().Be(3);
				entries[2].LogEntryIndex.Should().Be(2);
				entries[2].OriginalIndex.Should().Be(2);
				entries[2].OriginalLineNumber.Should().Be(3);
				entries[2].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[2].SourceId.Should().Be(new LogLineSourceId(1));
				entries[2].RawContent.Should().Be("2016-02-17 22:57:51,560 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - Hello");
				entries[2].LogLevel.Should().Be(LevelFlags.Info);
				entries[2].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 51, 560));
				entries[2].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(111));
				entries[2].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[3].Index.Should().Be(3);
				entries[3].LineNumber.Should().Be(4);
				entries[3].LogEntryIndex.Should().Be(3);
				entries[3].OriginalIndex.Should().Be(3);
				entries[3].OriginalLineNumber.Should().Be(4);
				entries[3].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[3].SourceId.Should().Be(new LogLineSourceId(0));
				entries[3].RawContent.Should().Be("2016-02-17 22:57:51,664 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - world!");
				entries[3].LogLevel.Should().Be(LevelFlags.Info);
				entries[3].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 51, 664));
				entries[3].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(215));
				entries[3].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(104));

				entries[4].Index.Should().Be(4);
				entries[4].LineNumber.Should().Be(5);
				entries[4].LogEntryIndex.Should().Be(4);
				entries[4].OriginalIndex.Should().Be(4);
				entries[4].OriginalLineNumber.Should().Be(5);
				entries[4].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[4].SourceId.Should().Be(new LogLineSourceId(1));
				entries[4].RawContent.Should().Be("2016-02-17 22:57:51,665 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Test.BusinessLogic.LogFileTest - world!");
				entries[4].LogLevel.Should().Be(LevelFlags.Info);
				entries[4].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 51, 665));
				entries[4].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(216));
				entries[4].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[5].Index.Should().Be(5);
				entries[5].LineNumber.Should().Be(6);
				entries[5].LogEntryIndex.Should().Be(5);
				entries[5].OriginalIndex.Should().Be(5);
				entries[5].OriginalLineNumber.Should().Be(6);
				entries[5].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[5].SourceId.Should().Be(new LogLineSourceId(0));
				entries[5].RawContent.Should().Be("2016-02-17 22:57:59,284 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[5].LogLevel.Should().Be(LevelFlags.Warning);
				entries[5].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 284));
				entries[5].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7835));
				entries[5].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(7619));

				entries[6].Index.Should().Be(6);
				entries[6].LineNumber.Should().Be(7);
				entries[6].LogEntryIndex.Should().Be(6);
				entries[6].OriginalIndex.Should().Be(6);
				entries[6].OriginalLineNumber.Should().Be(7);
				entries[6].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[6].SourceId.Should().Be(new LogLineSourceId(1));
				entries[6].RawContent.Should().Be("2016-02-17 22:57:59,285 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[6].LogLevel.Should().Be(LevelFlags.Warning);
				entries[6].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 285));
				entries[6].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7836));
				entries[6].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[7].Index.Should().Be(7);
				entries[7].LineNumber.Should().Be(8);
				entries[7].LogEntryIndex.Should().Be(7);
				entries[7].OriginalIndex.Should().Be(7);
				entries[7].OriginalLineNumber.Should().Be(8);
				entries[7].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[7].SourceId.Should().Be(new LogLineSourceId(0));
				entries[7].RawContent.Should().Be("2016-02-17 22:57:59,298 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[7].LogLevel.Should().Be(LevelFlags.Warning);
				entries[7].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 298));
				entries[7].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7849));
				entries[7].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(13));

				entries[8].Index.Should().Be(8);
				entries[8].LineNumber.Should().Be(9);
				entries[8].LogEntryIndex.Should().Be(8);
				entries[8].OriginalIndex.Should().Be(8);
				entries[8].OriginalLineNumber.Should().Be(9);
				entries[8].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[8].SourceId.Should().Be(new LogLineSourceId(1));
				entries[8].RawContent.Should().Be("2016-02-17 22:57:59,299 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[8].LogLevel.Should().Be(LevelFlags.Warning);
				entries[8].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 299));
				entries[8].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7850));
				entries[8].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[9].Index.Should().Be(9);
				entries[9].LineNumber.Should().Be(10);
				entries[9].LogEntryIndex.Should().Be(9);
				entries[9].OriginalIndex.Should().Be(9);
				entries[9].OriginalLineNumber.Should().Be(10);
				entries[9].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[9].SourceId.Should().Be(new LogLineSourceId(0));
				entries[9].RawContent.Should().Be(@"2016-02-17 22:57:59,302 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear1.log' doesn't have an ID yet, setting it to: b62ea0a3-c495-4f3f-b7c7-d1a0a66e361e");
				entries[9].LogLevel.Should().Be(LevelFlags.Info);
				entries[9].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 302));
				entries[9].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7853));
				entries[9].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(3));

				entries[10].Index.Should().Be(10);
				entries[10].LineNumber.Should().Be(11);
				entries[10].LogEntryIndex.Should().Be(10);
				entries[10].OriginalIndex.Should().Be(10);
				entries[10].OriginalLineNumber.Should().Be(11);
				entries[10].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[10].SourceId.Should().Be(new LogLineSourceId(1));
				entries[10].RawContent.Should().Be(@"2016-02-17 22:57:59,303 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear1.log' doesn't have an ID yet, setting it to: b62ea0a3-c495-4f3f-b7c7-d1a0a66e361e");
				entries[10].LogLevel.Should().Be(LevelFlags.Info);
				entries[10].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 303));
				entries[10].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7854));
				entries[10].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[11].Index.Should().Be(11);
				entries[11].LineNumber.Should().Be(12);
				entries[11].LogEntryIndex.Should().Be(11);
				entries[11].OriginalIndex.Should().Be(11);
				entries[11].OriginalLineNumber.Should().Be(12);
				entries[11].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[11].SourceId.Should().Be(new LogLineSourceId(0));
				entries[11].RawContent.Should().Be(@"2016-02-17 22:57:59,304 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear2.log' doesn't have an ID yet, setting it to: 0ff1c032-0754-405f-8193-2fa4dbfb7d07");
				entries[11].LogLevel.Should().Be(LevelFlags.Info);
				entries[11].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 304));
				entries[11].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7855));
				entries[11].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[12].Index.Should().Be(12);
				entries[12].LineNumber.Should().Be(13);
				entries[12].LogEntryIndex.Should().Be(12);
				entries[12].OriginalIndex.Should().Be(12);
				entries[12].OriginalLineNumber.Should().Be(13);
				entries[12].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[12].SourceId.Should().Be(new LogLineSourceId(1));
				entries[12].RawContent.Should().Be(@"2016-02-17 22:57:59,305 [CurrentAppDomainHost.ExecuteNodes] INFO  Tailviewer.Settings.DataSource - Data Source 'E:\Code\Tailviewer\bin\Debug\TestClear2.log' doesn't have an ID yet, setting it to: 0ff1c032-0754-405f-8193-2fa4dbfb7d07");
				entries[12].LogLevel.Should().Be(LevelFlags.Info);
				entries[12].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 305));
				entries[12].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7856));
				entries[12].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[13].Index.Should().Be(13);
				entries[13].LineNumber.Should().Be(14);
				entries[13].LogEntryIndex.Should().Be(13);
				entries[13].OriginalIndex.Should().Be(13);
				entries[13].OriginalLineNumber.Should().Be(14);
				entries[13].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[13].SourceId.Should().Be(new LogLineSourceId(0));
				entries[13].RawContent.Should().Be("2016-02-17 22:57:59,306 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[13].LogLevel.Should().Be(LevelFlags.Warning);
				entries[13].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 306));
				entries[13].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7857));
				entries[13].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[14].Index.Should().Be(14);
				entries[14].LineNumber.Should().Be(15);
				entries[14].LogEntryIndex.Should().Be(14);
				entries[14].OriginalIndex.Should().Be(14);
				entries[14].OriginalLineNumber.Should().Be(15);
				entries[14].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[14].SourceId.Should().Be(new LogLineSourceId(1));
				entries[14].RawContent.Should().Be("2016-02-17 22:57:59,307 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[14].LogLevel.Should().Be(LevelFlags.Warning);
				entries[14].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 307));
				entries[14].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7858));
				entries[14].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[15].Index.Should().Be(15);
				entries[15].LineNumber.Should().Be(16);
				entries[15].LogEntryIndex.Should().Be(15);
				entries[15].OriginalIndex.Should().Be(15);
				entries[15].OriginalLineNumber.Should().Be(16);
				entries[15].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[15].SourceId.Should().Be(new LogLineSourceId(1));
				entries[15].RawContent.Should().Be("2016-02-17 22:57:59,310 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[15].LogLevel.Should().Be(LevelFlags.Warning);
				entries[15].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 310));
				entries[15].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7861));
				entries[15].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(3));

				entries[16].Index.Should().Be(16);
				entries[16].LineNumber.Should().Be(17);
				entries[16].LogEntryIndex.Should().Be(16);
				entries[16].OriginalIndex.Should().Be(16);
				entries[16].OriginalLineNumber.Should().Be(17);
				entries[16].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[16].SourceId.Should().Be(new LogLineSourceId(0));
				entries[16].RawContent.Should().Be("2016-02-17 22:57:59,311 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.Settings.DataSources - Selected item '00000000-0000-0000-0000-000000000000' not found in data-sources, ignoring it...");
				entries[16].LogLevel.Should().Be(LevelFlags.Warning);
				entries[16].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 311));
				entries[16].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(7862));
				entries[16].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));

				entries[17].Index.Should().Be(17);
				entries[17].LineNumber.Should().Be(18);
				entries[17].LogEntryIndex.Should().Be(17);
				entries[17].OriginalIndex.Should().Be(17);
				entries[17].OriginalLineNumber.Should().Be(18);
				entries[17].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive1);
				entries[17].SourceId.Should().Be(new LogLineSourceId(0));
				entries[17].RawContent.Should().Be(@"2016-02-17 22:57:59,863 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.BusinessLogic.DataSources - DataSource 'foo (ec976867-195b-4adf-a819-a1427f0d9aac)' is assigned a parent 'f671f235-7084-4e57-b06a-d253f750fae6' but we don't know that one");
				entries[17].LogLevel.Should().Be(LevelFlags.Warning);
				entries[17].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 863));
				entries[17].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(8414));
				entries[17].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(552));

				entries[18].Index.Should().Be(18);
				entries[18].LineNumber.Should().Be(19);
				entries[18].LogEntryIndex.Should().Be(18);
				entries[18].OriginalIndex.Should().Be(18);
				entries[18].OriginalLineNumber.Should().Be(19);
				entries[18].OriginalDataSourceName.Should().Be(TextLogFileAcceptanceTest.FileTestLive2);
				entries[18].SourceId.Should().Be(new LogLineSourceId(1));
				entries[18].RawContent.Should().Be(@"2016-02-17 22:57:59,864 [CurrentAppDomainHost.ExecuteNodes] WARN  Tailviewer.BusinessLogic.DataSources - DataSource 'foo (ec976867-195b-4adf-a819-a1427f0d9aac)' is assigned a parent 'f671f235-7084-4e57-b06a-d253f750fae6' but we don't know that one");
				entries[18].LogLevel.Should().Be(LevelFlags.Warning);
				entries[18].Timestamp.Should().Be(new DateTime(2016, 2, 17, 22, 57, 59, 864));
				entries[18].ElapsedTime.Should().Be(TimeSpan.FromMilliseconds(8415));
				entries[18].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
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
			using (var source0 = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var source1 = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel2, new CustomTimestampParser()))
			using (var multi0 = new MultiLineLogFile(_scheduler, source0, TimeSpan.Zero))
			using (var multi1 = new MultiLineLogFile(_scheduler, source1, TimeSpan.Zero))
			using (var merged = new MergedLogFile(_scheduler, TimeSpan.Zero, multi0, multi1))
			{
				// TODO: Fix - the percentage gets reset because a log file implementation has a slightly botched Percetage calculation
				//merged.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(1000)).Be(Percentage.HundredPercent);
				merged.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(10)).BeGreaterOrEqualTo(10);

				var entries = merged.GetEntries(new LogFileSection(0, 11),
				                                new ILogFileColumnDescriptor[]
				                                {
					                                LogFileColumns.Timestamp,
					                                LogFileColumns.LogEntryIndex,
					                                LogFileColumns.LineNumber,
					                                LogFileColumns.RawContent,
					                                LogFileColumns.OriginalDataSourceName
				                                });

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