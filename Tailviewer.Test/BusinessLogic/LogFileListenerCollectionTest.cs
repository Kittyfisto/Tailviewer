using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogFileListenerCollectionTest
	{
		[Test]
		[Description(
			"Verifies that AddListener may be called multiple times, but if it is, then events aren't fired multiple times for each invocation"
			)]
		public void TestAddListener1()
		{
			ILogFile logFile = new Mock<ILogFile>().Object;
			var collection = new LogFileListenerCollection(logFile);
			var listener = new Mock<ILogFileListener>();
			var sections = new List<LogFileSection>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
			        .Callback((ILogFile file, LogFileSection y) => sections.Add(y));

			collection.AddListener(listener.Object, TimeSpan.FromSeconds(1), 10);
			new Action(() => collection.AddListener(listener.Object, TimeSpan.FromSeconds(1), 10)).ShouldNotThrow();

			collection.OnRead(10);
			sections.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 10)
				}, "Because even though we added the listener twice, it should never be invoked twice");
		}

		[Test]
		public void TestInvalidate()
		{
			var collection = new LogFileListenerCollection(new Mock<ILogFile>().Object);
			collection.OnRead(1);
			collection.CurrentLineIndex.Should().Be(1);
			collection.Invalidate(0, 1);
			collection.CurrentLineIndex.Should().Be(0);
		}
	}
}