﻿using System;
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

		public abstract ILogEntry Create(params ILogFileColumn[] columns);

		[Test]
		public void TestSetLogLevel()
		{
			var entry = CreateDefault();
			entry.LogLevel.Should().Be(LogFileColumns.LogLevel.DefaultValue);

			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.GetValue(LogFileColumns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = CreateDefault();
			entry.LogEntryIndex.Should().Be(LogFileColumns.LogEntryIndex.DefaultValue);

			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.GetValue(LogFileColumns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = CreateDefault();
			entry.DeltaTime.Should().Be(LogFileColumns.DeltaTime.DefaultValue);

			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(LogFileColumns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = CreateDefault();
			entry.ElapsedTime.Should().Be(LogFileColumns.ElapsedTime.DefaultValue);

			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(LogFileColumns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = CreateDefault();
			entry.Timestamp.Should().Be(LogFileColumns.Timestamp.DefaultValue);

			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.GetValue(LogFileColumns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = CreateDefault();
			entry.RawContent.Should().Be(LogFileColumns.RawContent.DefaultValue);

			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.GetValue(LogFileColumns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = CreateDefault();
			entry.Index.Should().Be(LogFileColumns.Index.DefaultValue);

			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.GetValue(LogFileColumns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = CreateDefault();
			entry.OriginalIndex.Should().Be(LogFileColumns.OriginalIndex.DefaultValue);

			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.GetValue(LogFileColumns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = CreateDefault();
			entry.LineNumber.Should().Be(LogFileColumns.LineNumber.DefaultValue);

			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.GetValue(LogFileColumns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = CreateDefault();
			entry.OriginalLineNumber.Should().Be(LogFileColumns.OriginalLineNumber.DefaultValue);

			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.GetValue(LogFileColumns.OriginalLineNumber).Should().Be(1337);
		}

		[Test]
		public void TestSetOriginalDataSourceName()
		{
			var entry = CreateDefault();
			entry.OriginalDataSourceName.Should().Be(LogFileColumns.OriginalDataSourceName.DefaultValue);

			entry.OriginalDataSourceName = "Foobar";
			entry.OriginalDataSourceName.Should().Be("Foobar");
			entry.GetValue(LogFileColumns.OriginalDataSourceName).Should().Be("Foobar");
		}

		[Test]
		public void TestSetSourceId()
		{
			var entry = Create(LogFileColumns.SourceId);
			entry.SourceId.Should().Be(LogFileColumns.SourceId.DefaultValue);

			entry.SourceId = new LogLineSourceId(42);
			entry.SourceId.Should().Be(new LogLineSourceId(42));
			entry.GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(42));
		}

		[Test]
		public void TestSetValueWrongType()
		{
			var entry = CreateEmpty();
			new Action(() => entry.SetValue(LogFileColumns.RawContent, 42)).Should().Throw<ArgumentException>();
			entry.Columns.Should().BeEmpty();
			new Action(() => entry.GetValue(LogFileColumns.RawContent)).Should().Throw<ArgumentException>();
		}
	}
}