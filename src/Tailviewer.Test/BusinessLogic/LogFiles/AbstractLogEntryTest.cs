using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
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
			entry.LogLevel.Should().Be(Columns.LogLevel.DefaultValue);

			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.GetValue(Columns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = CreateDefault();
			entry.LogEntryIndex.Should().Be(Columns.LogEntryIndex.DefaultValue);

			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.GetValue(Columns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = CreateDefault();
			entry.DeltaTime.Should().Be(Columns.DeltaTime.DefaultValue);

			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(Columns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = CreateDefault();
			entry.ElapsedTime.Should().Be(Columns.ElapsedTime.DefaultValue);

			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(Columns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = CreateDefault();
			entry.Timestamp.Should().Be(Columns.Timestamp.DefaultValue);

			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.GetValue(Columns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = CreateDefault();
			entry.RawContent.Should().Be(Columns.RawContent.DefaultValue);

			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.GetValue(Columns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = CreateDefault();
			entry.Index.Should().Be(Columns.Index.DefaultValue);

			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.GetValue(Columns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = CreateDefault();
			entry.OriginalIndex.Should().Be(Columns.OriginalIndex.DefaultValue);

			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.GetValue(Columns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = CreateDefault();
			entry.LineNumber.Should().Be(Columns.LineNumber.DefaultValue);

			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.GetValue(Columns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = CreateDefault();
			entry.OriginalLineNumber.Should().Be(Columns.OriginalLineNumber.DefaultValue);

			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.GetValue(Columns.OriginalLineNumber).Should().Be(1337);
		}

		[Test]
		public void TestSetOriginalDataSourceName()
		{
			var entry = CreateDefault();
			entry.OriginalDataSourceName.Should().Be(Columns.OriginalDataSourceName.DefaultValue);

			entry.OriginalDataSourceName = "Foobar";
			entry.OriginalDataSourceName.Should().Be("Foobar");
			entry.GetValue(Columns.OriginalDataSourceName).Should().Be("Foobar");
		}

		[Test]
		public void TestSetSourceId()
		{
			var entry = Create(Columns.SourceId);
			entry.SourceId.Should().Be(Columns.SourceId.DefaultValue);

			entry.SourceId = new LogLineSourceId(42);
			entry.SourceId.Should().Be(new LogLineSourceId(42));
			entry.GetValue(Columns.SourceId).Should().Be(new LogLineSourceId(42));
		}

		[Test]
		public void TestSetValueWrongType()
		{
			var entry = CreateEmpty();
			new Action(() => entry.SetValue(Columns.RawContent, 42)).Should().Throw<ArgumentException>();
			entry.Columns.Should().BeEmpty();
			new Action(() => entry.GetValue(Columns.RawContent)).Should().Throw<ArgumentException>();
		}
	}
}