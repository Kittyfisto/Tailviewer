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
		private List<LogFileSection> _sections;
		private Mock<ILogFileListener> _listener;

		[SetUp]
		public void SetUp()
		{
			_entries = new List<LogLine>();
			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
			        .Callback((LogFileSection section, LogLine[] entries) => _entries.CopyTo((int) section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);

			_sections = new List<LogFileSection>();
			_listener = new Mock<ILogFileListener>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>())).Callback((ILogFile l, LogFileSection s) => _sections.Add(s));
		}

		[Test]
		public void TestEntryLevelNone()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("ello", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, "Hello world!", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));
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
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("Test", true, LevelFlags.All)))
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
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("Test", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));
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
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("yikes", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));
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
		[Description("Verifies that listeners are notified eventually, even when the # of filtered entries is less than the minimum batch size")]
		public void TestListener()
		{
			_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
			_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create("yikes", true, LevelFlags.All)))
			{
				var sections = new List<LogFileSection>();
				var listener = new Mock<ILogFileListener>();

				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>())).Callback((ILogFile l, LogFileSection s) => sections.Add(s));
				// We deliberately set the batchSize to be greater than the amount of entries that will be matched.
				// If the FilteredLogFile is implemented correctly, then it will continously notifiy the listener until
				// the maximum wait time is elapsed.
				const int batchSize = 10;
				file.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), batchSize);
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));
				file.Start(TimeSpan.Zero);
				file.Wait();

				file.Count.Should().Be(2);
				WaitUntil(() => sections.Count == 2, TimeSpan.FromSeconds(1))
					.Should().BeTrue("Because the FilteredLogFile should've notified the listener eventually");

				sections.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 2)
					});
			}
		}

		[Test]
		[Description("Verifies that the filtered log file correctly listens to a reset event")]
		public void TestClear()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create(null, true, LevelFlags.Debug)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "DEBUG: Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				file.Start(TimeSpan.Zero);
				file.Wait();
				file.Count.Should().Be(2);

				_entries.Clear();
				file.OnLogFileModified(_logFile.Object, LogFileSection.Reset);
				file.Wait();
				file.Count.Should().Be(0);
			}
		}

		[Test]
		[Description("Verifies that the filtered log file repeatedly calls the listener when the source has been fully read")]
		public void TestWait()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create(null, true, LevelFlags.Debug)))
			{
				var sections = new List<LogFileSection>();
				var listener = new Mock<ILogFileListener>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));
				file.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), 3);

				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "DEBUG: Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				file.Start(TimeSpan.Zero);
				file.Wait();

				WaitUntil(() => sections.Count > 1, TimeSpan.FromMilliseconds(1000)).Should().BeTrue();
				sections[0].Should().Be(LogFileSection.Reset);
				sections[1].Should().Be(new LogFileSection(new LogLineIndex(0), 2));
			}
		}

		[Test]
		public void TestInvalidate1()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create(null, true, LevelFlags.Info)))
			{
				file.Start(TimeSpan.Zero);
				file.Wait();

				_entries.AddRange(new []
					{
						new LogLine(0, 0, "A", LevelFlags.Info),
						new LogLine(1, 1, "B", LevelFlags.Info),
						new LogLine(2, 2, "C", LevelFlags.Info),
						new LogLine(3, 3, "D", LevelFlags.Info)
					});

				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(2, 2, true));
				file.Wait();

				file.Count.Should().Be(2);
			}
		}

		[Test]
		public void TestInvalidate2()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create(null, true, LevelFlags.Info)))
			{
				file.AddListener(_listener.Object, TimeSpan.Zero, 1);
				file.Start(TimeSpan.Zero);
				file.Wait();

				_entries.AddRange(new[]
					{
						new LogLine(0, 0, "A", LevelFlags.Info),
						new LogLine(1, 1, "B", LevelFlags.Info),
						new LogLine(2, 2, "C", LevelFlags.Info),
						new LogLine(3, 3, "D", LevelFlags.Info)
					});
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				file.Wait();
				file.Count.Should().Be(4);

				file.OnLogFileModified(_logFile.Object, new LogFileSection(2, 2, true));
				file.Wait();
				file.Count.Should().Be(2);

				_sections.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 1),
						new LogFileSection(1, 1),
						new LogFileSection(2, 1),
						new LogFileSection(3, 1),
						new LogFileSection(2, 2, true)
					});
			}
		}

		[Test]
		[Description("Verifies that the FilteredLogFile won't get stuck in an endless loop when an Invalidate() follows a multiline log entry")]
		public void TestInvalidate3()
		{
			using (var file = new FilteredLogFile(_logFile.Object, Filter.Create(null, true, LevelFlags.Info)))
			{
				file.AddListener(_listener.Object, TimeSpan.Zero, 1);
				file.Start(TimeSpan.Zero);
				file.Wait();

				_entries.AddRange(new[]
					{
						new LogLine(0, 0, "A", LevelFlags.Info),
						new LogLine(1, 0, "B", LevelFlags.Info),
						new LogLine(2, 0, "C", LevelFlags.Info),
						new LogLine(3, 0, "D", LevelFlags.Info)
					});
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				file.Wait();

				file.OnLogFileModified(_logFile.Object, new LogFileSection(2, 2, true));
				file.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue("Because the filtered log file should be finished in 1 second");
				file.Count.Should().Be(2);

				_sections.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 1),
						new LogFileSection(1, 1),
						new LogFileSection(2, 1),
						new LogFileSection(3, 1),
						new LogFileSection(2, 2, true)
					});
			}
		}
	}
}