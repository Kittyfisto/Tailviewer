using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Entries
{
	[TestFixture]
	public sealed class LogEntryTest
		: AbstractReadOnlyLogEntryTest
	{
		[Test]
		public void TestConstruction1()
		{
			var entry = new LogEntry(Core.Columns.RawContent, Core.Columns.DeltaTime);
			entry.RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
			entry.DeltaTime.Should().Be(Core.Columns.DeltaTime.DefaultValue);
		}

		[Test]
		public void TestConstruction2()
		{
			var entry = new LogEntry(new List<IColumnDescriptor> { Core.Columns.Timestamp, Core.Columns.LineNumber});
			entry.Timestamp.Should().Be(Core.Columns.Timestamp.DefaultValue);
			entry.LineNumber.Should().Be(Core.Columns.LineNumber.DefaultValue);
		}

		[Test]
		public void TestSetLogLevel()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.Columns.Should().Equal(Core.Columns.LogLevel);
			entry.GetValue(Core.Columns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.Columns.Should().Equal(Core.Columns.LogEntryIndex);
			entry.GetValue(Core.Columns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.Columns.Should().Equal(Core.Columns.DeltaTime);
			entry.GetValue(Core.Columns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.Columns.Should().Equal(Core.Columns.ElapsedTime);
			entry.GetValue(Core.Columns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.Columns.Should().Equal(Core.Columns.Timestamp);
			entry.GetValue(Core.Columns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.Columns.Should().Equal(Core.Columns.RawContent);
			entry.GetValue(Core.Columns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.Columns.Should().Equal(Core.Columns.Index);
			entry.GetValue(Core.Columns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.Columns.Should().Equal(Core.Columns.OriginalIndex);
			entry.GetValue(Core.Columns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.Columns.Should().Equal(Core.Columns.LineNumber);
			entry.GetValue(Core.Columns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			entry.Columns.Should().BeEmpty();

			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.Columns.Should().Equal(Core.Columns.OriginalLineNumber);
			entry.GetValue(Core.Columns.OriginalLineNumber).Should().Be(1337);
		}

		[Test]
		public void TestSetValueWrongType()
		{
			var entry = new LogEntry(new IColumnDescriptor[0]);
			new Action(() => entry.SetValue(Core.Columns.RawContent, 42)).Should().Throw<ArgumentException>();
			entry.Columns.Should().BeEmpty();
			new Action(() => entry.GetValue(Core.Columns.RawContent)).Should().Throw<ArgumentException>();
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
				{Core.Columns.RawContent, "Starbuck"}
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
				{Core.Columns.RawContent, "Starbuck"}
			};
			var otherValues = new Dictionary<IColumnDescriptor, object>
			{
				{Core.Columns.RawContent, "Apollo"}
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
			var entry = new LogEntry(Core.Columns.RawContent);
			var otherEntry = new LogEntry(Core.Columns.RawContent, Core.Columns.Timestamp);
			Equals(entry, otherEntry).Should().BeFalse();

			var equalReadOnlyEntry = new ReadOnlyLogEntry(Core.Columns.RawContent, Core.Columns.Timestamp);
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