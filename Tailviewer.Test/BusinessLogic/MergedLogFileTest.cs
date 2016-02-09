using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class MergedLogFileTest
	{
		[Test]
		[Description("Verifies that creating a merged log file from two sources is possible")]
		public void TestCtor1()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			MergedLogFile logFile = null;
			new Action(() => logFile = new MergedLogFile(source1.Object, source2.Object))
				.ShouldNotThrow();
			logFile.Should().NotBeNull();
		}

		[Test]
		[Description("Verifies that starting a merged log file causes it to add listeners with the source files")]
		public void TestStart1()
		{
			var source = new Mock<ILogFile>();
			var listeners = new List<Tuple<ILogFileListener, TimeSpan, int>>();
			source.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			      .Callback((ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount) => listeners.Add(Tuple.Create(listener, maximumWaitTime, maximumLineCount)));

			var logFile = new MergedLogFile(source.Object);
			var waitTime = TimeSpan.FromSeconds(1);
			new Action(() => logFile.Start(waitTime)).ShouldNotThrow();
			listeners.Count.Should().Be(1, "Because the merged file should have registered exactly 1 listener with the source file");
			listeners[0].Item1.Should().NotBeNull();
			listeners[0].Item2.Should().Be(waitTime);
			listeners[0].Item3.Should().BeGreaterThan(0);
		}

		[Test]
		[Description("Verifies that disposing a non-started logfile works")]
		public void TestDispose1()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			var logFile = new MergedLogFile(source1.Object, source2.Object);
			new Action(logFile.Dispose).ShouldNotThrow();
		}

		[Test]
		[Description("Verifies that disposing a started logfile works")]
		public void TestDispose2()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			var logFile = new MergedLogFile(source1.Object, source2.Object);
			logFile.Start(TimeSpan.FromMilliseconds(1));
			new Action(logFile.Dispose).ShouldNotThrow();
		}
	}
}