using System;
using System.Collections.Generic;
using System.Windows;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Controls;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	public sealed class LogEntryListViewTest
	{
		private LogEntryListView _control;
		private Mock<ILogFile> _logFile;
		private List<LogLine> _lines;
		private List<ILogFileListener> _listeners;

		[SetUp]
		[STAThread]
		public void SetUp()
		{
			_control = new LogEntryListView
				{
					Width = 1024,
					Height = 768
				};
			var availableSize = new Size(1024, 768);
			_control.Measure(availableSize);
			_control.Arrange(new Rect(new Point(), availableSize));

			_lines = new List<LogLine>();
			_listeners = new List<ILogFileListener>();

			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.Count).Returns(() => _lines.Count);
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
			        .Callback((LogFileSection section, LogLine[] dest) =>
			                  _lines.CopyTo((int) section.Index, dest, 0, section.Count));
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			        .Callback((ILogFileListener listener, TimeSpan maximumTimeout, int maximumLines) =>
				        {
					        _listeners.Add(listener);
					        listener.OnLogFileModified(_logFile.Object,
					                                   new LogFileSection(0, _lines.Count));
				        });
		}

		[Test]
		[STAThread]
		[Description("Verifies that an empty log file can be represented")]
		public void TestSetLogFile1()
		{
			new Action(() => _control.LogFile = _logFile.Object).ShouldNotThrow();
			_control.LogFile.Should().BeSameAs(_logFile.Object);
		}

		[Test]
		[STAThread]
		[Description("Verfies that a log file with one line can be represented")]
		public void TestSetLogFile2()
		{
			_lines.Add(new LogLine(0, 0, "Foobar", LevelFlags.Debug));

			new Action(() => _control.LogFile = _logFile.Object).ShouldNotThrow();

			_control.VisibleTextLines.Count.Should().Be(1, "Because the log file contains one log line");
			_control.VisibleTextLines[0].LogLine.Should().Be(_lines[0]);
		}
	}
}