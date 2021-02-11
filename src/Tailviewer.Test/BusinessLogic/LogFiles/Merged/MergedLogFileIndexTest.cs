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
			source.AddEntry("Hello, World!", LevelFlags.Other, new DateTime(2019, 5, 28, 0, 53, 0));

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
			source.AddEntry("Hello, World!", LevelFlags.Other, new DateTime(2019, 5, 28, 19, 55, 10));

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			});

			var indices = index.Get(new LogFileSection(0, 1));
			indices.Count.Should().Be(1);
			indices[0].SourceId.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 28, 19, 55, 10));
		}

		[Test]
		public void TestAppendOneSourceTwoLines()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("Hello,", LevelFlags.Other, new DateTime(2019, 5, 27, 23, 37, 0));

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			});

			source.AddEntry("Hello,", LevelFlags.Other, new DateTime(2019, 5, 27, 23, 38, 0));
			changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(1, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(1, 1)
			});

			var indices = index.Get(new LogFileSection(0, 2));
			indices.Count.Should().Be(2);
			indices[0].SourceId.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 37, 0));

			indices[1].SourceId.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(1);
			indices[1].OriginalLogEntryIndex.Should().Be(1);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 38, 0));
		}

		[Test]
		public void TestAppendTwoSourcesWrongOrder()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("B", LevelFlags.Other, new DateTime(2019, 5, 27, 23, 10, 0));
			var source2 = new InMemoryLogFile();
			source2.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 27, 23, 09, 0));

			var index = new MergedLogFileIndex(source1, source2);
			var changes = index.Process(new MergedLogFilePendingModification(source1, new LogFileSection(0, 1)),
			                            new MergedLogFilePendingModification(source2, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 2)
			});

			var indices = index.Get(new LogFileSection(0, 2));
			indices.Count.Should().Be(2);
			indices[0].SourceId.Should().Be(1);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 9, 0));

			indices[1].SourceId.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(0);
			indices[1].OriginalLogEntryIndex.Should().Be(0);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 10, 0));
		}

		[Test]
		public void TestAppendTwoSourcesWrongOrderSeparateChangesFullInvalidation()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("B", LevelFlags.Other, new DateTime(2019, 5, 27, 23, 10, 0));
			var source2 = new InMemoryLogFile();
			source2.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 27, 23, 09, 0));

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
			indices[0].SourceId.Should().Be(1);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 9, 0));

			indices[1].SourceId.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(0);
			indices[1].OriginalLogEntryIndex.Should().Be(0);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 27, 23, 10, 0));
		}

		[Test]
		public void TestAppendTwoSourcesWrongOrderSeparateChangesPartialInvalidation()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 28, 00, 34, 0));
			source1.AddEntry("C", LevelFlags.Other, new DateTime(2019, 5, 28, 00, 36, 0));
			var source2 = new InMemoryLogFile();
			source2.AddEntry("B", LevelFlags.Other, new DateTime(2019, 5, 28, 00, 35, 0));
			source2.AddEntry("D", LevelFlags.Other, new DateTime(2019, 5, 28, 00, 37, 0));

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
		public void TestAppendOneSourceTwoIdenticalTimestamps()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 0));
			source.AddEntry("B", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 2));
			source.AddEntry("C1", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 4));
			source.AddEntry("C2", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 4));
			source.AddEntry("D", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 6));
			source.AddEntry("E", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 8));

			var index = new MergedLogFileIndex(source);
			index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 6)));

			var indices = index.Get(new LogFileSection(0, 6));
			indices[0].SourceId.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);

			indices[1].SourceId.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(1);

			indices[2].SourceId.Should().Be(0);
			indices[2].SourceLineIndex.Should().Be(2);

			indices[3].SourceId.Should().Be(0);
			indices[3].SourceLineIndex.Should().Be(3);

			indices[4].SourceId.Should().Be(0);
			indices[4].SourceLineIndex.Should().Be(4);

			indices[5].SourceId.Should().Be(0);
			indices[5].SourceLineIndex.Should().Be(5);
		}

		[Test]
		public void TestAppendTwoSourcesInterlocked()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 0));
			source1.AddEntry("B", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 2));
			source1.AddEntry("C", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 4));
			source1.AddEntry("D", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 6));
			source1.AddEntry("E", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 8));
			var source2 = new InMemoryLogFile();
			source2.AddEntry("1", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 1));
			source2.AddEntry("2", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 3));
			source2.AddEntry("3", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 5));
			source2.AddEntry("4", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 7));
			source2.AddEntry("5", LevelFlags.Other, new DateTime(2019, 5, 29, 00, 11, 9));

			var index = new MergedLogFileIndex(source1, source2);
			index.Process(new MergedLogFilePendingModification(source1, new LogFileSection(0, 5)));
			index.Process(new MergedLogFilePendingModification(source2, new LogFileSection(0, 5)));

			var indices = index.Get(new LogFileSection(0, 10));
			indices[0].SourceId.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);

			indices[1].SourceId.Should().Be(1);
			indices[1].SourceLineIndex.Should().Be(0);

			indices[2].SourceId.Should().Be(0);
			indices[2].SourceLineIndex.Should().Be(1);

			indices[3].SourceId.Should().Be(1);
			indices[3].SourceLineIndex.Should().Be(1);

			indices[4].SourceId.Should().Be(0);
			indices[4].SourceLineIndex.Should().Be(2);

			indices[5].SourceId.Should().Be(1);
			indices[5].SourceLineIndex.Should().Be(2);

			indices[6].SourceId.Should().Be(0);
			indices[6].SourceLineIndex.Should().Be(3);

			indices[7].SourceId.Should().Be(1);
			indices[7].SourceLineIndex.Should().Be(3);

			indices[8].SourceId.Should().Be(0);
			indices[8].SourceLineIndex.Should().Be(4);

			indices[9].SourceId.Should().Be(1);
			indices[9].SourceLineIndex.Should().Be(4);
		}

		[Test]
		public void TestOneSourceResetEmpty()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 28, 00, 34, 0));

			var index = new MergedLogFileIndex(source1);
			var changes = index.Process(new MergedLogFilePendingModification(source1, LogFileSection.Reset));
			changes.Should().BeEmpty("because the index itself is empty and thus its source resetting itself doesn't require any change");
		}

		[Test]
		public void TestOneSourceAppendReset()
		{
			var source1 = new InMemoryLogFile();
			source1.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 28, 00, 34, 0));

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
			source1.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 28, 00, 34, 0));

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

		[Test]
		public void TestOneSourceManySameTimestamps()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("A", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 053));
			source.AddEntry("B", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 100));
			source.AddEntry("C", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 100));
			source.AddEntry("D", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 100));
			source.AddEntry("E", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 115));
			source.AddEntry("F", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 115));
			source.AddEntry("G", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 115));
			source.AddEntry("H", LevelFlags.Other, new DateTime(2017, 9, 20, 15, 09, 02, 115));

			var index = new MergedLogFileIndex(source);
			index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 8)));

			var indices = index.Get(new LogFileSection(0, 8));
			indices[0].SourceLineIndex.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(1);
			indices[2].SourceLineIndex.Should().Be(2);
			indices[3].SourceLineIndex.Should().Be(3);
			indices[4].SourceLineIndex.Should().Be(4);
			indices[5].SourceLineIndex.Should().Be(5);
			indices[6].SourceLineIndex.Should().Be(6);
			indices[7].SourceLineIndex.Should().Be(7);
		}

		#region Skip log lines without timestamp

		[Test]
		[Description("Verifies that log lines without timestamp will be ignored")]
		public void TestAppendOneSourceOneLineWithoutTimestamp()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("Hello, World!", LevelFlags.Other);

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
			source.AddEntry("A", LevelFlags.Other, new DateTime(2019, 5, 28, 19, 30, 1));
			source.AddEntry("B", LevelFlags.Other);
			source.AddEntry("C", LevelFlags.Other, new DateTime(2019, 5, 28, 19, 30, 42));

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 3)));
			changes.Should().Equal(new object[] {new LogFileSection(0, 2)});

			index.Count.Should().Be(2);

			var indices = index.Get(new LogFileSection(0, 2));
			indices.Count.Should().Be(2);
			indices[0].SourceId.Should().Be(0);
			indices[0].SourceLineIndex.Should().Be(0);
			indices[0].OriginalLogEntryIndex.Should().Be(0);
			indices[0].MergedLogEntryIndex.Should().Be(0);
			indices[0].Timestamp.Should().Be(new DateTime(2019, 5, 28, 19, 30, 1));

			indices[1].SourceId.Should().Be(0);
			indices[1].SourceLineIndex.Should().Be(2);
			indices[1].OriginalLogEntryIndex.Should().Be(2);
			indices[1].MergedLogEntryIndex.Should().Be(1);
			indices[1].Timestamp.Should().Be(new DateTime(2019, 5, 28, 19, 30, 42));
		}

		#endregion
	}
}
