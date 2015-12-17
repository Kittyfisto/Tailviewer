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
	{
		private Mock<ILogFile> _logFile;
		private List<LogLine> _entries;

		[SetUp]
		public void SetUp()
		{
			_entries = new List<LogLine>();
			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
			        .Callback((LogFileSection section, LogLine[] entries) => _entries.CopyTo(section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.GetEntry(It.IsAny<int>())).Returns((int index) => _entries[index]);
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
	}
}