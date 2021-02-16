using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntryTest
		: AbstractReadOnlyLogEntryTest
	{
		[Test]
		public void TestConstruction1()
		{
			var entry = new LogEntry(LogColumns.RawContent, LogColumns.DeltaTime);
			entry.RawContent.Should().Be(LogColumns.RawContent.DefaultValue);
			entry.DeltaTime.Should().Be(LogColumns.DeltaTime.DefaultValue);
		}

		[Test]
		public void TestConstruction2()
		{
			var entry = new LogEntry(new List<IColumnDescriptor> { LogColumns.Timestamp, LogColumns.LineNumber});
			entry.Timestamp.Should().Be(LogColumns.Timestamp.DefaultValue);
			entry.LineNumber.Should().Be(LogColumns.LineNumber.DefaultValue);
		}

		[Test]
		public void TestSetLogLevel()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.Columns.Should().Equal(LogColumns.LogLevel);
			entry.GetValue(LogColumns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.Columns.Should().Equal(LogColumns.LogEntryIndex);
			entry.GetValue(LogColumns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.Columns.Should().Equal(LogColumns.DeltaTime);
			entry.GetValue(LogColumns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.Columns.Should().Equal(LogColumns.ElapsedTime);
			entry.GetValue(LogColumns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.Columns.Should().Equal(LogColumns.Timestamp);
			entry.GetValue(LogColumns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.Columns.Should().Equal(LogColumns.RawContent);
			entry.GetValue(LogColumns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.Columns.Should().Equal(LogColumns.Index);
			entry.GetValue(LogColumns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.Columns.Should().Equal(LogColumns.OriginalIndex);
			entry.GetValue(LogColumns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.Columns.Should().Equal(LogColumns.LineNumber);
			entry.GetValue(LogColumns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.Columns.Should().Equal(LogColumns.OriginalLineNumber);
			entry.GetValue(LogColumns.OriginalLineNumber).Should().Be(1337);
		}

		[Test]
		public void TestSetValueWrongType()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			new Action(() => entry.SetValue(LogColumns.RawContent, 42)).Should().Throw<ArgumentException>();
			entry.Columns.Should().BeEmpty();
			new Action(() => entry.GetValue(LogColumns.RawContent)).Should().Throw<ArgumentException>();
		}

		[Test]
		public void TestEqualBothEmpty()
		{
			var entry = new LogEntry();
			var equalEntry = new LogEntry();
			Equals(entry, equalEntry).Should().BeTrue();

			var equalReadOnlyEntry = new ReadOnlyLogEntry();
			Equals(entry, equalReadOnlyEntry).Should().BeTrue();
		}

		[Test]
		public void TestEqualSameValue()
		{
			var values = new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.RawContent, "Starbuck"}
			};
			var entry = new LogEntry(values);
			var equalEntry = new LogEntry(values);
			Equals(entry, equalEntry).Should().BeTrue();

			var equalReadOnlyEntry = new ReadOnlyLogEntry(values);
			Equals(entry, equalReadOnlyEntry).Should().BeTrue();
		}

		[Test]
		public void TestEqualDifferentValue()
		{
			var values = new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.RawContent, "Starbuck"}
			};
			var otherValues = new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.RawContent, "Apollo"}
			};
			var entry = new LogEntry(values);
			var otherEntry = new LogEntry(otherValues);
			Equals(entry, otherEntry).Should().BeFalse();

			var otherReadOnlyEntry = new ReadOnlyLogEntry(otherValues);
			Equals(entry, otherReadOnlyEntry).Should().BeFalse();
		}

		[Test]
		public void TestEqualBothEmpty_DifferentColumns()
		{
			var entry = new LogEntry(LogColumns.RawContent);
			var otherEntry = new LogEntry(LogColumns.RawContent, LogColumns.Timestamp);
			Equals(entry, otherEntry).Should().BeFalse();

			var equalReadOnlyEntry = new ReadOnlyLogEntry(LogColumns.RawContent, LogColumns.Timestamp);
			Equals(entry, equalReadOnlyEntry).Should().BeFalse();
		}

		protected override IReadOnlyLogEntry CreateDefault()
		{
			return new LogEntry();
		}

		protected override IReadOnlyLogEntry CreateEmpty()
		{
			return new LogEntry(new IColumnDescriptor[0]);
		}
	}
}