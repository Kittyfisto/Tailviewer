using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Columns
{
	[TestFixture]
	public sealed class GeneralColumnsTest
	{
		[Test]
		[Description("Verifies that the Minimum property doesn't regress")]
		public void TestWellKnownColumns()
		{
			Core.Columns.Minimum.Should().Equal(new object[]
			{
				Core.Columns.RawContent,
				Core.Columns.Index,
				Core.Columns.OriginalIndex,
				Core.Columns.LogEntryIndex,
				Core.Columns.LineNumber,
				Core.Columns.OriginalLineNumber,
				Core.Columns.OriginalDataSourceName,
				Core.Columns.LogLevel,
				Core.Columns.Timestamp,
				Core.Columns.ElapsedTime,
				Core.Columns.DeltaTime
			});

			Core.Columns.Minimum.Should().NotContain(Core.Columns.SourceId,
			                                         "because the source id column is specialized and only available in some cases");
		}

		[Test]
		[Description("Verifies that the Level property doesn't regress")]
		public void TestLevel()
		{
			Core.Columns.LogLevel.Id.Should().Be("log_level");
			Core.Columns.LogLevel.DataType.Should().Be<LevelFlags>();
		}

		[Test]
		[Description("Verifies that the index property doesn't regress")]
		public void TestIndex()
		{
			Core.Columns.Index.Id.Should().Be("index");
			Core.Columns.Index.DataType.Should().Be<LogLineIndex>();
			Core.Columns.Index.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the original index property doesn't regress")]
		public void TestOriginalIndex()
		{
			Core.Columns.OriginalIndex.Id.Should().Be("original_index");
			Core.Columns.OriginalIndex.DataType.Should().Be<LogLineIndex>();
			Core.Columns.OriginalIndex.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the log entry index property doesn't regress")]
		public void TestLogEntryIndex()
		{
			Core.Columns.LogEntryIndex.Id.Should().Be("log_entry_index");
			Core.Columns.LogEntryIndex.DataType.Should().Be<LogEntryIndex>();
			Core.Columns.LogEntryIndex.DefaultValue.Should().Be(LogEntryIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the LineNumber property doesn't regress")]
		public void TestLineNumber()
		{
			Core.Columns.LineNumber.Id.Should().Be("line_number");
			Core.Columns.LineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the OriginalLineNumber property doesn't regress")]
		public void TestOriginalLineNumber()
		{
			Core.Columns.OriginalLineNumber.Id.Should().Be("original_line_number");
			Core.Columns.OriginalLineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the Timestamp property doesn't regress")]
		public void TestTimestamp()
		{
			Core.Columns.Timestamp.Id.Should().Be("timestamp");
			Core.Columns.Timestamp.DataType.Should().Be<DateTime?>();
		}

		[Test]
		[Description("Verifies that the DeltaTime property doesn't regress")]
		public void TestDeltaTime()
		{
			Core.Columns.DeltaTime.Id.Should().Be("delta_time");
			Core.Columns.DeltaTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the Message property doesn't regress")]
		public void TestMessage()
		{
			Core.Columns.Message.Id.Should().Be("message");
			Core.Columns.Message.DataType.Should().Be<string>();
		}

		[Test]
		[Description("Verifies that the ElapsedTime property doesn't regress")]
		public void TestElapsedTime()
		{
			Core.Columns.ElapsedTime.Id.Should().Be("elapsed_time");
			Core.Columns.ElapsedTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the RawContent property doesn't regress")]
		public void TestRawContent()
		{
			Core.Columns.RawContent.Id.Should().Be("raw_content");
			Core.Columns.RawContent.DataType.Should().Be<string>();
		}

		[Test]
		public void TestCombine1()
		{
			Core.Columns.Combine(new IColumnDescriptor[0], Core.Columns.OriginalLineNumber)
			    .Should().Equal(Core.Columns.OriginalLineNumber);
		}

		[Test]
		public void TestCombine2()
		{
			Core.Columns.Combine(new IColumnDescriptor[]
			    {
				    Core.Columns.LineNumber
			    }, Core.Columns.OriginalLineNumber)
			    .Should().Equal(Core.Columns.LineNumber, Core.Columns.OriginalLineNumber);
		}

		[Test]
		[Description("Verifies that Combine doesn't introduce the same column twice")]
		public void TestCombine3()
		{
			Core.Columns.Combine(new IColumnDescriptor[]
			    {
				    Core.Columns.RawContent,
				    Core.Columns.LineNumber
			    }, Core.Columns.LineNumber)
			    .Should().Equal(Core.Columns.RawContent, Core.Columns.LineNumber);
		}
	}
}