using System;
using System.Collections.Generic;
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
	public sealed class MultiLineLogFileTest
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
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/183")]
		[Description("Verifies accessing entries by log file section")]
		public void TestMultilineNoLevel1()
		{
			using (var source = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var multi = new MultiLineLogFile(_scheduler, source, TimeSpan.Zero))
			{
				multi.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromMinutes(5)).Be(6);
				var entries = multi.GetEntries(new LogFileSection(0, 6),
				                               new IColumnDescriptor[]
				                               {
					                               Columns.Timestamp,
					                               Columns.LogEntryIndex,
					                               Columns.LineNumber,
					                               Columns.RawContent
				                               });

				var line = entries[0];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(Columns.LineNumber).Should().Be(1);
				line.GetValue(Columns.RawContent).Should().Be("2019-03-18 14:09:54:177 1 00:00:00:0000000 Information Initialize Globals");

				line = entries[1];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(Columns.LineNumber).Should().Be(2);
				line.GetValue(Columns.RawContent).Should().Be("Started BTPVM3372 05:30:00 6060");

				line = entries[2];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(Columns.LineNumber).Should().Be(3);
				line.GetValue(Columns.RawContent).Should().Be("2019-03-18 14:09:54:313 1 00:00:00:0000000 Information   Loading");

				line = entries[3];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(Columns.LineNumber).Should().Be(4);
				line.GetValue(Columns.RawContent).Should().Be("preferences Started BTPVM3372 05:30:00 6060");

				line = entries[4];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(Columns.LineNumber).Should().Be(5);
				line.GetValue(Columns.RawContent).Should().Be("2019-03-18 14:09:54:551 1 00:00:00:0000000 Information    RMClientURL:");

				line = entries[5];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(Columns.LineNumber).Should().Be(6);
				line.GetValue(Columns.RawContent).Should().Be("BTPVM3372 05:30:00 6060");
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/183")]
		[Description("Verifies accessing entries by log file index list")]
		public void TestMultilineNoLevel2()
		{
			using (var source = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var multi = new MultiLineLogFile(_scheduler, source, TimeSpan.Zero))
			{
				multi.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromMinutes(5)).Be(6);
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
					                               Columns.Timestamp,
					                               Columns.LogEntryIndex,
					                               Columns.LineNumber,
					                               Columns.RawContent
				                               });

				var line = entries[0];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(Columns.LineNumber).Should().Be(1);
				line.GetValue(Columns.RawContent).Should().Be("2019-03-18 14:09:54:177 1 00:00:00:0000000 Information Initialize Globals");

				line = entries[1];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 177));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(0));
				line.GetValue(Columns.LineNumber).Should().Be(2);
				line.GetValue(Columns.RawContent).Should().Be("Started BTPVM3372 05:30:00 6060");

				line = entries[2];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(Columns.LineNumber).Should().Be(3);
				line.GetValue(Columns.RawContent).Should().Be("2019-03-18 14:09:54:313 1 00:00:00:0000000 Information   Loading");

				line = entries[3];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 313));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(1));
				line.GetValue(Columns.LineNumber).Should().Be(4);
				line.GetValue(Columns.RawContent).Should().Be("preferences Started BTPVM3372 05:30:00 6060");

				line = entries[4];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(Columns.LineNumber).Should().Be(5);
				line.GetValue(Columns.RawContent).Should().Be("2019-03-18 14:09:54:551 1 00:00:00:0000000 Information    RMClientURL:");

				line = entries[5];
				line.GetValue(Columns.Timestamp).Should().Be(new DateTime(2019, 3, 18, 14, 9, 54, 551));
				line.GetValue(Columns.LogEntryIndex).Should().Be(new LogEntryIndex(2));
				line.GetValue(Columns.LineNumber).Should().Be(6);
				line.GetValue(Columns.RawContent).Should().Be("BTPVM3372 05:30:00 6060");
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/183")]
		[Description("Verifies accessing entries by log file index list")]
		public void TestMultilineNoLevel3()
		{
			using (var source = Create(TextLogFileAcceptanceTest.MultilineNoLogLevel1, new CustomTimestampParser()))
			using (var multi = new MultiLineLogFile(_scheduler, source, TimeSpan.Zero))
			{
				multi.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromMinutes(5)).Be(6);

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
