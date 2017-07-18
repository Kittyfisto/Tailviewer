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
			new Action(() => _proxy.Dispose()).ShouldNotThrow();
			_logFile.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void TestStartTimestamp()
		{
			_logFile.Setup(x => x.StartTimestamp).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.StartTimestamp;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.StartTimestamp, Times.Once);
		}

		[Test]
		public void TestLastModified()
		{
			_logFile.Setup(x => x.LastModified).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.LastModified;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.LastModified, Times.Once);
		}

		[Test]
		public void TestSize()
		{
			_logFile.Setup(x => x.Size).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.Size;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.Size, Times.Once);
		}

		[Test]
		public void TestExists()
		{
			_logFile.Setup(x => x.Exists).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.Exists;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.Exists, Times.Once);
		}

		[Test]
		public void TestEndOfSourceReached()
		{
			_logFile.Setup(x => x.EndOfSourceReached).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.EndOfSourceReached;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.EndOfSourceReached, Times.Once);
		}

		[Test]
		public void TestCount()
		{
			_logFile.Setup(x => x.Count).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.Count;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.Count, Times.Once);
		}

		[Test]
		public void TestOriginalCount()
		{
			_logFile.Setup(x => x.OriginalCount).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.OriginalCount;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.OriginalCount, Times.Once);
		}

		[Test]
		public void TestMaxCharactersPerLine()
		{
			_logFile.Setup(x => x.MaxCharactersPerLine).Throws<SystemException>();
			new Action(() =>
			{
				var unused = _proxy.MaxCharactersPerLine;
			}).ShouldNotThrow();
			_logFile.Verify(x => x.MaxCharactersPerLine, Times.Once);
		}

		[Test]
		public void TestAddListener()
		{
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>())).Throws<SystemException>();
			var listener = new Mock<ILogFileListener>().Object;
			var maximumWaitTime = TimeSpan.FromSeconds(42);
			var maximumLineCount = 9001;
			new Action(() => _proxy.AddListener(listener, maximumWaitTime, maximumLineCount)).ShouldNotThrow();
			_logFile.Verify(x => x.AddListener(It.Is<ILogFileListener>(y => y == listener),
				It.Is<TimeSpan>(y => y == maximumWaitTime),
				It.Is<int>(y => y == maximumLineCount)), Times.Once);
		}

		[Test]
		public void TestRemoveListener()
		{
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogFileListener>())).Throws<SystemException>();
			var listener = new Mock<ILogFileListener>().Object;
			new Action(() => _proxy.RemoveListener(listener)).ShouldNotThrow();
			_logFile.Verify(x => x.RemoveListener(It.Is<ILogFileListener>(y => y == listener)), Times.Once);
		}

		[Test]
		public void TestGetSection()
		{
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>())).Throws<SystemException>();
			var section = new LogFileSection(42, 9001);
			var lines = new LogLine[9001];
			new Action(() => _proxy.GetSection(section, lines)).ShouldNotThrow();
			_logFile.Verify(x => x.GetSection(It.Is<LogFileSection>(y => y == section),
				It.Is<LogLine[]>(y => y == lines)), Times.Once);
		}

		[Test]
		public void TestGetLine1()
		{
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Throws<SystemException>();
			var index = 9001;
			new Action(() => _proxy.GetLine(index)).ShouldNotThrow();
			_logFile.Verify(x => x.GetLine(It.Is<int>(y => y == index)), Times.Once);
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex()
		{
			_logFile.Setup(x => x.GetLogLineIndexOfOriginalLineIndex(It.IsAny<LogLineIndex>())).Throws<SystemException>();
			var index = new LogLineIndex(42);
			new Action(() => _proxy.GetLogLineIndexOfOriginalLineIndex(index)).ShouldNotThrow();
			_logFile.Verify(x => x.GetLogLineIndexOfOriginalLineIndex(It.Is<LogLineIndex>(y => y == index)), Times.Once);
		}

		[Test]
		public void TestGetOriginalIndexFrom()
		{
			_logFile.Setup(x => x.GetOriginalIndexFrom(It.IsAny<LogLineIndex>())).Throws<SystemException>();
			var index = new LogLineIndex(42);
			new Action(() => _proxy.GetOriginalIndexFrom(index)).ShouldNotThrow();
			_logFile.Verify(x => x.GetOriginalIndexFrom(It.Is<LogLineIndex>(y => y == index)), Times.Once);
		}

		[Test]
		public void TestGetOriginalIndicesFrom1()
		{
			_logFile.Setup(x => x.GetOriginalIndicesFrom(It.IsAny<LogFileSection>(), It.IsAny<LogLineIndex[]>())).Throws<SystemException>();
			var section = new LogFileSection(9001, 42);
			var indices = new LogLineIndex[42];
			new Action(() => _proxy.GetOriginalIndicesFrom(section, indices)).ShouldNotThrow();
			_logFile.Verify(x => x.GetOriginalIndicesFrom(It.Is<LogFileSection>(y => y == section),
				It.Is<LogLineIndex[]>(y => y == indices)), Times.Once);
		}

		[Test]
		public void TestGetOriginalIndicesFrom2()
		{
			_logFile.Setup(x => x.GetOriginalIndicesFrom(It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<LogLineIndex[]>())).Throws<SystemException>();
			var src = new LogLineIndex[42];
			var dest = new LogLineIndex[42];
			new Action(() => _proxy.GetOriginalIndicesFrom(src, dest)).ShouldNotThrow();
			_logFile.Verify(x => x.GetOriginalIndicesFrom(It.Is<IReadOnlyList<LogLineIndex>>(y => y == src),
				It.Is<LogLineIndex[]>(y => y == dest)), Times.Once);
		}
	}
}