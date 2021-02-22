using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Entries;

namespace Tailviewer.Test.BusinessLogic.Entries
{
	[TestFixture]
	public sealed class LogEntryTest
		: AbstractReadOnlyLogEntryTest
	{
		[Test]
		public void TestConstruction1()
		{
			var entry = new LogEntry(GeneralColumns.RawContent, GeneralColumns.DeltaTime);
			entry.RawContent.Should().Be(GeneralColumns.RawContent.DefaultValue);
			entry.DeltaTime.Should().Be(GeneralColumns.DeltaTime.DefaultValue);
		}

		[Test]
		public void TestConstruction2()
		{
			var entry = new LogEntry(new List<IColumnDescriptor> { GeneralColumns.Timestamp, GeneralColumns.LineNumber});
			entry.Timestamp.Should().Be(GeneralColumns.Timestamp.DefaultValue);
			entry.LineNumber.Should().Be(GeneralColumns.LineNumber.DefaultValue);
		}

		[Test]
		public void TestSetLogLevel()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.Columns.Should().Equal(GeneralColumns.LogLevel);
			entry.GetValue(GeneralColumns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.Columns.Should().Equal(GeneralColumns.LogEntryIndex);
			entry.GetValue(GeneralColumns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.Columns.Should().Equal(GeneralColumns.DeltaTime);
			entry.GetValue(GeneralColumns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.Columns.Should().Equal(GeneralColumns.ElapsedTime);
			entry.GetValue(GeneralColumns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.Columns.Should().Equal(GeneralColumns.Timestamp);
			entry.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.Columns.Should().Equal(GeneralColumns.RawContent);
			entry.GetValue(GeneralColumns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.Columns.Should().Equal(GeneralColumns.Index);
			entry.GetValue(GeneralColumns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.Columns.Should().Equal(GeneralColumns.OriginalIndex);
			entry.GetValue(GeneralColumns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.Columns.Should().Equal(GeneralColumns.LineNumber);
			entry.GetValue(GeneralColumns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.Columns.Should().Equal(GeneralColumns.OriginalLineNumber);
			entry.GetValue(GeneralColumns.OriginalLineNumber).Should().Be(1337);
		}

		[Test]
		public void TestSetValueWrongType()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			new Action(() => entry.SetValue(GeneralColumns.RawContent, 42)).Should().Throw<ArgumentException>();
			entry.Columns.Should().BeEmpty();
			new Action(() => entry.GetValue(GeneralColumns.RawContent)).Should().Throw<ArgumentException>();
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
				{GeneralColumns.RawContent, "Starbuck"}
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
				{GeneralColumns.RawContent, "Starbuck"}
			};
			var otherValues = new Dictionary<IColumnDescriptor, object>
			{
				{GeneralColumns.RawContent, "Apollo"}
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
			var entry = new LogEntry(GeneralColumns.RawContent);
			var otherEntry = new LogEntry(GeneralColumns.RawContent, GeneralColumns.Timestamp);
			Equals(entry, otherEntry).Should().BeFalse();

			var equalReadOnlyEntry = new ReadOnlyLogEntry(GeneralColumns.RawContent, GeneralColumns.Timestamp);
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