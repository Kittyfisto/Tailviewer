using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Columns;

namespace Tailviewer.Test.BusinessLogic.Entries
{
	[TestFixture]
	public abstract class AbstractLogEntryTest
	{
		public abstract ILogEntry CreateDefault();

		public abstract ILogEntry CreateEmpty();

		public abstract ILogEntry Create(params IColumnDescriptor[] columns);

		[Test]
		public void TestSetLogLevel()
		{
			var entry = CreateDefault();
			entry.LogLevel.Should().Be(GeneralColumns.LogLevel.DefaultValue);

			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.GetValue(GeneralColumns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = CreateDefault();
			entry.LogEntryIndex.Should().Be(GeneralColumns.LogEntryIndex.DefaultValue);

			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.GetValue(GeneralColumns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = CreateDefault();
			entry.DeltaTime.Should().Be(GeneralColumns.DeltaTime.DefaultValue);

			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(GeneralColumns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = CreateDefault();
			entry.ElapsedTime.Should().Be(GeneralColumns.ElapsedTime.DefaultValue);

			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(GeneralColumns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = CreateDefault();
			entry.Timestamp.Should().Be(GeneralColumns.Timestamp.DefaultValue);

			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.GetValue(GeneralColumns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = CreateDefault();
			entry.RawContent.Should().Be(GeneralColumns.RawContent.DefaultValue);

			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.GetValue(GeneralColumns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = CreateDefault();
			entry.Index.Should().Be(GeneralColumns.Index.DefaultValue);

			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.GetValue(GeneralColumns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = CreateDefault();
			entry.OriginalIndex.Should().Be(GeneralColumns.OriginalIndex.DefaultValue);

			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.GetValue(GeneralColumns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = CreateDefault();
			entry.LineNumber.Should().Be(GeneralColumns.LineNumber.DefaultValue);

			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.GetValue(GeneralColumns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = CreateDefault();
			entry.OriginalLineNumber.Should().Be(GeneralColumns.OriginalLineNumber.DefaultValue);

			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.GetValue(GeneralColumns.OriginalLineNumber).Should().Be(1337);
		}

		[Test]
		public void TestSetOriginalDataSourceName()
		{
			var entry = CreateDefault();
			entry.OriginalDataSourceName.Should().Be(GeneralColumns.OriginalDataSourceName.DefaultValue);

			entry.OriginalDataSourceName = "Foobar";
			entry.OriginalDataSourceName.Should().Be("Foobar");
			entry.GetValue(GeneralColumns.OriginalDataSourceName).Should().Be("Foobar");
		}

		[Test]
		public void TestSetSourceId()
		{
			var entry = Create(GeneralColumns.SourceId);
			entry.SourceId.Should().Be(GeneralColumns.SourceId.DefaultValue);

			entry.SourceId = new LogLineSourceId(42);
			entry.SourceId.Should().Be(new LogLineSourceId(42));
			entry.GetValue(GeneralColumns.SourceId).Should().Be(new LogLineSourceId(42));
		}

		[Test]
		public void TestSetValueWrongType()
		{
			var entry = CreateEmpty();
			new Action(() => entry.SetValue(GeneralColumns.RawContent, 42)).Should().Throw<ArgumentException>();
			entry.Columns.Should().BeEmpty();
			new Action(() => entry.GetValue(GeneralColumns.RawContent)).Should().Throw<ArgumentException>();
		}
	}
}