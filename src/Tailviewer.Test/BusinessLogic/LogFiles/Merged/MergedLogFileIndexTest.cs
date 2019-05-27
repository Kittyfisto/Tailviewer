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
			source.AddEntry("Hello, World!", LevelFlags.None, DateTime.Now);

			var index = new MergedLogFileIndex(source);
			var changes = index.Process(new MergedLogFilePendingModification(source, new LogFileSection(0, 1)));
			changes.Should().Equal(new object[]
			{
				new LogFileSection(0, 1)
			});
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
	}
}
