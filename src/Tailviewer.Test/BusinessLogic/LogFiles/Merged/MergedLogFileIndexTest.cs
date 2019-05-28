using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Merged;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Merged
{
	[TestFixture]
	public sealed class MergedLogFileIndexTest
	{
		[Test]
		public void TestGetInvalid()
		{
			var index = new MergedLogFileIndex();
			index.Get(new LogFileSection(0, 1)).Should().Equal(new object[]
			{
				MergedLogLineIndex.Invalid
			});
		}

		[Test]
		public void TestGetPartialInvalid()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("Hello, World!", LevelFlags.None, new DateTime(2019, 5, 28, 0, 53, 0));

			var index = new MergedLogFileIndex(source);
			index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 1)));

			index.Get(new LogFileSection(0, 2)).Should().Equal(new object[]
			{
				new MergedLogLineIndex(0, 0, 0, 0, new DateTime(2019, 5, 28, 0, 53, 0)), 
				MergedLogLineIndex.Invalid
			});
		}

		[Test]
		public void TestAppendOneSourceOneLine()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("Hello, World!", LevelFlags.None, new DateTime(2019, 5, 28, 19, 55, 10));

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			});

			var indices = index.Get(new LogFileSection(0, 1));
			indices.Count.Should().Be(1);
			indices[0].LogFileIndex.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 28, 19, 55, 10));
		}

		[Test]
		public void TestAppendOneSourceTwoLines()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("Hello,", LevelFlags.None, new DateTime(2019, 5, 27, 23, 37, 0));

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			});

			source.AddEntry("Hello,", LevelFlags.None, new DateTime(2019, 5, 27, 23, 38, 0));
			changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(1, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(1, 1)
			});

			var indices = index.Get(new LogFileSection(0, 2));
			indices.Count.Should().Be(2);
			indices[0].LogFileIndex.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 37, 0));

			indices[1].LogFileIndex.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(1);
			indices[1].OriginalLogEntryIndex.Should().Be(1);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 38, 0));
		}

		[Test]
		public void TestAppendTwoSourcesWrongOrder()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("B", LevelFlags.None, new DateTime(2019, 5, 27, 23, 10, 0));
			var source2 = new InMemoryLogFile();
			source2.AddEntry("A", LevelFlags.None, new DateTime(2019, 5, 27, 23, 09, 0));

			var index = new MergedLogFileIndex(source1, source2);
			var changes = index.Process(new MergedLogFilePendingModification(source1, new LogFileSection(0, 1)),
			                            new MergedLogFilePendingModification(source2, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 2)
			});

			var indices = index.Get(new LogFileSection(0, 2));
			indices.Count.Should().Be(2);
			indices[0].LogFileIndex.Should().Be(1);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 9, 0));

			indices[1].LogFileIndex.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(0);
			indices[1].OriginalLogEntryIndex.Should().Be(0);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 10, 0));
		}

		[Test]
		public void TestAppendTwoSourcesWrongOrderSeparateChangesFullInvalidation()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("B", LevelFlags.None, new DateTime(2019, 5, 27, 23, 10, 0));
			var source2 = new InMemoryLogFile();
			source2.AddEntry("A", LevelFlags.None, new DateTime(2019, 5, 27, 23, 09, 0));

			var index = new MergedLogFileIndex(source1, source2);
			var changes = index.Process(new MergedLogFilePendingModification(source1, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			});

			changes = index.Process(new MergedLogFilePendingModification(source2, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				LogFileSection.Invalidate(0, 1),
				new LogFileSection(0, 2)
			});

			var indices = index.Get(new LogFileSection(0, 2));
			indices.Count.Should().Be(2);
			indices[0].LogFileIndex.Should().Be(1);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 9, 0));

			indices[1].LogFileIndex.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(0);
			indices[1].OriginalLogEntryIndex.Should().Be(0);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 10, 0));
		}

		[Test]
		public void TestAppendTwoSourcesWrongOrderSeparateChangesPartialInvalidation()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.None, new DateTime(2019, 5, 28, 00, 34, 0));
			source1.AddEntry("C", LevelFlags.None, new DateTime(2019, 5, 28, 00, 36, 0));
			var source2 = new InMemoryLogFile();
			source2.AddEntry("B", LevelFlags.None, new DateTime(2019, 5, 28, 00, 35, 0));
			source2.AddEntry("D", LevelFlags.None, new DateTime(2019, 5, 28, 00, 37, 0));

			var index = new MergedLogFileIndex(source1, source2);
			var changes = index.Process(new MergedLogFilePendingModification(source1, new LogFileSection(0, 2)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 2)
			});

			changes = index.Process(new MergedLogFilePendingModification(source2, new LogFileSection(0, 2)));
			changes.Should().Equal(new object[]
			{
				LogFileSection.Invalidate(1, 1),
				new LogFileSection(1, 3)
			});
		}

		[Test]
		public void TestOneSourceResetEmpty()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.None, new DateTime(2019, 5, 28, 00, 34, 0));

			var index = new MergedLogFileIndex(source1);
			var changes = index.Process(new MergedLogFilePendingModification(source1, LogFileSection.Reset));
			changes.Should().BeEmpty("because the index itself is empty and thus its source resetting itself doesn't require any change");
		}

		[Test]
		public void TestOneSourceAppendReset()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.None, new DateTime(2019, 5, 28, 00, 34, 0));

			var index = new MergedLogFileIndex(source1);
			var changes = index.Process(new MergedLogFilePendingModification(source1, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			});

			changes = index.Process(new MergedLogFilePendingModification(source1, LogFileSection.Reset));
			changes.Should().Equal(new object[]
			{
				LogFileSection.Reset
			});
			index.Count.Should().Be(0);
			index.Get(new LogFileSection(0, 1)).Should().Equal(new object[]
			{
				MergedLogLineIndex.Invalid
			});
		}

		[Test]
		public void TestOneSourceResetAndAppend()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.None, new DateTime(2019, 5, 28, 00, 34, 0));

			var index = new MergedLogFileIndex(source1);
			var changes = index.Process(
				new MergedLogFilePendingModification(source1, new LogFileSection(0, 2)),
				new MergedLogFilePendingModification(source1, LogFileSection.Reset),
				new MergedLogFilePendingModification(source1, new LogFileSection(0, 1))
				);
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			}, "because the index shouldn't process changes belonging to source1 prior to the last reset");
			index.Count.Should().Be(1);
		}

		#region Skip log lines without timestamp

		[Test]
		[Description("Verifies that log lines without timestamp will be ignored")]
		public void TestAppendOneSourceOneLineWithoutTimestamp()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("Hello, World!", LevelFlags.None);

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 1)));
			changes.Should().BeEmpty("because the only added line doesn't have a timestamp and thus cannot be added to the merged log file");

			index.Count.Should().Be(0);
			index.Get(new LogFileSection(0, 1)).Should().Equal(new object[]
			{
				MergedLogLineIndex.Invalid
			});
		}

		[Test]
		[Description("Verifies that log lines without timestamp will be ignored")]
		public void TestAppendOneSourceThreeOneLinesOneWithoutTimestamp()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("A", LevelFlags.None, new DateTime(2019, 5, 28, 19, 30, 1));
			source.AddEntry("B", LevelFlags.None);
			source.AddEntry("C", LevelFlags.None, new DateTime(2019, 5, 28, 19, 30, 42));

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 3)));
			changes.Should().Equal(new object[] {new LogFileSection(0, 2)});

			index.Count.Should().Be(2);

			var indices = index.Get(new LogFileSection(0, 2));
			indices.Count.Should().Be(2);
			indices[0].LogFileIndex.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 28, 19, 30, 1));

			indices[1].LogFileIndex.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(2);
			indices[1].OriginalLogEntryIndex.Should().Be(2);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 28, 19, 30, 42));
		}

		#endregion
	}
}
