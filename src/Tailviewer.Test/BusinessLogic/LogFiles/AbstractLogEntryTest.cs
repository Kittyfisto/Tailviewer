﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Columns;

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
			entry.LogLevel.Should().Be(LogColumns.LogLevel.DefaultValue);

			entry.LogLevel = LevelFlags.Fatal;
			entry.LogLevel.Should().Be(LevelFlags.Fatal);
			entry.GetValue(LogColumns.LogLevel).Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestSetLogEntryIndex()
		{
			var entry = CreateDefault();
			entry.LogEntryIndex.Should().Be(LogColumns.LogEntryIndex.DefaultValue);

			entry.LogEntryIndex = 42;
			entry.LogEntryIndex.Should().Be(42);
			entry.GetValue(LogColumns.LogEntryIndex).Should().Be(42);
		}

		[Test]
		public void TestSetDeltaTime()
		{
			var entry = CreateDefault();
			entry.DeltaTime.Should().Be(LogColumns.DeltaTime.DefaultValue);

			entry.DeltaTime = TimeSpan.FromSeconds(23);
			entry.DeltaTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(LogColumns.DeltaTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetElapsedTime()
		{
			var entry = CreateDefault();
			entry.ElapsedTime.Should().Be(LogColumns.ElapsedTime.DefaultValue);

			entry.ElapsedTime = TimeSpan.FromSeconds(23);
			entry.ElapsedTime.Should().Be(TimeSpan.FromSeconds(23));
			entry.GetValue(LogColumns.ElapsedTime).Should().Be(TimeSpan.FromSeconds(23));
		}

		[Test]
		public void TestSetTimestamp()
		{
			var entry = CreateDefault();
			entry.Timestamp.Should().Be(LogColumns.Timestamp.DefaultValue);

			entry.Timestamp = new DateTime(2017, 12, 20, 13, 33, 0);
			entry.Timestamp.Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
			entry.GetValue(LogColumns.Timestamp).Should().Be(new DateTime(2017, 12, 20, 13, 33, 0));
		}

		[Test]
		public void TestSetRawContent()
		{
			var entry = CreateDefault();
			entry.RawContent.Should().Be(LogColumns.RawContent.DefaultValue);

			entry.RawContent = "The last Jedi";
			entry.RawContent.Should().Be("The last Jedi");
			entry.GetValue(LogColumns.RawContent).Should().Be("The last Jedi");
		}

		[Test]
		public void TestSetIndex()
		{
			var entry = CreateDefault();
			entry.Index.Should().Be(LogColumns.Index.DefaultValue);

			entry.Index = 9001;
			entry.Index.Should().Be(9001);
			entry.GetValue(LogColumns.Index).Should().Be(9001);
		}

		[Test]
		public void TestSetOriginalIndex()
		{
			var entry = CreateDefault();
			entry.OriginalIndex.Should().Be(LogColumns.OriginalIndex.DefaultValue);

			entry.OriginalIndex = 8999;
			entry.OriginalIndex.Should().Be(8999);
			entry.GetValue(LogColumns.OriginalIndex).Should().Be(8999);
		}

		[Test]
		public void TestSetLineNumber()
		{
			var entry = CreateDefault();
			entry.LineNumber.Should().Be(LogColumns.LineNumber.DefaultValue);

			entry.LineNumber = 42;
			entry.LineNumber.Should().Be(42);
			entry.GetValue(LogColumns.LineNumber).Should().Be(42);
		}

		[Test]
		public void TestSetOriginalLineNumber()
		{
			var entry = CreateDefault();
			entry.OriginalLineNumber.Should().Be(LogColumns.OriginalLineNumber.DefaultValue);

			entry.OriginalLineNumber = 1337;
			entry.OriginalLineNumber.Should().Be(1337);
			entry.GetValue(LogColumns.OriginalLineNumber).Should().Be(1337);
		}

		[Test]
		public void TestSetOriginalDataSourceName()
		{
			var entry = CreateDefault();
			entry.OriginalDataSourceName.Should().Be(LogColumns.OriginalDataSourceName.DefaultValue);

			entry.OriginalDataSourceName = "Foobar";
			entry.OriginalDataSourceName.Should().Be("Foobar");
			entry.GetValue(LogColumns.OriginalDataSourceName).Should().Be("Foobar");
		}

		[Test]
		public void TestSetSourceId()
		{
			var entry = Create(LogColumns.SourceId);
			entry.SourceId.Should().Be(LogColumns.SourceId.DefaultValue);

			entry.SourceId = new LogLineSourceId(42);
			entry.SourceId.Should().Be(new LogLineSourceId(42));
			entry.GetValue(LogColumns.SourceId).Should().Be(new LogLineSourceId(42));
		}

		[Test]
		public void TestSetValueWrongType()
		{
			var entry = CreateEmpty();
			new Action(() => entry.SetValue(LogColumns.RawContent, 42)).Should().Throw<ArgumentException>();
			entry.Columns.Should().BeEmpty();
			new Action(() => entry.GetValue(LogColumns.RawContent)).Should().Throw<ArgumentException>();
		}
	}
}