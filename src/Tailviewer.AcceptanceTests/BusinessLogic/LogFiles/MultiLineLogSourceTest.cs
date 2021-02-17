using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class MultiLineLogSourceTest
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

		private TextLogSource Create(string fileName,
		                           ITimestampParser timestampParser = null)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance<ITaskScheduler>(_scheduler);
			if (timestampParser != null)
				serviceContainer.RegisterInstance<ITimestampParser>(timestampParser);
			serviceContainer.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			serviceContainer.RegisterInstance<ILogEntryParserPlugin>(new SimpleLogEntryParserPlugin());
			return new TextLogSource(serviceContainer, fileName);
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/183")]
		[Description("Verifies accessing entries by log file section")]
		public void TestMultilineNoLevel1()
		{
			using (var source = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var multi = new MultiLineLogSource(_scheduler, source, TimeSpan.Zero))
			{
				multi.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromMinutes(5)).Be(6);
				var entries = multi.GetEntries(new LogFileSection(0, 6),
				                               new IColumnDescriptor[]
				                               {
					                               GeneralColumns.Timestamp,
					                               GeneralColumns.LogEntryIndex,
					                               GeneralColumns.LineNumber,
					                               GeneralColumns.RawContent
				                               });

				var line = entries[0];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(1);
				line.GetValue(GeneralColumns.RawContent).Should().Be("2019-03-18 14:09:54:177 1 00:00:00:0000000 Information Initialize Globals");

				line = entries[1];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(2);
				line.GetValue(GeneralColumns.RawContent).Should().Be("Started BTPVM3372 05:30:00 6060");

				line = entries[2];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(3);
				line.GetValue(GeneralColumns.RawContent).Should().Be("2019-03-18 14:09:54:313 1 00:00:00:0000000 Information   Loading");

				line = entries[3];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(4);
				line.GetValue(GeneralColumns.RawContent).Should().Be("preferences Started BTPVM3372 05:30:00 6060");

				line = entries[4];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(5);
				line.GetValue(GeneralColumns.RawContent).Should().Be("2019-03-18 14:09:54:551 1 00:00:00:0000000 Information    RMClientURL:");

				line = entries[5];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(6);
				line.GetValue(GeneralColumns.RawContent).Should().Be("BTPVM3372 05:30:00 6060");
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/183")]
		[Description("Verifies accessing entries by log file index list")]
		public void TestMultilineNoLevel2()
		{
			using (var source = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var multi = new MultiLineLogSource(_scheduler, source, TimeSpan.Zero))
			{
				multi.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromMinutes(5)).Be(6);
				var entries = multi.GetEntries(new List<LogLineIndex>
				                               {
					                               new LogLineIndex(0),
					                               new LogLineIndex(1),
					                               new LogLineIndex(2),
					                               new LogLineIndex(3),
					                               new LogLineIndex(4),
					                               new LogLineIndex(5)
				                               },
				                               new IColumnDescriptor[]
				                               {
					                               GeneralColumns.Timestamp,
					                               GeneralColumns.LogEntryIndex,
					                               GeneralColumns.LineNumber,
					                               GeneralColumns.RawContent
				                               });

				var line = entries[0];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(1);
				line.GetValue(GeneralColumns.RawContent).Should().Be("2019-03-18 14:09:54:177 1 00:00:00:0000000 Information Initialize Globals");

				line = entries[1];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(2);
				line.GetValue(GeneralColumns.RawContent).Should().Be("Started BTPVM3372 05:30:00 6060");

				line = entries[2];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(3);
				line.GetValue(GeneralColumns.RawContent).Should().Be("2019-03-18 14:09:54:313 1 00:00:00:0000000 Information   Loading");

				line = entries[3];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(4);
				line.GetValue(GeneralColumns.RawContent).Should().Be("preferences Started BTPVM3372 05:30:00 6060");

				line = entries[4];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(5);
				line.GetValue(GeneralColumns.RawContent).Should().Be("2019-03-18 14:09:54:551 1 00:00:00:0000000 Information    RMClientURL:");

				line = entries[5];
				line.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(GeneralColumns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(GeneralColumns.LineNumber).Should().Be(6);
				line.GetValue(GeneralColumns.RawContent).Should().Be("BTPVM3372 05:30:00 6060");
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/183")]
		[Description("Verifies accessing entries by log file index list")]
		public void TestMultilineNoLevel3()
		{
			using (var source = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var multi = new MultiLineLogSource(_scheduler, source, TimeSpan.Zero))
			{
				multi.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromMinutes(5)).Be(6);

				var line = multi.GetEntry(0);
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.LogEntryIndex.Should().Be(0);
				line.Index.Should().Be(0);
				line.RawContent.Should().Be("2019-03-18 14:09:54:177 1 00:00:00:0000000 Information Initialize Globals");

				line = multi.GetEntry(1);
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.LogEntryIndex.Should().Be(0);
				line.Index.Should().Be(1);
				line.RawContent.Should().Be("Started BTPVM3372 05:30:00 6060");

				line = multi.GetEntry(2);
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.LogEntryIndex.Should().Be(1);
				line.Index.Should().Be(2);
				line.RawContent.Should().Be("2019-03-18 14:09:54:313 1 00:00:00:0000000 Information   Loading");

				line = multi.GetEntry(3);
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.LogEntryIndex.Should().Be(1);
				line.Index.Should().Be(3);
				line.RawContent.Should().Be("preferences Started BTPVM3372 05:30:00 6060");

				line = multi.GetEntry(4);
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.LogEntryIndex.Should().Be(2);
				line.Index.Should().Be(4);
				line.RawContent.Should().Be("2019-03-18 14:09:54:551 1 00:00:00:0000000 Information    RMClientURL:");

				line = multi.GetEntry(5);
				line.Timestamp.Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.LogEntryIndex.Should().Be(2);
				line.Index.Should().Be(5);
				line.RawContent.Should().Be("BTPVM3372 05:30:00 6060");
			}
		}
	}
}
