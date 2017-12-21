using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntry2Test
		: AbstractReadOnlyLogEntryTest
	{
		[Test]
		public void TestConstruction1()
		{
			var entry = new LogEntry2();
			entry.Columns.Should().BeEmpty();
			new Action(() => { var unused = entry.DeltaTime; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.ElapsedTime; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.Index; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.LineNumber; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.LogEntryIndex; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.LogLevel; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.OriginalIndex; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.OriginalLineNumber; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.RawContent; }).ShouldThrow<NoSuchColumnException>();
			new Action(() => { var unused = entry.Timestamp; }).ShouldThrow<NoSuchColumnException>();
		}

		[Test]
		public void TestConstruction2()
		{
			var entry = new LogEntry2(LogFileColumns.RawContent, LogFileColumns.DeltaTime);
			entry.RawContent.Should().Be(LogFileColumns.RawContent.DefaultValue);
			entry.DeltaTime.Should().Be(LogFileColumns.DeltaTime.DefaultValue);
		}

		[Test]
		public void TestConstruction3()
		{
			var entry = new LogEntry2(new List<ILogFileColumn> { LogFileColumns.Timestamp, LogFileColumns.LineNumber});
			entry.Timestamp.Should().Be(LogFileColumns.Timestamp.DefaultValue);
			entry.LineNumber.Should().Be(LogFileColumns.LineNumber.DefaultValue);
		}

		[Test]
		public void TestSetLogLevel()
		{
			var entry = new LogEntry2();
			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.GetValue(LogFileColumns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = new LogEntry2();
			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.GetValue(LogFileColumns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = new LogEntry2();
			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(LogFileColumns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = new LogEntry2();
			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(LogFileColumns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = new LogEntry2();
			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.GetValue(LogFileColumns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = new LogEntry2();
			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.GetValue(LogFileColumns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = new LogEntry2();
			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.GetValue(LogFileColumns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = new LogEntry2();
			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.GetValue(LogFileColumns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = new LogEntry2();
			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.GetValue(LogFileColumns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = new LogEntry2();
			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.GetValue(LogFileColumns.OriginalLineNumber).Should().Be(1337);
		}

		protected override IReadOnlyLogEntry CreateEmpty()
		{
			return new LogEntry2();
		}
	}
}