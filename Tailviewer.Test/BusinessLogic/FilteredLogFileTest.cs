using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class FilteredLogFileTest
		: AbstractTest
	{
		private Mock<ILogFile> _logFile;
		private List<LogLine> _entries;

		[SetUp]
		public void SetUp()
		{
			_entries = new List<LogLine>();
			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
			        .Callback((LogFileSection section, LogLine[] entries) => _entries.CopyTo((int) section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.GetEntry(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);
		}

		[Test]
		public void TestEntryLevelNone()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("ello", true, LevelFlags.All, false)))
			{
				_entries.Add(new LogLine(0, "Hello world!", LevelFlags.None));
				file.OnLogFileModified(new LogFileSection(0, 1));
				file.Start(TimeSpan.Zero);
				file.Wait();

				file.Count.Should().Be(1);
				file.GetSection(new LogFileSection(0, 1))
				    .Should().Equal(new[]
					    {
						    new LogLine(0, "Hello world!", LevelFlags.None)
					    });
			}
		}

		[Test]
		public void TestEmptyLogFile()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("Test", true, LevelFlags.All, false)))
			{
				file.Start(TimeSpan.Zero);
				file.Wait();
				file.Count.Should().Be(0);
			}
		}

		[Test]
		[Description("Verifies that all lines belonging to an entry pass the filter, even though only one line passes it")]
		public void TestMultiLineLogEntry1()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("Test", true, LevelFlags.All, false)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
				file.OnLogFileModified(new LogFileSection(0, 2));
				file.Start(TimeSpan.Zero);
				file.Wait();

				file.Count.Should().Be(2);
				file.GetSection(new LogFileSection(0, 2))
					.Should().Equal(new[]
					    {
						    new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug),
							new LogLine(1, 0, "Yikes", LevelFlags.None)
					    });
			}
		}

		[Test]
		[Description("Verifies that all lines belonging to an entry pass the filter, even though only the second line passes it")]
		public void TestMultiLineLogEntry2()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("yikes", true, LevelFlags.All, false)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
				file.OnLogFileModified(new LogFileSection(0, 2));
				file.Start(TimeSpan.Zero);
				file.Wait();

				file.Count.Should().Be(2);
				file.GetSection(new LogFileSection(0, 2))
					.Should().Equal(new[]
					    {
						    new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug),
							new LogLine(1, 0, "Yikes", LevelFlags.None)
					    });
			}
		}

		[Test]
		[Description("Verifies that the filtered log file correctly listens to a reset event")]
		public void TestClear()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create(null, true, LevelFlags.Debug, false)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "DEBUG: Yikes", LevelFlags.None));
				file.OnLogFileModified(new LogFileSection(0, 2));

				file.Start(TimeSpan.Zero);
				file.Wait();
				file.Count.Should().Be(2);

				_entries.Clear();
				file.OnLogFileModified(LogFileSection.Reset);
				file.Wait();
				file.Count.Should().Be(0);
			}
		}
	}
}