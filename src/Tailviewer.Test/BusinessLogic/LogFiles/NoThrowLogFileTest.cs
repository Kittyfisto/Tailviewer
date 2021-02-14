using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class NoThrowLogFileTest
		: AbstractLogFileTest
	{
		private Mock<ILogFile> _logFile;
		private string _pluginName;
		private NoThrowLogFile _proxy;

		[SetUp]
		public void Setup()
		{
			_logFile = new Mock<ILogFile>();
			_pluginName = "Buggy plugin";
			_proxy = new NoThrowLogFile(_logFile.Object, _pluginName);
		}

		[Test]
		public void TestDispose()
		{
			_logFile.Setup(x => x.Dispose()).Throws<SystemException>();
			new Action(() => _proxy.Dispose()).Should().NotThrow();
			_logFile.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void TestStartTimestamp()
		{
			_logFile.Setup(x => x.GetProperty(LogFileProperties.StartTimestamp)).Throws<SystemException>();
			_proxy.GetProperty(LogFileProperties.StartTimestamp).Should().BeNull();
			_logFile.Verify(x => x.GetProperty(LogFileProperties.StartTimestamp), Times.Once);
		}

		[Test]
		public void TestLastModified()
		{
			_logFile.Setup(x => x.GetProperty(LogFileProperties.LastModified)).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.GetProperty(LogFileProperties.LastModified);
			}).Should().NotThrow();
			_logFile.Verify(x => x.GetProperty(LogFileProperties.LastModified), Times.Once);
		}

		[Test]
		public void TestExists()
		{
			_logFile.Setup(x => x.GetProperty(LogFileProperties.EmptyReason)).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.GetProperty(LogFileProperties.EmptyReason);
			}).Should().NotThrow();
			_logFile.Verify(x => x.GetProperty(LogFileProperties.EmptyReason), Times.Once);
		}

		[Test]
		public void TestCount()
		{
			_logFile.Setup(x => x.GetProperty(LogFileProperties.LogEntryCount)).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.GetProperty(LogFileProperties.LogEntryCount);
			}).Should().NotThrow();
			_logFile.Verify(x => x.GetProperty(LogFileProperties.LogEntryCount), Times.Once);
		}

		[Test]
		public void TestAddListener()
		{
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>())).Throws<SystemException>();
			var listener = new Mock<ILogFileListener>().Object;
			var maximumWaitTime = TimeSpan.FromSeconds(42);
			var maximumLineCount = 9001;
			new Action(() => _proxy.AddListener(listener, maximumWaitTime, maximumLineCount)).Should().NotThrow();
			_logFile.Verify(x => x.AddListener(It.Is<ILogFileListener>(y => y == listener),
				It.Is<TimeSpan>(y => y == maximumWaitTime),
				It.Is<int>(y => y == maximumLineCount)), Times.Once);
		}

		[Test]
		public void TestRemoveListener()
		{
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogFileListener>())).Throws<SystemException>();
			var listener = new Mock<ILogFileListener>().Object;
			new Action(() => _proxy.RemoveListener(listener)).Should().NotThrow();
			_logFile.Verify(x => x.RemoveListener(It.Is<ILogFileListener>(y => y == listener)), Times.Once);
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex()
		{
			_logFile.Setup(x => x.GetLogLineIndexOfOriginalLineIndex(It.IsAny<LogLineIndex>())).Throws<SystemException>();
			var index = new LogLineIndex(42);
			new Action(() => _proxy.GetLogLineIndexOfOriginalLineIndex(index)).Should().NotThrow();
			_logFile.Verify(x => x.GetLogLineIndexOfOriginalLineIndex(It.Is<LogLineIndex>(y => y == index)), Times.Once);
		}

		[Test]
		public void TestGetColumn1()
		{
			_logFile.Setup(x => x.GetColumn(It.IsAny<LogFileSection>(), It.IsAny<ILogFileColumnDescriptor<string>>(), It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<LogFileQueryOptions>())).Throws<SystemException>();

			var section = new LogFileSection(42, 100);
			var buffer = new string[9101];
			var destinationIndex = 9001;
			var queryOptions = new LogFileQueryOptions(LogFileQueryMode.FromCacheOnly);
			new Action(() => _proxy.GetColumn(section, LogFileColumns.RawContent, buffer, destinationIndex, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetColumn(It.Is<LogFileSection>(y => y == section),
			                                 LogFileColumns.RawContent,
			                                 buffer,
			                                 destinationIndex,
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetColumn2()
		{
			_logFile.Setup(x => x.GetColumn(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<ILogFileColumnDescriptor<string>>(), It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<LogFileQueryOptions>())).Throws<SystemException>();

			var indices = new LogLineIndex[] {1, 2, 3};
			var buffer = new string[201];
			var queryOptions = new LogFileQueryOptions(LogFileQueryMode.FromCacheOnly);
			new Action(() => _proxy.GetColumn(indices, LogFileColumns.RawContent, buffer, 101, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetColumn(It.Is<IReadOnlyList<LogLineIndex>>(y => y == indices),
			                                 It.Is<ILogFileColumnDescriptor<string>>(y => Equals(y, LogFileColumns.RawContent)),
			                                 It.Is<string[]>(y => ReferenceEquals(y, buffer)),
			                                 It.Is<int>(y => y == 101),
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetEntries1()
		{
			_logFile.Setup(x => x.GetEntries(It.IsAny<LogFileSection>(), It.IsAny<ILogEntries>(), It.IsAny<int>(), It.IsAny<LogFileQueryOptions>())).Throws<SystemException>();

			var section = new LogFileSection(42, 100);
			var buffer = new Mock<ILogEntries>().Object;
			var destinationIndex = 9001;
			var queryOptions = new LogFileQueryOptions(LogFileQueryMode.FromCacheOnly);
			new Action(() => _proxy.GetEntries(section, buffer, destinationIndex, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetEntries(It.Is<LogFileSection>(y => y == section),
			                                 It.Is<ILogEntries>(y => ReferenceEquals(y, buffer)),
			                                 destinationIndex,
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetEntries2()
		{
			_logFile.Setup(x => x.GetEntries(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<ILogEntries>(), It.IsAny<int>(), It.IsAny<LogFileQueryOptions>())).Throws<SystemException>();

			var indices = new LogLineIndex[] { 1, 2, 3 };
			var buffer = new Mock<ILogEntries>().Object;
			var destinationIndex = 101;
			var queryOptions = new LogFileQueryOptions(LogFileQueryMode.FromCacheOnly);
			new Action(() => _proxy.GetEntries(indices, buffer, destinationIndex, queryOptions)).Should().NotThrow();

			_logFile.Verify(x => x.GetEntries(It.Is<IReadOnlyList<LogLineIndex>>(y => y == indices),
			                                 It.Is<ILogEntries>(y => ReferenceEquals(y, buffer)),
			                                 destinationIndex,
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetColumns1()
		{
			_logFile.Setup(x => x.Columns).Returns(new[] {LogFileColumns.DeltaTime, LogFileColumns.ElapsedTime});
			_proxy.Columns.Should().Equal(LogFileColumns.DeltaTime, LogFileColumns.ElapsedTime);
			_logFile.Verify(x => x.Columns, Times.Once);
		}

		[Test]
		public void TestGetColumns2()
		{
			_logFile.Setup(x => x.Columns).Throws<NullReferenceException>();
			_proxy.Columns.Should().BeEmpty();
			_logFile.Verify(x => x.Columns, Times.Once);
		}

		protected override ILogFile CreateEmpty()
		{
			var source = new InMemoryLogFile();
			return new NoThrowLogFile(source, "");
		}

		protected override ILogFile CreateFromContent(IReadOnlyLogEntries content)
		{
			var source = new InMemoryLogFile(content);
			return new NoThrowLogFile(source, "");
		}
	}
}